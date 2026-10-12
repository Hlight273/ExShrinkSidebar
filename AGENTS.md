# AI Agent 核心系统指令 (AGENTS.md)

你是本项目的核心 AI 编程助手。在任何对话和任务开始前，你必须**优先且严格**遵守本文件中的所有规则。
你的工作模式是：**遵守全局约束 -> 识别用户意图 -> 匹配触发词 -> 读取对应 SKILL -> 执行任务**。

> 本文件提供项目级默认约束，不能覆盖系统、开发者指令或用户明确授权。项目内 SKILL 与参考文档应遵循本文件；冲突时说明影响，并按适用的上级指令及当前授权处理。

## 项目技能与上下文路由

技能位于 `.agents/skills/`；自动发现不可用时按下列路径读取。使用前说明技能名称与用途，已读且未变化的内容不重复加载。

| 任务 | 技能入口 |
| --- | --- |
| 开发、排错、评审或架构分析的基础上下文 | [project-context-bootstrap](.agents/skills/project-context-bootstrap/SKILL.md) |
| 业务提案、实现、排错、重构、评审及 OpenSpec apply | [business-context-router](.agents/skills/business-context-router/SKILL.md) |
| 生成中文 Git 提交信息 | [git-commit-message](.agents/skills/git-commit-message/SKILL.md) |
| 定位并修复已有缺陷或回归 | [bugfix-delivery](.agents/skills/bugfix-delivery/SKILL.md) |
| 保持行为基线的结构重构 | [refactor-delivery](.agents/skills/refactor-delivery/SKILL.md) |
| 审查 Git 差异或指定补丁 | [git-diff-review](.agents/skills/git-diff-review/SKILL.md) |

业务任务先按能力和层级路由，再读取相关规格、架构章节及直接代码依赖。默认不加载全部主规格、整份架构或历史归档；有依赖、契约冲突或历史证据需要时逐步扩展，并简述本次路由结果。纯文字修正和提交信息任务不强制完整业务路由。

工作流说明见 [AIContextAndCodingWorkflow.md](docs/AIContextAndCodingWorkflow.md)，实现边界见 [FrameworkArchitecture.md](docs/FrameworkArchitecture.md)。提交信息 skill 只生成文字，不自动暂存、提交或推送。

### 1. 工作流底线

- **先谋后动**：每一次编码任务，必须先给出完整实现方案，明确输入 `确认执行` 后再实施编程。
- **拒绝猜测**：若需求、数据或行为存在不确定性，必须先提出问题与假设，等待确认后继续，严禁自行脑补实现。
- **统一中文沟通**：与用户进行回答、说明、提问、确认时，必须统一使用中文；除非用户明确要求使用其他语言。
- **技能使用透明度**：当前任务触发并使用任何 `SKILL` 时，必须在读取或执行技能前，通过会话明确告知用户所使用的技能名称及用途；同时使用多个技能时必须一并列出。禁止仅在内部调用技能而不向用户展示。
- **禁止自行创建测试产物**：除非用户在当前任务中明确要求编写或执行测试，否则禁止为功能验证或回归测试创建临时文件、临时脚本、测试脚本、测试框架、`testspec`、调试探针或其他测试产物；不得以测试完成后再删除为由规避此限制。

### 2.代码质量与规范

