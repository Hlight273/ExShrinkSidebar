# action-execution

## Purpose

定义点击配置入口后的动作展开和执行语义。实现参考 `Script/Core/BtnCfgLogicRunner.cs` 与 `Script/Utils/ExplorerWindowHelper.cs`。

## Requirements

### Requirement: 组合动作按顺序展开
系统 SHALL 在后台任务中执行入口，先按子列表顺序深度优先展开嵌套 Combine，再逐一调用叶子动作；Combine 本身不执行外部动作。

#### Scenario: 嵌套组合
- **WHEN** 动作树为 Combine(A, Combine(B, C), D)
- **THEN** 执行动作调用顺序为 A、B、C、D。

#### Scenario: 空节点与叶子
- **WHEN** 展开空入口、空 Combine 或带有残留子列表的非 Combine 节点
- **THEN** 空入口和空 Combine 产生零步骤，非 Combine 仅产生自身步骤。

### Requirement: 执行目标与记事本
系统 SHALL 对 Execute 使用 `arg.path` 作为目标、`arg.arguments` 作为参数并通过 Shell 启动；Notepad SHALL 启动 `notepad.exe`，参数为空。

#### Scenario: 启动执行目标
- **WHEN** Execute 配置了非空白目标与参数
- **THEN** 使用对应目标和参数调用 Process.Start，UseShellExecute 为 true。

### Requirement: 打开目录与可选窗口放置
系统 SHALL 对 OpenFolder 使用 `arg.path` 打开目录；仅在设置 `windowCorner` 时传入角落、宽度、高度配置并尝试重排资源管理器窗口。

#### Scenario: 未启用窗口放置
- **WHEN** OpenFolder 的 windowCorner 为 null
- **THEN** 打开目录但不要求应用角落或窗口尺寸配置。

#### Scenario: 启用窗口放置
- **WHEN** OpenFolder 设置了角落和尺寸
- **THEN** 尝试按配置移动并调整资源管理器窗口，重排属于尽力而为的行为。

### Requirement: 无效目标与失败处理
系统 SHALL 在 Execute 或 OpenFolder 目标为空白时返回失败；单步返回 false 不阻止后续步骤。外层执行遇到未被单步处理的异常时记录调试信息并结束本次链。

#### Scenario: 空目标之后还有动作
- **WHEN** 空目标 Execute 后面跟随 Notepad
- **THEN** 跳过启动空目标，仍调用 Notepad。

## Known limitations

顺序表示动作调用顺序，不保证等待启动的外部进程结束。多次点击可启动并行任务；没有取消、事务回滚或统一的用户可见错误反馈。资源管理器重排依赖 Windows/COM，不保证所有打开方式均成功。
