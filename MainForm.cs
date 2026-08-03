using System.Diagnostics;
using PhoenixToolkit.Models;
using PhoenixToolkit.Services;

namespace PhoenixToolkit;

public partial class MainForm : Form
{
    private CloseBehaviorPreference closeBehaviorPreference = CloseBehaviorPreference.AskEveryTime;
    private NotifyIcon trayIcon = null!;
    private ContextMenuStrip trayContextMenu = null!;
    private bool isExplicitExit;
    private bool hasShownTrayTip;

    public MainForm()
    {
        InitializeComponent();
        InitializeTrayComponents();
        ApplyRuntimeModePresentation();
        LoadConfig();
        RefreshStatus();
    }

    private void LoadConfig()
    {
        var config = ConfigService.Load();
        closeBehaviorPreference = config.CloseBehaviorPreference;
        txtSourceDir.Text = config.SourceDir;
        txtLocalBaseDir.Text = config.LocalBaseDir;
        numFetchInterval.Value = config.FetchIntervalMinutes;
        numKeepAll.Value = config.CleanupWeeks.KeepAllWeeks;
        numKeepDaily.Value = config.CleanupWeeks.KeepDailyWeeks;
        numDeleteAfter.Value = config.CleanupWeeks.DeleteAfterWeeks;
        timeCleanup.Value = ParseCleanupTime(config.CleanupTime);
        ResetPathTextScroll(txtSourceDir);
        ResetPathTextScroll(txtLocalBaseDir);
    }

    private AppConfig BuildConfigFromUI()
    {
        return new AppConfig(
            SourceDir: txtSourceDir.Text.Trim(),
            LocalBaseDir: txtLocalBaseDir.Text.Trim(),
            FetchIntervalMinutes: (int)numFetchInterval.Value,
            CleanupTime: timeCleanup.Value.ToString("HH:mm"),
            CleanupWeeks: new CleanupWeeks(
                KeepAllWeeks: (int)numKeepAll.Value,
                KeepDailyWeeks: (int)numKeepDaily.Value,
                DeleteAfterWeeks: (int)numDeleteAfter.Value
            ),
            CloseBehaviorPreference: closeBehaviorPreference
        );
    }

    private void SaveConfig()
    {
        var config = BuildConfigFromUI();
        ConfigService.Save(config);
    }

    private void InitializeTrayComponents()
    {
        components ??= new System.ComponentModel.Container();

        trayContextMenu = new ContextMenuStrip(components);
        var openMainMenuItem = new ToolStripMenuItem("打开主页面", null, (_, _) => RestoreFromTray());
        var settingsMenuItem = new ToolStripMenuItem("设置", null, (_, _) => OpenSettingsDialog());
        var exitMenuItem = new ToolStripMenuItem("退出", null, (_, _) => ExitFromTray());
        trayContextMenu.Items.AddRange(new ToolStripItem[]
        {
            openMainMenuItem,
            settingsMenuItem,
            new ToolStripSeparator(),
            exitMenuItem
        });

        trayIcon = new NotifyIcon(components)
        {
            ContextMenuStrip = trayContextMenu,
            Icon = Icon ?? SystemIcons.Application,
            Text = "Phoenix 测试辅助工具",
            Visible = false
        };
        trayIcon.DoubleClick += (_, _) => RestoreFromTray();
        trayIcon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                RestoreFromTray();
        };

        FormClosing += MainForm_FormClosing;
        FormClosed += (_, _) => trayIcon.Visible = false;
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        var closeAction = CloseBehaviorService.ResolveCloseAction(
            closeBehaviorPreference,
            isExplicitExit,
            e.CloseReason);

        if (closeAction == CloseBehaviorAction.PromptUser)
            closeAction = PromptForCloseAction();

