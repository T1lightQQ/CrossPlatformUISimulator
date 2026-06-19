using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class ApplicationTelemetrySingleton : IApplicationTelemetry
    {
        private static readonly Lazy<ApplicationTelemetrySingleton> _instance =
            new Lazy<ApplicationTelemetrySingleton>(() => new ApplicationTelemetrySingleton());

        public static ApplicationTelemetrySingleton Instance => _instance.Value;

        private readonly ConcurrentDictionary<string, int> _operationCounts = new();
        private readonly GlobalUiSettings _settings;

        internal ApplicationTelemetrySingleton()
        {
            _settings = new GlobalUiSettings { DefaultTheme = "Fluent", DebugMode = true };
        }

        public void LogOperation(string category, string action, TimeSpan duration, string? metadata = null)
        {
            string key = $"{category}.{action}";
            _operationCounts.AddOrUpdate(key, 1, (_, count) => count + 1);
        }

        public IReadOnlyDictionary<string, int> GetOperationCounts()
        {
            return _operationCounts;
        }

        public GlobalUiSettings GetCurrentSettings()
        {
            return _settings with { };
        }

        public void ResetForTesting()
        {
            _operationCounts.Clear();
        }
    }
}