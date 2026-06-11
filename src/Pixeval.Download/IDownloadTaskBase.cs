// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

namespace Pixeval.Download;

public interface IDownloadTaskBase
{
    /// <summary>
    /// 只有<see cref="CurrentState"/>是<see cref="DownloadState.Running"/>或<see cref="DownloadState.Paused"/>值有效
    /// </summary>
    double ProgressPercentage { get; }
    
    /// <summary>
    /// 当前下载任务的状态
    /// </summary>
    DownloadState CurrentState { get; }

    /// <summary>
    /// 下载目标路径
    /// </summary>
    /// <remarks>
    /// 表示文件所在的地址，不包含未解析的宏@{...}，但可能无法被直接解析（因为包含token"&lt;name:formatter&gt;"），<br/>
    /// 当路径是一个文件时，必须是一个有效的地址（不包含token）<br/>
    /// 当路径是多个文件时，每个子任务通过替换token，得到每个子任务的目标路径<br/>
    /// 文件名可以包含token，但其文件夹路径不能包含token（即所有子任务一定使用相同的文件夹路径）<br/>
    /// 在多文件任务中使用该属性很危险，无法得到确切的目标路径
    /// </remarks>
    string Destination { get; }

    /// <summary>
    /// 当<see cref="CurrentState"/>是<see cref="DownloadState.Error"/>时表示可持久化的失败原因，其他状态值为null
    /// </summary>
    string? ErrorMessage { get; }

    /// <summary>
    /// 打开本地文件位置，一般来说单文件下载任务会直接是文件，多文件下载任务会是所在文件夹（即<see cref="Destination"/>路径的不含token的部分）
    /// </summary>
    string OpenLocalDestination { get; }

    /// <summary>
    /// 是否正在处理
    /// </summary>
    bool IsProcessing { get; }

    /// <summary>
    /// 尝试重试下载，只有当<see cref="CurrentState"/>是以下状态之一时才会执行操作
    /// </summary>
    /// <remarks>
    /// <see cref="DownloadState.Error"/><br/>
    /// <see cref="DownloadState.Completed"/><br/>
    /// <see cref="DownloadState.Cancelled"/>
    /// </remarks>
    void Reset();

    /// <summary>
    /// 尝试暂停下载，只有当<see cref="CurrentState"/>是以下状态之一时才会执行操作
    /// </summary>
    /// <remarks>
    /// <see cref="DownloadState.Running"/><br/>
    /// <see cref="DownloadState.Queued"/>
    /// </remarks>
    void Pause();

    /// <summary>
    /// 尝试恢复下载，只有当<see cref="CurrentState"/>是以下状态之一时才会执行操作
    /// </summary>
    /// <remarks>
    /// <see cref="DownloadState.Paused"/>
    /// </remarks>
    void Resume();

    /// <summary>
    /// 尝试取消下载，只有当<see cref="CurrentState"/>是以下状态之一时才会执行操作
    /// </summary>
    /// <remarks>
    /// <see cref="DownloadState.Queued"/><br/>
    /// <see cref="DownloadState.Running"/><br/>
    /// <see cref="DownloadState.Pending"/><br/>
    /// <see cref="DownloadState.Paused"/>
    /// </remarks>
    void Cancel();

    /// <summary>
    /// 尝试删除下载任务
    /// </summary>
    void Delete();
}
