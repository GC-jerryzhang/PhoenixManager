using System.Collections.Concurrent;
using Microsoft.Toolkit.Uwp.Notifications;
using PhoenixToolkit.Models;

namespace PhoenixToolkit.Services;

public static class InstallActivationService
{
    private static readonly ManualResetEventSlim StartupActivationReceived = new(false);
    private static readonly ConcurrentQueue<ToastNotificationActivatedEventArgsCompat> PendingActivations = new();
    private static int _initialized;
    private static int _drainInProgress;

    public static void Initialize()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
            return;

        ToastNotificationManagerCompat.OnActivated += OnToastActivated;
    }

    public static bool HandleStartupActivationIfNeeded()
    {
        if (!ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
            return false;

        StartupActivationReceived.Wait(TimeSpan.FromSeconds(5));
        DrainPendingActivations();
        return true;
    }

    private static void OnToastActivated(ToastNotificationActivatedEventArgsCompat activationArgs)
    {
        PendingActivations.Enqueue(activationArgs);
        StartupActivationReceived.Set();
        ThreadPool.QueueUserWorkItem(_ => DrainPendingActivations());
    }

    private static bool DrainPendingActivations()
    {
        if (Interlocked.Exchange(ref _drainInProgress, 1) == 1)
            return false;

        var handledAny = false;

        try
        {
            while (PendingActivations.TryDequeue(out var activationArgs))
            {
                handledAny |= TryHandleActivation(activationArgs);
            }

            return handledAny;
        }
        finally
        {
            Interlocked.Exchange(ref _drainInProgress, 0);
        }
    }

    private static bool TryHandleActivation(ToastNotificationActivatedEventArgsCompat activationArgs)
    {
        ToastArguments toastArguments;

        try
        {
            toastArguments = ToastArguments.Parse(activationArgs.Argument);
        }
        catch
        {
            return false;
        }

        if (!toastArguments.TryGetValue("action", out var action) ||
            !string.Equals(action, "install", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!toastArguments.TryGetValue("planId", out var planId) ||
            string.IsNullOrWhiteSpace(planId))
        {
            NotificationService.ShowInstallFailure("通知缺少安装任务编号，无法执行安装。");
            return true;
        }

        ExecuteInstallPlan(planId);
        return true;
    }

    public static void ExecuteInstallPlan(string planId)
    {
        var config = ConfigService.Load();

        if (!InstallPlanService.TryStartPlan(config, planId, out var plan, out var errorMessage))
        {
            NotificationService.ShowInstallFailure(errorMessage);
            AppendInstallLog(config, errorMessage, "ERROR");
            return;
        }

        try
        {
            ValidatePlan(config, plan!);
            AppendInstallLog(config, BuildStartMessage(plan!));
            InstallerLaunchService.ExecutePlan(plan!);
            InstallPlanService.MarkCompleted(config, planId);
            AppendInstallLog(config, $"安装任务执行完成: {planId}");
        }
        catch (Exception ex)
        {
            InstallPlanService.MarkFailed(config, planId, ex.Message);
            AppendInstallLog(config, $"安装任务执行失败: {planId} - {ex.Message}", "ERROR");
            NotificationService.ShowInstallFailure(ex.Message);
        }
    }

    private static void ValidatePlan(AppConfig config, InstallPlan plan)
    {
        ValidatePackage(plan.DesignerPackage, config.DesignerDir);
        ValidatePackage(plan.ServerPackage, config.ServerDir);
    }

    private static void ValidatePackage(FetchedPackageInfo? package, string expectedRoot)
    {
        if (package is null)
            return;

        if (!File.Exists(package.FullPath))
            throw new FileNotFoundException($"安装包不存在: {package.FileName}", package.FullPath);

        var fullPath = Path.GetFullPath(package.FullPath);
        var fullRoot = Path.GetFullPath(expectedRoot);
        var relativePath = Path.GetRelativePath(fullRoot, fullPath);

        if (relativePath.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relativePath))
            throw new InvalidOperationException($"安装包路径不合法: {package.FileName}");
    }

    private static string BuildStartMessage(InstallPlan plan)
    {
        if (plan.DesignerPackage is not null && plan.ServerPackage is not null)
        {
            return $"开始执行安装任务: {plan.Id}，顺序为 Designer -> Server";
        }

        var package = plan.DesignerPackage ?? plan.ServerPackage
            ?? throw new InvalidOperationException("安装任务中没有可执行的安装包。");

        return $"开始执行安装任务: {plan.Id}，安装包 {package.FileName}";
    }

    private static void AppendInstallLog(AppConfig config, string message, string level = "INFO")
    {
        Directory.CreateDirectory(config.LogDir);
        var logPath = Path.Combine(config.LogDir, $"install-phoenix_{DateTime.Now:yyyyMMdd}.log");
        var entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
        File.AppendAllLines(logPath, new[] { entry });
    }
}
