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

        var planPath = GetPlanPath(config, planId);
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
        return Path.Combine(config.InstallPlanDir, $"{planId}.json");
    }

    private static void CleanupExpiredPlans(AppConfig config)
    {
        Directory.CreateDirectory(config.InstallPlanDir);

        foreach (var path in Directory.GetFiles(config.InstallPlanDir, "*.json"))
        {
            try
            {
                var age = DateTimeOffset.Now - File.GetLastWriteTimeUtc(path);
                if (age.TotalDays > RetentionDays)
                    File.Delete(path);
            }
            catch
            {
                // Ignore cleanup failures so fetch/install flow remains available.
            }
        }
    }
}
