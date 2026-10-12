# dock-sidebar

## Purpose

定义 Windows 桌面 Dock 的停靠、唤出、显示器选择和入口展示行为。实现参考 `Script/Core/DockManager.cs`、`DockState.cs` 与 `UI/Views/MainWindow/MainWindow.xaml.cs`。

## Requirements

### Requirement: 四边停靠与自动显隐
系统 SHALL 支持上、下、左、右停靠；上/下使用横向布局，左/右使用纵向布局。鼠标进入当前显示器停靠边缘触发带时滑入，远离停靠区域或进入其他显示器时滑出。

#### Scenario: 边缘唤出与离开
- **WHEN** 鼠标进入当前停靠边缘的 2 DIP 触发带
- **THEN** Dock 滑入显示，并在鼠标远离边缘超过 Dock 厚度加 20 DIP 或离开当前显示器时滑出。

### Requirement: 显示器选择与可用边缘
系统 SHALL 同时维护一个活动 Dock，允许选择有效显示器；拒绝停靠到与其他显示器连接的边缘。切换显示器时优先沿用原边缘，否则选择可用边缘，没有可用边缘则保持原状态。

#### Scenario: 切换目标显示器
- **WHEN** 用户选择不同的有效显示器且目标具有可用边缘
- **THEN** 更新显示器与边缘状态，并重建主窗口以应用目标屏幕的位置与 DPI。

#### Scenario: 无效选择
- **WHEN** 请求无效显示器索引或目标所有边缘均不可用
- **THEN** Dock 状态保持不变。

### Requirement: DPI 与沿边拖动
系统 SHALL 在物理像素与 WPF DIP 之间换算位置和尺寸，允许沿停靠边缘拖动并限制在当前屏幕边界内；拖动时暂停鼠标触发的显隐处理，释放后恢复。

#### Scenario: 拖动后再次唤出
- **WHEN** 用户沿边拖动 Dock 并释放，再触发隐藏和显示
- **THEN** 显隐以当前窗口物理位置为起点，保留沿边移动的位置。

### Requirement: Dock 置顶
系统 SHALL 在显示及失焦后维护 Dock 的置顶层级，恢复置顶时不主动激活窗口抢占焦点。

#### Scenario: Dock 失焦
- **WHEN** 可见 Dock 失去焦点
- **THEN** 系统恢复原生置顶层级而不主动夺取当前输入焦点。

### Requirement: 根列表顺序与容量
系统 SHALL 按根列表原有顺序显示容量以内的前 N 个节点；完全忽略旧 `dockDisplayOrder` 的值，不按其排序、过滤或去重。容量为 `max(0, floor(屏幕方向长度 × 0.9 / ((72 + 16) × DPI缩放)) - 3)`，方向长度使用物理像素。

#### Scenario: 重复与越界序号
- **WHEN** 容量为 5，配置按列表顺序具有序号 3、1、3、6
- **THEN** 按根列表原顺序显示全部四个节点，不因旧序号重复或越界隐藏节点。

#### Scenario: 容量为零
- **WHEN** 当前屏幕与缩放计算出的容量为 0
- **THEN** 不显示用户配置入口。

## Known limitations

当前是一个 Dock 在显示器间切换，不提供每屏独立 Dock；显示器热插拔恢复不作为已实现保证。停靠位置、显示器和边缘没有写入按钮配置文件。