- **函数排布可读性**：为保证代码整体阅读性，新增功能时应优先延续现有函数代码块的排布顺序，将新增函数放在对应同类函数代码块后方，避免随意插入打乱整体阅读流。若无特殊原因，应保持初始化相关函数在前，例如先放 `Init`，再按需放置 `_BindEvents()`，随后再放其他私有函数与对外接口；不需要绑定事件的模块不得为了形式统一而保留空 `_BindEvents()`。若确需调整既有函数顺序，必须先说明原因与收益。
- **避免无意义的简单封装**：只有逻辑本身较复杂，且已存在两个及以上实际调用点需要复用时，才允许抽取函数。仅包含少量简单语句的逻辑，即使具有独立业务语义、单点可读性或潜在复用价值，也必须直接写在调用位置；逻辑复杂但当前只使用一次时，默认同样直接展开。抽取函数前必须在实现方案中说明复杂度、实际复用点和抽取收益。为满足对象门面、模块公开接口或生命周期入口等架构契约。
- **常量与枚举注释**：业务代码中新增的常量、配置值、状态枚举、类型枚举等定义必须添加中文注释，说明其业务含义、用途及关键取值语义；自动生成的配置或数据文件不要求手工补充注释；禁止仅保留名称而无注释说明。
- **OpenSpec 中文输出**：所有 OpenSpec `/opsx` 相关交互、进度播报、状态摘要、归档和应用提示必须使用简体中文；命令名、文件路径、`schema id`、`artifact id`、`change name` 等标识保留原文。
- **自动生成文件禁止修改**：编写代码时严禁修改自动生成的配置或数据文件，包括打表、导出或其他生成流程产物；如需变更内容，必须修改对应的源数据或生成流程后重新生成，禁止直接编辑产物文件。

### 3. 文件与编码安全

- **编码格式**：文档等文件必须保持 **UTF-8 无 BOM** 格式。

### 4. 注释与类型

- **有效注释**：所有注释使用中文。对外暴露或长期保留的函数必须有完整中文注释。对于 **复杂逻辑或算法**：
  - 必须对关键步骤逐行或分段添加注释
  - 注释应解释「为什么这么做」，而不仅是「做了什么」
- **类型安全**：模块对外函数、公共数据结构、回调参数必须添加必要的类型注解或结构说明。复杂对象结构必须显式说明字段语义。非必要不得使用弱类型；必须使用时要说明理由。

## 项目与构建环境

ExShrinkSidebar 是面向 Windows 的 .NET 8 / WPF 桌面侧边栏，使用 Windows Forms 托盘、Win32 和 SHDocVw COM 互操作。解决方案为 `ExShrinkSidebar.sln`，项目为 `ExShrinkSidebar.csproj`，目标框架为 `net8.0-windows`。

### 常用命令

在仓库根目录、Visual Studio Developer PowerShell 中执行，使用 Visual Studio 或 Build Tools 提供的完整 .NET Framework 版 MSBuild，并安装 .NET 桌面开发组件、.NET 8 SDK 及对应 Windows SDK：

```powershell
# 常规代码、XAML、资源修改：还原并构建 Debug
msbuild ExShrinkSidebar.sln /restore /p:Configuration=Debug

# 仅在 Release 或发布相关修改时增加
msbuild ExShrinkSidebar.sln /restore /p:Configuration=Release

# 规格相关修改
openspec validate --all --strict --no-interactive

# 所有修改结束时检查差异格式
git diff --check
```

若 `msbuild` 不在 PATH，先通过 Visual Studio 安装信息或 `vswhere` 定位完整 MSBuild，以实际绝对路径调用；不要猜测安装路径。无法找到所需工具时报告阻碍。

本项目包含 COMReference，`dotnet build` 已出现 `MSB4803: ResolveComReference` 不受支持的错误，因此不作为默认构建命令；`dotnet run` 默认也会触发同一构建问题。启动验证应运行本次成功构建实际生成的 EXE，并核对路径和时间，避免误测旧产物。不要通过删除 COM 引用或平台功能绕过构建失败。

## AI 验证流程

当前没有独立测试项目或稳定的自动化测试套件。默认允许执行现有构建、规格校验和相关功能冒烟验证；构建正常生成的 `bin/`、`obj/` 不属于禁止新增的测试产物。新增临时测试文件、脚本、框架、testspec 或调试探针仍遵循第 1 节的授权规则。

