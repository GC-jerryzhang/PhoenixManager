namespace PhoenixToolkit.Models;

internal sealed record PhoenixServerMigrationBackup(
    string BackupDirectory,
    bool HasServerData,
    bool HasConfig,
    bool HasWorkspace,
    bool ServiceWasRunning);
