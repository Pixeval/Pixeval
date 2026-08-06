Pixeval can expose the running desktop app as a local MCP server, allowing MCP-capable AI tools to read the current Pixeval account, search Pixiv, view works/users/comments/history, understand download macros and filter queries, and — after your explicit approval — perform actions such as bookmarking, commenting, following, and adding items to the download queue.

Pixeval's MCP is an "in-GUI-process" Streamable HTTP MCP.Pixeval's MCP is an "in-GUI-process" Streamable HTTP MCP.Pixeval's MCP is an "in-GUI-process" Streamable HTTP MCP. In other words, Pixeval itself is the MCP server and the AI tool is the MCP client; you don't need to launch an additional exe or configure environment variables.

## What You Can Do

Read-only capabilities generally include:

- Viewing Pixeval's current MCP status, the currently logged-in account, and capability toggles.
- Searching illustrations, manga, novels, users, rankings, bookmarks, tags, and trending tags.
- Reading works, users, novel content, comments, comment replies, download tasks, browsing history, search history, download history, Watch Later, and subscription history.
- Reading extension status, extension type statistics, and extension settings structure, but not returning extension setting values.
- Analyzing download macros, previewing download paths, analyzing filter queries, and filtering works with Pixeval's filter language.
- Using the SauceNAO API key configured in Pixeval for reverse image search.
- Returning works, novels, users, and thumbnails to the client as MCP resources.Returning works, novels, users, and thumbnails to the client as MCP resources. Binary resources such as thumbnails are sent by the Pixeval MCP server to the AI client, with size limited by the cap set in Settings.

Write capabilities are disabled by default.Write capabilities are disabled by default. Once enabled, AI tools can request Pixeval to perform:

- Modifying download macros.
- Adding or deleting comments.
- Adding or removing bookmarks, Watch Later, and following users.
- Adding works to the Pixeval download queue and controlling download tasks.
- Adding, deleting, and syncing work subscriptions.

## Enabling MCP

Go to "Settings" in Pixeval and find the "MCP Settings" group:

- "Enable MCP Server": when enabled, Pixeval starts the local MCP service in the desktop app.
- "MCP Port": default is `52163`."MCP Port": default is `52163`. Changing the port restarts the MCP service, and connected clients must reconnect using the new address.
- "Enable MCP write tool": Disabled by default.Once enabled, connected AI tools can request comments, collections, downloads, follows and tool calls that need write access.
- "MCP Binary Resource Size Limit (MB)": limits the size of binary resources, such as thumbnails, that the Pixeval MCP server returns to clients. When the limit is exceeded, MCP returns a clear error instead of continuing to transfer the large file.When the limit is exceeded, MCP returns a clear error instead of continuing to transfer the large file.When the limit is exceeded, MCP returns a clear error instead of continuing to transfer the large file.When the limit is exceeded, MCP returns a clear error instead of continuing to transfer the large file.

MCP only runs on the desktop.MCP only runs on the desktop.MCP only runs on the desktop. Android, iOS, and browser versions do not start the MCP service.

## Connection Address

The default connection address is:

```text
http://127.0.0.1:52163/mcp
```

If you changed the port, replace `52163` with the port you set.

Pixeval only listens on `127.0.0.1` (the local loopback address).Pixeval only listens on `127.0.0.1` (the local loopback address).Pixeval only listens on `127.0.0.1` (the local loopback address). This way, AI tools on the same computer can connect to Pixeval, but devices on the LAN or public internet cannot access it directly.

> [!INFO]
> Before connecting, make sure:
>
> - Pixeval is running.
> - The MCP server is enabled in Settings.
> - The AI tool and Pixeval are running on the same computer.
> - The port is not occupied by another program.
> - Tools that need to call the Pixiv API generally require you to be logged in to Pixeval.

## Configuring AI Tools

Different AI tools have different UI names and config file locations, but as long as they support Streamable HTTP MCP or Remote MCP, the core configuration is the same URL:

```text
http://127.0.0.1:52163/mcp
```

If the tool has a GUI, generally choose "Add MCP server", set the type to `HTTP`, `Streamable HTTP`, or `Remote`, name it `Pixeval`, and fill in the URL above.

Different tools use different config file formats; you can mainly reference other MCP services you have configured successfully. Below are several common examples.Below are several common examples.Below are several common examples.Below are several common examples.

If the tool uses a config file similar to `mcp.json` and supports HTTP MCP, you can reference the following:

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

Some tools may use the key name `mcpServers`:

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

If the tool's documentation says its TOML MCP config supports HTTP URLs, you can reference this form:

```toml
[mcp_servers.pixeval]
type = "http"
url = "http://127.0.0.1:52163/mcp"
```

If an AI tool only supports `stdio` MCP, meaning it can only launch a command-line program as the MCP server, it cannot directly connect to Pixeval's built-in HTTP MCP. Pixeval does not provide a separate stdio exe; in this case, you need to wait for the tool to support HTTP MCP, or use a trusted HTTP-to-stdio bridge tool.Pixeval does not provide a separate stdio exe; in this case, you need to wait for the tool to support HTTP MCP, or use a trusted HTTP-to-stdio bridge tool.Pixeval does not provide a separate stdio exe; in this case, you need to wait for the tool to support HTTP MCP, or use a trusted HTTP-to-stdio bridge tool.Pixeval does not provide a separate stdio exe; in this case, you need to wait for the tool to support HTTP MCP, or use a trusted HTTP-to-stdio bridge tool.

