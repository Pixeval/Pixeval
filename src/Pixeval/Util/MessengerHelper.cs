// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.Mvvm.Messaging;

namespace Pixeval.Util;

public static class MessengerHelper
{
    public static bool TryRegister<TRecipient, TMessage>(this IMessenger messenger, TRecipient recipient, MessageHandler<TRecipient, TMessage> handler)
        where TMessage : class
        where TRecipient : class
    {
        if (!messenger.IsRegistered<TMessage>(recipient))
        {
            messenger.Register(recipient, handler);
            return true;
        }

        return false;
    }
}