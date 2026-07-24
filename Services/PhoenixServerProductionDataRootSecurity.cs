using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using PhoenixToolkit.Models;
using System.Security.AccessControl;
using System.Security.Principal;

namespace PhoenixToolkit.Services;

internal static class PhoenixServerProductionDataRootSecurity
{
    private const uint ReadControl = 0x00020000;
    private const uint WriteDac = 0x00040000;
    private const uint FileReadAttributes = 0x00000080;
    private const uint FileShareRead = 0x00000001;
    private const uint FileShareWrite = 0x00000002;
    private const uint FileShareDelete = 0x00000004;
    private const uint OpenExisting = 3;
    private const uint FileFlagBackupSemantics = 0x02000000;
    private const uint FileFlagOpenReparsePoint = 0x00200000;
    private const uint MoveFileReplaceExisting = 0x00000001;
    private const uint MoveFileWriteThrough = 0x00000008;
    private const uint OwnerSecurityInformation = 0x00000001;
    private const uint DaclSecurityInformation = 0x00000004;
    private const uint ProtectedDaclSecurityInformation = 0x80000000;

    internal static void EnsureTrustedAndSecure(PhoenixServerDataMigrationPaths paths)
    {
        var dataRoot = GetDataRoot(paths);
        var serverRoot = Path.GetDirectoryName(dataRoot)
            ?? throw new InvalidOperationException("无法解析 PhoenixServer 规范数据根目录。");

        if (!Directory.Exists(serverRoot))
        {
            throw new InvalidOperationException(
                $"新版 PhoenixServer 未创建规范数据根目录，拒绝恢复数据: {serverRoot}");
        }

        EnsureTrustedAndSecureDirectory(serverRoot);
        Directory.CreateDirectory(dataRoot);
        EnsureTrustedAndSecureDirectory(dataRoot);
    }

    internal static bool IsNormalizedRootPresent(PhoenixServerDataMigrationPaths paths)
    {
        var dataRoot = GetDataRoot(paths);
        var serverRoot = Path.GetDirectoryName(dataRoot)
            ?? throw new InvalidOperationException("无法解析 PhoenixServer 规范数据根目录。");
        return Directory.Exists(serverRoot);
    }

    internal static void CleanupRollbackEntries(PhoenixServerDataMigrationPaths paths)
    {
        EnsureTrustedAndSecure(paths);
        var dataRoot = GetDataRoot(paths);
        foreach (var entry in Directory.EnumerateFileSystemEntries(dataRoot))
        {
            var entryName = Path.GetFileName(entry);
            if (entryName.StartsWith(".", StringComparison.Ordinal) &&
                entryName.Contains(".pre-migration-", StringComparison.Ordinal))
            {
                DeleteEntry(entry);
            }
        }
    }

    internal static void EnsureTrustedAndSecureDirectory(string directoryPath)
    {
        var fullPath = Path.GetFullPath(directoryPath);
        using var directoryHandle = OpenDirectoryNoFollow(fullPath);
        var originalIdentity = GetFileIdentity(directoryHandle, fullPath);
        EnsureTrustedOwner(directoryHandle, fullPath);
        ApplyProtectedServiceAcl(directoryHandle, fullPath);

        using var verifiedHandle = OpenDirectoryNoFollow(fullPath);
        var verifiedIdentity = GetFileIdentity(verifiedHandle, fullPath);
        if (originalIdentity != verifiedIdentity)
            throw new InvalidOperationException($"迁移目录在加固期间发生变化: {fullPath}");
    }

