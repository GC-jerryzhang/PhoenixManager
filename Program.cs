using PhoenixToolkit;
using PhoenixToolkit.Services;

namespace PhoenixToolkit;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        var launchArgs = RuntimeModeService.Initialize(args);

        ProtocolActivationService.EnsureRegistered();
        InstallActivationService.Initialize();

        if (ProtocolActivationService.TryHandleLaunchArgs(launchArgs))
            return;

        // CLI mode: --fetch or --cleanup (called by scheduled tasks, no GUI)
        if (launchArgs.Length > 0)
        {
            var config = ConfigService.Load();

            switch (launchArgs[0].ToLowerInvariant())
            {
                case "--fetch":
                    FetchService.Execute(config);
                    return;

                case "--cleanup":
                    CleanupService.Execute(config);
                    return;
            }
        }

        if (InstallActivationService.HandleStartupActivationIfNeeded())
            return;

        // GUI mode
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.Run(new MainForm());
    }
}
