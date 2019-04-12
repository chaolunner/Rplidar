#if NET_4_6
using System;
#else
using UniRx;
#endif

namespace AlphaECS
{
    public interface IEventSystem
    {
        void Publish<T>(T message);
        IObservable<T> OnEvent<T>();
    }
}