    internal static void ReplaceEntry(string sourcePath, string destinationPath)
    {
        if (!MoveFileEx(
                sourcePath,
                destinationPath,
                MoveFileReplaceExisting | MoveFileWriteThrough))
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error(),
                $"无法以安全方式替换迁移目标: {destinationPath}");
        }
    }

    internal static void DeleteEntry(string path)
    {
        var attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.Directory) == 0)
        {
            File.Delete(path);
            return;
        }

        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            if (!RemoveDirectory(path))
            {
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    $"无法删除迁移重解析点: {path}");
            }

            return;
        }

        foreach (var entry in Directory.EnumerateFileSystemEntries(path))
            DeleteEntry(entry);

        Directory.Delete(path, recursive: false);
    }

    private static string GetDataRoot(PhoenixServerDataMigrationPaths paths)
    {
        var dataRoot = Path.GetFullPath(
            Path.GetDirectoryName(paths.ServerDataDirectory)
            ?? throw new InvalidOperationException("无法解析 server-data 的父目录。"));
        var expectedServerData = Path.Combine(dataRoot, "server-data");
        var expectedConfig = Path.Combine(dataRoot, "config.json");
        var expectedWorkspace = Path.Combine(dataRoot, "workspace");

        if (!PathsEqual(paths.ServerDataDirectory, expectedServerData) ||
            !PathsEqual(paths.ConfigPath, expectedConfig) ||
            !PathsEqual(paths.WorkspaceDirectory, expectedWorkspace))
        {
            throw new InvalidOperationException("PhoenixServer 规范数据路径不符合预期结构。");
        }

        return dataRoot;
    }

    private static bool PathsEqual(string left, string right) =>
        string.Equals(
            Path.GetFullPath(left),
            Path.GetFullPath(right),
            StringComparison.OrdinalIgnoreCase);

    private static SafeFileHandle OpenDirectoryNoFollow(string directoryPath)
    {
        var handle = CreateFile(
            directoryPath,
            ReadControl | WriteDac | FileReadAttributes,
            FileShareRead | FileShareWrite | FileShareDelete,
            IntPtr.Zero,
            OpenExisting,
            FileFlagBackupSemantics | FileFlagOpenReparsePoint,
            IntPtr.Zero);

        if (handle.IsInvalid)
        {
            handle.Dispose();
            throw new Win32Exception(
                Marshal.GetLastWin32Error(),
                $"无法安全打开迁移目录: {directoryPath}");
        }

        return handle;
    }

    private static FileIdentity GetFileIdentity(SafeFileHandle handle, string directoryPath)
    {
        if (!GetFileInformationByHandle(handle, out var information))
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error(),
                $"无法读取迁移目录属性: {directoryPath}");
        }

        if ((information.FileAttributes & (uint)FileAttributes.Directory) == 0)
            throw new InvalidOperationException($"迁移目标不是目录: {directoryPath}");
        if ((information.FileAttributes & (uint)FileAttributes.ReparsePoint) != 0)
            throw new InvalidOperationException($"不支持迁移重解析点: {directoryPath}");

        return new FileIdentity(
            information.VolumeSerialNumber,
            information.FileIndexHigh,
            information.FileIndexLow);
    }

    private static void EnsureTrustedOwner(SafeFileHandle handle, string directoryPath)
    {
        var result = GetSecurityInfo(
            handle,
            SeFileObject,
            OwnerSecurityInformation,
            out var owner,
            out _,
            out _,
            out _,
            out var securityDescriptor);
        if (result != 0)
        {
            throw new Win32Exception(
                unchecked((int)result),
                $"无法读取迁移目录所有者: {directoryPath}");
        }

        try
        {
            var ownerIdentifier = new SecurityIdentifier(owner);
            var administrators = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            var localSystem = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            if (!ownerIdentifier.Equals(administrators) && !ownerIdentifier.Equals(localSystem))
            {
                throw new InvalidOperationException(
                    $"迁移目录所有者不受信任，拒绝向其写入数据: {directoryPath}");
            }
        }
        finally
        {
            if (securityDescriptor != IntPtr.Zero)
                _ = LocalFree(securityDescriptor);
        }
    }

    private static void ApplyProtectedServiceAcl(SafeFileHandle handle, string directoryPath)
    {
        var administrators = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
        var localSystem = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        var accessControlList = new RawAcl(revision: 2, capacity: 2);
        accessControlList.InsertAce(0, CreateFullControlAce(localSystem));
        accessControlList.InsertAce(1, CreateFullControlAce(administrators));

        var descriptor = new RawSecurityDescriptor(
            ControlFlags.DiscretionaryAclPresent | ControlFlags.DiscretionaryAclProtected,
            owner: null,
            group: null,
            systemAcl: null,
            discretionaryAcl: accessControlList);
        var descriptorBytes = new byte[descriptor.BinaryLength];
        descriptor.GetBinaryForm(descriptorBytes, 0);
        var descriptorPointer = Marshal.AllocHGlobal(descriptorBytes.Length);

        try
        {
            Marshal.Copy(descriptorBytes, 0, descriptorPointer, descriptorBytes.Length);
            if (!SetKernelObjectSecurity(
                    handle,
                    DaclSecurityInformation | ProtectedDaclSecurityInformation,
                    descriptorPointer))
            {
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    $"无法加固 PhoenixServer 迁移目录权限: {directoryPath}");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(descriptorPointer);
        }
    }

    private static CommonAce CreateFullControlAce(SecurityIdentifier identity) =>
        new(
            AceFlags.ContainerInherit | AceFlags.ObjectInherit,
            AceQualifier.AccessAllowed,
            (int)FileSystemRights.FullControl,
            identity,
            isCallback: false,
            opaque: null);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandle(
        SafeFileHandle file,
        out ByHandleFileInformation fileInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MoveFileEx(
        string existingFileName,
        string? newFileName,
        uint flags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveDirectory(string path);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr memory);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetKernelObjectSecurity(
        SafeFileHandle handle,
        uint securityInformation,
        IntPtr securityDescriptor);

    [DllImport("advapi32.dll")]
    private static extern uint GetSecurityInfo(
        SafeFileHandle handle,
        uint objectType,
        uint securityInformation,
        out IntPtr owner,
        out IntPtr group,
        out IntPtr discretionaryAccessControlList,
        out IntPtr systemAccessControlList,
        out IntPtr securityDescriptor);

    [StructLayout(LayoutKind.Sequential)]
    private struct ByHandleFileInformation
    {
        public uint FileAttributes;
        public uint CreationTimeLow;
        public uint CreationTimeHigh;
        public uint LastAccessTimeLow;
        public uint LastAccessTimeHigh;
        public uint LastWriteTimeLow;
        public uint LastWriteTimeHigh;
        public uint VolumeSerialNumber;
        public uint FileSizeHigh;
        public uint FileSizeLow;
        public uint NumberOfLinks;
        public uint FileIndexHigh;
        public uint FileIndexLow;
    }

    private readonly record struct FileIdentity(
        uint VolumeSerialNumber,
        uint FileIndexHigh,
        uint FileIndexLow);

    private const uint SeFileObject = 1;
}
