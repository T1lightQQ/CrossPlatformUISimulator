using System;
using System.Threading;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Behavioral.Observer
{
    public class SafeObserverSubject : IUIStateSubject
    {
        private readonly IUIComponent _owner;
        private volatile IUIStateObserver[] _observers = Array.Empty<IUIStateObserver>();
        private readonly object _lock = new();

        public SafeObserverSubject(IUIComponent owner) => _owner = owner;

        public void Attach(IUIStateObserver observer)
        {
            if (observer == null) return;
            lock (_lock)
            {
                var oldArr = _observers;
                // Проверка на дубликаты
                if (Array.IndexOf(oldArr, observer) >= 0) return;

                var newArr = new IUIStateObserver[oldArr.Length + 1];
                Array.Copy(oldArr, newArr, oldArr.Length);
                newArr[^1] = observer;
                _observers = newArr; // Атомарная замена ссылки
            }
        }

        public void Detach(IUIStateObserver observer)
        {
            if (observer == null) return;
            lock (_lock)
            {
                var oldArr = _observers;
                int idx = Array.IndexOf(oldArr, observer);
                if (idx < 0) return;

                var newArr = new IUIStateObserver[oldArr.Length - 1];
                Array.Copy(oldArr, 0, newArr, 0, idx);
                Array.Copy(oldArr, idx + 1, newArr, idx, oldArr.Length - idx - 1);
                _observers = newArr; // Атомарная замена ссылки
            }
        }

        public void Notify(UIStateChangeData data)
        {
            // Чтение volatile ссылки исключает необходимость блокировки во время итерации (защита от Deadlock)
            var currentObservers = Volatile.Read(ref _observers);

            foreach (var observer in currentObservers)
            {
                try
                {
                    observer.OnStateChange(_owner, data);
                }
                catch (Exception ex)
                {
                    observer.OnError(_owner, ex);
                }
            }
        }

        public void ClearAll()
        {
            lock (_lock)
            {
                _observers = Array.Empty<IUIStateObserver>();
            }
        }
    }
}