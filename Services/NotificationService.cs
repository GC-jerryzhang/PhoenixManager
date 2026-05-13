using Microsoft.Toolkit.Uwp.Notifications;
using PhoenixToolkit.Models;

namespace PhoenixToolkit.Services;

public static class NotificationService
{
    public static void ShowFetchResult(InstallPlan? installPlan)
    {
        if (installPlan is null)
            return;

        var installUri = ProtocolActivationService.BuildInstallUri(installPlan.Id);
        var lines = new List<string>();
        if (installPlan.DesignerPackage is not null)
            lines.Add($"Designer: {installPlan.DesignerPackage.FileName}");
        if (installPlan.ServerPackage is not null)
            lines.Add($"Server: {installPlan.ServerPackage.FileName}");

        var actionText = installPlan.DesignerPackage is not null && installPlan.ServerPackage is not null
            ? "点击通知先安装 Designer，再安装 Server"
            : "点击通知开始安装";

        new ToastContentBuilder()
            .SetProtocolActivation(installUri)
            .AddText("Phoenix 新安装包已拉取")
            .AddText(string.Join(Environment.NewLine, lines))
            .AddText(actionText)
            .Show(toast =>
            {
                toast.ExpirationTime = DateTimeOffset.Now.AddMinutes(5);
            });
    }

    public static void ShowInstallFailure(string message)
    {
        new ToastContentBuilder()
            .AddText("Phoenix 安装启动失败")
            .AddText(message)
            .Show(toast =>
            {
                toast.ExpirationTime = DateTimeOffset.Now.AddMinutes(5);
            });
    }
}
