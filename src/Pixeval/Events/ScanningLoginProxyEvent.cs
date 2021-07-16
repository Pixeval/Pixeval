using System.Threading.Tasks;

namespace Pixeval.Events
{
    /// <summary>
    /// Raises before the scanning of the LoginProxy files, and completes the <see cref="ScanTask"/>
    /// when the scan completes
    /// </summary>
    public class ScanningLoginProxyEvent : IEvent
    {
        public ScanningLoginProxyEvent(Task scanTask)
        {
            Sender = null;
            ScanTask = scanTask;
        }

        public object? Sender { get; }

        public Task ScanTask { get; }
    }
}