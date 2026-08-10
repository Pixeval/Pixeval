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

## 退回普通工具的情况

- 文件在 `.agents`、`.codex`、生成目录、子模块或其他 Rider 未索引位置，MCP 搜不到。
- 需要编辑 Skill、Markdown、脚本模板、资产文件等非解决方案代码。
- 需要 git 状态、diff、提交、分支等版本控制操作。
- MCP 返回结果不足、超时或工具不存在。此时先说明原因，再用 `rg`、`PowerShell`、`apply_patch` 或其他合适工具补上。
