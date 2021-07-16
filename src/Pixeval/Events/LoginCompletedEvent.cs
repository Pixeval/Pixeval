using Mako.Preference;
using Pixeval.Pages;

namespace Pixeval.Events
{
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