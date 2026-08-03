namespace PhoenixToolkit.Services;

internal static class ProductLogPathService
{
    private const string LogRootDirectoryName = "PhoenixDesigner";

    internal const string DesignerJavaDirectory = "designer-java";
    internal const string DesignerNodeDirectory = "designer-node";
    internal const string RuntimeJavaDirectory = "runtime-java";
    internal const string ServerJavaDirectory = "server-java";

    internal static string Resolve(string subDirectory)
    {
        var logRoot = Path.Combine(Path.GetTempPath(), LogRootDirectoryName);

        if (string.IsNullOrEmpty(subDirectory))
            return logRoot;

        if (Path.IsPathRooted(subDirectory) ||
            subDirectory.Contains(':', StringComparison.Ordinal) ||
            subDirectory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(segment => segment is "." or ".."))
        {
            throw new ArgumentException("日志子目录不合法。", nameof(subDirectory));
        }

        return Path.Combine(logRoot, subDirectory);
    }
}
