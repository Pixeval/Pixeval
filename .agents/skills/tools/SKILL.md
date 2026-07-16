---
name: tools
description: 代码工具选择
---

# Tools

在处理代码任务前，先检查环境中是否有相关 MCP 工具，尤其是 Rider、ReSharper、Visual Studio 等 IDE MCP。若可用，优先使用 MCP 的项目索引、符号搜索、语义理解、重构、格式化、构建、运行和诊断能力，而不是手动搓搜索脚本、解析代码或做全局字符串替换。修改完 XAML 文件后尝试用 xstyler（即 XAML-Styler）来格式化。

对于 T4 模板文件，检查环境中是否有相应工具（如 dotnet-t4），若有则使用。

## 总原则

- 先用 `tool_search` 查找当前可用 MCP；发现 `mcp__rider` 时，把它作为 C#/.NET、Avalonia、Rider 解决方案内代码任务的首选工具。
- 调用 Rider MCP 时尽量传入 `projectPath`，这能减少多项目或多窗口时的歧义。
- 使用 MCP 做它擅长的事：按符号搜索、获取类型/签名/文档、IDE 重构、项目内搜索、问题诊断、格式化、构建、运行配置。
- 只在 MCP 不覆盖、文件不在 IDE 项目索引中、需要处理非代码资源或需要精确补丁审阅时，退回 `rg`、`PowerShell`、`apply_patch` 等普通工具。
- 不要用裸文本替换模拟语义重构。重命名符号、移动命名空间、格式化、构建验证，应优先交给 IDE MCP。

## 当前 Rider MCP 功能

### 项目和文件定位

- `search_file`: 按 glob 查找项目内文件，支持路径过滤和排除规则。需要找 `.cs`、`.axaml`、项目文件、配置文件时优先用它。
- `find_files_by_name_keyword`: 按文件名关键字快速查找，适合只知道文件名片段时使用。它走 IDE 索引，通常比普通文件系统遍历更快。
- `find_files_by_glob`: 按 glob 递归查找文件。适合需要限定子目录、文件数量、超时时间，或需要包含 excluded/ignored 文件时使用。
- `open_file_in_editor`: 需要让用户在 Rider 中看到某个文件时使用。它不是阅读文件的主工具，而是协作展示工具。

### 文本和正则搜索

- `search_text`: 项目内快速文本搜索，返回匹配坐标。适合找字符串、配置键、XAML 属性、日志文本、错误消息。
- `search_regex`: 项目内正则搜索，返回匹配坐标。适合找结构相近但不完全相同的代码片段。
- `search_in_files_by_text`: 使用 IntelliJ 搜索引擎做文本搜索，并返回带 `||` 高亮的上下文。适合需要快速看命中片段时使用。
- `search_in_files_by_regex`: 使用 IntelliJ 搜索引擎做正则搜索，并返回带 `||` 高亮的上下文。适合正则探索和人工判断上下文。

优先级：找符号用 `search_symbol`；找普通文本用 `search_text`；需要高亮片段再用 `search_in_files_by_text`；需要路径 glob 找文件用 `search_file`。

### 符号理解和语义查询

- `search_symbol`: 搜索类、方法、字段等符号。阅读代码前先用它定位入口、类型、ViewModel、服务、扩展方法。查不到项目符号时，再考虑 `include_external=true` 查 SDK 或依赖。
- `get_symbol_info`: 获取指定文件位置的符号信息，相当于 IDE Quick Documentation。需要确认类型、签名、声明、文档、重载或引用目标时使用。
- `run_inspection_kts`: 编译并运行 `inspection.kts` 检查脚本。适合需要自定义静态分析、验证某种代码模式、批量找 IDE PSI 层面的结构问题时使用。

不要通过手写正则推断类型关系、继承关系或 API 签名；先让 IDE MCP 给出语义信息。

### 修改和重构

- `rename_refactoring`: 重命名变量、方法、类、字段等程序符号。它会按 IDE 语义更新引用，是重命名符号的首选，优先于手动全局替换。
- `move_type_to_namespace`: 将 class、struct、interface、enum 等类型移动到目标命名空间，并更新引用。需要调整命名空间时优先用它。
- `replace_text_in_file`: 在已知精确旧文本时做定点替换，并自动保存。适合小范围、确定性的文本修改；涉及语义重构时不要用它替代 `rename_refactoring`。
- 普通 `apply_patch`: 适合修改 skill、文档、非项目文件，或需要可审阅的多行代码补丁。若修改 Rider 项目内代码，改完后继续用 MCP 做格式化和诊断。

### 格式化、诊断和构建

- `reformat_file`: 使用当前解决方案的 IDE 代码风格格式化文件。修改 C#、XAML、项目内代码后优先运行。
- `get_file_problems`: 对单个文件运行 Rider 代码分析，返回错误、严重级别和行列。编辑后先查受影响文件。
- `build_solution`: 构建当前解决方案或指定文件。代码修改完成后用它验证。能提供 `filesToRebuild` 时先做小范围编译；风险较大或跨项目修改后构建整个解决方案。

验证顺序建议：`reformat_file` -> `get_file_problems` -> `build_solution`。如果某步失败，根据 MCP 返回的问题定位修复。

### 运行和终端

- `get_run_configurations`: 不传 `filePath` 时列出项目运行配置；传 `filePath` 时发现文件中的可运行入口，如测试、main 方法或 IDE gutter run point。
- `execute_terminal_command`: 在 IDE 集成终端执行命令。适合必须在 IDE 环境中运行的命令，或需要复用用户终端上下文时使用。普通非交互命令仍可用 shell，但不要绕开已有的运行配置和测试入口。

运行测试或程序前，先用 `get_run_configurations` 找已有配置或文件 run point；不要凭记忆手写复杂启动命令。

## 推荐工作流

1. 发现工具：先 `tool_search` 搜 Rider/ReSharper/Visual Studio MCP。
2. 定位代码：用 `search_symbol`、`search_file`、`search_text` 找入口和引用线索。
3. 理解语义：用 `get_symbol_info` 确认类型、签名和声明位置。
4. 修改代码：语义改动用 `rename_refactoring`、`move_type_to_namespace`；精确文本改动用 `replace_text_in_file` 或 `apply_patch`。
5. 收尾验证：用 `reformat_file`、`get_file_problems`、`build_solution`，必要时用 `get_run_configurations` 和运行配置验证行为。

## 退回普通工具的情况

- 文件在 `.agents`、`.codex`、生成目录、子模块或其他 Rider 未索引位置，MCP 搜不到。
- 需要编辑 Skill、Markdown、脚本模板、资产文件等非解决方案代码。
- 需要 git 状态、diff、提交、分支等版本控制操作。
- MCP 返回结果不足、超时或工具不存在。此时先说明原因，再用 `rg`、`PowerShell`、`apply_patch` 或其他合适工具补上。
