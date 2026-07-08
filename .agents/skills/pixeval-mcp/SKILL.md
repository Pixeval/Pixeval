---
name: pixeval-mcp
description: Pixeval MCP 维护指南。仅当修订 MCP 功能相关时需要读取并遵守。
---

# Pixeval MCP

先遵守项目的 `tools`、`code-style`、`format-style` skill；C# 语义查询、格式化、构建优先用 Rider MCP。

## 架构边界

- MCP 是 Pixeval GUI 进程内的 Streamable HTTP 服务，不发布独立 exe，不使用环境变量，监听固定为 `127.0.0.1:{port}/mcp`。
- `src/Pixeval.Mcp` 放协议层：工具、资源、DTO、JSON source generation、cursor store、`IPixevalMcpRuntime`。它不能引用主项目。
- 主项目实现运行时能力，位置在 `src/Pixeval/Models/McpServer`。需要 Pixeval 设置、下载、历史、扩展、缓存、HTTP client、日志时，通过 `IPixevalMcpRuntime` 暴露。
- MCP 仅 Desktop 启动；不要为了 Browser/WASM 改 MCP 代码，除非用户明确要求。
- 工具名不要加 `pixeval_` 共同前缀，不留兼容别名，不保留旧方法名。

## 工具设计

- 优先返回专用 DTO，不直接把 Mako model 暴露为 MCP 输出。DTO 的转换方法放在对应 DTO 类里，例如 `FromWork`、`FromRuntime`。
- 工具参数能用枚举就用枚举，优先使用 Mako 里的枚举或已有 Pixeval MCP 枚举；不要用 string 再手写 parser。STJ enum 输出以 `snake_case`/camelCase 能支持的命名为准，不额外维护宽松解析表。
- `count` 表示本次最多返回多少条，默认 20，使用 `PixevalMcpHelpers.ClampCount` 限制在 `1..100`。不要再引入 `limit`。
- 远程连续列表必须用 cursor：首个工具返回 `hasMore`/`nextCursor`，继续读取统一走 `more(cursor, count)`，不要让 AI 重复传原查询条件。
- Cursor token 是 Pixeval 本地状态，形如 `pixeval:{kind}:{engineGuid:N}`；不要使用 Pixiv 官方分页语义。
- 列表工具如果支持 `workFilter`，筛选表达式错误时返回带 diagnostics 的结构化结果，不要静默返回空列表。
- 看起来轻量的工具不要隐式做大量远程详情请求。批量 `works`/`users` 这种显式 get-by-id 例外；其他工具若会下载原图、读取正文、批量加载 Pixiv 命中，必须通过清晰参数或工具名表达。

## 缓存约定

- 所有拿到 `WorkBase`、`User`、`UserBasicInfo` 的列表/cursor 路径，应尽量调用 runtime cache：`CacheWorks`、`CacheUsers`、`CacheUserInfos`。
- 后续按 id 操作必须优先走 runtime getter：`GetWorkAsync`、`GetIllustrationAsync`、`GetNovelAsync`、`GetUserAsync`、`GetUserBasicInfoAsync`。不要直接调用 Mako get-by-id 绕过缓存。
- 写操作需要按 id 使用作品/用户时，也先用 runtime getter；例如下载、稍后看、关注、订阅。收藏可直接调用 Mako bookmark API，不要为了收藏先拉作品详情。
- 二进制资源和正文读取本身可能很重；可以缓存元数据，但不要把大二进制内容塞进普通模型 cache，除非用户明确要求并设计容量上限。

## 权限与设置

- 写工具默认由 Pixeval MCP 设置控制，调用前必须 `EnsureWriteToolsEnabled`。
- 只读工具可以读取 Pixeval 本地状态，但不要暴露登录 token、cookie、代理地址、SauceNAO key、扩展设置值或不必要的绝对本机路径。
- `settings_summary` 只能返回脱敏摘要；新增设置摘要字段时优先在 DTO/扩展方法里集中转换。
- 端口和启用状态应能通过 dispose/restart 内部 server 动态生效；不要要求重启 Pixeval，除非底层生命周期无法避免。

## JSON 与文档

- 新 DTO、枚举、工具返回类型要加入 `PixevalMcpJsonContext`，保持 AOT/trim 友好。
- 工具返回保持 `TextContentBlock` JSON + `StructuredContent` 模式。
- 工具参数描述不要重复枚举可选值；schema 已经会暴露 enum。
- HelpPage 的工具列表应从 MCP `tools/list` 用 STJ DTO 解析，不要手写字符串解析。
- 涉及下载宏、筛选语句、扩展时，工具描述中提醒 `help` 可查看现有文档；不要在工具描述里复制完整语法。

## 验证

- 修改 MCP 后至少构建 `src/Pixeval.Mcp/Pixeval.Mcp.csproj` 和 `src/Pixeval/Pixeval.csproj`。
- 检查工具 schema 时确认没有旧工具名、手写 enum parser、未加入 source-gen 的 DTO。
