namespace PhoenixToolkit.Models;

public sealed record CleanupWeeks(
    int KeepAllWeeks = 3,
    int KeepDailyWeeks = 6,
    int DeleteAfterWeeks = 9
);

public sealed record AppConfig(
    string SourceDir = @"\\xafile\ToolsFile1\Projects\Phoenix\Installers\develop",
    string LocalBaseDir = @"D:\TestReceive\historyPackage",
    int FetchIntervalMinutes = 5,
    string CleanupTime = "09:30",
    CleanupWeeks? CleanupWeeks = null
)
{
    public CleanupWeeks CleanupWeeks { get; init; } = CleanupWeeks ?? new CleanupWeeks();

    public string DesignerDir => Path.Combine(LocalBaseDir, "designer");
    public string ServerDir => Path.Combine(LocalBaseDir, "server");
    public string LogDir => Path.Combine(LocalBaseDir, "log");
}
