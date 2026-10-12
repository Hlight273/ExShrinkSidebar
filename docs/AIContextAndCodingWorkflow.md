# AI 上下文与编码工作流

## 定位与成熟度

本流程适配自 eggy_stealanegg_copy 的上下文初始化、业务路由、提交信息 skill 和工作流分层，用于 ExShrinkSidebar 的 WPF 开发。它是可用于企业团队的 AI 协作基础，不是企业交付体系已经完备的证明。

已经落地的是上下文按需读取、主规格与变更分离、架构边界、验证分层和提交事实约束。当前没有稳定自动化测试套件；现有 GitHub Actions 主要用于发布，不能代替 PR 验证门禁。强制评审、分支保护、权限治理和发布回滚等需要团队与托管平台配置，本次未配置或宣称已具备。

## 信息各自负责什么

| 来源 | 职责 |
| --- | --- |
| [AGENTS.md](../AGENTS.md) | 工程约束、授权边界、验证与交付要求 |
| [openspec/config.yaml](../openspec/config.yaml) | 简短项目背景与导航 |
| [主规格](../openspec/README.md) | 当前约定的业务行为和已知限制 |
| `openspec/changes/<change>/` | 本次目标差异、设计和任务状态 |
| [FrameworkArchitecture.md](FrameworkArchitecture.md) | 实现边界、状态所有权、生命周期和排查入口 |
| `App.xaml.cs`、`Script/`、`UI/` | 当前实际实现，可能与预期存在缺陷或漂移 |
| `openspec/changes/archive/` | 历史设计与过程证据，默认不读取 |

文档不改变平台指令及用户授权的优先级。不能将源项目中“仓库规则高于用户”的声明当作实际权限机制。

## 技能接入

项目 skill 位于 `.agents/skills/`，由 AGENTS.md 提供路径和触发条件；即使会话没有自动列出新 skill，也可按显式路径读取。不要声称已安装未生成的斜杠命令。

| 场景 | 技能 |
| --- | --- |
| 初次进入项目开发、分析或排错 | [project-context-bootstrap](../.agents/skills/project-context-bootstrap/SKILL.md) |
| 业务提案、实现、排错、重构、评审、OpenSpec apply | [business-context-router](../.agents/skills/business-context-router/SKILL.md) |
| 生成提交信息或总结拟提交内容 | [git-commit-message](../.agents/skills/git-commit-message/SKILL.md) |
| 根因分析、缺陷修复和回归验证 | [bugfix-delivery](../.agents/skills/bugfix-delivery/SKILL.md) |
| 结构、依赖和生命周期重构 | [refactor-delivery](../.agents/skills/refactor-delivery/SKILL.md) |
| 工作区或指定补丁评审 | [git-diff-review](../.agents/skills/git-diff-review/SKILL.md) |

使用前说明名称和用途。当前任务已读取且未变化的上下文不重复读；只涉及文字修正或提交信息的任务无需强制完整业务路由。

业务路由完成后，按任务进入缺陷修复、重构或差异评审 skill。修复需有根因证据，重构需有行为基线，评审默认只报告可操作问题。三个技能均沿用 AGENTS.md 的验证分层与授权范围，不自动创建测试产物或执行 Git 提交。

## 从需求到交付

1. 识别目标与范围，读取工程规则，确认当前用户授权。按路径和标题发现上下文，再读相关主规格、架构章节与目标代码。
2. 显示简短路由结果。通过直接调用、事件、线程和数据所有权补齐依赖；信息充分后停止扩展。
3. 新能力、行为和兼容性变更先记录 OpenSpec change，包含提案、增量规格、任务及必要设计。既定行为修复、纯文档更正不制造空提案。
4. 给出可审阅方案；只要求方案时停在方案。实施遵循 AGENTS.md 的编码流程和当前有效授权，不把已授权的文档迁移重新视为待批准业务编码。
5. 实施时读取当前 schema 要求的 change 上下文文件，按任务更新状态；关键语义不明确时澄清，不把合理的内部实现选择全部升级为审批。
6. 按 AGENTS.md 的矩阵执行静态检查、构建及相关运行验证，分别记录通过、失败和未执行。不能操作 UI 时明确保留运行验收缺口。
7. 实现与适用验证完成后同步主规格并归档，执行 OpenSpec 严格校验；同步架构与用户文档。尚未完成的必要任务不能勾选完成。
8. 用户要求提交信息时调用对应 skill；实际提交、推送、发布依各自授权范围执行，不由文档生成隐式触发。

## 路由示例：调整 Dock 展示序号

先读 `dock-sidebar`；若涉及输入合法性或保存兼容，再读 `config-editor`、`config-storage`。查看架构的“停靠与显示器”“配置所有权与保存”，搜索 `dockDisplayOrder`，从 MainWindow 的展示筛选追踪到编辑控件及 NormalizeConfig。

检查序号范围、重复处理、保存刷新与 DPI 容量计算。已有重复序号仅显示首个节点是当前约定，不能只改编辑器而漏掉展示规则。一般不需要读取动作执行、Explorer COM 或历史归档；若用户要求排查旧版本回归，再定向读取相关历史。

## 冲突与维护

活动 change 改变主规格是正常目标差异；代码违反现行规格则需判断缺陷，不能直接将错误“同步”为规范。无法通过当前请求、代码和文档解决的关键分歧应说明位置与影响。

新增能力时维护主规格；架构职责或入口变化时更新 FrameworkArchitecture；路由机制变化时更新 skill 与本文。固定表只作发现入口，实际依赖始终从代码确认，避免维护每项业务的全量文件清单。

本文仅迁移稳定工程方法，不迁移游戏服务端/客户端、EggyAPI、实体门面和编辑器 CLI 约束。测试授权与构建方式沿用本项目规则。