        switch (closeAction)
        {
            case CloseBehaviorAction.MinimizeToTray:
                e.Cancel = true;
                MinimizeToTray();
                break;

            case CloseBehaviorAction.PromptUser:
                e.Cancel = true;
                break;

            default:
                trayIcon.Visible = false;
                break;
        }
    }

    private CloseBehaviorAction PromptForCloseAction()
    {
        using var dialog = new CloseChoiceDialog();
        return dialog.ShowDialog(this) == DialogResult.OK
            ? dialog.SelectedAction
            : CloseBehaviorAction.PromptUser;
    }

    private void MinimizeToTray()
    {
        Hide();
        ShowInTaskbar = false;
        trayIcon.Visible = true;

        if (hasShownTrayTip)
            return;

        hasShownTrayTip = true;
        try
        {
            trayIcon.ShowBalloonTip(
                2500,
                "Phoenix 测试辅助工具",
                "已最小化到系统托盘。右键托盘图标可打开菜单。",
                ToolTipIcon.Info);
        }
        catch (InvalidOperationException)
        {
            // Some Windows shell states reject balloon tips; the tray icon itself is still usable.
        }
    }

    private void RestoreFromTray()
    {
        trayIcon.Visible = false;
        ShowInTaskbar = true;
        if (WindowState == FormWindowState.Minimized)
            WindowState = FormWindowState.Normal;

        Show();
        Activate();
    }

    private void OpenSettingsDialog()
    {
        using var dialog = new ApplicationSettingsDialog();
        if (Visible)
        {
            dialog.ShowDialog(this);
        }
        else
        {
            dialog.StartPosition = FormStartPosition.CenterScreen;
            dialog.ShowDialog();
        }

        closeBehaviorPreference = ConfigService.Load().CloseBehaviorPreference;
    }

    private void ExitFromTray()
    {
        isExplicitExit = true;
        trayIcon.Visible = false;
        Close();
    }

    private void RefreshStatus()
    {
        if (RuntimeModeService.IsDevelopment)
        {
            lblStatus.Text = "状态: DEV 模式 - 可直接调试，计划任务安装请使用正式发布版";
            lblStatus.ForeColor = Color.DarkOrange;
            return;
        }

        var (fetchInstalled, cleanupInstalled) = SchedulerService.GetStatus();

        if (fetchInstalled && cleanupInstalled)
        {
            var config = BuildConfigFromUI();
            lblStatus.Text = $"状态: ● 已安装 - 每{config.FetchIntervalMinutes}分钟拉取 / 每天{config.CleanupTime}清理";
            lblStatus.ForeColor = Color.Green;
        }
        else if (fetchInstalled || cleanupInstalled)
        {
            var partial = fetchInstalled ? "拉取" : "清理";
            lblStatus.Text = $"状态: ◐ 部分安装 - 仅{partial}任务已注册";
            lblStatus.ForeColor = Color.Orange;
        }
        else
        {
            lblStatus.Text = "状态: ○ 未安装";
            lblStatus.ForeColor = Color.Gray;
        }
    }

    private void BtnBrowseSource_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择共享源目录",
            UseDescriptionForTitle = true
        };
        // Only pre-select if the path is a local directory that exists.
        // UNC paths cause FolderBrowserDialog to hang while enumerating the network.
        var current = txtSourceDir.Text.Trim();
        if (!current.StartsWith(@"\\") && Directory.Exists(current))
            dialog.SelectedPath = current;

        if (dialog.ShowDialog() == DialogResult.OK)
            txtSourceDir.Text = dialog.SelectedPath;
    }

    private void BtnBrowseLocal_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择本地保存目录",
            UseDescriptionForTitle = true
        };
        var current = txtLocalBaseDir.Text.Trim();
        if (Directory.Exists(current))
            dialog.SelectedPath = current;

        if (dialog.ShowDialog() == DialogResult.OK)
            txtLocalBaseDir.Text = dialog.SelectedPath;
    }

    private void BtnInstall_Click(object? sender, EventArgs e)
    {
        SaveConfig();
        var config = BuildConfigFromUI();
        var result = SchedulerService.Install(config);
        MessageBox.Show(result, "安装服务", MessageBoxButtons.OK, MessageBoxIcon.Information);
        RefreshStatus();
    }

    private void BtnUninstall_Click(object? sender, EventArgs e)
    {
        var result = SchedulerService.Uninstall();
        MessageBox.Show(result, "卸载服务", MessageBoxButtons.OK, MessageBoxIcon.Information);
        RefreshStatus();
    }

    private async void BtnFetchNow_Click(object? sender, EventArgs e)
    {
        SaveConfig();
        var config = BuildConfigFromUI();
        btnFetchNow.Enabled = false;
        btnFetchNow.Text = "拉取中...";

        try
        {
            var result = await Task.Run(() => FetchService.Execute(config));
            MessageBox.Show(result, "拉取结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"拉取失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnFetchNow.Enabled = true;
            btnFetchNow.Text = "立即拉取";
        }
    }

    private async void BtnCleanupNow_Click(object? sender, EventArgs e)
    {
        SaveConfig();
        var config = BuildConfigFromUI();
        btnCleanupNow.Enabled = false;
        btnCleanupNow.Text = "清理中...";

        try
        {
            var result = await Task.Run(() => CleanupService.Execute(config));
            using var dialog = new CleanupResultDialog(result);
            dialog.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"清理失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnCleanupNow.Enabled = true;
            btnCleanupNow.Text = "立即清理";
        }
    }

    private void OpenLogFolder(string subDir)
    {
        var path = ProductLogPathService.Resolve(subDir);

        if (Directory.Exists(path))
            Process.Start("explorer.exe", path);
        else
            MessageBox.Show($"日志目录不存在:\n{path}", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnLogDesignerJava_Click(object? sender, EventArgs e) =>
        OpenLogFolder(ProductLogPathService.DesignerJavaDirectory);

    private void BtnLogDesignerNode_Click(object? sender, EventArgs e) =>
        OpenLogFolder(ProductLogPathService.DesignerNodeDirectory);

    private void BtnLogRuntimeJava_Click(object? sender, EventArgs e) =>
        OpenLogFolder(ProductLogPathService.RuntimeJavaDirectory);

    private void BtnLogServerJava_Click(object? sender, EventArgs e) =>
        OpenLogFolder(ProductLogPathService.ServerJavaDirectory);

    private void BtnLogRoot_Click(object? sender, EventArgs e) =>
        OpenLogFolder("");

    private static DateTime ParseCleanupTime(string cleanupTime)
    {
        if (TimeOnly.TryParse(cleanupTime, out var parsedTime))
            return DateTime.Today.Add(parsedTime.ToTimeSpan());

        return DateTime.Today.AddHours(9).AddMinutes(30);
    }

    private static void ResetPathTextScroll(TextBox textBox)
    {
        textBox.SelectionStart = 0;
        textBox.SelectionLength = 0;
    }

    private void ApplyRuntimeModePresentation()
    {
        if (!RuntimeModeService.IsDevelopment)
            return;

        Text += RuntimeModeService.DisplaySuffix;
        lblSubtitle.Text += "（DEV 模式：配置与状态写入独立目录）";
    }
}
