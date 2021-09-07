namespace Pixeval.Events
{
    /// <summary>
    /// This event will be published once the scan process of the login proxy's zip file is completed.
    /// See <see cref="AppContext.CopyLoginProxyIfRequiredAsync"/>
    /// </summary>
    public class ScanningLoginProxyEvent : IEvent
    {
        public ScanningLoginProxyEvent()
        {
            Parameter = null;
        }

        public object? Parameter { get; }
    }
}