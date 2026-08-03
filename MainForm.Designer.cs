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
        this.contentPanel = new Panel();
        this.rootLayout = new TableLayoutPanel();
        this.headerPanel = new Panel();
        this.lblTitle = new Label();
        this.lblSubtitle = new Label();
        this.pathsCard = CreateCardPanel();
        this.pathsCardLayout = CreateCardLayout();
        this.lblPathsTitle = CreateSectionTitle("基础配置");
        this.lblPathsDesc = CreateSectionDescription("设置共享安装包目录与本地缓存目录");
        this.pathsFieldsLayout = new TableLayoutPanel();
        this.lblSourceDir = CreateFieldLabel("共享源目录");
        this.txtSourceDir = new TextBox();
        this.btnBrowseSource = new Button();
        this.lblLocalDir = CreateFieldLabel("本地保存目录");
        this.txtLocalBaseDir = new TextBox();
        this.btnBrowseLocal = new Button();
        this.strategyLayout = new TableLayoutPanel();
        this.fetchCard = CreateCardPanel();
        this.fetchCardLayout = CreateCardLayout();
        this.lblFetchTitle = CreateSectionTitle("拉取策略");
        this.lblFetchDesc = CreateSectionDescription("按固定频率检查并拉取最新安装包");
        this.fetchInlinePanel = new FlowLayoutPanel();
        this.lblInterval = CreateFieldLabel("拉取间隔");
        this.numFetchInterval = new NumericUpDown();
        this.lblIntervalUnit = CreateInlineLabel("分钟");
        this.cleanupCard = CreateCardPanel();
        this.cleanupCardLayout = CreateCardLayout();
        this.lblCleanupTitle = CreateSectionTitle("清理策略");
        this.lblCleanupDesc = CreateSectionDescription("控制历史版本保留周期和清理时间");
        this.cleanupFieldsLayout = new TableLayoutPanel();
        this.lblKeepAll = CreateFieldLabel("保留全部");
        this.numKeepAll = new NumericUpDown();
        this.lblKeepAllUnit = CreateInlineLabel("周内");
        this.lblKeepDaily = CreateFieldLabel("每天保留一个");
        this.numKeepDaily = new NumericUpDown();
        this.lblKeepDailyUnit = CreateInlineLabel("周内");
        this.lblDeleteAfter = CreateFieldLabel("超过删除");
        this.numDeleteAfter = new NumericUpDown();
        this.lblDeleteAfterUnit = CreateInlineLabel("周");
        this.lblCleanupTime = CreateFieldLabel("清理时间");
        this.timeCleanup = new DateTimePicker();
        this.lblCleanupHint = CreateSectionDescription("24 小时制");
        this.logsCard = CreateCardPanel();
        this.logsCardLayout = CreateCardLayout();
        this.lblLogsTitle = CreateSectionTitle("产品日志");
        this.lblLogsDesc = CreateSectionDescription("快速打开常用日志目录");
        this.logsButtonsLayout = new TableLayoutPanel();
        this.btnLogDesignerJava = new Button();
        this.btnLogDesignerNode = new Button();
        this.btnLogRuntimeJava = new Button();
        this.btnLogServerJava = new Button();
        this.btnLogRoot = new Button();
        this.actionsLayout = new TableLayoutPanel();
        this.btnInstall = new Button();
        this.btnUninstall = new Button();
        this.btnFetchNow = new Button();
        this.btnCleanupNow = new Button();
        this.statusCard = CreateCardPanel();
        this.statusLayout = new TableLayoutPanel();
        this.lblStatus = new Label();
        ((System.ComponentModel.ISupportInitialize)this.numFetchInterval).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this.numKeepAll).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this.numKeepDaily).BeginInit();
        ((System.ComponentModel.ISupportInitialize)this.numDeleteAfter).BeginInit();
        this.contentPanel.SuspendLayout();
        this.rootLayout.SuspendLayout();
        this.headerPanel.SuspendLayout();
        this.pathsCard.SuspendLayout();
        this.pathsCardLayout.SuspendLayout();
        this.pathsFieldsLayout.SuspendLayout();
        this.strategyLayout.SuspendLayout();
        this.fetchCard.SuspendLayout();
        this.fetchCardLayout.SuspendLayout();
        this.fetchInlinePanel.SuspendLayout();
        this.cleanupCard.SuspendLayout();
        this.cleanupCardLayout.SuspendLayout();
        this.cleanupFieldsLayout.SuspendLayout();
        this.logsCard.SuspendLayout();
        this.logsCardLayout.SuspendLayout();
        this.logsButtonsLayout.SuspendLayout();
        this.actionsLayout.SuspendLayout();
        this.statusCard.SuspendLayout();
        this.statusLayout.SuspendLayout();
        this.SuspendLayout();
        //
        // contentPanel
        //
        this.contentPanel.AutoScroll = true;
        this.contentPanel.BackColor = SystemColors.Control;
        this.contentPanel.Controls.Add(this.rootLayout);
        this.contentPanel.Dock = DockStyle.Fill;
        this.contentPanel.Padding = new Padding(14, 12, 14, 12);
        //
        // rootLayout
        //
        this.rootLayout.AutoSize = true;
        this.rootLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this.rootLayout.ColumnCount = 1;
        this.rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        this.rootLayout.Controls.Add(this.headerPanel, 0, 0);
        this.rootLayout.Controls.Add(this.pathsCard, 0, 1);
        this.rootLayout.Controls.Add(this.strategyLayout, 0, 2);
        this.rootLayout.Controls.Add(this.logsCard, 0, 3);
        this.rootLayout.Controls.Add(this.actionsLayout, 0, 4);
        this.rootLayout.Controls.Add(this.statusCard, 0, 5);
        this.rootLayout.Dock = DockStyle.Top;
        this.rootLayout.Margin = new Padding(0);
        this.rootLayout.RowCount = 6;
        this.rootLayout.RowStyles.Add(new RowStyle());
        this.rootLayout.RowStyles.Add(new RowStyle());
        this.rootLayout.RowStyles.Add(new RowStyle());
        this.rootLayout.RowStyles.Add(new RowStyle());
        this.rootLayout.RowStyles.Add(new RowStyle());
        this.rootLayout.RowStyles.Add(new RowStyle());
        this.rootLayout.Size = new Size(1236, 650);
        //
        // headerPanel
        //
        this.headerPanel.Controls.Add(this.lblSubtitle);
        this.headerPanel.Controls.Add(this.lblTitle);
        this.headerPanel.Dock = DockStyle.Top;
        this.headerPanel.Margin = new Padding(0, 0, 0, 8);
        this.headerPanel.Size = new Size(1236, 58);
        //
        // lblTitle
        //
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new Font("Microsoft YaHei", 15F, FontStyle.Bold, GraphicsUnit.Point);
        this.lblTitle.Location = new Point(0, 0);
        this.lblTitle.Text = "Phoenix 测试辅助工具";
        //
        // lblSubtitle
        //
        this.lblSubtitle.AutoSize = true;
        this.lblSubtitle.Font = new Font("Microsoft YaHei", 9F, FontStyle.Regular, GraphicsUnit.Point);
        this.lblSubtitle.ForeColor = Color.FromArgb(96, 103, 112);
        this.lblSubtitle.Location = new Point(2, 35);
        this.lblSubtitle.Text = "自动拉取安装包、按策略清理历史版本，并支持从通知直接开始安装";
        //
        // pathsCard
        //
        this.pathsCard.Controls.Add(this.pathsCardLayout);
        this.pathsCard.Dock = DockStyle.Top;
        this.pathsCard.Margin = new Padding(0, 0, 0, 8);
        this.pathsCard.Size = new Size(1236, 136);
        //
        // pathsCardLayout
        //
        this.pathsCardLayout.Controls.Add(this.lblPathsTitle, 0, 0);
        this.pathsCardLayout.Controls.Add(this.lblPathsDesc, 0, 1);
        this.pathsCardLayout.Controls.Add(this.pathsFieldsLayout, 0, 2);
        this.pathsCardLayout.Dock = DockStyle.Fill;
        //
        // pathsFieldsLayout
        //
        this.pathsFieldsLayout.ColumnCount = 3;
        this.pathsFieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112F));
        this.pathsFieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        this.pathsFieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
        this.pathsFieldsLayout.Controls.Add(this.lblSourceDir, 0, 0);
        this.pathsFieldsLayout.Controls.Add(this.txtSourceDir, 1, 0);
        this.pathsFieldsLayout.Controls.Add(this.btnBrowseSource, 2, 0);
        this.pathsFieldsLayout.Controls.Add(this.lblLocalDir, 0, 1);
        this.pathsFieldsLayout.Controls.Add(this.txtLocalBaseDir, 1, 1);
        this.pathsFieldsLayout.Controls.Add(this.btnBrowseLocal, 2, 1);
        this.pathsFieldsLayout.Dock = DockStyle.Fill;
        this.pathsFieldsLayout.Location = new Point(0, 44);
        this.pathsFieldsLayout.Margin = new Padding(0, 8, 0, 0);
        this.pathsFieldsLayout.RowCount = 2;
        this.pathsFieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        this.pathsFieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        this.pathsFieldsLayout.Size = new Size(1212, 68);
        //
        // txtSourceDir
        //
        this.txtSourceDir.Dock = DockStyle.Fill;
        this.txtSourceDir.Margin = new Padding(0, 0, 6, 10);
        //
        // btnBrowseSource
        //
        ConfigureSecondaryButton(this.btnBrowseSource, "浏览");
        this.btnBrowseSource.Dock = DockStyle.Fill;
        this.btnBrowseSource.Margin = new Padding(0, 0, 0, 10);
        this.btnBrowseSource.Click += BtnBrowseSource_Click;
        //
        // txtLocalBaseDir
        //
        this.txtLocalBaseDir.Dock = DockStyle.Fill;
        this.txtLocalBaseDir.Margin = new Padding(0, 0, 6, 0);
        //
        // btnBrowseLocal
        //
        ConfigureSecondaryButton(this.btnBrowseLocal, "浏览");
        this.btnBrowseLocal.Dock = DockStyle.Fill;
        this.btnBrowseLocal.Margin = new Padding(0);
        this.btnBrowseLocal.Click += BtnBrowseLocal_Click;
        //
        // strategyLayout
        //
        this.strategyLayout.ColumnCount = 2;
        this.strategyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31F));
        this.strategyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 69F));
        this.strategyLayout.Controls.Add(this.fetchCard, 0, 0);
        this.strategyLayout.Controls.Add(this.cleanupCard, 1, 0);
        this.strategyLayout.Dock = DockStyle.Top;
        this.strategyLayout.Margin = new Padding(0, 0, 0, 8);
        this.strategyLayout.RowCount = 1;
        this.strategyLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        this.strategyLayout.Size = new Size(1236, 144);
        //
        // fetchCard
        //
        this.fetchCard.Controls.Add(this.fetchCardLayout);
        this.fetchCard.Dock = DockStyle.Fill;
        this.fetchCard.Margin = new Padding(0, 0, 4, 0);
        this.fetchCard.Size = new Size(377, 144);
        //
        // fetchCardLayout
        //
        this.fetchCardLayout.Controls.Add(this.lblFetchTitle, 0, 0);
        this.fetchCardLayout.Controls.Add(this.lblFetchDesc, 0, 1);
        this.fetchCardLayout.Controls.Add(this.fetchInlinePanel, 0, 2);
        this.fetchCardLayout.Dock = DockStyle.Fill;
        //
        // fetchInlinePanel
        //
        this.fetchInlinePanel.AutoSize = true;
        this.fetchInlinePanel.Controls.Add(this.lblInterval);
        this.fetchInlinePanel.Controls.Add(this.numFetchInterval);
        this.fetchInlinePanel.Controls.Add(this.lblIntervalUnit);
        this.fetchInlinePanel.Dock = DockStyle.Fill;
        this.fetchInlinePanel.FlowDirection = FlowDirection.LeftToRight;
        this.fetchInlinePanel.Location = new Point(0, 44);
        this.fetchInlinePanel.Margin = new Padding(0, 10, 0, 0);
        this.fetchInlinePanel.WrapContents = false;
        //
        // numFetchInterval
        //
        this.numFetchInterval.Margin = new Padding(0, 2, 8, 0);
        this.numFetchInterval.Maximum = new decimal(new int[] {
            1440,
            0,
            0,
            0});
        this.numFetchInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
        this.numFetchInterval.Size = new Size(88, 23);
        this.numFetchInterval.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
        //
        // cleanupCard
        //
        this.cleanupCard.Controls.Add(this.cleanupCardLayout);
        this.cleanupCard.Dock = DockStyle.Fill;
        this.cleanupCard.Margin = new Padding(4, 0, 0, 0);
        this.cleanupCard.Size = new Size(851, 144);
        //
        // cleanupCardLayout
        //
        this.cleanupCardLayout.Controls.Add(this.lblCleanupTitle, 0, 0);
        this.cleanupCardLayout.Controls.Add(this.lblCleanupDesc, 0, 1);
        this.cleanupCardLayout.Controls.Add(this.cleanupFieldsLayout, 0, 2);
        this.cleanupCardLayout.Dock = DockStyle.Fill;
        //
        // cleanupFieldsLayout
        //
        this.cleanupFieldsLayout.ColumnCount = 8;
        this.cleanupFieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
        this.cleanupFieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));
        this.cleanupFieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42F));
        this.cleanupFieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118F));
        this.cleanupFieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));
        this.cleanupFieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42F));
        this.cleanupFieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        this.cleanupFieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        this.cleanupFieldsLayout.Controls.Add(this.lblKeepAll, 0, 0);
        this.cleanupFieldsLayout.Controls.Add(this.numKeepAll, 1, 0);
        this.cleanupFieldsLayout.Controls.Add(this.lblKeepAllUnit, 2, 0);
        this.cleanupFieldsLayout.Controls.Add(this.lblKeepDaily, 3, 0);
        this.cleanupFieldsLayout.Controls.Add(this.numKeepDaily, 4, 0);
        this.cleanupFieldsLayout.Controls.Add(this.lblKeepDailyUnit, 5, 0);
        this.cleanupFieldsLayout.Controls.Add(this.lblDeleteAfter, 0, 1);
        this.cleanupFieldsLayout.Controls.Add(this.numDeleteAfter, 1, 1);
        this.cleanupFieldsLayout.Controls.Add(this.lblDeleteAfterUnit, 2, 1);
        this.cleanupFieldsLayout.Controls.Add(this.lblCleanupTime, 3, 1);
        this.cleanupFieldsLayout.Controls.Add(this.timeCleanup, 4, 1);
        this.cleanupFieldsLayout.Controls.Add(this.lblCleanupHint, 6, 1);
        this.cleanupFieldsLayout.Dock = DockStyle.Fill;
        this.cleanupFieldsLayout.Location = new Point(0, 44);
        this.cleanupFieldsLayout.Margin = new Padding(0, 10, 0, 0);
        this.cleanupFieldsLayout.RowCount = 2;
        this.cleanupFieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        this.cleanupFieldsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        this.cleanupFieldsLayout.Size = new Size(827, 76);
        //
        // numKeepAll
        //
        this.numKeepAll.Margin = new Padding(0, 2, 8, 0);
        this.numKeepAll.Maximum = new decimal(new int[] {
            52,
            0,
            0,
            0});
        this.numKeepAll.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
        this.numKeepAll.Size = new Size(78, 23);
        this.numKeepAll.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
        //
        // numKeepDaily
        //
        this.numKeepDaily.Margin = new Padding(0, 2, 8, 0);
        this.numKeepDaily.Maximum = new decimal(new int[] {
            52,
            0,
            0,
            0});
        this.numKeepDaily.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
        this.numKeepDaily.Size = new Size(78, 23);
        this.numKeepDaily.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
        //
        // numDeleteAfter
        //
        this.numDeleteAfter.Margin = new Padding(0, 2, 8, 0);
        this.numDeleteAfter.Maximum = new decimal(new int[] {
            52,
            0,
            0,
            0});
        this.numDeleteAfter.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
        this.numDeleteAfter.Size = new Size(78, 23);
        this.numDeleteAfter.Value = new decimal(new int[] {
            9,
            0,
            0,
            0});
        //
        // timeCleanup
        //
        this.timeCleanup.CustomFormat = "HH:mm";
        this.timeCleanup.Format = DateTimePickerFormat.Custom;
        this.timeCleanup.Margin = new Padding(0, 2, 8, 0);
        this.timeCleanup.ShowUpDown = true;
        this.timeCleanup.Size = new Size(86, 23);
        //
        // logsCard
        //
        this.logsCard.Controls.Add(this.logsCardLayout);
        this.logsCard.Dock = DockStyle.Top;
        this.logsCard.Margin = new Padding(0, 0, 0, 8);
        this.logsCard.Size = new Size(1236, 126);
        //
        // logsCardLayout
        //
        this.logsCardLayout.Controls.Add(this.lblLogsTitle, 0, 0);
        this.logsCardLayout.Controls.Add(this.lblLogsDesc, 0, 1);
        this.logsCardLayout.Controls.Add(this.logsButtonsLayout, 0, 2);
        this.logsCardLayout.Dock = DockStyle.Fill;
        //
        // logsButtonsLayout
        //
        this.logsButtonsLayout.ColumnCount = 5;
        this.logsButtonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        this.logsButtonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        this.logsButtonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        this.logsButtonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        this.logsButtonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        this.logsButtonsLayout.Controls.Add(this.btnLogDesignerJava, 0, 0);
        this.logsButtonsLayout.Controls.Add(this.btnLogDesignerNode, 1, 0);
        this.logsButtonsLayout.Controls.Add(this.btnLogRuntimeJava, 2, 0);
        this.logsButtonsLayout.Controls.Add(this.btnLogServerJava, 3, 0);
        this.logsButtonsLayout.Controls.Add(this.btnLogRoot, 4, 0);
        this.logsButtonsLayout.Dock = DockStyle.Top;
        this.logsButtonsLayout.Location = new Point(0, 44);
        this.logsButtonsLayout.Margin = new Padding(0, 10, 0, 0);
        this.logsButtonsLayout.RowCount = 1;
        this.logsButtonsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        this.logsButtonsLayout.Size = new Size(1212, 40);
        //
        // log buttons
        //
        ConfigureSecondaryButton(this.btnLogDesignerJava, "Designer Java");
        ConfigureSecondaryButton(this.btnLogDesignerNode, "Designer Node");
        ConfigureSecondaryButton(this.btnLogRuntimeJava, "Runtime Java");
        ConfigureSecondaryButton(this.btnLogServerJava, "Server Java");
        ConfigureSecondaryButton(this.btnLogRoot, "日志根目录");
        this.btnLogDesignerJava.Dock = DockStyle.Fill;
        this.btnLogDesignerNode.Dock = DockStyle.Fill;
        this.btnLogRuntimeJava.Dock = DockStyle.Fill;
        this.btnLogServerJava.Dock = DockStyle.Fill;
        this.btnLogRoot.Dock = DockStyle.Fill;
        this.btnLogDesignerJava.Margin = new Padding(0, 0, 6, 0);
        this.btnLogDesignerNode.Margin = new Padding(0, 0, 6, 0);
        this.btnLogRuntimeJava.Margin = new Padding(0, 0, 6, 0);
        this.btnLogServerJava.Margin = new Padding(0, 0, 6, 0);
        this.btnLogRoot.Margin = new Padding(0);
        this.btnLogDesignerJava.Click += BtnLogDesignerJava_Click;
        this.btnLogDesignerNode.Click += BtnLogDesignerNode_Click;
        this.btnLogRuntimeJava.Click += BtnLogRuntimeJava_Click;
        this.btnLogServerJava.Click += BtnLogServerJava_Click;
        this.btnLogRoot.Click += BtnLogRoot_Click;
        //
        // actionsLayout
        //
        this.actionsLayout.ColumnCount = 4;
        this.actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        this.actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        this.actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        this.actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        this.actionsLayout.Controls.Add(this.btnInstall, 0, 0);
        this.actionsLayout.Controls.Add(this.btnUninstall, 1, 0);
        this.actionsLayout.Controls.Add(this.btnFetchNow, 2, 0);
        this.actionsLayout.Controls.Add(this.btnCleanupNow, 3, 0);
        this.actionsLayout.Dock = DockStyle.Top;
        this.actionsLayout.Margin = new Padding(0, 0, 0, 8);
        this.actionsLayout.RowCount = 1;
        this.actionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        this.actionsLayout.Size = new Size(1236, 38);
        //
        // action buttons
        //
        ConfigurePrimaryButton(this.btnInstall, "安装服务");
        ConfigureSecondaryButton(this.btnUninstall, "卸载服务");
        ConfigureSecondaryButton(this.btnFetchNow, "立即拉取");
        ConfigureSecondaryButton(this.btnCleanupNow, "立即清理");
        this.btnInstall.Dock = DockStyle.Fill;
        this.btnUninstall.Dock = DockStyle.Fill;
        this.btnFetchNow.Dock = DockStyle.Fill;
        this.btnCleanupNow.Dock = DockStyle.Fill;
        this.btnInstall.Margin = new Padding(0, 0, 6, 0);
        this.btnUninstall.Margin = new Padding(0, 0, 6, 0);
        this.btnFetchNow.Margin = new Padding(0, 0, 6, 0);
        this.btnCleanupNow.Margin = new Padding(0);
        this.btnInstall.Click += BtnInstall_Click;
        this.btnUninstall.Click += BtnUninstall_Click;
        this.btnFetchNow.Click += BtnFetchNow_Click;
        this.btnCleanupNow.Click += BtnCleanupNow_Click;
        //
        // statusCard
        //
        this.statusCard.Controls.Add(this.statusLayout);
        this.statusCard.Dock = DockStyle.Top;
        this.statusCard.Margin = new Padding(0);
        this.statusCard.Size = new Size(1236, 52);
        //
        // statusLayout
        //
        this.statusLayout.ColumnCount = 1;
        this.statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        this.statusLayout.Controls.Add(this.lblStatus, 0, 0);
        this.statusLayout.Dock = DockStyle.Fill;
        this.statusLayout.Padding = new Padding(12, 12, 12, 8);
        this.statusLayout.RowCount = 1;
        this.statusLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        //
        // lblStatus
        //
        this.lblStatus.AutoSize = true;
        this.lblStatus.Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold, GraphicsUnit.Point);
        this.lblStatus.ForeColor = Color.FromArgb(39, 98, 54);
        this.lblStatus.Text = "状态: 检查中...";
        //
        // MainForm
        //
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.None;
        this.BackColor = SystemColors.Control;
        this.ClientSize = new Size(1280, 720);
        this.Controls.Add(this.contentPanel);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.Font = new Font("Microsoft YaHei", 9F, FontStyle.Regular, GraphicsUnit.Point);
        this.MaximizeBox = false;
        this.MinimizeBox = true;
        this.MinimumSize = new Size(1280, 720);
        this.MaximumSize = new Size(1280, 720);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "Phoenix 测试辅助工具";
        ((System.ComponentModel.ISupportInitialize)this.numFetchInterval).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numKeepAll).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numKeepDaily).EndInit();
        ((System.ComponentModel.ISupportInitialize)this.numDeleteAfter).EndInit();
        this.contentPanel.ResumeLayout(false);
        this.contentPanel.PerformLayout();
        this.rootLayout.ResumeLayout(false);
        this.headerPanel.ResumeLayout(false);
        this.headerPanel.PerformLayout();
        this.pathsCard.ResumeLayout(false);
        this.pathsCardLayout.ResumeLayout(false);
        this.pathsCardLayout.PerformLayout();
        this.pathsFieldsLayout.ResumeLayout(false);
        this.pathsFieldsLayout.PerformLayout();
        this.strategyLayout.ResumeLayout(false);
        this.fetchCard.ResumeLayout(false);
        this.fetchCardLayout.ResumeLayout(false);
        this.fetchCardLayout.PerformLayout();
        this.fetchInlinePanel.ResumeLayout(false);
        this.fetchInlinePanel.PerformLayout();
        this.cleanupCard.ResumeLayout(false);
        this.cleanupCardLayout.ResumeLayout(false);
        this.cleanupCardLayout.PerformLayout();
        this.cleanupFieldsLayout.ResumeLayout(false);
        this.cleanupFieldsLayout.PerformLayout();
        this.logsCard.ResumeLayout(false);
        this.logsCardLayout.ResumeLayout(false);
        this.logsCardLayout.PerformLayout();
        this.logsButtonsLayout.ResumeLayout(false);
        this.actionsLayout.ResumeLayout(false);
        this.statusCard.ResumeLayout(false);
        this.statusLayout.ResumeLayout(false);
        this.statusLayout.PerformLayout();
        this.ResumeLayout(false);
    }

    private static Panel CreateCardPanel()
    {
        return new Panel
        {
            BackColor = SystemColors.Window,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private static TableLayoutPanel CreateCardLayout()
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 10, 12, 10),
            RowCount = 3
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        return layout;
    }

    private static Label CreateSectionTitle(string text)
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 9.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = SystemColors.ControlText,
            Margin = new Padding(0),
            Text = text
        };
    }

    private static Label CreateSectionDescription(string text)
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 8.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = Color.FromArgb(96, 96, 96),
            Margin = new Padding(0, 3, 0, 0),
            Text = text
        };
    }

    private static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 9F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = SystemColors.ControlText,
            Margin = new Padding(0, 4, 8, 0),
            Text = text
        };
    }

    private static Label CreateInlineLabel(string text)
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 9F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = SystemColors.ControlText,
            Margin = new Padding(0, 4, 8, 0),
            Text = text
        };
    }

    private static void ConfigurePrimaryButton(Button button, string text)
    {
        button.FlatStyle = FlatStyle.Standard;
        button.Font = new Font("Microsoft YaHei", 9F, FontStyle.Bold, GraphicsUnit.Point);
        button.Height = 32;
        button.Text = text;
        button.UseVisualStyleBackColor = true;
    }

    private static void ConfigureSecondaryButton(Button button, string text)
    {
        button.FlatStyle = FlatStyle.Standard;
        button.Font = new Font("Microsoft YaHei", 9F, FontStyle.Regular, GraphicsUnit.Point);
        button.Height = 32;
        button.Text = text;
        button.UseVisualStyleBackColor = true;
    }

    private Panel contentPanel;
    private TableLayoutPanel rootLayout;
    private Panel headerPanel;
    private Label lblTitle;
    private Label lblSubtitle;
    private Panel pathsCard;
    private TableLayoutPanel pathsCardLayout;
    private Label lblPathsTitle;
    private Label lblPathsDesc;
    private TableLayoutPanel pathsFieldsLayout;
    private Label lblSourceDir;
    private TextBox txtSourceDir;
    private Button btnBrowseSource;
    private Label lblLocalDir;
    private TextBox txtLocalBaseDir;
    private Button btnBrowseLocal;
    private TableLayoutPanel strategyLayout;
    private Panel fetchCard;
    private TableLayoutPanel fetchCardLayout;
    private Label lblFetchTitle;
    private Label lblFetchDesc;
    private FlowLayoutPanel fetchInlinePanel;
    private Label lblInterval;
    private NumericUpDown numFetchInterval;
    private Label lblIntervalUnit;
    private Panel cleanupCard;
    private TableLayoutPanel cleanupCardLayout;
    private Label lblCleanupTitle;
    private Label lblCleanupDesc;
    private TableLayoutPanel cleanupFieldsLayout;
    private Label lblKeepAll;
    private NumericUpDown numKeepAll;
    private Label lblKeepAllUnit;
    private Label lblKeepDaily;
    private NumericUpDown numKeepDaily;
    private Label lblKeepDailyUnit;
    private Label lblDeleteAfter;
    private NumericUpDown numDeleteAfter;
    private Label lblDeleteAfterUnit;
    private Label lblCleanupTime;
    private DateTimePicker timeCleanup;
    private Label lblCleanupHint;
    private Panel logsCard;
    private TableLayoutPanel logsCardLayout;
    private Label lblLogsTitle;
    private Label lblLogsDesc;
    private TableLayoutPanel logsButtonsLayout;
    private Button btnLogDesignerJava;
    private Button btnLogDesignerNode;
    private Button btnLogRuntimeJava;
    private Button btnLogServerJava;
    private Button btnLogRoot;
    private TableLayoutPanel actionsLayout;
    private Button btnInstall;
    private Button btnUninstall;
    private Button btnFetchNow;
    private Button btnCleanupNow;
    private Panel statusCard;
    private TableLayoutPanel statusLayout;
    private Label lblStatus;
}
