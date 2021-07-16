using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixeval.Events
{
    public interface IEventChannel : IAsyncEnumerable<IEvent>
    {
        void Subscribe<T>(Action<T> eventHandler) where T : IEvent;

        ValueTask Publish<T>(T eventObj) where T : IEvent;

        void Start();

        void Close();
    }
}