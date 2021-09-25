using System;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Pixeval.Messages
{
    public class RecipientAdapter<TMessage> : IRecipient<TMessage> where TMessage : class
    {
        public RecipientAdapter(Action<TMessage> onReceive)
        {
            OnReceive = onReceive;
        }
        public Action<TMessage> OnReceive { get; }

        public void Receive(TMessage message)
        {
            OnReceive(message);
        }

        public static RecipientAdapter<TMessage> Create(Action<TMessage> onReceive)
        {
            return new(onReceive);
        }
    }
}