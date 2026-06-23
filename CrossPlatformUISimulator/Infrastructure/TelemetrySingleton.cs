using System.Threading;

namespace CrossPlatformUISimulator.Infrastructure
{
    
    public class TelemetrySingleton
    {
        // Синглтон
        
        private static readonly TelemetrySingleton _instance = new TelemetrySingleton();
        private int _deliveredEventsCount = 0;

        // Точка доступа(ссылка)
        public static TelemetrySingleton Instance => _instance;

        // Приват конструктор
        private TelemetrySingleton() { } 

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