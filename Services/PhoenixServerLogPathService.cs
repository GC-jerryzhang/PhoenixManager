using PhoenixToolkit.Models;

namespace PhoenixToolkit.Services;

internal static class PhoenixServerLogPathService
{
    internal static string Resolve(PhoenixServerDataMigrationPaths paths, string subDirectory)
    {
        if (string.IsNullOrEmpty(subDirectory))
            return paths.LogDirectory;

        if (Path.IsPathRooted(subDirectory) ||
            subDirectory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(segment => segment is "." or ".."))
        {
            throw new ArgumentException("日志子目录不合法。", nameof(subDirectory));
        }

        return Path.Combine(paths.LogDirectory, subDirectory);
    }

    internal static string Resolve(string subDirectory) =>
        Resolve(
            RuntimeModeService.IsDevelopment
                ? PhoenixServerDataMigrationPaths.CreateDevelopment(RuntimeModeService.StorageRoot)
                : PhoenixServerDataMigrationPaths.CreateDefault(),
            subDirectory);
}
