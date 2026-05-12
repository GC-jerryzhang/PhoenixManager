namespace PhoenixToolkit.Models;

public sealed record FetchedPackageInfo(
    PackageKind Kind,
    string FileName,
    string FullPath,
    DateTimeOffset FetchedAt
);
