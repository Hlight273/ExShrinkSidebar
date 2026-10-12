# config-editor

## Purpose

定义图形化动作树编辑与保存边界。实现参考 `UI/Views/EditConfigView/` 中的 AddConfigPanel、SubConfigView 和 NodeDetailWindow。

## Requirements

### Requirement: 工作副本保存与放弃
编辑器 SHALL 打开正式配置的深拷贝；仅在“保存并关闭”时调用 UpdateAndSave 并发出 ON_CONFIG_SAVED，主侧边栏收到事件后刷新。

#### Scenario: 保存编辑
- **WHEN** 用户修改节点并保存
- **THEN** 正式配置被更新并写盘，Dock 刷新入口，编辑器关闭。

#### Scenario: 关闭而不保存
- **WHEN** 用户修改工作副本后直接关闭编辑器
- **THEN** 正式内存配置与磁盘配置不受这些编辑影响。

### Requirement: 树结构与根展示序号编辑
编辑器 SHALL 支持新增根节点、给 Combine 新增子节点、修改名称与类型、展开或收起组合、删除节点及其子树；根节点可设置正整数 Dock 展示序号，子节点不提供该控件。

#### Scenario: 新增根配置
- **WHEN** 用户新增根节点
- **THEN** 使用预留 ID 创建 Execute 节点，空列表的展示序号为 1，否则为当前最大根序号加 1。

#### Scenario: 删除组合
- **WHEN** 用户删除一个 Combine 节点
- **THEN** 从所属列表移除该节点及其子树，其他节点保留。

### Requirement: 同级拖拽排序
编辑器 SHALL 允许节点在同一父节点的列表内拖到目标前或目标后，拒绝跨父节点移动及自身拖放；根列表排序不自动改写 Dock 展示序号。

#### Scenario: 重排子动作
- **WHEN** 用户将组合内一个子节点拖到同级节点之后并保存
- **THEN** logicChain 顺序随之改变，下一次执行按新顺序展开。

#### Scenario: 跨父节点拖放
- **WHEN** 拖动节点与目标节点的 Parent 不同
- **THEN** 不改变配置树结构。

### Requirement: 独立参数面板
编辑器 SHALL 为 Execute 提供目标与参数编辑，为 OpenFolder 提供目录及可选角落/尺寸；详情确认后写回工作副本，取消不提交输入。

#### Scenario: 禁用窗口放置
- **WHEN** 用户取消启用窗口放置并确认详情
- **THEN** windowCorner 清空、宽高归零；启用时无效或非正整数尺寸也转换为 0。

### Requirement: 编辑器尺寸调整
编辑器 SHALL 支持四角拉伸并受最小尺寸约束，通过滚动查看超出窗口的树内容。

#### Scenario: 深层配置树
- **WHEN** 展开的树超出窗口宽度或高度
- **THEN** 用户可通过滚动查看内容，并可拉伸窗口调整可见区域。

## Known limitations

不支持跨层级拖拽、复制节点或脚本模板导入导出。根节点列表顺序与 Dock 展示序号是不同概念；重复序号的显示规则见 dock-sidebar。
