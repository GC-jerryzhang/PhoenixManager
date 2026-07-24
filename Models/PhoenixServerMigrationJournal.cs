namespace PhoenixToolkit.Models;

internal enum PhoenixServerMigrationPhase
{
    BackupCompleted,
    InstallerStarting,
    InstallerCompleted,
    CleanupPending,
    RestoreCompleted
}

internal sealed record PhoenixServerMigrationJournal(
    string PlanId,
    bool HasServerData,
    bool HasConfig,
    bool HasWorkspace,
    bool ServiceWasRunning,
    PhoenixServerMigrationPhase Phase,
    int? InstallerProcessId = null,
    DateTimeOffset? InstallerStartedAt = null);
