# ExShrinkSidebar

ExShrinkSidebar 是一个基于 `.NET 8` 和 `WPF` 的 Windows 桌面侧边栏 Dock 工具。

它的目标是在鼠标进入屏幕边缘时弹出一个统一入口面板，让用户通过配置定义入口按钮和动作链，并通过图形化编辑器维护这些配置。

## 项目功能

- 支持侧边栏在所选显示器的上、下、左、右可用边缘滑入滑出，并通过托盘切换显示器；同一时间只有一个 Dock。
- 侧边栏按编辑器中的根配置列表顺序显示入口，旧配置中的 dockDisplayOrder 字段不再参与展示。
- Dock 会根据当前屏幕的横/竖可用长度、DPI 和图标尺寸计算容量，仅显示根列表容量内的前 N 项。
- 点击入口后顺序执行动作链。
- 支持的动作类型：
  - `OpenFolder`
  - `Execute`
  - `Notepad`
  - `Combine`
- 提供图形化配置编辑器，用树形结构维护动作配置。
- 支持同级节点拖拽排序：有效目标立即描边，松手提交，Esc 取消；不延迟或让位，排序复用现有控件。节点名称后显示只读同级序号。支持通过托盘切换中英文。

## 使用方法

1. 在 Visual Studio Developer PowerShell 中使用完整 MSBuild 构建项目（需要 .NET 桌面开发组件和 .NET 8 SDK）

```powershell
msbuild ExShrinkSidebar.sln /restore /p:Configuration=Debug
```

2. 启动本次构建输出的 EXE。项目使用 COM 引用，不使用默认会重新构建的 `dotnet run`；详细验证流程见 [AGENTS.md](./AGENTS.md)。
3. 将鼠标移动到当前设置的屏幕边缘，呼出侧边栏。
4. 点击入口按钮执行对应动作。
5. 点击侧边栏末尾的脚本编辑入口，打开配置编辑器。
6. 在编辑器中调整根配置顺序、子动作和额外参数后保存，Dock 随根配置顺序更新。

## 相关文档

- [AI 上下文与编码工作流](./docs/AIContextAndCodingWorkflow.md)
- [框架架构与实现边界](./docs/FrameworkArchitecture.md)
- [OpenSpec 主规格与维护流程](./openspec/README.md)
- [协作入口](./AGENTS.md)
- [修改记录](./docs/CHANGELOG.md)
- [项目风格与架构](./docs/ARCHITECTURE.md)
