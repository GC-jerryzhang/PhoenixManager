using System.Text.Json;
using System.Text.Json.Serialization;
using PhoenixToolkit.Models;
using PhoenixToolkit.Services;

namespace PhoenixToolkit.Tests;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [STAThread]
    private static int Main()
    {
        var tests = new Action[]
        {
            CleanupExpiredPlansDeletesExpiredInactivePlans,
            CleanupExpiredPlansFallsBackToFileTimestampForMalformedPlans,
            CreatePlanReusesInstallPlanCleanupRules,
            CleanupServiceRunsInstallPlanCleanupAndKeepsLockedPlans,
            CleanupResultDialogUsesFixedScrollableLayout,
            CleanupResultDialogHasBottomConfirmButton
        };

        foreach (var test in tests)
        {
            try
            {
                test();
                Console.WriteLine($"PASS {test.Method.Name}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"FAIL {test.Method.Name}");
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        Console.WriteLine($"Executed {tests.Length} tests.");
        return 0;
    }

    private static void CleanupExpiredPlansDeletesExpiredInactivePlans()
    {
        RunIsolated(config =>
        {
            var oldPending = WritePlan(
                config,
                "pending-old",
                new InstallPlan(
                    Id: "pending-old",
                    DesignerPackage: null,
                    ServerPackage: null,
                    Status: InstallPlanStatus.Pending,
                    CreatedAt: DateTimeOffset.Now.AddDays(-10)));

            var recentPending = WritePlan(
                config,
                "pending-recent",
                new InstallPlan(
                    Id: "pending-recent",
                    DesignerPackage: null,
                    ServerPackage: null,
                    Status: InstallPlanStatus.Pending,
                    CreatedAt: DateTimeOffset.Now.AddDays(-2)));

            var oldCompleted = WritePlan(
                config,
                "completed-old",
                new InstallPlan(
                    Id: "completed-old",
                    DesignerPackage: null,
                    ServerPackage: null,
                    Status: InstallPlanStatus.Completed,
                    CreatedAt: DateTimeOffset.Now.AddDays(-10),
                    CompletedAt: DateTimeOffset.Now.AddDays(-9)));

            var oldFailed = WritePlan(
                config,
                "failed-old",
                new InstallPlan(
                    Id: "failed-old",
                    DesignerPackage: null,
                    ServerPackage: null,
                    Status: InstallPlanStatus.Failed,
                    CreatedAt: DateTimeOffset.Now.AddDays(-10),
                    CompletedAt: DateTimeOffset.Now.AddDays(-8),
                    LastError: "boom"));

            var runningOld = WritePlan(
                config,
                "running-old",
                new InstallPlan(
                    Id: "running-old",
                    DesignerPackage: null,
                    ServerPackage: null,
                    Status: InstallPlanStatus.Running,
                    CreatedAt: DateTimeOffset.Now.AddDays(-30),
                    StartedAt: DateTimeOffset.Now.AddDays(-30)));

            var logs = new List<string>();
            InstallPlanService.CleanupExpiredPlans(config, (message, level) => logs.Add($"{level}:{message}"));

            Assert.False(File.Exists(oldPending), "Expired pending plan should be deleted.");
            Assert.True(File.Exists(recentPending), "Recent pending plan should be kept.");
            Assert.False(File.Exists(oldCompleted), "Expired completed plan should be deleted.");
            Assert.False(File.Exists(oldFailed), "Expired failed plan should be deleted.");
            Assert.True(File.Exists(runningOld), "Running plan should be kept.");
            Assert.Contains(logs, entry => entry.Contains("Delete install plan: pending-old.json", StringComparison.Ordinal));
            Assert.Contains(logs, entry => entry.Contains("Keep install plan: running-old.json", StringComparison.Ordinal));
        });
    }

    private static void CleanupExpiredPlansFallsBackToFileTimestampForMalformedPlans()
    {
        RunIsolated(config =>
        {
            var malformedPath = Path.Combine(config.InstallPlanDir, "malformed.json");
            Directory.CreateDirectory(config.InstallPlanDir);
            File.WriteAllText(malformedPath, "{ not-valid-json");
            File.SetLastWriteTimeUtc(malformedPath, DateTime.UtcNow.AddDays(-10));

            var logs = new List<string>();
            InstallPlanService.CleanupExpiredPlans(config, (message, level) => logs.Add($"{level}:{message}"));

            Assert.False(File.Exists(malformedPath), "Old malformed plan should be deleted by file timestamp.");
            Assert.Contains(logs, entry => entry.Contains("metadata invalid: malformed.json", StringComparison.Ordinal));
        });
    }

    private static void CreatePlanReusesInstallPlanCleanupRules()
    {
        RunIsolated(config =>
        {
            var expiredPending = WritePlan(
                config,
                "expired-before-create",
                new InstallPlan(
                    Id: "expired-before-create",
                    DesignerPackage: null,
                    ServerPackage: null,
                    Status: InstallPlanStatus.Pending,
                    CreatedAt: DateTimeOffset.Now.AddDays(-9)));

            var runningPlan = WritePlan(
                config,
                "running-before-create",
                new InstallPlan(
                    Id: "running-before-create",
                    DesignerPackage: null,
                    ServerPackage: null,
                    Status: InstallPlanStatus.Running,
                    CreatedAt: DateTimeOffset.Now.AddDays(-20),
                    StartedAt: DateTimeOffset.Now.AddDays(-20)));

            var createdPlan = InstallPlanService.CreatePlan(
                config,
                new FetchedPackageInfo(
                    Kind: PackageKind.Designer,
                    FileName: "designer.exe",
                    FullPath: Path.Combine(config.DesignerDir, "designer.exe"),
                    FetchedAt: DateTimeOffset.Now),
                serverPackage: null);

            Assert.NotNull(createdPlan, "CreatePlan should return a new plan when a package is available.");
            Assert.False(File.Exists(expiredPending), "CreatePlan should reuse cleanup rules for expired plans.");
            Assert.True(File.Exists(runningPlan), "CreatePlan should keep running plans.");
            Assert.True(File.Exists(Path.Combine(config.InstallPlanDir, $"{createdPlan!.Id}.json")), "New plan file should be written.");
        });
    }

    private static void CleanupServiceRunsInstallPlanCleanupAndKeepsLockedPlans()
    {
        RunIsolated(config =>
        {
            Directory.CreateDirectory(config.DesignerDir);
            Directory.CreateDirectory(config.ServerDir);
            Directory.CreateDirectory(config.LogDir);

            var expiredPending = WritePlan(
                config,
                "cleanup-expired",
                new InstallPlan(
                    Id: "cleanup-expired",
                    DesignerPackage: null,
                    ServerPackage: null,
                    Status: InstallPlanStatus.Pending,
                    CreatedAt: DateTimeOffset.Now.AddDays(-12)));

            var runningPlan = WritePlan(
                config,
                "cleanup-running",
                new InstallPlan(
                    Id: "cleanup-running",
                    DesignerPackage: null,
                    ServerPackage: null,
                    Status: InstallPlanStatus.Running,
                    CreatedAt: DateTimeOffset.Now.AddDays(-25),
                    StartedAt: DateTimeOffset.Now.AddDays(-25)));

            var lockedPath = WritePlan(
                config,
                "cleanup-locked",
                new InstallPlan(
                    Id: "cleanup-locked",
                    DesignerPackage: null,
                    ServerPackage: null,
                    Status: InstallPlanStatus.Pending,
                    CreatedAt: DateTimeOffset.Now.AddDays(-11)));

            using var lockedStream = new FileStream(
                lockedPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);

            var result = CleanupService.Execute(config);

            Assert.False(File.Exists(expiredPending), "CleanupService should trigger install plan deletion.");
            Assert.True(File.Exists(runningPlan), "CleanupService should keep running plans.");
            Assert.True(File.Exists(lockedPath), "Locked plan should be kept.");
            Assert.Contains(result, "Install plan cleanup", "Cleanup output should include install plan cleanup details.");
            Assert.Contains(result, "cleanup-locked.json", "Cleanup output should mention locked plan warnings.");

            var logPath = Path.Combine(config.LogDir, $"cleanup-phoenix_{DateTime.Now:yyyyMMdd}.log");
            Assert.True(File.Exists(logPath), "CleanupService should write a cleanup log file.");
            var logText = File.ReadAllText(logPath);
            Assert.Contains(logText, "Install plan summary", "Cleanup log should include install plan summary.");
        });
    }

    private static void CleanupResultDialogUsesFixedScrollableLayout()
    {
        using var dialog = new CleanupResultDialog(BuildLongCleanupResult());
        var resultTextBox = FindRequiredControl<TextBox>(dialog, "cleanupResultTextBox");

        Assert.Equal(new Size(760, 520), dialog.ClientSize, "Cleanup result dialog should use a fixed client size.");
        Assert.Equal(FormBorderStyle.FixedDialog, dialog.FormBorderStyle, "Cleanup result dialog should not auto-resize with content.");
        Assert.Equal(dialog.MinimumSize, dialog.MaximumSize, "Cleanup result dialog should have bounded size.");
        Assert.True(resultTextBox.Multiline, "Cleanup result text box should support multiple lines.");
        Assert.True(resultTextBox.ReadOnly, "Cleanup result text box should be read-only.");
        Assert.Equal(ScrollBars.Both, resultTextBox.ScrollBars, "Cleanup result text box should allow scrolling.");
        Assert.False(resultTextBox.WordWrap, "Cleanup result text should preserve line layout and horizontal scrolling.");
        Assert.Contains(resultTextBox.Text, "第 40 行", "Cleanup result dialog should preserve the full cleanup text.");
    }

    private static void CleanupResultDialogHasBottomConfirmButton()
    {
        using var dialog = new CleanupResultDialog("清理完成");
        var confirmButton = FindRequiredControl<Button>(dialog, "cleanupResultConfirmButton");
        var actionsPanel = FindRequiredControl<FlowLayoutPanel>(dialog, "cleanupResultActionsPanel");
        var rootLayout = AssertType<TableLayoutPanel>(dialog.Controls[0], "Cleanup result dialog should use a root layout panel.");

        Assert.Equal("确定", confirmButton.Text, "Cleanup result dialog should expose an explicit confirm button.");
        Assert.Equal(DialogResult.OK, confirmButton.DialogResult, "Confirm button should close the dialog.");
        Assert.Same(confirmButton, dialog.AcceptButton, "Confirm button should be the default accept action.");
        Assert.Same(confirmButton, dialog.CancelButton, "Confirm button should also handle cancel/escape.");
        Assert.Equal(FlowDirection.RightToLeft, actionsPanel.FlowDirection, "Confirm button should live in the bottom action area.");
        Assert.Equal(2, rootLayout.GetPositionFromControl(actionsPanel).Row, "Action area should be placed at the bottom row.");
    }

    private static void RunIsolated(Action<AppConfig> test)
    {
        var root = Path.Combine(Path.GetTempPath(), "PhoenixToolkit.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            test(new AppConfig(LocalBaseDir: root));
        }
        finally
        {
            try
            {
                Directory.Delete(root, recursive: true);
            }
            catch
            {
                // Ignore temp cleanup failures in the ad-hoc runner.
            }
        }
    }

    private static string WritePlan(AppConfig config, string fileName, InstallPlan plan)
    {
        Directory.CreateDirectory(config.InstallPlanDir);
        var path = Path.Combine(config.InstallPlanDir, $"{fileName}.json");
        var json = JsonSerializer.Serialize(plan, JsonOptions);
        File.WriteAllText(path, json);
        return path;
    }

    private static string BuildLongCleanupResult()
    {
        return string.Join(
            Environment.NewLine,
            Enumerable.Range(1, 40).Select(index => $"第 {index} 行清理结果 - 这是用于验证滚动区域的长文本。"));
    }

    private static T FindRequiredControl<T>(Control root, string name)
        where T : Control
    {
        var control = root.Controls.Find(name, searchAllChildren: true).FirstOrDefault();
        if (control is T typedControl)
            return typedControl;

        throw new InvalidOperationException($"Expected to find control '{name}' of type {typeof(T).Name}.");
    }

    private static class Assert
    {
        public static void True(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        public static void False(bool condition, string message)
        {
            True(!condition, message);
        }

        public static void NotNull<T>(T? value, string message)
            where T : class
        {
            if (value is null)
                throw new InvalidOperationException(message);
        }

        public static void Contains(IEnumerable<string> values, Func<string, bool> predicate)
        {
            if (!values.Any(predicate))
                throw new InvalidOperationException("Expected collection to contain a matching value.");
        }

        public static void Contains(string text, string expectedSubstring, string message)
        {
            if (!text.Contains(expectedSubstring, StringComparison.Ordinal))
                throw new InvalidOperationException(message);
        }

        public static void Equal<T>(T expected, T actual, string message)
            where T : notnull
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new InvalidOperationException($"{message} Expected: {expected}. Actual: {actual}.");
        }

        public static void Same(object expected, object? actual, string message)
        {
            if (!ReferenceEquals(expected, actual))
                throw new InvalidOperationException(message);
        }
    }

    private static T AssertType<T>(object value, string message)
        where T : class
    {
        if (value is T typedValue)
            return typedValue;

        throw new InvalidOperationException(message);
    }
}
