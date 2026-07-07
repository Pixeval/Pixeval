Pixeval 可以把正在运行的桌面端暴露为一个本机 MCP 服务器，让支持 MCP 的 AI 工具读取 Pixeval 当前账号、搜索 Pixiv、查看作品/用户/评论/历史、理解下载宏和筛选语句，并在你明确允许后执行收藏、评论、关注、加入下载队列等操作。

Pixeval 的 MCP 是“GUI 进程内”的 Streamable HTTP MCP。也就是说，Pixeval 本体就是 MCP 服务端，AI 工具是 MCP 客户端；你不需要再启动一个额外的 exe，也不需要配置环境变量。

## 可以做什么

只读能力通常包括：

- 查看 Pixeval 当前 MCP 状态、当前登录账号和能力开关。
- 搜索插画、漫画、小说、用户、排行榜、收藏、标签和趋势标签。
- 读取作品、用户、小说正文、评论、评论回复、下载任务、浏览历史、搜索历史、下载历史、稍后观看和订阅历史。
- 读取扩展状态、扩展类型统计和扩展设置项结构，但不会返回扩展设置值。
- 分析下载宏、预览下载路径、分析筛选语句、用 Pixeval 的筛选语句过滤作品。
- 使用 Pixeval 配置的 SauceNAO API key 做以图搜图。
- 通过 MCP 资源把作品、小说、用户和缩略图返回给客户端。缩略图等二进制资源由 Pixeval MCP 服务端发送给 AI 客户端，大小受设置中的上限限制。

写入能力默认关闭。开启后，AI 工具可以请求 Pixeval 执行：

- 修改下载宏。
- 添加或删除评论。
- 添加或取消收藏、稍后观看、关注用户。
- 把作品加入 Pixeval 下载队列、控制下载任务。
- 添加、删除和同步作品订阅。

## 启用 MCP

进入 Pixeval 的“设置”，找到“MCP 设置”分组：

- “启用 MCP 服务器”：打开后，Pixeval 会在桌面端启动本机 MCP 服务。
- “MCP 端口”：默认是 `52163`。端口改动会重启 MCP 服务，已连接的客户端需要使用新地址重新连接。
- “启用 MCP 写工具”：默认关闭。开启后，已连接的 AI 工具才能请求评论、收藏、下载、关注等写操作。
- “MCP 二进制资源大小上限（MB）”：限制 Pixeval MCP 服务端返回给客户端的缩略图等二进制资源大小。超过上限时，MCP 会返回清晰错误，而不是继续传输大文件。

MCP 只在桌面端运行。Android、iOS 和浏览器版本不会启动 MCP 服务。

## 连接地址

默认连接地址是：

```text
http://127.0.0.1:52163/mcp
```

如果你修改了端口，请把 `52163` 换成你设置的端口。

Pixeval 只监听 `127.0.0.1`（本机回环地址）。这样同一台电脑上的 AI 工具可以连接 Pixeval，但局域网和公网设备不能直接访问。

连接前请确认：

- Pixeval 正在运行。
- 设置中已经启用 MCP 服务器。
- AI 工具和 Pixeval 在同一台电脑上运行。
- 端口没有被其他程序占用。
- 需要调用 Pixiv API 的工具通常要求 Pixeval 已登录。

## 配置 AI 工具

不同 AI 工具的界面名称和配置文件位置不同，但只要它支持 Streamable HTTP MCP 或 Remote MCP，核心配置都是同一个 URL：

```text
http://127.0.0.1:52163/mcp
```

如果工具提供图形化界面，一般选择添加 MCP 服务器，类型选择 `HTTP`、`Streamable HTTP` 或 `Remote`，名称可以填 `Pixeval`，URL 填上面的地址。

不同工具的配置文件格式可能不同，你主要可以参考其他已成功配置的 MCP 服务。下面给出几种常见的写法示例。

如果工具使用类似 `mcp.json` 的配置文件，并且支持 HTTP MCP，可以参考下面的写法：

```json
{
  "servers": {
    "pixeval": {
      "type": "http",
      "url": "http://127.0.0.1:52163/mcp"
    }
  }
}
```

有些工具使用的键名可能是 `mcpServers`：

```json
{
  "mcpServers": {
    "pixeval": {
      "type": "http",
      "url": "http://127.0.0.1:52163/mcp"
    }
  }
}
```

如果工具文档说明它的 TOML MCP 配置支持 HTTP URL，可以参考这种形式：

