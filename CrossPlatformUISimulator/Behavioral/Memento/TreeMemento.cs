using System.Collections.Generic;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Behavioral.Memento
{
    // Убрали дубликат ExtrinsicComponentState, теперь структура берется напрямую из CrossPlatformUISimulator.Common

    internal class TreeConfigurationMemento : IMemento
    {
        // Словарь: Key = ID компонента, Value = его сохраненное состояние
        public Dictionary<string, ExtrinsicComponentState> SnapshotStates { get; }

        public TreeConfigurationMemento(Dictionary<string, ExtrinsicComponentState> states)
        {
            SnapshotStates = states;
        }
    }
}