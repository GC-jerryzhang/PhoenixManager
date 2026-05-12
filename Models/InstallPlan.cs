namespace PhoenixToolkit.Models;

public sealed record InstallPlan(
    string Id,
    FetchedPackageInfo? DesignerPackage,
    FetchedPackageInfo? ServerPackage,
    InstallPlanStatus Status = InstallPlanStatus.Pending,
    DateTimeOffset CreatedAt = default,
    DateTimeOffset? StartedAt = null,
    DateTimeOffset? CompletedAt = null,
    string? LastError = null
)
{
    public DateTimeOffset CreatedAt { get; init; } =
        CreatedAt == default ? DateTimeOffset.Now : CreatedAt;
}
