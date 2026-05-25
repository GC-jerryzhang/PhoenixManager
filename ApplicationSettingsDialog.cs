using PhoenixToolkit.Models;
using PhoenixToolkit.Services;

namespace PhoenixToolkit;

public sealed class ApplicationSettingsDialog : Form
{
    private readonly ComboBox closeBehaviorPreferenceComboBox;
    private readonly Button settingsSaveButton;
    private readonly Button settingsCancelButton;

    public ApplicationSettingsDialog()
    {
        closeBehaviorPreferenceComboBox = new ComboBox
        {
            Name = "closeBehaviorPreferenceComboBox",
            DropDownStyle = ComboBoxStyle.DropDownList,
            FormattingEnabled = true,
            Dock = DockStyle.Fill
        };
        closeBehaviorPreferenceComboBox.Items.AddRange(new object[]
        {
            CloseBehaviorPreference.AskEveryTime,
            CloseBehaviorPreference.ExitApplication,
            CloseBehaviorPreference.MinimizeToTray
        });
        closeBehaviorPreferenceComboBox.Format += CloseBehaviorPreferenceComboBox_Format;

        settingsSaveButton = new Button
        {
            Name = "settingsSaveButton",
            Text = "保存",
            DialogResult = DialogResult.None,
            Width = 88
        };
        settingsSaveButton.Click += SettingsSaveButton_Click;

        settingsCancelButton = new Button
        {
            Name = "settingsCancelButton",
            Text = "取消",
            DialogResult = DialogResult.Cancel,
            Width = 88
        };

        InitializeComponent();
        LoadCurrentConfig();
    }

    public CloseBehaviorPreference SelectedCloseBehaviorPreference =>
        closeBehaviorPreferenceComboBox.SelectedItem is CloseBehaviorPreference preference
            ? preference
            : CloseBehaviorPreference.AskEveryTime;

    public void SaveSelectedCloseBehaviorPreference()
    {
        var config = ConfigService.Load();
        ConfigService.Save(config with
        {
            CloseBehaviorPreference = SelectedCloseBehaviorPreference
        });

        DialogResult = DialogResult.OK;
    }

    private void InitializeComponent()
    {
        var rootLayout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            RowCount = 4
        };
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle());
        rootLayout.RowStyles.Add(new RowStyle());
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

        var titleLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 11F, FontStyle.Bold, GraphicsUnit.Point),
            Text = "软件设置"
        };

        var descriptionLabel = new Label
        {
            AutoSize = true,
            ForeColor = Color.FromArgb(96, 96, 96),
            Margin = new Padding(0, 4, 0, 12),
            Text = "控制关闭窗口时的默认行为"
        };

        var fieldLayout = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 0),
            RowCount = 1
        };
        fieldLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112F));
        fieldLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        fieldLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var fieldLabel = new Label
        {
            AutoSize = true,
            Margin = new Padding(0, 4, 8, 0),
            Text = "关闭行为"
        };

        var actionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 8, 0, 0),
            WrapContents = false
        };
        actionsPanel.Controls.Add(settingsSaveButton);
        actionsPanel.Controls.Add(settingsCancelButton);

        fieldLayout.Controls.Add(fieldLabel, 0, 0);
        fieldLayout.Controls.Add(closeBehaviorPreferenceComboBox, 1, 0);

        rootLayout.Controls.Add(titleLabel, 0, 0);
        rootLayout.Controls.Add(descriptionLabel, 0, 1);
        rootLayout.Controls.Add(fieldLayout, 0, 2);
        rootLayout.Controls.Add(actionsPanel, 0, 3);

        AcceptButton = settingsSaveButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.None;
        CancelButton = settingsCancelButton;
        ClientSize = new Size(420, 176);
        Controls.Add(rootLayout);
        Font = new Font("Microsoft YaHei", 9F, FontStyle.Regular, GraphicsUnit.Point);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(420, 176);
        MaximumSize = new Size(420, 176);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "软件设置";
    }

    private void LoadCurrentConfig()
    {
        var config = ConfigService.Load();
        closeBehaviorPreferenceComboBox.SelectedItem = config.CloseBehaviorPreference;
    }

    private void SettingsSaveButton_Click(object? sender, EventArgs e)
    {
        SaveSelectedCloseBehaviorPreference();
        Close();
    }

    private static void CloseBehaviorPreferenceComboBox_Format(object? sender, ListControlConvertEventArgs e)
    {
        if (e.ListItem is CloseBehaviorPreference preference)
            e.Value = GetCloseBehaviorPreferenceText(preference);
    }

    private static string GetCloseBehaviorPreferenceText(CloseBehaviorPreference preference)
    {
        return preference switch
        {
            CloseBehaviorPreference.ExitApplication => "直接彻底退出",
            CloseBehaviorPreference.MinimizeToTray => "直接最小化到系统托盘",
            _ => "每次关闭时询问"
        };
    }
}