```toml
[mcp_servers.pixeval]
type = "http"
url = "http://127.0.0.1:52163/mcp"
```

如果某个 AI 工具只支持 `stdio` MCP，也就是只能启动一个命令行程序作为 MCP 服务器，那么它不能直接连接 Pixeval 的内置 HTTP MCP。Pixeval 不提供单独的 stdio exe；这种情况下需要等待该工具支持 HTTP MCP，或者使用你自己信任的 HTTP 到 stdio 桥接工具。

## 常见客户端提示

### VS Code、Cursor 等编辑器

这类工具通常支持在用户设置、工作区设置或项目配置中添加 MCP 服务器。选择 HTTP 类型，URL 填：

```text
http://127.0.0.1:52163/mcp
```

如果配置文件要求 JSON，优先使用工具文档推荐的键名；Pixeval 侧不需要 token、命令行参数或环境变量。

### Claude、ChatGPT、Codex 等 AI 工具

如果工具支持添加远程 MCP 或 HTTP MCP 服务器，并且客户端运行在你的电脑上，名称填 `Pixeval`，URL 填 Pixeval 的 MCP 地址即可。

如果工具当前只接受命令行 stdio MCP 配置，例如只让你填写 `command` 和 `args`，则不能直接配置 Pixeval 内置 MCP。

如果工具运行在云端，例如网页版产品的云端连接器，它通常不能访问你电脑上的 `127.0.0.1`。这种情况下即使填写 Pixeval 的本机地址，也无法连到正在运行的 Pixeval。

### 本地脚本或调试工具

调试时可以直接向 `http://127.0.0.1:52163/mcp` 发送 MCP JSON-RPC 请求。成功连接后，先调用 `tools/list` 查看当前可用工具，再调用 `help`、`status` 或 `capabilities` 了解 Pixeval 当前能力。

## 权限与安全

Pixeval MCP 的权限跟随当前正在运行的 Pixeval：

- MCP 使用 Pixeval 当前登录账号和当前网络设置。
- MCP 不会向客户端返回 refresh token、cookie、代理地址、扩展设置值等敏感信息。
- 写工具默认关闭。开启后，AI 工具可以代表你对 Pixiv 或 Pixeval 本地数据执行写操作，请只连接你信任的客户端。
- Pixeval 只监听本机地址，不暴露到局域网或公网。
- 缩略图等二进制资源会由 Pixeval 服务端返回给客户端，大小受“二进制资源大小上限（MB）”限制。

## 让 AI 更好地使用 Pixeval

连接成功后，可以让 AI 先调用：

- `status`：确认 Pixeval 是否登录、当前账号是谁。
- `capabilities`：查看写工具和二进制资源等能力是否启用。
- `settings_summary`：读取脱敏后的 Pixeval 设置概览。
- `help`：读取 Pixeval 已有帮助文档。

如果 AI 需要处理下载路径，请让它先调用：

```text
help(topic: "download_macro")
```

如果 AI 需要写筛选语句，请让它先调用：

```text
help(topic: "work_filter")
```

如果 AI 需要理解扩展系统，请让它先调用：

```text
help(topic: "extensions")
```

## 常见问题

### AI 工具连不上 Pixeval

请检查 Pixeval 是否正在运行、MCP 是否已启用、端口是否正确，以及 AI 工具是否真的支持 HTTP MCP。只支持 stdio 的工具不能直接连接 Pixeval。
还可以尝试先打开 Pixeval 后再启动 AI 工具。

### 能看到工具列表，但搜索或读取 Pixiv 失败

请确认 Pixeval 已登录，并且 Pixeval 本身可以正常访问 Pixiv。MCP 会复用 Pixeval 当前账号和网络设置。

### 为什么写操作失败

默认不会允许 AI 工具执行写操作。请在“MCP 设置”里开启“启用 MCP 写工具”。开启后仍可能因为 Pixiv 权限、作品状态、评论长度、网络错误等原因失败。

### 为什么缩略图资源读取失败

可能是作品不存在、Pixeval 未登录、网络请求失败，或者图片大小超过了“MCP 二进制资源大小上限（MB）”。这个上限控制的是 Pixeval MCP 服务端返回给 AI 客户端的数据大小。

### 修改端口后为什么客户端断开了

端口改变时，Pixeval 会停止旧端口上的 MCP 服务，并在新端口重新启动。已经连接到旧地址的客户端需要改用新的 `http://127.0.0.1:端口/mcp` 地址重新连接。
