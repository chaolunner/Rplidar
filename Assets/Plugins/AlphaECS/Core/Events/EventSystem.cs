using UniRx;
using System;

namespace AlphaECS
{
    public class EventSystem : IEventSystem
    {
        public IMessageBroker MessageBroker { get; private set; }

        public EventSystem(IMessageBroker messageBroker)
        { MessageBroker = messageBroker; }

        public void Publish<T>(T message)
        { MessageBroker.Publish(message); }

        public IObservable<T> OnEvent<T>()
        { return MessageBroker.Receive<T>(); }

    }
}