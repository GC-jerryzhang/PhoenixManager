namespace PhoenixToolkit.Models;

public sealed record PhoenixServerDataMigrationPaths(
    string LegacyServerDataDirectory,
    string LegacyConfigPath,
    string LegacyWorkspaceDirectory,
    string ServerDataDirectory,
    string ConfigPath,
    string WorkspaceDirectory,
    string LogDirectory)
{
    public static PhoenixServerDataMigrationPaths CreateDefault()
    {
        var legacyServerRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "PhoenixServer");
        var normalizedServerRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "PhoenixServer");
        var normalizedDataRoot = Path.Combine(normalizedServerRoot, "Data");

        return new PhoenixServerDataMigrationPaths(
            LegacyServerDataDirectory: Path.Combine(legacyServerRoot, "server-data"),
            LegacyConfigPath: Path.Combine(legacyServerRoot, "config.json"),
            LegacyWorkspaceDirectory: Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                "PhoenixWorkspace"),
            ServerDataDirectory: Path.Combine(normalizedDataRoot, "server-data"),
            ConfigPath: Path.Combine(normalizedDataRoot, "config.json"),
            WorkspaceDirectory: Path.Combine(normalizedDataRoot, "workspace"),
            LogDirectory: Path.Combine(normalizedServerRoot, "Logs"));
    }

    public static PhoenixServerDataMigrationPaths CreateDevelopment(string storageRoot)
    {
        var legacyServerRoot = Path.Combine(storageRoot, "phoenixserver-legacy", "PhoenixServer");
        var normalizedServerRoot = Path.Combine(storageRoot, "phoenixserver-normalized", "PhoenixServer");
        var normalizedDataRoot = Path.Combine(normalizedServerRoot, "Data");

        return new PhoenixServerDataMigrationPaths(
            LegacyServerDataDirectory: Path.Combine(legacyServerRoot, "server-data"),
            LegacyConfigPath: Path.Combine(legacyServerRoot, "config.json"),
            LegacyWorkspaceDirectory: Path.Combine(storageRoot, "phoenixserver-legacy", "PhoenixWorkspace"),
            ServerDataDirectory: Path.Combine(normalizedDataRoot, "server-data"),
            ConfigPath: Path.Combine(normalizedDataRoot, "config.json"),
            WorkspaceDirectory: Path.Combine(normalizedDataRoot, "workspace"),
            LogDirectory: Path.Combine(normalizedServerRoot, "Logs"));
    }
}
