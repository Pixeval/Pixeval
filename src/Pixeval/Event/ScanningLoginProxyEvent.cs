using System.Threading.Tasks;

namespace Pixeval.Event
{
    /// <summary>
    /// This event will be published once the scan process of the login proxy's zip file is completed.
    /// See <see cref="AppContext.CopyLoginProxyIfRequiredAsync"/>
    /// </summary>
    public class ScanningLoginProxyEvent : IEvent
    {
        public ScanningLoginProxyEvent()
        {
            Sender = null;
        }

        public object? Sender { get; }
    }
}