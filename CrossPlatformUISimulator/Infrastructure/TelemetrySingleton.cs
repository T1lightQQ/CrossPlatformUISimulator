using System.Threading;

namespace CrossPlatformUISimulator.Infrastructure
{
    // Паттерн Singleton (Одиночка) с потокобезопасной инициализацией
    public class TelemetrySingleton
    {
        private static readonly TelemetrySingleton _instance = new TelemetrySingleton();
        private int _deliveredEventsCount = 0;

        public static TelemetrySingleton Instance => _instance;

        private TelemetrySingleton() { } // Закрытый конструктор

        public void LogEvent(string status, string details)
        {
            if (status == "Dispatched")
            {
                Interlocked.Increment(ref _deliveredEventsCount);
            }
        }

        public int GetDeliveredCount() => _deliveredEventsCount;
    }
}