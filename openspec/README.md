# OpenSpec 主规格

本目录于 2026-10-12 使用 OpenSpec CLI 1.11.0 初始化，采用 `spec-driven` schema。本次依据现有源码直接建立基线，不虚构历史变更或运行时验收结果。

## 主规格索引

主规格是 `specs/*/spec.md` 的集合，按能力维护，不另复制一份易漂移的总规格。

| 能力 | 主规格 | 主要实现 |
| --- | --- | --- |
| 停靠与展示 | [dock-sidebar](specs/dock-sidebar/spec.md) | DockManager、DockState、MainWindow、ScreenHelper |
| 配置持久化与兼容 | [config-storage](specs/config-storage/spec.md) | ConfigManager、ButtonConfig、ResHelper |
| 动作链执行 | [action-execution](specs/action-execution/spec.md) | BtnCfgLogicRunner、ExplorerWindowHelper |
| 图形化配置编辑 | [config-editor](specs/config-editor/spec.md) | AddConfigPanel、SubConfigView、NodeDetailWindow |
| 托盘与语言 | [tray-localization](specs/tray-localization/spec.md) | App、ContextMenuManager、LocalizationHelper |

## 维护流程

1. 阅读主规格与协作指南。新增行为使用 `openspec new change <change-name>` 创建变更。
2. 在变更目录撰写 `proposal.md`、`specs/<capability>/spec.md`、`tasks.md`，需要时补 `design.md`。增量使用 `ADDED/MODIFIED/REMOVED Requirements`，需求包含 SHALL/MUST，场景使用 WHEN/THEN。
3. 实现并验证；主规格只接纳已实现行为。恢复既定行为的修复或纯文档校正可直接维护主规格。
4. 执行 `openspec archive <change-name> --yes`，由 CLI 合并增量至主规格并移到 `changes/archive/`。若已手动同步，核对后才使用 `--skip-specs`，不要跳过验证。
5. 执行以下命令，同步 README、架构与修改记录，并报告真实验证结果。

```powershell
openspec list --specs
openspec validate --all --strict --no-interactive
```

代码修改还应使用完整 .NET Framework 版 MSBuild 执行 `msbuild ExShrinkSidebar.sln /restore /p:Configuration=Debug`；分层验证流程见 [AGENTS.md](../AGENTS.md)。Windows UI、显示器切换、DPI、资源管理器重排需按相关场景验证。CLI 校验只证明规格格式有效，不能代替运行时验收。

## 边界

- `config.yaml` 提供项目上下文与产物规则；根目录 `AGENTS.md` 提供持久协作入口。
- `changes/` 当前没有待实施变更；新增提案时创建对应子目录。
- 初始化采用 `--tools none`，流程可直接通过 CLI 使用，不依赖未生成的 `/opsx:*` 快捷命令。
- 已知限制不代表待实现承诺，改善这些行为需另建变更。

## 基线验证记录（2026-10-12）

- `openspec validate --all --strict --no-interactive`：5 项主规格全部通过，共 23 条需求。
- `git diff --check`：通过。
- `dotnet build ExShrinkSidebar.sln`：还原完成，构建失败。SDK 9.0.315 的 `Microsoft.Common.CurrentVersion.targets(3081,5)` 报 `MSB4803`：`.NET Core 版本的 MSBuild 不支持“ResolveComReference”。请使用 .NET Framework 版本的 MSBuild。` 相关项目为 `ExShrinkSidebar.csproj`；本次未移除 SHDocVw COM 引用来绕过问题。
- 本次仅建立规格及维护文档，没有执行 Windows UI 运行时验收；行为基线来自源码核对。
