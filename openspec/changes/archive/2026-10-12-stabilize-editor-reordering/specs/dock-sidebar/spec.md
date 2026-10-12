## RENAMED Requirements

- FROM: `### Requirement: 展示序号与容量`
- TO: `### Requirement: 根列表顺序与容量`

## MODIFIED Requirements
### Requirement: 根列表顺序与容量
系统 SHALL 按根列表原有顺序显示容量以内的前 N 个节点；完全忽略旧 `dockDisplayOrder` 的值，不按其排序、过滤或去重。容量为 `max(0, floor(屏幕方向长度 × 0.9 / ((72 + 16) × DPI缩放)) - 3)`，方向长度使用物理像素。

#### Scenario: 重复与越界序号
- **WHEN** 容量为 5，配置按列表顺序具有序号 3、1、3、6
- **THEN** 按根列表原顺序显示全部四个节点，不因旧序号重复或越界隐藏节点。

#### Scenario: 容量为零
- **WHEN** 当前屏幕与缩放计算出的容量为 0
- **THEN** 不显示用户配置入口。
