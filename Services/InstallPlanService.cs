using System.Text.Json;
using PhoenixToolkit.Models;

namespace PhoenixToolkit.Services;

public static class InstallPlanService
{
    private const int RetentionDays = 7;

    public static InstallPlan? CreatePlan(
        AppConfig config,
        FetchedPackageInfo? designerPackage,
        FetchedPackageInfo? serverPackage)
    {
        if (designerPackage is null && serverPackage is null)
            return null;

        CleanupExpiredPlans(config);
        Directory.CreateDirectory(config.InstallPlanDir);

        var plan = new InstallPlan(
            Id: Guid.NewGuid().ToString("N"),
            DesignerPackage: designerPackage,
            ServerPackage: serverPackage);

        WritePlan(config, plan);
        return plan;
    }

    public static void CleanupExpiredPlans(AppConfig config, Action<string, string>? log = null)
    {
        Directory.CreateDirectory(config.InstallPlanDir);

        var planPaths = Directory.GetFiles(config.InstallPlanDir, "*.json")
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (planPaths.Count == 0)
        {
            log?.Invoke($"Install plans: nothing to clean (retaining {RetentionDays} days).", "INFO");
            return;
        }

        log?.Invoke($"--- Install plan cleanup: {planPaths.Count} files ---", "INFO");

        var cutoff = DateTimeOffset.Now.AddDays(-RetentionDays);
        var kept = 0;
        var deleted = 0;

        foreach (var path in planPaths)
        {
            var fileName = Path.GetFileName(path);

            try
            {
                var decision = EvaluateCleanupDecision(path, cutoff, log);
                if (!decision.ShouldDelete)
                {
                    log?.Invoke($"Keep install plan: {fileName} ({decision.Reason})", "INFO");
                    kept++;
                    continue;
                }

                File.Delete(path);
                log?.Invoke($"Delete install plan: {fileName} ({decision.Reason})", "INFO");
                deleted++;
            }
            catch (IOException ex)
            {
                log?.Invoke(
                    $"Keep install plan: {fileName} (cleanup skipped: {ex.Message})",
                    "WARNING");
                kept++;
            }
            catch (UnauthorizedAccessException ex)
            {
                log?.Invoke(
                    $"Keep install plan: {fileName} (cleanup skipped: {ex.Message})",
                    "WARNING");
                kept++;
            }
        }

        log?.Invoke($"Install plan summary: kept={kept}, deleted={deleted}", "INFO");
    }

    public static bool TryStartPlan(
        AppConfig config,
        string planId,
        out InstallPlan? plan,
        out string errorMessage)
    {
        return TryUpdatePlan(
            config,
            planId,
            existing =>
            {
                if (existing.Status == InstallPlanStatus.Running)
                    throw new InvalidOperationException("该安装任务正在执行，请勿重复点击通知。");

                if (existing.Status is InstallPlanStatus.Completed or InstallPlanStatus.Failed)
                    throw new InvalidOperationException("该安装任务已处理完成，不能重复执行。");

                return existing with
                {
                    Status = InstallPlanStatus.Running,
                    StartedAt = DateTimeOffset.Now,
                    CompletedAt = null,
                    LastError = null
                };
            },
            out plan,
            out errorMessage);
    }

    public static void MarkCompleted(AppConfig config, string planId)
    {
        TryUpdatePlan(
            config,
            planId,
            existing => existing with
            {
                Status = InstallPlanStatus.Completed,
                CompletedAt = DateTimeOffset.Now,
                LastError = null
            },
            out _,
            out _);
    }

    public static void MarkFailed(AppConfig config, string planId, string errorMessage)
    {
        TryUpdatePlan(
            config,
            planId,
            existing => existing with
            {
                Status = InstallPlanStatus.Failed,
                CompletedAt = DateTimeOffset.Now,
                LastError = errorMessage
            },
            out _,
            out _);
    }

