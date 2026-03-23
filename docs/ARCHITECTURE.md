# 项目风格与架构

## 项目定位

ExShrinkSidebar 是一个基于 `.NET 8` 与 `WPF` 的 Windows 桌面侧边栏 Dock 工具。

目标是提供一个靠屏幕边缘唤出的统一入口面板，用户通过配置定义入口按钮和动作链，并通过图形化编辑器维护这些配置。

## 当前产品风格

- 平台：Windows 桌面应用
- 技术栈：WPF
- 视觉方向：
  - 深灰背景
  - 蓝色主强调色
  - 图标按钮尽量保持简洁、弱边框、浅灰图标
- 交互方向：
  - 靠边自动滑入滑出
  - 单入口点击触发动作链
  - 树形编辑器维护组合动作

## 目录结构

- `Asset/`
  - 图标、图片、资源文件、本地化资源。
- `Script/`
  - 核心逻辑、配置模型、事件系统、工具类、本地化辅助。
- `UI/`
  - 主窗口、编辑器窗口、各类 WPF 视图与 ViewModel。

## 核心模块

### 1. 侧边栏与停靠

- `Script/Core/DockManager.cs`
  - 负责屏幕边缘停靠、滑入滑出、窗口位置控制。
- `Script/Core/DockState.cs`
  - 维护当前 Dock 状态。
- `Script/Utils/ScreenHelper.cs`
  - 提供屏幕边界与方向辅助能力。

### 2. 配置管理

- `Script/Core/ConfigManager.cs`
  - 负责配置读取、保存、克隆、规范化。
  - 负责新节点 ID 分配与兼容迁移。
- `Script/Model/ButtonConfig.cs`
  - 定义入口配置、动作节点、动作参数结构。

### 3. 动作执行

- `Script/Core/BtnCfgLogicRunner.cs`
  - 先把配置树展开成顺序动作列表。
  - 再按顺序执行叶子动作。
  - 当前支持：
    - `OpenFolder`
    - `Execute`
    - `Notepad`
    - `Combine`

### 4. 图形化配置编辑器

- `UI/Views/EditConfigView/AddConfigPanel.xaml`
  - 编辑器主窗口。
- `UI/Views/EditConfigView/SubConfigView.xaml`
  - 树节点组件。
- `UI/Views/EditConfigView/NodeDetailWindow.xaml`
  - 额外参数面板。

当前编辑器原则：

- 树节点只负责结构编辑：
  - 类型
  - 名称
  - 功能摘要
  - 展开/删除/新增子节点
- 复杂参数统一在详情面板中维护。
- 打开编辑器后先编辑工作副本，保存时再写回正式配置。

## 本地化策略

- 资源文件位于：
  - `Asset/Properties/StringResources.resx`
  - `Asset/Properties/StringResources.en-US.resx`
- 枚举文本通过 `Description + ResourceManager` 获取。
- UI 代码文本统一通过：
  - `Script/Localization/UiTextCatalog.cs`
- 该类负责：
  - 定义资源 key
  - 提供回退文案
  - 尽量避免继续在逻辑代码中硬编码中文

## 当前代码风格约定

- 优先保持模块职责单一。
- 配置模型尽量集中在 `Script/Model`。
- 纯 UI 交互逻辑尽量放在对应视图代码后置中。
- 复杂字符串优先走本地化 key，不直接写死在逻辑代码里。
- 资源图标统一放在 `Asset/Resource/Icons/`。

## 后续建议

- 给编辑器补 ViewModel 分层，逐步减少 CodeBehind 体积。
- 给配置读写、动作展开、节点操作增加测试。
- 为动作失败和参数非法情况补用户可见的错误提示。
- 如果后续支持多屏，应把窗口定位和资源管理器窗口重排策略继续拆分。
