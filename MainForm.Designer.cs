namespace PhoenixToolkit;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.txtSourceDir = new TextBox();
        this.txtLocalBaseDir = new TextBox();
        this.btnBrowseSource = new Button();
        this.btnBrowseLocal = new Button();
        this.numFetchInterval = new NumericUpDown();
        this.numKeepAll = new NumericUpDown();
        this.numKeepDaily = new NumericUpDown();
        this.numDeleteAfter = new NumericUpDown();
        this.txtCleanupTime = new TextBox();
        this.btnInstall = new Button();
        this.btnUninstall = new Button();
        this.btnFetchNow = new Button();
        this.btnCleanupNow = new Button();
        this.lblStatus = new Label();
        this.grpFetch = new GroupBox();
        this.grpCleanup = new GroupBox();
        this.lblSourceDir = new Label();
        this.lblLocalDir = new Label();
        this.lblInterval = new Label();
        this.lblIntervalUnit = new Label();
        this.lblKeepAll = new Label();
        this.lblKeepAllUnit = new Label();
        this.lblKeepDaily = new Label();
        this.lblKeepDailyUnit = new Label();
        this.lblDeleteAfter = new Label();
        this.lblDeleteAfterUnit = new Label();
        this.lblCleanupTime = new Label();

        ((System.ComponentModel.ISupportInitialize)this.numFetchInterval).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this.numKeepAll).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this.numKeepDaily).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this.numDeleteAfter).BeginInit();
        this.grpFetch.SuspendLayout();
        this.grpCleanup.SuspendLayout();
        this.SuspendLayout();

        // grpLogs - 产品日志
        this.grpLogs = new GroupBox();
        this.btnLogDesigner = new Button();
        this.btnLogDesignerServer = new Button();
        this.btnLogRuntimeServer = new Button();
        this.btnLogServerLocal = new Button();
        this.btnLogRoot = new Button();
        this.grpLogs.SuspendLayout();

        // lblSourceDir
        this.lblSourceDir.AutoSize = true;
        this.lblSourceDir.Location = new Point(20, 25);
        this.lblSourceDir.Text = "共享源目录:";

        // txtSourceDir
        this.txtSourceDir.Location = new Point(110, 22);
        this.txtSourceDir.Size = new Size(370, 23);

        // btnBrowseSource
        this.btnBrowseSource.Location = new Point(486, 21);
        this.btnBrowseSource.Size = new Size(55, 25);
        this.btnBrowseSource.Text = "浏览";
        this.btnBrowseSource.Click += BtnBrowseSource_Click;

        // lblLocalDir
        this.lblLocalDir.AutoSize = true;
        this.lblLocalDir.Location = new Point(20, 58);
        this.lblLocalDir.Text = "本地保存目录:";

        // txtLocalBaseDir
        this.txtLocalBaseDir.Location = new Point(110, 55);
        this.txtLocalBaseDir.Size = new Size(370, 23);

        // btnBrowseLocal
        this.btnBrowseLocal.Location = new Point(486, 54);
        this.btnBrowseLocal.Size = new Size(55, 25);
        this.btnBrowseLocal.Text = "浏览";
        this.btnBrowseLocal.Click += BtnBrowseLocal_Click;

        // grpFetch - 拉取策略
        this.grpFetch.Location = new Point(20, 92);
        this.grpFetch.Size = new Size(520, 55);
        this.grpFetch.Text = "拉取策略";

        // lblInterval
        this.lblInterval.AutoSize = true;
        this.lblInterval.Location = new Point(15, 22);
        this.lblInterval.Text = "拉取间隔:";

        // numFetchInterval
        this.numFetchInterval.Location = new Point(85, 20);
        this.numFetchInterval.Size = new Size(60, 23);
        this.numFetchInterval.Minimum = 1;
        this.numFetchInterval.Maximum = 1440;
        this.numFetchInterval.Value = 5;

        // lblIntervalUnit
        this.lblIntervalUnit.AutoSize = true;
        this.lblIntervalUnit.Location = new Point(150, 22);
        this.lblIntervalUnit.Text = "分钟";

        this.grpFetch.Controls.AddRange(new Control[] {
            this.lblInterval, this.numFetchInterval, this.lblIntervalUnit
        });

        // grpCleanup - 清理策略
        this.grpCleanup.Location = new Point(20, 155);
        this.grpCleanup.Size = new Size(520, 125);
        this.grpCleanup.Text = "清理策略";

        // lblKeepAll
        this.lblKeepAll.AutoSize = true;
        this.lblKeepAll.Location = new Point(15, 25);
        this.lblKeepAll.Text = "保留全部:";

        this.numKeepAll.Location = new Point(85, 23);
        this.numKeepAll.Size = new Size(60, 23);
        this.numKeepAll.Minimum = 1;
        this.numKeepAll.Maximum = 52;
        this.numKeepAll.Value = 3;

        this.lblKeepAllUnit.AutoSize = true;
        this.lblKeepAllUnit.Location = new Point(150, 25);
        this.lblKeepAllUnit.Text = "周内";

        // lblKeepDaily
        this.lblKeepDaily.AutoSize = true;
        this.lblKeepDaily.Location = new Point(210, 25);
        this.lblKeepDaily.Text = "每天保留一个:";

        this.numKeepDaily.Location = new Point(300, 23);
        this.numKeepDaily.Size = new Size(60, 23);
        this.numKeepDaily.Minimum = 1;
        this.numKeepDaily.Maximum = 52;
        this.numKeepDaily.Value = 6;

        this.lblKeepDailyUnit.AutoSize = true;
        this.lblKeepDailyUnit.Location = new Point(365, 25);
        this.lblKeepDailyUnit.Text = "周内";

        // lblDeleteAfter
        this.lblDeleteAfter.AutoSize = true;
        this.lblDeleteAfter.Location = new Point(15, 58);
        this.lblDeleteAfter.Text = "超过删除:";

        this.numDeleteAfter.Location = new Point(85, 56);
        this.numDeleteAfter.Size = new Size(60, 23);
        this.numDeleteAfter.Minimum = 1;
        this.numDeleteAfter.Maximum = 52;
        this.numDeleteAfter.Value = 9;

        this.lblDeleteAfterUnit.AutoSize = true;
        this.lblDeleteAfterUnit.Location = new Point(150, 58);
        this.lblDeleteAfterUnit.Text = "周";

        // lblCleanupTime
        this.lblCleanupTime.AutoSize = true;
        this.lblCleanupTime.Location = new Point(210, 58);
        this.lblCleanupTime.Text = "清理时间:";

        this.txtCleanupTime.Location = new Point(300, 56);
        this.txtCleanupTime.Size = new Size(60, 23);
        this.txtCleanupTime.Text = "09:30";

        this.grpCleanup.Controls.AddRange(new Control[] {
            this.lblKeepAll, this.numKeepAll, this.lblKeepAllUnit,
            this.lblKeepDaily, this.numKeepDaily, this.lblKeepDailyUnit,
            this.lblDeleteAfter, this.numDeleteAfter, this.lblDeleteAfterUnit,
            this.lblCleanupTime, this.txtCleanupTime
        });

        // Buttons
        this.btnInstall.Location = new Point(20, 370);
        this.btnInstall.Size = new Size(100, 32);
        this.btnInstall.Text = "安装服务";
        this.btnInstall.Click += BtnInstall_Click;

        this.btnUninstall.Location = new Point(130, 370);
        this.btnUninstall.Size = new Size(100, 32);
        this.btnUninstall.Text = "卸载服务";
        this.btnUninstall.Click += BtnUninstall_Click;

        this.btnFetchNow.Location = new Point(280, 370);
        this.btnFetchNow.Size = new Size(100, 32);
        this.btnFetchNow.Text = "立即拉取";
        this.btnFetchNow.Click += BtnFetchNow_Click;

        this.btnCleanupNow.Location = new Point(390, 370);
        this.btnCleanupNow.Size = new Size(100, 32);
        this.btnCleanupNow.Text = "立即清理";
        this.btnCleanupNow.Click += BtnCleanupNow_Click;

        // lblStatus
        this.lblStatus.Location = new Point(20, 415);
        this.lblStatus.Size = new Size(520, 20);
        this.lblStatus.Text = "状态: 检查中...";

        // grpLogs
        this.grpLogs.Location = new Point(20, 290);
        this.grpLogs.Size = new Size(520, 65);
        this.grpLogs.Text = "产品日志";

        this.btnLogDesigner.Location = new Point(15, 25);
        this.btnLogDesigner.Size = new Size(90, 28);
        this.btnLogDesigner.Text = "Designer";
        this.btnLogDesigner.Click += BtnLogDesigner_Click;

        this.btnLogDesignerServer.Location = new Point(112, 25);
        this.btnLogDesignerServer.Size = new Size(100, 28);
        this.btnLogDesignerServer.Text = "Designer后端";
        this.btnLogDesignerServer.Click += BtnLogDesignerServer_Click;

        this.btnLogRuntimeServer.Location = new Point(219, 25);
        this.btnLogRuntimeServer.Size = new Size(90, 28);
        this.btnLogRuntimeServer.Text = "Runtime";
        this.btnLogRuntimeServer.Click += BtnLogRuntimeServer_Click;

        this.btnLogServerLocal.Location = new Point(316, 25);
        this.btnLogServerLocal.Size = new Size(90, 28);
        this.btnLogServerLocal.Text = "Server";
        this.btnLogServerLocal.Click += BtnLogServerLocal_Click;

        this.btnLogRoot.Location = new Point(413, 25);
        this.btnLogRoot.Size = new Size(90, 28);
        this.btnLogRoot.Text = "日志根目录";
        this.btnLogRoot.Click += BtnLogRoot_Click;

        this.grpLogs.Controls.AddRange(new Control[] {
            this.btnLogDesigner, this.btnLogDesignerServer,
            this.btnLogRuntimeServer, this.btnLogServerLocal, this.btnLogRoot
        });

        // MainForm
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(560, 450);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "Phoenix 测试辅助工具";

        this.Controls.AddRange(new Control[] {
            this.lblSourceDir, this.txtSourceDir, this.btnBrowseSource,
            this.lblLocalDir, this.txtLocalBaseDir, this.btnBrowseLocal,
            this.grpFetch, this.grpCleanup, this.grpLogs,
            this.btnInstall, this.btnUninstall, this.btnFetchNow, this.btnCleanupNow,
            this.lblStatus
        });

        ((System.ComponentModel.ISupportInitialize)this.numFetchInterval).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numKeepAll).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numKeepDaily).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numDeleteAfter).EndInit();
        this.grpFetch.ResumeLayout(false);
        this.grpFetch.PerformLayout();
        this.grpCleanup.ResumeLayout(false);
        this.grpCleanup.PerformLayout();
        this.grpLogs.ResumeLayout(false);
        this.grpLogs.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private TextBox txtSourceDir;
    private TextBox txtLocalBaseDir;
    private Button btnBrowseSource;
    private Button btnBrowseLocal;
    private NumericUpDown numFetchInterval;
    private NumericUpDown numKeepAll;
    private NumericUpDown numKeepDaily;
    private NumericUpDown numDeleteAfter;
    private TextBox txtCleanupTime;
    private Button btnInstall;
    private Button btnUninstall;
    private Button btnFetchNow;
    private Button btnCleanupNow;
    private Label lblStatus;
    private GroupBox grpFetch;
    private GroupBox grpCleanup;
    private Label lblSourceDir;
    private Label lblLocalDir;
    private Label lblInterval;
    private Label lblIntervalUnit;
    private Label lblKeepAll;
    private Label lblKeepAllUnit;
    private Label lblKeepDaily;
    private Label lblKeepDailyUnit;
    private Label lblDeleteAfter;
    private Label lblDeleteAfterUnit;
    private Label lblCleanupTime;
    private GroupBox grpLogs;
    private Button btnLogDesigner;
    private Button btnLogDesignerServer;
    private Button btnLogRuntimeServer;
    private Button btnLogServerLocal;
    private Button btnLogRoot;
}
