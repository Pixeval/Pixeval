namespace Pixeval.Download
{
    public enum DownloadState
    {
        Created,

        Queued,

        Running,

        Cancelled,
        
        Error,

        Completed
    }
}