1. **确定范围**：根据实际改动选择下表中的验证项，先说明需要验证的行为和预期结果，不要求每次运行完整发布流程。
2. **静态与构建检查**：检查差异、资源引用和配置兼容性；代码、XAML、资源修改执行 Debug 构建。构建失败先判断是本次修改还是环境问题，保留命令与错误信息。
3. **运行验证**：构建成功后，在 Windows 上使用可用的界面操作工具或人工配合执行相关冒烟场景。记录操作、预期、实际结果；只启动进程不能算功能验证通过。
4. **失败处理**：修复本次修改造成的问题后重跑受影响检查；与本次修改无关的问题或环境阻碍如实报告，不扩大修改范围以强行通过。
5. **收尾报告**：分别列出已通过、失败、未执行的检查及原因。无法操作界面、缺少显示器/DPI 环境或工具时列出待人工验证场景；不得把构建成功或源码阅读写成运行时测试通过。

| 修改范围 | 必要验证 |
| --- | --- |
| 仅文档 | 差异、链接与命令一致性检查，无需构建应用 |
| OpenSpec 规格或配置 | 文档检查及 OpenSpec 严格校验，不以格式校验代替行为验收 |
| C#、XAML、资源 | Debug 构建及改动涉及的功能冒烟；检查资源 URI 和输出文件 |
| 配置读写、模型、编辑器 | 旧字段兼容、空值规范化、保存后 Dock 刷新、关闭不保存不影响正式配置 |
| 动作链 | 嵌套组合顺序、空目标与失败路径；区分调用顺序与外部进程完成顺序 |
| 停靠、窗口、托盘 | 按影响范围检查四边显隐、横纵布局、沿边拖动与恢复、显示器切换、DPI、置顶和相关菜单 |
| Explorer 窗口放置 | 打开目录、启用/禁用放置、角落与尺寸；记录尽力重排失败的实际情况 |
| 发布属性、打包、运行时资源 | Release 发布、检查产物及资源，从发布目录启动验证 |

运行配置与动作测试前，确认不会覆盖用户真实配置或执行用户配置中的未知外部命令；不得直接用损坏 JSON 测试覆盖现有用户配置。需要隔离数据或新增测试产物但尚无授权时，说明所需条件，将相关验证标为待执行。只清理本次验证启动的进程，不终止用户原有实例。不要提交 `bin/`、`obj/`、`.vs/`、发布包、个人配置或 IDE 用户文件。

### 发布验证

发布采用 win-x64 单文件、自包含模式，以 `.github/workflows/publish-windows.yml` 为准。本地验证仅执行构建/发布和启动检查，不触发上传或 GitHub Release。

```powershell
msbuild ExShrinkSidebar.sln /restore /t:Publish `
  /p:Configuration=Release `
  /p:RuntimeIdentifier=win-x64 `
  /p:SelfContained=true `
  /p:PublishSingleFile=true `
  /p:PublishTrimmed=false `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  /p:IncludeAllContentForSelfExtract=true
```

GitHub Actions 的发布目录为 `bin/Release/net8.0-windows/win-x64/publish/`，打包文件为 `artifacts/ExShrinkSidebar-Windows.zip`。`Properties/PublishProfiles/FolderProfile.pubxml` 当前指定的目录另有一层 `win-x64/`；使用发布配置文件时应检查实际输出，不能混用两种路径。不要随意更改发布属性，尤其是 `PublishSingleFile`、`SelfContained` 和原生库自解压选项；相关修改必须同时验证发布产物。

## OpenSpec 主规格维护

- 开始功能工作前阅读 [主规格索引](openspec/README.md) 和受影响的 `openspec/specs/*/spec.md`。
- `openspec/specs/` 保存已实现行为；`openspec/changes/` 保存实施中的变更。新增能力、行为或配置兼容性变更，先记录提案、增量规格、任务，必要时补设计。
- 完成实现与适用验证后，同步主规格并归档；默认使用 `openspec archive <change-name> --yes`。只有核对主规格已手动同步后，才使用 `--skip-specs`，避免重复合并。
- 恢复既定行为的修复或文档更正可以直接维护相应主规格，不创建空提案；未实现的计划不能提前写为已实现行为。
- 涉及规格时必须运行 `openspec validate --all --strict --no-interactive`。不能只更新提案、任务或修改记录而遗漏主规格。
- 同步维护 README、架构说明和已知限制；主规格与代码不一致时核对实际实现，并明确属于实现缺陷还是文档漂移。
## 目录与职责

