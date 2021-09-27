using Pixeval.CoreApi.Preference;
using Pixeval.Pages.Misc;

namespace Pixeval.Messages
{
    /// <summary>
    /// This event will be published once the login procedure is finished successfully.
    /// See <see cref="LoginPage.LoginPage_OnLoaded"/>
    /// </summary>
    public record LoginCompletedMessage(LoginPage Sender, Session Session);
}