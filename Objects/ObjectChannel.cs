using System;
using Pzxlane.Objects.Exceptions;

namespace Pzxlane.Objects
{
    public class ObjectChannel<T>
    {
        private event Action<T> OnObjectReceived;

        private Action<T> eventAction;

        public void Send(T item)
        {
            (OnObjectReceived ?? throw new NullReferenceException(nameof(OnObjectReceived)))(item);
        }

        public void Detach()
        {
            OnObjectReceived -= eventAction;
            eventAction = null;
        }

        public void Attach(Action<T> action)
        {
            if (eventAction != null)
            {
                throw new ConflictException("You cannot bind more than one event to ObjectChannel");
            }

            OnObjectReceived += action;
        }
    }
}