using PhoenixToolkit.Services;

namespace PhoenixToolkit;

public sealed class CloseChoiceDialog : Form
{
    private CloseBehaviorAction selectedAction = CloseBehaviorAction.PromptUser;

    public CloseChoiceDialog()
    {
        InitializeComponent();
    }

    public CloseBehaviorAction SelectedAction => selectedAction;

    private void InitializeComponent()
    {
        var rootLayout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            RowCount = 3
        };
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle());
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

        var titleLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 10F, FontStyle.Bold, GraphicsUnit.Point),
            Text = "关闭 Phoenix 测试辅助工具？"
        };

        var messageLabel = new Label
        {
            AutoSize = true,
            ForeColor = Color.FromArgb(80, 80, 80),
            Margin = new Padding(0, 8, 0, 0),
            Text = "请选择彻底退出，或将软件保留在系统托盘中。"
        };

        var actionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 12, 0, 0),
            WrapContents = false
        };

        var exitButton = new Button
        {
            Text = "彻底退出",
            Width = 108
        };
        exitButton.Click += (_, _) =>
        {
            selectedAction = CloseBehaviorAction.ExitApplication;
            DialogResult = DialogResult.OK;
            Close();
        };

        var trayButton = new Button
        {
            Text = "最小化到系统托盘",
            Width = 148
        };
        trayButton.Click += (_, _) =>
        {
            selectedAction = CloseBehaviorAction.MinimizeToTray;
            DialogResult = DialogResult.OK;
            Close();
        };

        actionsPanel.Controls.Add(exitButton);
        actionsPanel.Controls.Add(trayButton);

        rootLayout.Controls.Add(titleLabel, 0, 0);
        rootLayout.Controls.Add(messageLabel, 0, 1);
        rootLayout.Controls.Add(actionsPanel, 0, 2);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(420, 148);
        Controls.Add(rootLayout);
        Font = new Font("Microsoft YaHei", 9F, FontStyle.Regular, GraphicsUnit.Point);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(420, 148);
        MaximumSize = new Size(420, 148);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "关闭确认";
    }
}
