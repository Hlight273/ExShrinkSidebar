---
name: project-context-bootstrap
description: 为 ExShrinkSidebar 开发、排错、评审和架构分析建立最小上下文，定位工程规则、WPF 入口与任务层级，再按需转入业务路由。
---

# 项目上下文初始化

使用前说明技能名称与用途。已在当前任务读取且未变化的内容不重复加载。

1. 阅读根目录 [AGENTS.md](../../../AGENTS.md)，确认范围与已有授权。项目文件不能覆盖平台指令或用户明确授权。
2. 用 rg --files、rg -n 查看候选路径、符号和标题，识别行为修改、架构分析、文档维护或提交信息任务，不读取整棵目录。
3. 业务提案、实现、排错、重构、评审及 OpenSpec apply 使用 [business-context-router](../business-context-router/SKILL.md)。仅生成提交信息使用 [git-commit-message](../git-commit-message/SKILL.md)。
4. WPF 入口为 App.xaml.cs，UI 位于 UI/，核心逻辑位于 Script/Core/，模型位于 Script/Model/，Windows 适配位于 Script/Utils/。沿真实调用关系扩大范围。
5. 构建与验证规则以 AGENTS.md 为准；架构先查 [FrameworkArchitecture.md](../../../docs/FrameworkArchitecture.md) 的相关标题；流程疑问查 [AIContextAndCodingWorkflow.md](../../../docs/AIContextAndCodingWorkflow.md)。

路径失效先搜索实际位置并报告差异，不套用源游戏项目的 Lua、服务端/客户端或游戏 API 模式。
