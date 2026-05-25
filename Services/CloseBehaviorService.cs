using PhoenixToolkit.Models;

namespace PhoenixToolkit.Services;

public enum CloseBehaviorAction
{
    PromptUser,
    ExitApplication,
    MinimizeToTray
}

public static class CloseBehaviorService
{
    public static CloseBehaviorAction ResolveCloseAction(
        CloseBehaviorPreference preference,
        bool isExplicitExit,
        CloseReason closeReason)
    {
        if (isExplicitExit || closeReason != CloseReason.UserClosing)
            return CloseBehaviorAction.ExitApplication;

        return preference switch
        {
            CloseBehaviorPreference.ExitApplication => CloseBehaviorAction.ExitApplication,
            CloseBehaviorPreference.MinimizeToTray => CloseBehaviorAction.MinimizeToTray,
            _ => CloseBehaviorAction.PromptUser
        };
    }
}
