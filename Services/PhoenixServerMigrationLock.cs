namespace PhoenixToolkit.Services;

internal static class PhoenixServerMigrationLock
{
    private const string LockFileName = ".migration.lock";

    internal static IDisposable Acquire(string migrationRoot) =>
        OpenLockFile(migrationRoot);

    internal static IDisposable? TryAcquire(string migrationRoot)
    {
        try
        {
            return OpenLockFile(migrationRoot);
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static FileStream OpenLockFile(string migrationRoot)
    {
        Directory.CreateDirectory(migrationRoot);
        return new FileStream(
            Path.Combine(migrationRoot, LockFileName),
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None);
    }
}
