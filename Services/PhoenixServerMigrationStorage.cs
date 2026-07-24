namespace PhoenixToolkit.Services;

internal static class PhoenixServerMigrationStorage
{
    private const string ToolkitDirectoryName = "PhoenixToolkit";
    private const string MigrationDirectoryName = "server-migrations";

    internal static string GetDefaultRoot()
    {
        var programDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var toolkitRoot = Path.Combine(programDataRoot, ToolkitDirectoryName);
        EnsureSecureDirectory(toolkitRoot);

        var migrationRoot = Path.Combine(toolkitRoot, MigrationDirectoryName);
        EnsureSecureDirectory(migrationRoot);
        return migrationRoot;
    }

    internal static void EnsureSecureDirectory(string directoryPath)
    {
        var directory = new DirectoryInfo(directoryPath);
        directory.Create();
        PhoenixServerProductionDataRootSecurity.EnsureTrustedAndSecureDirectory(directory.FullName);
    }

    internal static void EnsureNotReparsePoint(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidOperationException($"不支持迁移重解析点: {path}");
    }

}
