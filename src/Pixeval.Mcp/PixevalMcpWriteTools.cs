// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.ComponentModel;
using Mako.Global.Enum;
using Mako.Model;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Pixeval.Mcp.Dtos;
using static Pixeval.Mcp.PixevalMcpHelpers;

namespace Pixeval.Mcp;

[McpServerToolType]
internal sealed class PixevalMcpWriteTools(IPixevalMcpRuntime runtime)
{
    [McpServerTool(Name = "set_download_macro", Title = "Set Pixeval download macro",
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalSetDownloadMacroResultDto))]
    [Description(
        "Sets Pixeval's current download path macro after validation. Use help for syntax. Requires Pixeval MCP write tools to be enabled.")]
    public CallToolResult SetDownloadMacro(
        [Description("New Pixeval download path macro text.")]
        string text) =>
        Execute(nameof(SetDownloadMacro), () =>
        {
            runtime.EnsureWriteToolsEnabled();
            return PixevalMcpResult.Success(runtime.SetDownloadMacro(text));
        });

    [McpServerTool(Name = "add_comment", Title = "Add Pixiv comment", OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalCommentDto))]
    [Description(
        "Adds a text comment or stamp comment to an illustration/manga or novel. Requires Pixeval MCP write tools to be enabled.")]
    public Task<CallToolResult> AddCommentAsync(
        [Description("Work type.")] SimpleWorkType workType,
        [Description("Pixiv work id.")] long workId,
        [Description("Text content. Use either content or stampId, not both. Text comments must be 1..140 characters.")]
        string? content = null,
        [Description("Pixiv stamp id. Use either content or stampId, not both.")]
        int? stampId = null,
        [Description("Optional top-level parent comment id when replying.")]
        long? parentCommentId = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(AddCommentAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            runtime.EnsureWriteToolsEnabled();

            var hasContent = !string.IsNullOrWhiteSpace(content);
            var hasStamp = stampId is > 0;
            if (hasContent == hasStamp)
                throw new PixevalMcpException("Provide exactly one of content or stampId.");
            if (content is { Length: > 140 })
                throw new PixevalMcpException("Pixiv comments must be 140 characters or less.");

            var comment = await AddCommentCoreAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
            return PixevalMcpResult.Success(PixevalCommentDto.FromComment(comment, runtime.CurrentUser?.Id));

            Task<Comment> AddCommentCoreAsync() =>
                parentCommentId switch
                {
                    { } parent when hasContent => runtime.MakoClient.AddWorkCommentAsync(
                        workType,
                        workId,
                        parent,
                        content!),
                    { } parent => runtime.MakoClient.AddWorkCommentAsync(
                        workType,
                        workId,
                        parent,
                        stampId!.Value),
                    _ when hasContent => runtime.MakoClient.AddWorkCommentAsync(workType, workId, content!),
                    _ => runtime.MakoClient.AddWorkCommentAsync(workType, workId, stampId!.Value)
                };
        });

    [McpServerTool(Name = "delete_comment", Title = "Delete Pixiv comment", OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalOperationResultDto))]
    [Description(
        "Deletes a Pixiv illustration/manga or novel comment. Pixiv decides whether the current account has permission.")]
    public Task<CallToolResult> DeleteCommentAsync(
        [Description("Work type.")] SimpleWorkType workType,
        [Description("Pixiv comment id.")] long commentId,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(DeleteCommentAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            runtime.EnsureWriteToolsEnabled();

            var success = await runtime.MakoClient.DeleteWorkCommentAsync(workType, commentId)
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);
            return PixevalMcpResult.Success(new PixevalOperationResultDto(
                success,
                success ? "Comment deleted." : "Pixiv did not delete the comment."));
        });

    [McpServerTool(Name = "set_bookmark", Title = "Set Pixiv bookmark", OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalBookmarkResultDto))]
    [Description(
        "Adds or removes a Pixiv illustration/manga or novel bookmark. Requires Pixeval MCP write tools to be enabled.")]
    public Task<CallToolResult> SetBookmarkAsync(
        [Description("Work type.")] SimpleWorkType workType,
        [Description("Pixiv work id.")] long id,
        [Description("True to add/update the bookmark, false to remove it.")]
        bool bookmarked,
        [Description("Bookmark privacy when adding.")]
        PrivacyPolicy privacy = PrivacyPolicy.Public,
        [Description("Optional bookmark tags when adding.")]
        IReadOnlyList<string>? tags = null,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(SetBookmarkAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            runtime.EnsureWriteToolsEnabled();
            return PixevalMcpResult.Success(await runtime.SetBookmarkAsync(
                    workType,
                    id,
                    bookmarked,
                    privacy,
                    tags,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "set_watch_later", Title = "Set Pixeval watch later", OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalWatchLaterResultDto))]
    [Description(
        "Adds or removes a work from Pixeval's local watch-later history. Requires Pixeval MCP write tools to be enabled.")]
    public Task<CallToolResult> SetWatchLaterAsync(
        [Description("Work type.")] SimpleWorkType workType,
        [Description("Pixiv work id.")] long id,
        [Description("True to add to watch later, false to remove.")]
        bool watchLater,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(SetWatchLaterAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            runtime.EnsureWriteToolsEnabled();
            return PixevalMcpResult.Success(await runtime.SetWatchLaterAsync(
                    workType,
                    id,
                    watchLater,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "follow_user", Title = "Follow Pixiv user", OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalFollowUserResultDto))]
    [Description("Follows or unfollows a Pixiv user. Requires Pixeval MCP write tools to be enabled.")]
    public Task<CallToolResult> FollowUserAsync(
        [Description("Pixiv user id.")] long userId,
        [Description("True to follow, false to unfollow.")]
        bool followed,
        [Description("Follow privacy when following.")]
        PrivacyPolicy privacy = PrivacyPolicy.Public,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(FollowUserAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            runtime.EnsureWriteToolsEnabled();
            return PixevalMcpResult.Success(await runtime.FollowUserAsync(
                    userId,
                    followed,
                    privacy,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "queue_download", Title = "Queue Pixeval download", OpenWorld = true,
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalMcpDownloadTaskDto))]
    [Description(
        "Queues a work download in Pixeval's existing download manager using Pixeval's configured download path macro. Use help for macro syntax. Requires write tools.")]
    public Task<CallToolResult> QueueDownloadAsync(
        [Description("Work type.")] SimpleWorkType workType,
        [Description("Pixiv work id.")] long id,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(QueueDownloadAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            runtime.EnsureWriteToolsEnabled();
            var task = await runtime.QueueDownloadAsync(
                    workType,
                    id,
                    cancellationToken)
                .ConfigureAwait(false);
            return PixevalMcpResult.Success(task);
        });

    [McpServerTool(Name = "control_download", Title = "Control Pixeval download task",
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalDownloadTaskControlResultDto))]
    [Description("Controls an existing Pixeval download task by queue index or destination.")]
    public CallToolResult ControlDownload(
        [Description("Zero-based queue index from download_tasks.")]
        int? queueIndex = null,
        [Description("Exact destination from download_tasks. Used when queueIndex is empty.")]
        string? destination = null,
        [Description("Download task action.")] PixevalDownloadAction action = PixevalDownloadAction.Pause,
        [Description("When action is remove, also delete local downloaded files.")]
        bool deleteLocalFiles = false) =>
        Execute(nameof(ControlDownload), () =>
        {
            runtime.EnsureWriteToolsEnabled();
            return PixevalMcpResult.Success(runtime.ControlDownload(
                queueIndex,
                destination,
                action,
                deleteLocalFiles));
        });

    [McpServerTool(Name = "add_subscription", Title = "Add Pixeval work subscription",
        OpenWorld = true, UseStructuredContent = true,
        OutputSchemaType = typeof(PixevalWorkSubscriptionOperationResultDto))]
    [Description(
        "Adds or updates a Pixeval work subscription and queues a subscription sync. Requires Pixeval MCP write tools to be enabled.")]
    public Task<CallToolResult> AddSubscriptionAsync(
        [Description("Pixiv user id.")] long userId,
        [Description("Subscription type.")] PixevalWorkSubscriptionType subscriptionType,
        [Description(
            "Work kind for the subscription type.")]
        PixevalWorkSubscriptionWorkKind workKind,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(nameof(AddSubscriptionAsync), async () =>
        {
            runtime.EnsureLoggedIn();
            runtime.EnsureWriteToolsEnabled();
            return PixevalMcpResult.Success(await runtime.AddSubscriptionAsync(
                    userId,
                    subscriptionType,
                    workKind,
                    cancellationToken)
                .ConfigureAwait(false));
        });

    [McpServerTool(Name = "remove_subscription", Title = "Remove Pixeval work subscription",
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalWorkSubscriptionOperationResultDto))]
    [Description(
        "Removes a Pixeval work subscription by historyEntryId, or by userId + subscriptionType + workKind. Requires write tools.")]
    public CallToolResult RemoveSubscription(
        [Description("History entry id from work subscription history.")]
        int? historyEntryId = null,
        [Description("Pixiv user id. Required when historyEntryId is empty.")]
        long? userId = null,
        [Description("Subscription type. Required when historyEntryId is empty.")]
        PixevalWorkSubscriptionType? subscriptionType = null,
        [Description("Work kind. Required when historyEntryId is empty.")]
        PixevalWorkSubscriptionWorkKind? workKind = null) =>
        Execute(nameof(RemoveSubscription), () =>
        {
            runtime.EnsureWriteToolsEnabled();
            return PixevalMcpResult.Success(runtime.RemoveSubscription(
                historyEntryId,
                userId,
                subscriptionType,
                workKind));
        });

    [McpServerTool(Name = "sync_subscriptions", Title = "Sync Pixeval work subscriptions",
        UseStructuredContent = true, OutputSchemaType = typeof(PixevalOperationResultDto))]
    [Description("Queues Pixeval work subscription sync for all subscriptions. Requires write tools.")]
    public CallToolResult SyncSubscriptions() =>
        Execute(nameof(SyncSubscriptions), () =>
        {
            runtime.EnsureWriteToolsEnabled();
            return PixevalMcpResult.Success(runtime.SyncSubscriptions());
        });

    private CallToolResult Execute(string toolName, Func<CallToolResult> action)
    {
        try
        {
            return action();
        }
        catch (PixevalMcpException e)
        {
            return PixevalMcpResult.Error(e.Message);
        }
        catch (Exception e)
        {
            runtime.LogToolException(toolName, e);
            return PixevalMcpResult.Error("Pixeval MCP tool failed. See Pixeval MCP logs for details.");
        }
    }

    private async Task<CallToolResult> ExecuteAsync(string toolName, Func<Task<CallToolResult>> action)
    {
        try
        {
            return await action().ConfigureAwait(false);
        }
        catch (PixevalMcpException e)
        {
            return PixevalMcpResult.Error(e.Message);
        }
        catch (OperationCanceledException)
        {
            return PixevalMcpResult.Error("The MCP request was canceled.");
        }
        catch (Exception e)
        {
            runtime.LogToolException(toolName, e);
            return PixevalMcpResult.Error("Pixeval MCP tool failed. See Pixeval MCP logs for details.");
        }
    }
}
