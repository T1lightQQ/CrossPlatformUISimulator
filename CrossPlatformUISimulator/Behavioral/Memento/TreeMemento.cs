using System.Collections.Generic;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Behavioral.Memento
{
    // Внешнее легковесное состояние конкретного компонента
    public record ExtrinsicComponentState(Rectangle Bounds, string Text, bool Enabled, StyleKey Style);

    // Конкретный снимок (Memento)
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