using Pixeval.CoreApi.Preference;
using Pixeval.Pages.Misc;

namespace Pixeval.Events
{
    /// <summary>
    /// This event will be published once the login procedure is finished successfully.
    /// See <see cref="LoginPage.LoginPage_OnLoaded"/>
    /// </summary>
    public class LoginCompletedEvent : IEvent
    {
        public LoginCompletedEvent(LoginPage sender, Session session)
        {
            Parameter = sender;
            Session = session;
        }

        public object? Parameter { get; }

        public Session Session { get; }
    }
}