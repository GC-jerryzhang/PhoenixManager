namespace PhoenixToolkit;

public sealed class CleanupResultDialog : Form
{
    private const int DialogWidth = 760;
    private const int DialogHeight = 520;

    public CleanupResultDialog(string resultText)
    {
        var contentTextBox = new TextBox
        {
            Name = "cleanupResultTextBox",
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
            Text = resultText
        };

        var confirmButton = new Button
        {
            Name = "cleanupResultConfirmButton",
            AutoSize = true,
            DialogResult = DialogResult.OK,
            MinimumSize = new Size(96, 32),
            Text = "确定",
            UseVisualStyleBackColor = true
        };

        var actionsPanel = new FlowLayoutPanel
        {
            Name = "cleanupResultActionsPanel",
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0)
        };
        actionsPanel.Controls.Add(confirmButton);

        var rootLayout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            RowCount = 3
        };
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle());
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle());
        rootLayout.Controls.Add(new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft YaHei", 9F, FontStyle.Regular, GraphicsUnit.Point),
            Margin = new Padding(0, 0, 0, 8),
            Text = "清理已完成，可滚动查看完整结果。"
        }, 0, 0);
        rootLayout.Controls.Add(contentTextBox, 0, 1);
        rootLayout.Controls.Add(actionsPanel, 0, 2);

        AcceptButton = confirmButton;
        CancelButton = confirmButton;
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = new Size(DialogWidth, DialogHeight);
        Controls.Add(rootLayout);
        Font = new Font("Microsoft YaHei", 9F, FontStyle.Regular, GraphicsUnit.Point);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MaximumSize = Size;
        MinimumSize = Size;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "清理结果";
    }
}
