# toast-install-activation

- 日期: 2026-05-13
- 记录位置: `.claude/investigations/`

## 输入线索

- 问题类型：产品代码缺陷
- 用户原始描述：点击通知后，没有执行安装
- 已知失败信号：通知展示正常，但点击后没有启动安装包
- 初始边界：通知构建、toast 激活、安装计划读取、安装包启动

## 问题描述

- 背景：应用在拉取到 Designer/Server 安装包后会弹出 Windows 通知，期望用户点击通知即可触发安装
- 现象：通知能看到，点击后没有任何安装动作，也没有失败提示
- 期望：点击通知后直接进入安装流程；若同时有两个包，先安装 Designer，再安装 Server
- 实际：安装计划文件一直停留在 `Pending`，没有进入执行态
- 影响范围：所有依赖 toast 点击触发安装的场景
- 环境或版本边界：Windows 桌面程序，单文件 WinForms EXE，`requireAdministrator`

## 已确认事实

- `D:\TestReceive\historyPackage\state\install-plans\*.json` 正常生成，说明拉取后创建安装计划是成功的
- 最近多个安装计划的 `status` 都保持 `0`（`Pending`），没有切换到 `Running/Completed/Failed`
- `D:\TestReceive\historyPackage\log\` 下不存在任何 `install-phoenix_*.log`
- `InstallActivationService.ExecuteInstallPlan` 在进入后必定会先写安装日志，因此“无日志”支持“根本没进入安装执行链路”
- 当前通知正文通过 `AddArgument("action","install")` 和 `AddArgument("planId", ...)` 依赖 `ToastNotificationManagerCompat.OnActivated` / `WasCurrentProcessToastActivated()` 来处理点击
- 当前工程没有实现 `DesktopNotificationManagerCompat.RegisterAumidAndComServer/RegisterActivator` 所需的 COM activator

## 初始假设

- 假设 1：点击通知后，toast 激活没有真正进入应用
  - 支持它的现象：计划一直是 `Pending`，没有安装日志
  - 推翻它的方法：若能看到 `install-phoenix_*.log` 或计划被标记为 `Running/Failed`，说明已经进入应用

- 假设 2：进入应用了，但安装包启动失败
  - 支持它的现象：若看到 `install-phoenix_*.log` 中存在失败记录
  - 推翻它的方法：当前完全没有安装日志，说明还没走到启动安装包这一步

## 验证计划

| 假设 | 验证方法 | 预期信号 | 结果 |
| --- | --- | --- | --- |
| 假设 1 | 查看安装计划状态和安装日志 | 若未激活则计划保持 `Pending` 且无安装日志 | 符合 |
| 假设 1 | 核对通知激活实现是否具备 Win32 非打包应用要求 | 若未做完整激活注册，则点击不稳定或失效 | 符合 |
| 假设 2 | 查 `install-phoenix_*.log` | 若进入执行则会留下日志 | 未发现日志，排除 |

## 调查过程

### 第一步：检查安装计划与日志

- 操作：查看 `D:\TestReceive\historyPackage\state\install-plans\*.json` 和 `D:\TestReceive\historyPackage\log\`
- 证据：
  - 多个安装计划文件存在，最近计划仍是 `status: 0`
  - 不存在 `install-phoenix_*.log`
- 结论：点击通知后没有进入 `ExecuteInstallPlan`

### 第二步：检查当前通知点击实现

- 操作：阅读 `NotificationService.cs`、`InstallActivationService.cs`、`Program.cs`
- 证据：
  - 通知使用 `AddArgument` 写入 `action/install/planId`
  - 程序依赖 `ToastNotificationManagerCompat.OnActivated` 和 `WasCurrentProcessToastActivated()`
  - 没有任何协议激活或显式命令行激活兜底
- 结论：当前方案完全依赖 toolkit 的 toast 激活回调链

### 第三步：核对 toolkit 文档约束

- 操作：检查本地 NuGet 文档 `Microsoft.Toolkit.Uwp.Notifications.xml`
- 证据：
  - `DesktopNotificationManagerCompat.RegisterAumidAndComServer<T>` / `RegisterActivator<T>` 文档明确说明：经典 Win32 程序若要可靠响应激活，需要完成 AUMID 和 COM activator 注册
  - 当前项目没有这套注册实现
- 结论：现有实现的激活前提不完整，导致通知点击不会稳定回到应用

## 根因分析

- 根因类型：产品代码缺陷
- 根因说明：当前实现把通知点击安装建立在 `ToastNotificationManagerCompat` 的应用激活回调上，但项目没有完整实现经典 Win32 toast 激活所需的注册链路；在当前 `requireAdministrator` 的单文件 WinForms EXE 形态下，点击通知没有稳定回到进程，因此安装计划始终停留在 `Pending`
- 关键证据：
  - 安装计划一直 `Pending`
  - 没有 `install-phoenix_*.log`
  - 代码中没有 COM activator 注册
  - toolkit 文档明确给出经典 Win32 激活的前提要求

## 已排除解释

- 安装包路径不合法或文件不存在
- 安装进程启动失败
- 安装计划序列化失败

## 风险与边界

- 如果继续沿用当前 `OnActivated` 方案，后续仍可能在不同机器/权限上下文中不稳定
- COM activator 方案实现成本更高，并且和当前管理员权限模型耦合更强

## 方案对比

| 方案 | 做法 | 结果或风险 | 结论 |
| --- | --- | --- | --- |
| 推荐方案 | 改成协议激活：通知点击拉起 `phoenixtoolkit://install?planId=...`，程序从命令行 URI 解析并执行安装 | 触发链路清晰，不依赖 toolkit 的隐式 toast 激活回调 | 推荐 |
| 备选方案 | 补齐 `DesktopNotificationManagerCompat` COM activator 注册 | 实现复杂，调试成本高，与当前 EXE 权限模型耦合 | 不优先 |

## 结论

- 一句话结论：通知点击没有执行安装的根因是 toast 激活链路没真正打通，而不是安装流程本身失败
- 推荐动作：改成协议激活方案，并保留现有安装计划执行逻辑
- 如果暂不修复：用户点击通知仍然不会触发安装，只能手工打开安装包

## 后续动作

- [x] 完成根因定位
- [x] 落地协议激活修复代码
