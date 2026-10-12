# ExShrinkSidebar 框架架构

## 定位与边界

这是当前 WPF 程序的实现导航，不是另一个业务规格副本。业务契约见 [主规格索引](../openspec/README.md)，工作流程见 [AIContextAndCodingWorkflow.md](AIContextAndCodingWorkflow.md)，验证命令见 [AGENTS.md](../AGENTS.md)。视觉风格见 [ARCHITECTURE.md](ARCHITECTURE.md)。

本项目采用 WPF 视图/code-behind、核心管理器、配置模型与 Windows 平台适配；并非完整 MVVM，也没有游戏项目的服务端、客户端、Context 或实体模块框架。不要为了迁移文档强行引入这些层级。

## 启动与退出

- `App.xaml` 使用 `OnExplicitShutdown`；主窗口关闭不等于主动退出应用。
- `App.xaml.cs` 的 OnStartup 选择初始语言、请求顶部停靠、创建主窗口和托盘并显示窗口。顶部是否可用仍由 DockState 校验。
- MainWindow Loaded 时创建 DockManager、布局并刷新按钮；显示器改变时 App 重建主窗口。
- 托盘退出调用 Shutdown；App 释放 NotifyIcon，主窗口 Cleanup 释放 DockManager 的动画、Hook 和相关窗口订阅。

排查窗口重建时同时检查旧实例清理和新实例订阅，不能仅检查新窗口是否显示。

## 模块职责

| 模块 | 当前职责与入口 |
| --- | --- |
| `UI/Views/MainWindow/` | Dock 布局、入口显示、拖动与点击 |
| `UI/Views/EditConfigView/` | 配置工作副本的树编辑及详情面板 |
| `UI/Views/ContextMenu/` | 托盘菜单、语言和显示器选择 |
| `UI/ViewModels/Common/` | 窗口基类、绑定及清理辅助 |
| `Script/Core/` | Dock 状态与控制、配置管理、动作执行、Hook、事件总线 |
| `Script/Model/` | AppConfig、ButtonConfig、动作与角落枚举、DockModel |
| `Script/Utils/` | 路径、屏幕和 Explorer/Windows 适配 |
| `Script/Localization/`、`Asset/Properties/` | 文本 key、资源与区域设置 |

局部视图交互可保留 code-behind；跨视图规则放入适当核心模块或 ViewModel。平台细节优先放在已有适配位置，不在多个视图复制 COM/Win32 调用。

## 停靠与显示器

`DockState` 持有一个活动屏幕与边缘，校验边缘连接情况并发送 ON_DOCK_MODEL_CHANGED。`DockManager` 订阅 HookManager 的鼠标事件，通过 Win32 位置控制和计时器实现滑入滑出，再同步 WPF 坐标。

MainWindow 决定入口容量、序号筛选及布局；拖动时暂停 DockManager 的鼠标显隐处理，释放后恢复。物理像素与 DIP 不可混用，屏幕变化要核对重建、缩放与边缘可用性。

排查入口：DockState → MainWindow.updateView → DockManager；入口缺失先检查展示序号、重复值、容量与缩放，不应首先怀疑配置丢失。

## 配置所有权与保存

`ConfigManager.Instance.Config` 是正式内存配置，JSON 由 ResHelper 定位到 `%APPDATA%/ExShrinkBar/Config/config.json`。ConfigManager 负责加载、规范化、克隆和保存；序列化字段在 ButtonConfig 定义。

编辑器持有 CloneConfig 的工作副本，正式配置仅在 UpdateAndSave 时更新。规范化重建父引用、补集合与参数、兼容旧 script 目标；Parent/IsExpanded 不持久化。新增编辑器节点使用 ReserveNextId，不能用根列表最大 ID 替代全树预留。

当前写入不是事务式：UpdateAndSave 先替换正式对象再写文件，读取异常回退还可能覆盖旧文件。不能在未实现备份或回滚时承诺保存失败不影响状态；改善此行为需要独立变更。

## 编辑器与配置链路

```text
AddConfigButton 打开或激活编辑器
→ AddConfigPanel.Init / CloneConfig
→ SubConfigView 修改树、名称、类型、根展示序号
→ NodeDetailWindow 确认参数写回工作副本
→ 保存：UpdateAndSave → ON_CONFIG_SAVED → MainWindow 刷新
```

AddConfigPanel 处理节点删除和同级排序请求；非保存关闭丢弃编辑。根列表排序不改 Dock 展示序号。节点详情的确认仍只写工作副本，不独立持久化。

新增字段需同时检查模型默认值、兼容规范化、编辑器绑定、摘要、保存刷新和相关规格，不能只加一个输入框。

## 动作执行与平台适配

`IconButton` 点击调用 `BtnCfgLogicRunner.Run`，后台任务先递归展开 Combine，再调用叶子动作。Execute/Notepad 使用 Shell 启动，OpenFolder 委托 ExplorerWindowHelper 打开目录并尽力重排窗口。

后台动作不是外部进程完成队列；步骤返回 false 不终止后续调用，未被单步捕获的异常由外层处理。新增动作需同步类型、执行分支、参数 UI、摘要、本地化、图标映射和规格。

COM 依赖决定了完整 MSBuild 的构建要求。不要为使 dotnet build 通过而删掉 SHDocVw 引用，也不要把 Explorer 重排写成必然成功。

## 事件与线程生命周期

`Script/Core/Event/EventMgr.cs` 的 EVENT 使用事件名和参数类型进行进程内通知。emit 获取监听副本并在调用线程依次执行，没有自动切换到 WPF Dispatcher。后台通知更新 UI 时必须明确线程切换。

监听者负责释放订阅；窗口基类使用 Cleanup/targetOff，编辑器显式 off。新增 lambda 或长期订阅时核对真实委托 Target 与释放方式，不假设全部捕获闭包都会被窗口清理。事件处理异常当前写入 Debug，不代表上层获得可靠的失败反馈。

## 本地化与资源

LocalizationHelper 应用区域设置；UiTextCatalog 和中英文 resx 提供文本。托盘切换语言发出 ON_LANGUAGE_CHANGED，订阅者刷新文本。显示器菜单仍有中文硬编码，参数详情窗口不保证即时语言刷新。

WPF 打包图标与运行时文件用途不同：`Asset/Resource/Icons/`、`Asset/Texture/` 按 Resource 使用，`Asset/Resource/ButtonIcons/` 等运行时内容由 csproj 配置复制。新增资源核对 Build Action、路径、发布输出；Designer 文件从 resx 重新生成。

## 构建发布与验证入口

构建、发布命令和验证矩阵统一维护在 AGENTS.md。CI 的发布实现为 `.github/workflows/publish-windows.yml`，手动 FolderProfile 输出路径与 CI 不同。编译验证、规格校验和 Windows 运行验收是三类不同证据。

尚未落实为保证的能力包括完整自动化回归、配置事务保存、每屏独立 Dock 和统一动作错误反馈。不要把架构建议当作已实现功能。
