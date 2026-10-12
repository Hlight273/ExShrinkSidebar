# config-storage

## Purpose

定义配置树的持久化、默认值、兼容迁移及编辑隔离。实现参考 `Script/Core/ConfigManager.cs`、`Script/Model/ButtonConfig.cs` 与 `Script/Utils/ResHelper.cs`。

## Requirements

### Requirement: 用户配置位置与首次初始化
系统 SHALL 在 `%APPDATA%/ExShrinkBar/Config/config.json` 读写缩进 JSON；首次无文件时生成并保存一个 ID 为 1 的 Notepad 根节点。

#### Scenario: 首次启动
- **WHEN** 用户配置文件不存在且目录可写
- **THEN** 创建必要目录和默认配置文件，并将默认节点加载到内存。

### Requirement: 配置树序列化
系统 SHALL 持久化根列表 `buttons` 和节点的 `id`、`name`、`icon`、`dockDisplayOrder`、`configType`、`logicChain`、`arg`；`Parent`、`IsExpanded`、`useChainLogic` 不写入 JSON。`dockDisplayOrder` 仅保留为旧配置兼容字段，不参与任何展示或排序行为。动作枚举数值保持 Combine=0、OpenFolder=1、Execute=2、Notepad=3。

#### Scenario: 保存后重新加载
- **WHEN** 保存具有嵌套 Combine 和动作参数的配置并重新加载
- **THEN** 保留节点列表顺序和参数，由规范化重建父引用，而不从 JSON 恢复 UI 展开状态。

### Requirement: 规范化与旧配置兼容
系统 SHALL 将空根集合、空子集合、空节点参数补齐，将空名称和图标补为空字符串，重建父引用；Combine 默认展开。Execute 的 `arg.path` 为空白且旧 `arg.script` 非空白时 SHALL 将 script 迁入 path。

#### Scenario: 旧执行目标迁移
- **WHEN** Execute 节点只有旧 script 目标而没有 path
- **THEN** 规范化后 path 等于 script；已有非空白 path 时不覆盖。

#### Scenario: 旧根节点缺少展示序号
- **WHEN** 根节点缺少旧展示序号，或旧序号为零、负数、重复值、越界值
- **THEN** 不补齐或重新赋值该字段，根列表及动作链顺序保持原样。

### Requirement: 工作副本与编辑器 ID 预留
系统 SHALL 提供独立的深拷贝配置用于编辑，并通过 `ReserveNextId()` 为编辑器新节点预留大于正式配置整棵树最大 ID 的递增 ID。

#### Scenario: 连续新增尚未保存的节点
- **WHEN** 编辑器连续调用 ReserveNextId 创建根或子节点
- **THEN** 每次返回不同的递增 ID，不与正式配置树中的 ID 冲突。

### Requirement: 读取异常回退
系统 SHALL 在配置读取或反序列化抛出异常时回退到空配置，规范化后尝试保存。

#### Scenario: 损坏的 JSON
- **WHEN** 配置内容不是有效 JSON 且文件可写
- **THEN** 使用空 buttons 配置并覆盖保存，不自动恢复为首次启动的 Notepad 示例。

## Known limitations

当前没有损坏文件备份、原子写入或保存失败恢复机制；读取异常的回退可能覆盖原文件。规范化不保证修复重复 ID 或列表中的 null 节点。`AddButton` 的旧快捷接口仅按根列表分配 ID，不应视为编辑器的全树 ID 预留接口。
