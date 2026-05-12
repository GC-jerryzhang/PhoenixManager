using System.Diagnostics;
using PhoenixToolkit.Models;

namespace PhoenixToolkit.Services;

public static class InstallerLaunchService
{
    public static void ExecutePlan(InstallPlan plan)
    {
        if (plan.DesignerPackage is null && plan.ServerPackage is null)
            throw new InvalidOperationException("安装任务中没有可执行的安装包。");

        if (plan.DesignerPackage is not null)
            LaunchAndWait(plan.DesignerPackage);

        if (plan.ServerPackage is not null)
            LaunchAndWait(plan.ServerPackage);
    }

    private static void LaunchAndWait(FetchedPackageInfo package)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = package.FullPath,
            WorkingDirectory = Path.GetDirectoryName(package.FullPath) ?? AppContext.BaseDirectory,
            UseShellExecute = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"无法启动安装包: {package.FileName}");

        process.WaitForExit();
    }
}
