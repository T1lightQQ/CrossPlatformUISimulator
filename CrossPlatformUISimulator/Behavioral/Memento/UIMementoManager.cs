using System.Collections.Generic;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Behavioral.Memento
{
    // Класс Опекун (Caretaker) для работы с историей снимков
    public class UIMementoManager
    {
        private readonly List<(string Label, IMemento Snapshot)> _history = new();
        private readonly int _maxEntries;

        public UIMementoManager(int maxEntries = 20) => _maxEntries = maxEntries;

        public void SaveCheckpoint(string label, IOriginator root)
        {
            if (_history.Count >= _maxEntries)
                _history.RemoveAt(0);

            _history.Add((label, root.CreateMemento()));
        }

        public void RestoreCheckpoint(string label, IOriginator root)
        {
            int idx = _history.FindIndex(c => c.Label == label);
            if (idx != -1)
            {
                root.Restore(_history[idx].Snapshot);
            }
            else
            {
                throw new KeyNotFoundException($"Чекпоинт с меткой '{label}' не найден в истории.");
            }
        }
    }
}