using JetBrains.Annotations;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Pixeval.Util.UI
{
    public class AutoActivateObservableRecipient : ObservableRecipient
    {
        public AutoActivateObservableRecipient()
        {
            IsActive = true;
        }

        public AutoActivateObservableRecipient([NotNull] IMessenger messenger) : base(messenger)
        {
            IsActive = true;
        }
    }
}