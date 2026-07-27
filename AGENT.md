# ExShrinkSidebar 协作指南

## 项目概览

ExShrinkSidebar 是一个基于 .NET 8、WPF 与 Windows Forms 互操作的 Windows 桌面侧边栏工具。鼠标靠近屏幕边缘时会显示停靠栏；用户可通过图形化编辑器配置按钮和动作链。

- 解决方案：`ExShrinkSidebar.sln`
- 项目文件：`ExShrinkSidebar.csproj`
- 目标框架：`net8.0-windows`
- 应用类型：WPF `WinExe`，同时启用 Windows Forms（托盘图标、屏幕信息）
- 运行平台：Windows；涉及窗口停靠、资源管理器窗口和 COM 互操作的功能不能在非 Windows 环境中完整验证。

## 常用命令

在仓库根目录执行：

```powershell
dotnet restore ExShrinkSidebar.sln
dotnet build ExShrinkSidebar.sln
dotnet build ExShrinkSidebar.sln -c Release
dotnet run --project ExShrinkSidebar.csproj
```

当前没有独立测试项目或稳定的自动化测试套件。修改后至少运行 `dotnet build ExShrinkSidebar.sln`；涉及 WPF 界面、停靠、托盘或资源管理器窗口时，还应在 Windows 上做一次手动冒烟验证。

发布采用 win-x64 单文件、自包含模式。GitHub Actions 的发布流程见 `.github/workflows/publish-windows.yml`，其输出目录为 `bin/Release/net8.0-windows/win-x64/publish/`，并打包为 `artifacts/ExShrinkSidebar-Windows.zip`。不要随意更改发布属性（尤其是 `PublishSingleFile`、`SelfContained` 及原生库自解压选项），除非同时验证发布产物。

## 目录与职责

- `App.xaml`、`App.xaml.cs`：应用启动、退出、托盘图标和初始语言选择。
- `Script/Core/`：停靠状态与窗口控制、全局 Hook、配置读写、动作执行、事件系统。
- `Script/Model/`：配置数据模型和枚举。配置结构变更应优先在这里定义。
- `Script/Utils/`：屏幕、资源路径、资源管理器窗口等平台辅助能力。
- `Script/Localization/`、`Asset/Properties/`：本地化辅助类、`.resx` 资源和生成的资源访问器。
- `UI/Views/`：WPF 视图及其 code-behind；`UI/ViewModels/`：可复用窗口基类与 ViewModel。
- `Asset/Resource/ButtonIcons/`：运行时复制到输出目录的按钮图标。
- `Asset/Resource/Icons/`、`Asset/Texture/`：WPF 打包资源。

## 关键设计约束

### 配置与动作链

- 持久化配置的实际位置是 `%APPDATA%/ExShrinkBar/Config/config.json`，由 `ResHelper.GetButtonConfigPath()` 决定；它不是仓库内的示例文件。不要把用户配置、测试产生的配置或个人路径提交到 Git。
- `ConfigManager` 负责加载、规范化、克隆和保存配置。读写或模型迁移改动必须保持 `NormalizeConfig` 对空集合、空参数、父节点引用和旧 `script` 字段到 `Execute.arg.path` 的兼容处理。
- `ButtonConfig` 是树结构：`Combine` 节点通过 `logicChain` 包含子节点；其他节点为可执行叶子。新增节点时使用 `ConfigManager.ReserveNextId()` 分配整个树内唯一 ID。
- 编辑器操作的是配置工作副本；只有保存时才调用 `UpdateAndSave` 写入正式配置，并通过 `EventIds.ON_CONFIG_SAVED` 通知主侧边栏刷新。不要在编辑过程中直接修改 `ConfigManager.Instance.Config`。
- `BtnCfgLogicRunner` 会先展平嵌套 `Combine`，再按顺序执行叶节点。新增动作类型时需同步更新枚举、执行分支、编辑器显示、摘要/本地化及默认图标映射。

### 停靠与 UI

- `DockManager` 和 `DockState` 共同维护靠边、滑入滑出及屏幕位置。修改停靠行为时同时检查四个边缘、横纵布局、DPI 坐标转换以及拖拽暂停/恢复逻辑。
- 现有编辑器包含一定量 code-behind；小范围 UI 交互可沿用此模式，跨视图状态或可复用状态应优先放入对应 ViewModel/核心模块，避免把业务规则堆进 XAML code-behind。
- WPF 资源的 Build Action 和运行时文件的 CopyToOutputDirectory 都在 `.csproj` 中显式声明。新增图标时，按其使用方式选择 `Resource` 或 `Content`，并验证发布目录中需要的运行时文件仍存在。

### 本地化与文本编码

- 新增面向用户的文本时，优先在 `Asset/Properties/StringResources.resx` 和 `StringResources.en-US.resx` 同时添加 key，再经 `UiTextCatalog` 或本地化辅助方法访问；不要在业务逻辑中散落硬编码文本。
- 枚举显示名通过 `Description` 和资源管理器解析；新增 `ExConfigType` 的显示文本也必须补充资源 key。
- 所有源码、XAML、Markdown、YAML 和 `.resx` 文件使用 UTF-8。修改包含中文内容的旧文件时，先确认读取/写入编码，避免将乱码或 BOM 差异混入无关改动。
- `StringResources.Designer.cs` 是由 `.resx` 生成的文件；仅在资源生成器无法使用时才手工调整，并让其与资源 key 保持一致。

## 变更与验证清单

1. 先定位改动所在层级，保持 `Core`、`Model`、`Utils` 和 `UI` 的职责边界。
2. 修改配置模型、配置保存或动作类型时，检查旧配置可加载、空值可规范化，并验证编辑器保存后主栏会刷新。
3. 修改图标或 XAML 资源时，构建后检查资源 URI 与输出文件；不要只在 IDE 调试环境中验证。
4. 运行 `dotnet build ExShrinkSidebar.sln`。如构建受 Windows SDK、COM 引用或当前机器环境限制而失败，记录完整错误和已尝试的命令，不要通过删除平台功能绕过问题。
5. 仅提交与需求相关的源码和资源；不要提交 `bin/`、`obj/`、`.vs/`、发布包、个人配置或 IDE 用户文件。

## 提交习惯

- 使用小而聚焦的提交，提交信息可沿用仓库现有的 `feat:`、`fix:`、`chore:` 前缀。
- 不要改写或丢弃工作区中与当前任务无关的用户改动。
- 修改架构、用户可见行为或已知限制时，同步更新 `README.md` 或 `docs/` 下相应文档。
