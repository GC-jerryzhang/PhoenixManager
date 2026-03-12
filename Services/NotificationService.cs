using Microsoft.Toolkit.Uwp.Notifications;

namespace PhoenixManager.Services;

public static class NotificationService
{
    public static void ShowFetchResult(string? designerName, string? serverName)
    {
        if (designerName is null && serverName is null)
            return;

        var lines = new List<string>();
        if (designerName is not null)
            lines.Add($"Designer: {designerName}");
        if (serverName is not null)
            lines.Add($"Server: {serverName}");

        new ToastContentBuilder()
            .AddText("Phoenix 新安装包已拉取")
            .AddText(string.Join(Environment.NewLine, lines))
            .Show(toast =>
            {
                toast.ExpirationTime = DateTimeOffset.Now.AddMinutes(5);
            });
    }
}
