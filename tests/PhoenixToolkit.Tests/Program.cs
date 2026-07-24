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
            FetchServiceUsesWindowsPlatformSubdirectoriesWhenAvailable,
            FetchServiceFallsBackToFlatSourceDirectoryWhenWindowsSubdirectoriesAreMissing,
            FetchServiceDoesNotFallBackWhenWindowsPlatformSubdirectoryIsEmpty,
            FetchServiceDoesNotFallBackWhenWindowsPlatformLayoutIsIncomplete,
            FetchServiceDoesNotFallBackWhenOnlyLinuxPlatformDirectoriesArePresent,
            CleanupExpiredPlansDeletesExpiredInactivePlans,
            CleanupExpiredPlansFallsBackToFileTimestampForMalformedPlans,
            CreatePlanReusesInstallPlanCleanupRules,
            CleanupServiceRunsInstallPlanCleanupAndKeepsLockedPlans,
            CleanupResultDialogUsesFixedScrollableLayout,
            CleanupResultDialogHasBottomConfirmButton,
            DevelopmentModeRecognizesLaunchArgumentsAndUsesIsolatedConfig,
            DevelopmentModeBlocksSchedulerChanges,
            MainFormDisplaysDevelopmentModeMarker,
            MissingCloseBehaviorPreferenceDefaultsToAskEveryTime,
            CloseBehaviorPreferenceRoundTripsThroughConfigService,
            CloseBehaviorDecisionRespectsPreferenceAndBypassRules,
            SettingsDialogShowsSavesAndCancelsCloseBehaviorPreference,
            MainFormAndCloseDialogExposeTrayCloseControls
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

    private static void FetchServiceUsesWindowsPlatformSubdirectoriesWhenAvailable()
    {
        RunIsolated(config =>
        {
            var sourceDir = Path.Combine(config.LocalBaseDir, "source");
            var configuredSource = config with { SourceDir = sourceDir };
            var designerSourceDir = Path.Combine(sourceDir, "Designer-Windows");
            var serverSourceDir = Path.Combine(sourceDir, "Server-Windows");
            Directory.CreateDirectory(designerSourceDir);
            Directory.CreateDirectory(serverSourceDir);

            Assert.Equal(
                designerSourceDir,
                FetchService.ResolveWindowsPackageSourceDirectory(configuredSource, PackageKind.Designer),
                "Designer should use its Windows platform subdirectory when available.");
            Assert.Equal(
                serverSourceDir,
                FetchService.ResolveWindowsPackageSourceDirectory(configuredSource, PackageKind.Server),
                "Server should use its Windows platform subdirectory when available.");
        });
    }

    private static void FetchServiceFallsBackToFlatSourceDirectoryWhenWindowsSubdirectoriesAreMissing()
    {
        RunIsolated(config =>
        {
            var sourceDir = Path.Combine(config.LocalBaseDir, "source");
            Directory.CreateDirectory(sourceDir);
            var configuredSource = config with { SourceDir = sourceDir };

            Assert.Equal(
                sourceDir,
                FetchService.ResolveWindowsPackageSourceDirectory(configuredSource, PackageKind.Designer),
                "Designer should retain support for the legacy flat source layout.");
            Assert.Equal(
                sourceDir,
                FetchService.ResolveWindowsPackageSourceDirectory(configuredSource, PackageKind.Server),
                "Server should retain support for the legacy flat source layout.");
        });
    }

    private static void FetchServiceDoesNotFallBackWhenWindowsPlatformSubdirectoryIsEmpty()
    {
        RunIsolated(config =>
        {
            var sourceDir = Path.Combine(config.LocalBaseDir, "source");
            var serverSourceDir = Path.Combine(sourceDir, "Server-Windows");
            Directory.CreateDirectory(serverSourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "Phoenix-Server-Windows-202607241249.exe"), "legacy package");
            var configuredSource = config with { SourceDir = sourceDir };

            Assert.Equal(
                serverSourceDir,
                FetchService.ResolveWindowsPackageSourceDirectory(configuredSource, PackageKind.Server),
                "An existing Windows platform directory should remain authoritative even when it is empty.");
        });
    }

    private static void FetchServiceDoesNotFallBackWhenWindowsPlatformLayoutIsIncomplete()
    {
        RunIsolated(config =>
        {
            var sourceDir = Path.Combine(config.LocalBaseDir, "source");
            Directory.CreateDirectory(Path.Combine(sourceDir, "Designer-Windows"));
            File.WriteAllText(Path.Combine(sourceDir, "Phoenix-Server-Windows-202607241249.exe"), "legacy package");
            var configuredSource = config with { SourceDir = sourceDir };

            Assert.Equal(
                Path.Combine(sourceDir, "Server-Windows"),
                FetchService.ResolveWindowsPackageSourceDirectory(configuredSource, PackageKind.Server),
                "A partially migrated platform layout should not read a legacy Server package from the root directory.");
        });
    }

    private static void FetchServiceDoesNotFallBackWhenOnlyLinuxPlatformDirectoriesArePresent()
    {
        RunIsolated(config =>
        {
            var sourceDir = Path.Combine(config.LocalBaseDir, "source");
            Directory.CreateDirectory(Path.Combine(sourceDir, "Designer-Linux"));
            File.WriteAllText(Path.Combine(sourceDir, "Phoenix-Windows-0.0.1-Setup.exe"), "legacy package");
            var configuredSource = config with { SourceDir = sourceDir };

            Assert.Equal(
                Path.Combine(sourceDir, "Designer-Windows"),
                FetchService.ResolveWindowsPackageSourceDirectory(configuredSource, PackageKind.Designer),
                "A Linux platform directory should prevent a Windows fetch from reading a legacy package at the root.");
        });
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

    private static void DevelopmentModeRecognizesLaunchArgumentsAndUsesIsolatedConfig()
    {
        RunInDevelopmentMode(devRoot =>
        {
            var launchArgs = RuntimeModeService.Initialize(new[] { "--dev", "--cleanup" });

            Assert.True(RuntimeModeService.IsDevelopment, "Runtime mode should switch to development when --dev is present.");
            Assert.Equal(1, launchArgs.Length, "Development flag should be removed from the launch argument list.");
            Assert.Equal("--cleanup", launchArgs[0], "Remaining launch arguments should be preserved.");
            Assert.Equal(Path.Combine(devRoot, "config.json"), ConfigService.ConfigPath, "Dev mode config should be isolated from the publish directory.");

            var config = ConfigService.Load();

            Assert.True(File.Exists(ConfigService.ConfigPath), "Dev mode should create an isolated config file when missing.");
            Assert.Equal(Path.Combine(devRoot, "historyPackage"), config.LocalBaseDir, "Dev mode default local storage should live under the dev root.");
        });
    }

    private static void DevelopmentModeBlocksSchedulerChanges()
    {
        RunInDevelopmentMode(devRoot =>
        {
            RuntimeModeService.Initialize(new[] { "--dev" });

            var installMessage = SchedulerService.Install(new AppConfig(LocalBaseDir: Path.Combine(devRoot, "historyPackage")));
            var uninstallMessage = SchedulerService.Uninstall();
            var (fetchInstalled, cleanupInstalled) = SchedulerService.GetStatus();

            Assert.Contains(installMessage, "DEV 模式", "Dev mode should block task installation.");
            Assert.Contains(uninstallMessage, "DEV 模式", "Dev mode should block task uninstallation.");
            Assert.False(fetchInstalled, "Dev mode should not report production task installation state.");
            Assert.False(cleanupInstalled, "Dev mode should not report production task installation state.");
        });
    }

    private static void MainFormDisplaysDevelopmentModeMarker()
    {
        RunInDevelopmentMode(_ =>
        {
            RuntimeModeService.Initialize(new[] { "--dev" });

            using var form = new MainForm();

            Assert.Contains(form.Text, "[DEV]", "Main window title should clearly indicate development mode.");
        });
    }

    private static void MissingCloseBehaviorPreferenceDefaultsToAskEveryTime()
    {
        RunInDevelopmentMode(devRoot =>
        {
            RuntimeModeService.Initialize(new[] { "--dev" });
            Directory.CreateDirectory(devRoot);
            File.WriteAllText(
                ConfigService.ConfigPath,
                $$"""
                {
                  "sourceDir": "\\\\example\\share",
                  "localBaseDir": "{{Path.Combine(devRoot, "historyPackage").Replace("\\", "\\\\")}}",
                  "fetchIntervalMinutes": 15,
                  "cleanupTime": "08:30",
                  "cleanupWeeks": {
                    "keepAllWeeks": 3,
                    "keepDailyWeeks": 6,
                    "deleteAfterWeeks": 9
                  }
                }
                """);

            var config = ConfigService.Load();

            Assert.Equal(
                CloseBehaviorPreference.AskEveryTime,
                config.CloseBehaviorPreference,
                "Missing close behavior preference should default to asking every time.");

            File.WriteAllText(
                ConfigService.ConfigPath,
                $$"""
                {
                  "sourceDir": "\\\\example\\share",
                  "localBaseDir": "{{Path.Combine(devRoot, "historyPackage").Replace("\\", "\\\\")}}",
                  "fetchIntervalMinutes": 15,
                  "cleanupTime": "08:30",
                  "cleanupWeeks": {
                    "keepAllWeeks": 3,
                    "keepDailyWeeks": 6,
                    "deleteAfterWeeks": 9
                  },
                  "closeBehaviorPreference": "UnknownValue"
                }
                """);

            config = ConfigService.Load();

            Assert.Equal(
                CloseBehaviorPreference.AskEveryTime,
                config.CloseBehaviorPreference,
                "Unreadable close behavior preference should default to asking every time.");
        });
    }

    private static void CloseBehaviorPreferenceRoundTripsThroughConfigService()
    {
        RunInDevelopmentMode(devRoot =>
        {
            RuntimeModeService.Initialize(new[] { "--dev" });
            var config = new AppConfig(
                LocalBaseDir: Path.Combine(devRoot, "historyPackage"),
                CloseBehaviorPreference: CloseBehaviorPreference.MinimizeToTray);

            ConfigService.Save(config);
            var json = File.ReadAllText(ConfigService.ConfigPath);
            var loaded = ConfigService.Load();

            Assert.Contains(
                json,
                "\"closeBehaviorPreference\": \"MinimizeToTray\"",
                "Close behavior preference should be persisted as readable JSON.");
            Assert.Equal(
                CloseBehaviorPreference.MinimizeToTray,
                loaded.CloseBehaviorPreference,
                "Close behavior preference should round-trip through ConfigService.");
        });
    }

    private static void CloseBehaviorDecisionRespectsPreferenceAndBypassRules()
    {
        Assert.Equal(
            CloseBehaviorAction.ExitApplication,
            CloseBehaviorService.ResolveCloseAction(
                CloseBehaviorPreference.MinimizeToTray,
                isExplicitExit: true,
                CloseReason.UserClosing),
            "Explicit exit should bypass minimize-to-tray preference.");
        Assert.Equal(
            CloseBehaviorAction.ExitApplication,
            CloseBehaviorService.ResolveCloseAction(
                CloseBehaviorPreference.AskEveryTime,
                isExplicitExit: false,
                CloseReason.WindowsShutDown),
            "Windows shutdown should bypass the close prompt.");
        Assert.Equal(
            CloseBehaviorAction.ExitApplication,
            CloseBehaviorService.ResolveCloseAction(
                CloseBehaviorPreference.AskEveryTime,
                isExplicitExit: false,
                CloseReason.ApplicationExitCall),
            "Application exit calls should bypass the close prompt.");
        Assert.Equal(
            CloseBehaviorAction.PromptUser,
            CloseBehaviorService.ResolveCloseAction(
                CloseBehaviorPreference.AskEveryTime,
                isExplicitExit: false,
                CloseReason.UserClosing),
            "AskEveryTime should prompt for user close actions.");
        Assert.Equal(
            CloseBehaviorAction.ExitApplication,
            CloseBehaviorService.ResolveCloseAction(
                CloseBehaviorPreference.ExitApplication,
                isExplicitExit: false,
                CloseReason.UserClosing),
            "ExitApplication preference should exit directly.");
        Assert.Equal(
            CloseBehaviorAction.MinimizeToTray,
            CloseBehaviorService.ResolveCloseAction(
                CloseBehaviorPreference.MinimizeToTray,
                isExplicitExit: false,
                CloseReason.UserClosing),
            "MinimizeToTray preference should minimize directly.");
    }

    private static void SettingsDialogShowsSavesAndCancelsCloseBehaviorPreference()
    {
        RunInDevelopmentMode(devRoot =>
        {
            RuntimeModeService.Initialize(new[] { "--dev" });
            ConfigService.Save(new AppConfig(
                LocalBaseDir: Path.Combine(devRoot, "historyPackage"),
                CloseBehaviorPreference: CloseBehaviorPreference.ExitApplication));

            using (var cancelDialog = new ApplicationSettingsDialog())
            {
                var preferenceComboBox = FindRequiredControl<ComboBox>(cancelDialog, "closeBehaviorPreferenceComboBox");

                Assert.Equal(
                    CloseBehaviorPreference.ExitApplication,
                    cancelDialog.SelectedCloseBehaviorPreference,
                    "Settings dialog should select the current close behavior preference.");

                preferenceComboBox.SelectedItem = CloseBehaviorPreference.MinimizeToTray;
            }

            Assert.Equal(
                CloseBehaviorPreference.ExitApplication,
                ConfigService.Load().CloseBehaviorPreference,
                "Closing settings without saving should leave close behavior unchanged.");

            using (var saveDialog = new ApplicationSettingsDialog())
            {
                var preferenceComboBox = FindRequiredControl<ComboBox>(saveDialog, "closeBehaviorPreferenceComboBox");
                _ = FindRequiredControl<Button>(saveDialog, "settingsSaveButton");

                preferenceComboBox.SelectedItem = CloseBehaviorPreference.MinimizeToTray;
                saveDialog.SaveSelectedCloseBehaviorPreference();
            }

            Assert.Equal(
                CloseBehaviorPreference.MinimizeToTray,
                ConfigService.Load().CloseBehaviorPreference,
                "Saving settings should persist the selected close behavior preference.");
        });
    }

    private static void MainFormAndCloseDialogExposeTrayCloseControls()
    {
        RunInDevelopmentMode(_ =>
        {
            RuntimeModeService.Initialize(new[] { "--dev" });

            using var form = new MainForm();
            var trayContextMenu = GetPrivateField<ContextMenuStrip>(form, "trayContextMenu");
            var trayIcon = GetPrivateField<NotifyIcon>(form, "trayIcon");

            Assert.False(trayIcon.Visible, "Tray icon should start hidden while the main window is open.");
            var trayMenuTexts = trayContextMenu.Items
                .OfType<ToolStripMenuItem>()
                .Select(item => item.Text ?? string.Empty);
            Assert.Contains(
                trayMenuTexts,
                text => text == "打开主页面");
            Assert.Contains(
                trayMenuTexts,
                text => text == "设置");
            Assert.Contains(
                trayMenuTexts,
                text => text == "退出");

            using var closeDialog = new CloseChoiceDialog();
            Assert.NotNull(
                FindButtonByText(closeDialog, "彻底退出"),
                "Close choice dialog should expose a full-exit action.");
            Assert.NotNull(
                FindButtonByText(closeDialog, "最小化到系统托盘"),
                "Close choice dialog should expose a minimize-to-tray action.");
        });
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

    private static void RunInDevelopmentMode(Action<string> test)
    {
        var devRoot = Path.Combine(Path.GetTempPath(), "PhoenixToolkit.DevTests", Guid.NewGuid().ToString("N"));
        var previousDevRoot = Environment.GetEnvironmentVariable("PHOENIXTOOLKIT_DEV_ROOT");
        Directory.CreateDirectory(devRoot);

        try
        {
            Environment.SetEnvironmentVariable("PHOENIXTOOLKIT_DEV_ROOT", devRoot);
            test(devRoot);
        }
        finally
        {
            RuntimeModeService.Initialize(Array.Empty<string>());
            Environment.SetEnvironmentVariable("PHOENIXTOOLKIT_DEV_ROOT", previousDevRoot);

            try
            {
                Directory.Delete(devRoot, recursive: true);
            }
            catch
            {
                // Ignore temp cleanup failures in the ad-hoc runner.
            }
        }
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

    private static Button? FindButtonByText(Control root, string text)
    {
        foreach (Control child in root.Controls)
        {
            if (child is Button button && button.Text == text)
                return button;

            var nestedButton = FindButtonByText(child, text);
            if (nestedButton is not null)
                return nestedButton;
        }

        return null;
    }

    private static T GetPrivateField<T>(object instance, string fieldName)
        where T : class
    {
        var field = instance.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (field?.GetValue(instance) is T value)
            return value;

        throw new InvalidOperationException($"Expected private field '{fieldName}' of type {typeof(T).Name}.");
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
