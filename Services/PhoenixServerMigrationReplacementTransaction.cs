namespace PhoenixToolkit.Services;

internal sealed class PhoenixServerMigrationReplacementTransaction
{
    private readonly Action<string, string> replaceEntry;
    private readonly Action<string> deleteEntry;
    private readonly List<ReplacedEntry> replacedEntries = [];

    internal PhoenixServerMigrationReplacementTransaction()
        : this(
            PhoenixServerProductionDataRootSecurity.ReplaceEntry,
            PhoenixServerProductionDataRootSecurity.DeleteEntry)
    {
    }

    internal PhoenixServerMigrationReplacementTransaction(
        Action<string, string> replaceEntry,
        Action<string> deleteEntry)
    {
        this.replaceEntry = replaceEntry;
        this.deleteEntry = deleteEntry;
    }

    internal void Replace(string stagingPath, string destinationPath)
    {
        var rollbackPath = PathExists(destinationPath)
            ? Path.Combine(
                Path.GetDirectoryName(destinationPath)!,
                $".{Path.GetFileName(destinationPath)}.pre-migration-{Guid.NewGuid():N}")
            : null;

        if (rollbackPath is not null)
            replaceEntry(destinationPath, rollbackPath);

        try
        {
            replaceEntry(stagingPath, destinationPath);
        }
        catch (Exception replaceException)
        {
            if (rollbackPath is null)
                throw;

            try
            {
                DeleteIfExists(destinationPath);
                replaceEntry(rollbackPath, destinationPath);
            }
            catch (Exception rollbackException)
            {
                throw new InvalidOperationException(
                    $"替换 PhoenixServer 迁移目标失败，且无法回滚原数据: {destinationPath}",
                    new AggregateException(replaceException, rollbackException));
            }

            throw;
        }

        replacedEntries.Add(new ReplacedEntry(destinationPath, rollbackPath));
    }

    internal void Commit()
    {
        foreach (var replacedEntry in replacedEntries)
        {
            if (replacedEntry.RollbackPath is not null)
                DeleteIfExists(replacedEntry.RollbackPath);
        }

        replacedEntries.Clear();
    }

    internal void Rollback()
    {
        List<Exception>? failures = null;
        foreach (var replacedEntry in Enumerable.Reverse(replacedEntries))
        {
            try
            {
                DeleteIfExists(replacedEntry.DestinationPath);
                if (replacedEntry.RollbackPath is not null)
                    replaceEntry(replacedEntry.RollbackPath, replacedEntry.DestinationPath);
            }
            catch (Exception ex)
            {
                (failures ??= []).Add(ex);
            }
        }

        if (failures is { Count: > 0 })
            throw new AggregateException("无法完整回滚 PhoenixServer 迁移目标。", failures);
    }

    private void DeleteIfExists(string path)
    {
        if (PathExists(path))
            deleteEntry(path);
    }

    private static bool PathExists(string path)
    {
        try
        {
            _ = File.GetAttributes(path);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }

    private sealed record ReplacedEntry(string DestinationPath, string? RollbackPath);
}
