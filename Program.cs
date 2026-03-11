using PhoenixManager;
using PhoenixManager.Services;

// CLI mode: --fetch or --cleanup (called by scheduled tasks, no GUI)
if (args.Length > 0)
{
    var config = ConfigService.Load();

    switch (args[0].ToLowerInvariant())
    {
        case "--fetch":
            FetchService.Execute(config);
            return;

        case "--cleanup":
            CleanupService.Execute(config);
            return;

        default:
            // Unknown argument, fall through to GUI
            break;
    }
}

// GUI mode
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.SetHighDpiMode(HighDpiMode.SystemAware);
Application.Run(new MainForm());
