# Phoenix 测试辅助工具 (PhoenixToolkit)

安装包自动拉取/清理 + 产品日志快速打开。单文件 exe，双击即用。

## 运行

1. 安装 [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0/runtime)（如未安装）
2. 双击 `PhoenixToolkit.exe` 运行
3. 填写共享源目录和本地保存目录，点击「安装服务」注册计划任务

> 需要管理员权限（注册计划任务需要）。

## 命令行

```
PhoenixToolkit.exe              # 打开 GUI
PhoenixToolkit.exe --fetch      # 静默执行一次拉取
PhoenixToolkit.exe --cleanup    # 静默执行一次清理
```

## 开发调试

```powershell
dotnet run -- --dev
dotnet run -- --dev --fetch
dotnet run -- --dev --cleanup
```

- `--dev` 会进入开发模式，避免每次改动都先发布打包再验证。
- 开发模式的配置和本地状态会写入 `%LOCALAPPDATA%\PhoenixToolkit\dev\`。
- 开发模式下不会自动注册协议，也不会安装/卸载计划任务；这些系统级行为请使用正式发布版验证。

## 构建

```powershell
dotnet publish .\PhoenixToolkit.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./publish
```