- `App.xaml`、`App.xaml.cs`：应用启动、退出、托盘图标和初始语言选择。
- `Script/Core/`：停靠状态与窗口控制、全局 Hook、配置读写、动作执行、事件系统。
- `Script/Model/`：配置数据模型和枚举。配置结构变更应优先在这里定义。
- `Script/Utils/`：屏幕、资源路径、资源管理器窗口等平台辅助能力。
- `Script/Localization/`、`Asset/Properties/`：本地化辅助类、`.resx` 资源和生成的资源访问器。
- `UI/Views/`：WPF 视图及其 code-behind；`UI/ViewModels/`：可复用窗口基类与 ViewModel。
- `Asset/Resource/ButtonIcons/`：运行时复制到输出目录的按钮图标。
- `Asset/Resource/Icons/`、`Asset/Texture/`：WPF 打包资源。

## 关键设计约束

### 配置与动作链

- 持久化配置的实际位置是 `%APPDATA%/ExShrinkBar/Config/config.json`，由 `ResHelper.GetButtonConfigPath()` 决定；它不是仓库内的示例文件。不要把用户配置、测试产生的配置或个人路径提交到 Git。
- `ConfigManager` 负责加载、规范化、克隆和保存配置。读写或模型迁移改动必须保持 `NormalizeConfig` 对空集合、空参数、父节点引用和旧 `script` 字段到 `Execute.arg.path` 的兼容处理。
- `ButtonConfig` 是树结构：`Combine` 节点通过 `logicChain` 包含子节点；其他节点为可执行叶子。新增节点时使用 `ConfigManager.ReserveNextId()` 分配整个树内唯一 ID。
- 编辑器操作的是配置工作副本；只有保存时才调用 `UpdateAndSave` 写入正式配置，并通过 `EventIds.ON_CONFIG_SAVED` 通知主侧边栏刷新。不要在编辑过程中直接修改 `ConfigManager.Instance.Config`。
- `BtnCfgLogicRunner` 会先展平嵌套 `Combine`，再按顺序执行叶节点。新增动作类型时需同步更新枚举、执行分支、编辑器显示、摘要/本地化及默认图标映射。

### 停靠与 UI

- `DockManager` 和 `DockState` 共同维护靠边、滑入滑出及屏幕位置。修改停靠行为时同时检查四个边缘、横纵布局、DPI 坐标转换以及拖拽暂停/恢复逻辑。
- 现有编辑器包含一定量 code-behind；小范围 UI 交互可沿用此模式，跨视图状态或可复用状态应优先放入对应 ViewModel/核心模块，避免把业务规则堆进 XAML code-behind。
- WPF 资源的 Build Action 和运行时文件的 CopyToOutputDirectory 都在 `.csproj` 中显式声明。新增图标时，按其使用方式选择 `Resource` 或 `Content`，并验证发布目录中需要的运行时文件仍存在。

### 本地化与文本编码

- 新增面向用户的文本时，优先在 `Asset/Properties/StringResources.resx` 和 `StringResources.en-US.resx` 同时添加 key，再经 `UiTextCatalog` 或本地化辅助方法访问；不要在业务逻辑中散落硬编码文本。
- 枚举显示名通过 `Description` 和资源管理器解析；新增 `ExConfigType` 的显示文本也必须补充资源 key。
- 所有源码、XAML、Markdown、YAML 和 `.resx` 文件使用 UTF-8。修改包含中文内容的旧文件时，先确认读取/写入编码，避免将乱码或 BOM 差异混入无关改动。
- `StringResources.Designer.cs` 是由 `.resx` 生成的文件；必须修改资源源文件后重新生成，不得手工修改生成产物。

## 提交与改动范围

- 使用小而聚焦的提交，提交信息可沿用 `feat:`、`fix:`、`chore:` 前缀。
- 不改写或丢弃与当前任务无关的用户改动。
- 修改架构、用户可见行为或已知限制时，同步维护 `README.md` 或 `docs/` 下相应文档。