## Client Tips

### Editors such as VS Code and Cursor

These tools usually support adding MCP servers in user settings, workspace settings, or project configuration. Choose the HTTP type and fill in the URL:Choose the HTTP type and fill in the URL:Choose the HTTP type and fill in the URL:Choose the HTTP type and fill in the URL:

```text
http://127.0.0.1:52163/mcp
```

If the config file requires JSON, prefer the key names recommended by the tool's documentation; the Pixeval side does not need tokens, command-line arguments, or environment variables.

### AI tools such as Claude, ChatGPT, and Codex

If the tool supports adding a Remote MCP or HTTP MCP server and the client runs on your computer, set the name to `Pixeval` and fill in Pixeval's MCP address.

If the tool only accepts command-line stdio MCP config — for example, it only lets you fill in `command` and `args` — you cannot directly configure Pixeval's built-in MCP.

If the tool runs in the cloud, such as the cloud connector of a web product, it usually cannot access `127.0.0.1` on your computer. In that case, even if you fill in Pixeval's local address, it cannot connect to the running Pixeval.In that case, even if you fill in Pixeval's local address, it cannot connect to the running Pixeval.In that case, even if you fill in Pixeval's local address, it cannot connect to the running Pixeval.

### Local scripts or debugging tools

You can directly send MCP JSON-RPC requests to `http://127.0.0.1:52163/mcp` when debugging.You can directly send MCP JSON-RPC requests to `http://127.0.0.1:52163/mcp` when debugging.After connecting successfully, first call `tools/list` to view avaliable tools, then call `help`, `status` or `capabilities` to learn about Pixeval's current abilities.You can directly send MCP JSON-RPC requests to `http://127.0.0.1:52163/mcp` when debugging.After connecting successfully, first call `tools/list` to view avaliable tools, then call `help`, `status` or `capabilities` to learn about Pixeval's current abilities.

## Permissions and Security

Pixeval MCP's permissions follow the currently running Pixeval:

- MCP uses the account currently logged in to Pixeval and its current network settings.
- MCP does not return sensitive information to clients, such as refresh tokens, cookies, proxy addresses, or extension setting values.
- Write tools are disabled by default.Write tools are disabled by default.Write tools are disabled by default.Write tools are disabled by default. When enabled, AI tools can perform write operations on Pixiv or Pixeval's local data on your behalf; only connect clients you trust.
- Pixeval only listens on the local address and does not expose itself to the LAN or public internet.
- Binary resources such as thumbnails are returned by the Pixeval server to clients, limited by the "Binary Resource Size Limit (MB)".

## Helping AI Use Pixeval Better

After connecting successfully, you can have the AI first call:

- `status`: confirm whether Pixeval is logged in and which account is current.
- `capabilities`: check whether write tools and binary resources are enabled.
- `settings_summary`: read a sanitized overview of Pixeval's settings.
- `help`: read Pixeval's existing help documents.

If the AI needs to handle download paths, have it first call:

```text
help(topic: "download_macro")
```

If the AI needs to write filter queries, have it first call:

```text
help(topic: "work_filter")
```

If the AI needs to understand the extension system, have it first call:

```text
help(topic: "extensions")
```

## FAQ

### The AI tool cannot connect to Pixeval

Check whether Pixeval is running, MCP is enabled, the port is correct, and the AI tool actually supports HTTP MCP. Tools that only support stdio cannot directly connect to Pixeval.
You can also try starting Pixeval before launching the AI tool.Tools that only support stdio cannot directly connect to Pixeval.
You can also try starting Pixeval before launching the AI tool.Tools that only support stdio cannot directly connect to Pixeval.
You can also try starting Pixeval before launching the AI tool.Tools that only support stdio cannot directly connect to Pixeval.
You can also try starting Pixeval before launching the AI tool.

### I can see the tool list, but searching or reading from Pixiv fails

Make sure Pixeval is logged in and that Pixeval itself can access Pixiv normally. MCP reuses Pixeval's current account and network settings.MCP reuses Pixeval's current account and network settings.

### Why do write operations fail

Write operations are not allowed for AI tools by default.Write operations are not allowed for AI tools by default. Enable "Enable MCP Write Tools" in the "MCP Settings". Even when enabled, failures can still occur due to Pixiv permissions, work status, comment length, network errors, and other reasons.Even when enabled, failures can still occur due to Pixiv permissions, work status, comment length, network errors, and other reasons.

### Why does reading thumbnail resources fail

The work may not exist, Pixeval may not be logged in, the network request may have failed, or the image size may exceed the "MCP Binary Resource Size Limit (MB)". This limit controls the size of data the Pixeval MCP server returns to the AI client.This limit controls the size of data the Pixeval MCP server returns to the AI client.This limit controls the size of data the Pixeval MCP server returns to the AI client.This limit controls the size of data the Pixeval MCP server returns to the AI client.

### Why did the client disconnect after I changed the port

When the port changes, Pixeval stops the MCP service on the old port and restarts it on the new port. Clients already connected to the old address need to reconnect using the new `http://127.0.0.1:port/mcp` address.Clients already connected to the old address need to reconnect using the new `http://127.0.0.1:port/mcp` address.