    private static bool TryUpdatePlan(
        AppConfig config,
        string planId,
        Func<InstallPlan, InstallPlan> update,
        out InstallPlan? updatedPlan,
        out string errorMessage)
    {
        updatedPlan = null;
        errorMessage = string.Empty;

        string planPath;
        try
        {
            planPath = GetPlanPath(config, planId);
        }
        catch (ArgumentException)
        {
            errorMessage = "安装任务编号无效。";
            return false;
        }

        if (!File.Exists(planPath))
        {
            errorMessage = "安装任务不存在或已过期。";
            return false;
        }

        try
        {
            using var stream = new FileStream(
                planPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);

            var existingPlan = JsonSerializer.Deserialize(
                stream,
                AppConfigJsonContext.Default.InstallPlan);

            if (existingPlan is null)
            {
                errorMessage = "安装任务内容损坏，无法执行。";
                return false;
            }

            if (!string.Equals(existingPlan.Id, NormalizePlanId(planId), StringComparison.Ordinal))
            {
                errorMessage = "安装任务编号与内容不匹配。";
                return false;
            }

            var nextPlan = update(existingPlan);
            stream.Position = 0;
            stream.SetLength(0);
            JsonSerializer.Serialize(stream, nextPlan, AppConfigJsonContext.Default.InstallPlan);
            stream.Flush(true);

            updatedPlan = nextPlan;
            return true;
        }
        catch (IOException)
        {
            errorMessage = "该安装任务正在执行，请稍后再试。";
            return false;
        }
        catch (InvalidOperationException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
        catch (Exception ex)
        {
            errorMessage = $"读取安装任务失败: {ex.Message}";
            return false;
        }
    }

    private static void WritePlan(AppConfig config, InstallPlan plan)
    {
        var planPath = GetPlanPath(config, plan.Id);
        using var stream = new FileStream(
            planPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);

        JsonSerializer.Serialize(stream, plan, AppConfigJsonContext.Default.InstallPlan);
        stream.Flush(true);
    }

    private static string GetPlanPath(AppConfig config, string planId)
    {
        return Path.Combine(config.InstallPlanDir, $"{NormalizePlanId(planId)}.json");
    }

    private static string NormalizePlanId(string planId)
    {
        if (!Guid.TryParseExact(planId, "N", out var parsedPlanId))
            throw new ArgumentException("Invalid install plan identifier.", nameof(planId));

        return parsedPlanId.ToString("N");
    }

    private static InstallPlanCleanupDecision EvaluateCleanupDecision(
        string path,
        DateTimeOffset cutoff,
        Action<string, string>? log)
    {
        var fileName = Path.GetFileName(path);
        var fallbackTimestamp = new DateTimeOffset(File.GetLastWriteTimeUtc(path), TimeSpan.Zero);
        InstallPlan? plan = null;

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            plan = JsonSerializer.Deserialize(stream, AppConfigJsonContext.Default.InstallPlan);
            if (plan is null)
                log?.Invoke($"Install plan metadata unreadable: {fileName}, using file timestamp.", "WARNING");
        }
        catch (IOException)
        {
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (JsonException ex)
        {
            log?.Invoke($"Install plan metadata invalid: {fileName}, using file timestamp. {ex.Message}", "WARNING");
        }
        catch (Exception ex)
        {
            log?.Invoke($"Install plan metadata unavailable: {fileName}, using file timestamp. {ex.Message}", "WARNING");
        }

        if (plan?.Status == InstallPlanStatus.Running)
            return new InstallPlanCleanupDecision(false, "running plan");

        var (timestamp, source) = GetRetentionReference(plan, fallbackTimestamp);
        if (timestamp < cutoff)
            return new InstallPlanCleanupDecision(true, $"expired {source}");

        return new InstallPlanCleanupDecision(false, $"within retention by {source}");
    }

    private static (DateTimeOffset Timestamp, string Source) GetRetentionReference(
        InstallPlan? plan,
        DateTimeOffset fallbackTimestamp)
    {
        if (plan is null)
            return (fallbackTimestamp, "file timestamp");

        return plan.Status switch
        {
            InstallPlanStatus.Pending when plan.CreatedAt != default
                => (plan.CreatedAt, "pending created time"),

            InstallPlanStatus.Completed or InstallPlanStatus.Failed
                when plan.CompletedAt is { } completedAt && completedAt != default
                => (completedAt, $"{plan.Status.ToString().ToLowerInvariant()} completed time"),

            _ => (fallbackTimestamp, "file timestamp")
        };
    }

    private readonly record struct InstallPlanCleanupDecision(bool ShouldDelete, string Reason);
}
