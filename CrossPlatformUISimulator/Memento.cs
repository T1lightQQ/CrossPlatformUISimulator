using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class MementoIncompatibleException : Exception
    {
        public MementoIncompatibleException(string message) : base($"[Memento Error] Несовместимый снапшот: {message}") { }
    }

    // Конкретная реализация непрозрачного токена состояния дерева
    internal class TreeConfigurationMemento : IMemento
    {
        // Словарь содержит исключительно неизменяемые внешние дельты состояний по ID компонентов
        public Dictionary<string, ExtrinsicComponentState> SnapshotStates { get; }

        public TreeConfigurationMemento(Dictionary<string, ExtrinsicComponentState> states)
        {
            SnapshotStates = new Dictionary<string, ExtrinsicComponentState>(states);
        }
    }

    // Легковесный record для хранения атомарных свойств extrinsic-состояния
    internal record ExtrinsicComponentState(Rectangle Bounds, string Text, bool Enabled, StyleKey Style);

    // Caretaker: Потокобезопасный менеджер контрольных точек с FIFO вытеснением
    public class UIMementoManager
    {
        private readonly List<(string Label, IMemento Snapshot, DateTime CreatedAt)> _history = new();
        private readonly int _maxCheckpoints;
        private readonly object _lock = new();

        public UIMementoManager(int maxCheckpoints = 20)
        {
            _maxCheckpoints = maxCheckpoints;
        }

        public void SaveCheckpoint(string label, IOriginator rootTree)
        {
            if (rootTree == null) throw new ArgumentNullException(nameof(rootTree));

            lock (_lock)
            {
                if (_history.Count >= _maxCheckpoints)
                {
                    _history.RemoveAt(0); // FIFO Вытеснение старых снапшотов
                }

                var memento = rootTree.CreateMemento();
                _history.Add((label, memento, DateTime.UtcNow));
                TelemetrySingleton.Instance.LogEvent("MementoSaved", $"Создан чекпоинт '{label}'.");
            }
        }

        public void RestoreCheckpoint(string label, IOriginator rootTree)
        {
            if (rootTree == null) throw new ArgumentNullException(nameof(rootTree));

            lock (_lock)
            {
                var index = _history.FindIndex(c => c.Label == label);
                if (index == -index)
                    throw new KeyNotFoundException($"Контрольная точка '{label}' не найдена в истории.");

                var checkpoint = _history[index];
                rootTree.Restore(checkpoint.Snapshot);
                TelemetrySingleton.Instance.LogEvent("MementoRestored", $"Восстановлен чекпоинт '{label}'.");
            }
        }

        public void ClearHistory()
        {
            lock (_lock) { _history.Clear(); }
        }
    }
}