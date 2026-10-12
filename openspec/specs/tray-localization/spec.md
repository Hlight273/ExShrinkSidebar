# tray-localization

## Purpose

定义应用启动、托盘菜单及语言切换。实现参考 `App.xaml.cs`、`UI/Views/ContextMenu/ContextMenuManager.cs` 和 `Script/Localization/`。

## Requirements

### Requirement: 启动语言选择
系统 SHALL 在启动时将系统 UI 语言以 zh 开头的环境映射到 zh-CN，其他环境映射到 en-US，并应用到资源管理器和线程区域设置。

#### Scenario: 非中文系统启动
- **WHEN** 系统 UI 语言不是中文
- **THEN** 应用以 en-US 初始化本地化资源。

### Requirement: 托盘切换语言
系统 SHALL 提供 zh-CN 与 en-US 托盘选项；切换后更新区域设置、重建托盘菜单并发出 ON_LANGUAGE_CHANGED，已打开的配置编辑器刷新其订阅的文本及树视图。

#### Scenario: 编辑期间切换语言
- **WHEN** 打开配置编辑器并通过托盘切换语言
- **THEN** 编辑器标题、添加根节点、保存按钮及重绘的树文本使用新语言，工作副本仍保留。

### Requirement: 托盘显示器选择
系统 SHALL 在打开显示器子菜单时动态枚举显示器，标记当前显示器并通过 UI 线程提交选择。

#### Scenario: 打开显示器菜单
- **WHEN** 用户展开显示器子菜单
- **THEN** 菜单展示当前显示器列表；无显示器时展示不可点击的提示项。

### Requirement: 应用退出与清理
系统 SHALL 提供托盘退出命令，在退出时隐藏并释放托盘图标；主窗口清理时停止停靠动画、解除鼠标订阅并停止 Hook。

#### Scenario: 托盘退出
- **WHEN** 用户选择托盘退出
- **THEN** 应用执行 Shutdown 并清理托盘与窗口资源。

## Known limitations

语言选择不持久化，重启后重新使用系统 UI 语言。显示器菜单仍有中文硬编码，不能宣称所有 UI 已完整双语化；已打开的参数详情窗口不保证即时刷新语言。
