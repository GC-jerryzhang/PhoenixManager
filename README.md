# Phoenix Installer Manager

从共享目录自动拉取最新 Phoenix Designer / Server 安装包，并按分层策略清理旧版本。单文件 exe，双击即用。

## 功能

- **定时拉取** — 每 N 分钟从共享目录复制最新安装包到本地，带文件稳定性检查和幂等去重
- **分层清理** — 按周龄自动清理旧安装包（全部保留 → 每天保留一个 → 全部删除）
- **GUI 配置** — 所有参数可视化配置，点击即生效
- **计划任务管理** — 一键注册/卸载 Windows 计划任务
- **零闪烁** — WinExe 子系统程序，计划任务执行时不会弹窗、不会抢焦点

## 快速开始

1. 从 [Releases](../../releases) 下载 `PhoenixManager.exe`
2. 双击运行（首次会自动生成 `config.json`）
3. 填写共享源目录和本地保存目录
4. 点击「安装服务」注册计划任务

> 需要管理员权限（注册计划任务需要）。

## 界面

```
┌─ Phoenix Installer Manager ────────────────────────┐
│                                                     │
│  共享源目录:   [\\server\share\path       ] [浏览]  │
│  本地保存目录: [D:\local\path             ] [浏览]  │
│                                                     │
│  ── 拉取策略 ──                                     │
│  拉取间隔:    [5  ] 分钟                            │
│                                                     │
│  ── 清理策略 ──                                     │
│  保留全部:    [3  ] 周内    每天保留一个: [6  ] 周内 │
│  超过删除:    [9  ] 周      清理时间:    [09:30]     │
│                                                     │
│  [安装服务]  [卸载服务]  [立即拉取]  [立即清理]      │
│                                                     │
│  状态: ● 已安装 - 每5分钟拉取 / 每天09:30清理       │
└─────────────────────────────────────────────────────┘
```

## 配置文件

`config.json`（与 exe 同目录，自动生成）：

```json
{
  "sourceDir": "\\\\xafile\\ToolsFile1\\Projects\\Phoenix\\Installers\\develop",
  "localBaseDir": "D:\\TestReceive\\historyPackage",
  "fetchIntervalMinutes": 5,
  "cleanupTime": "09:30",
  "cleanupWeeks": {
    "keepAllWeeks": 3,
    "keepDailyWeeks": 6,
    "deleteAfterWeeks": 9
  }
}
```

本地目录结构固定为：

```
{localBaseDir}/
├── designer/    # Designer 安装包
├── server/      # Server 安装包
└── log/         # 运行日志
```

## 清理策略说明

| 周龄 | 策略 |
|------|------|
| 0 ~ `keepAllWeeks` 周 | 全部保留 |
| `keepAllWeeks` ~ `deleteAfterWeeks` 周 | 每天只保留第一个 build |
| 超过 `deleteAfterWeeks` 周 | 全部删除 |

另外，文件名异常的安装包（`Setu.exe`、`Setup .exe`）会被直接删除。

## 命令行模式

计划任务通过命令行参数静默调用，不会弹出任何窗口：

```
PhoenixManager.exe --fetch      # 执行一次拉取
PhoenixManager.exe --cleanup    # 执行一次清理
PhoenixManager.exe              # 打开 GUI
```

## 从源码构建

需要 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)。

```powershell
# 编译
dotnet build -c Release

# 发布单文件自包含 exe
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

产物在 `publish/PhoenixManager.exe`，约 155 MB，包含 .NET 运行时，同事无需额外安装。

## 项目结构

```
PhoenixManager/
├── Program.cs                 # 入口（CLI/GUI 双模式）
├── MainForm.cs/.Designer.cs   # WinForms 界面
├── Models/
│   └── AppConfig.cs           # 不可变 record 配置模型
└── Services/
    ├── ConfigService.cs       # config.json 读写
    ├── FetchService.cs        # 拉取逻辑
    ├── CleanupService.cs      # 清理逻辑
    └── SchedulerService.cs    # Windows 计划任务管理
```

## License

MIT
