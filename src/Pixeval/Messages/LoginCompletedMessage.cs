using Pixeval.CoreApi.Preference;
using Pixeval.Pages.Misc;

namespace Pixeval.Messages
{
    /// <summary>
    /// This event will be published once the login procedure is finished successfully.
    /// See <see cref="LoginPage.LoginPage_OnLoaded"/>
    /// </summary>
    public class LoginCompletedMessage
    {
        public LoginCompletedMessage(LoginPage sender, Session session)
        {
            Sender = sender;
            Session = session;
        }

        public LoginPage Sender { get; }

        public Session Session { get; }
    }
}