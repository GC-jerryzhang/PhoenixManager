using System.Diagnostics;
using PhoenixToolkit.Models;
using PhoenixToolkit.Services;

namespace PhoenixToolkit;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        LoadConfig();
        RefreshStatus();
    }

    private void LoadConfig()
    {
        var config = ConfigService.Load();
        txtSourceDir.Text = config.SourceDir;
        txtLocalBaseDir.Text = config.LocalBaseDir;
        numFetchInterval.Value = config.FetchIntervalMinutes;
        numKeepAll.Value = config.CleanupWeeks.KeepAllWeeks;
        numKeepDaily.Value = config.CleanupWeeks.KeepDailyWeeks;
        numDeleteAfter.Value = config.CleanupWeeks.DeleteAfterWeeks;
        txtCleanupTime.Text = config.CleanupTime;
    }

    private AppConfig BuildConfigFromUI()
    {
        return new AppConfig(
            SourceDir: txtSourceDir.Text.Trim(),
            LocalBaseDir: txtLocalBaseDir.Text.Trim(),
            FetchIntervalMinutes: (int)numFetchInterval.Value,
            CleanupTime: txtCleanupTime.Text.Trim(),
            CleanupWeeks: new CleanupWeeks(
                KeepAllWeeks: (int)numKeepAll.Value,
                KeepDailyWeeks: (int)numKeepDaily.Value,
                DeleteAfterWeeks: (int)numDeleteAfter.Value
            )
        );
    }

    private void SaveConfig()
    {
        var config = BuildConfigFromUI();
        ConfigService.Save(config);
    }

    private void RefreshStatus()
    {
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
            MessageBox.Show(result, "清理结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        var path = string.IsNullOrEmpty(subDir)
            ? Path.Combine(Path.GetTempPath(), "phoenix-logs")
            : Path.Combine(Path.GetTempPath(), "phoenix-logs", subDir);

        if (Directory.Exists(path))
            Process.Start("explorer.exe", path);
        else
            MessageBox.Show($"日志目录不存在:\n{path}", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnLogDesigner_Click(object? sender, EventArgs e) =>
        OpenLogFolder("designer");

    private void BtnLogDesignerServer_Click(object? sender, EventArgs e) =>
        OpenLogFolder("designer-server");

    private void BtnLogRuntimeServer_Click(object? sender, EventArgs e) =>
        OpenLogFolder("runtime-server");

    private void BtnLogServerLocal_Click(object? sender, EventArgs e) =>
        OpenLogFolder("server-local");

    private void BtnLogRoot_Click(object? sender, EventArgs e) =>
        OpenLogFolder("");
}
