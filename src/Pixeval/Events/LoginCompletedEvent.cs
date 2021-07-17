using Mako.Preference;
using Pixeval.Pages;

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
            Sender = sender;
            Session = session;
        }

        public object? Sender { get; }

        public Session Session { get; }
    }
}