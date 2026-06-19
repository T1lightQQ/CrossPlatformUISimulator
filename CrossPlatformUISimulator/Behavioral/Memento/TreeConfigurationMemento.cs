using System.Collections.Generic;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Behavioral.Memento
{
    internal class TreeConfigurationMemento : IMemento
    {
        // Явно указываем пространство Common, полностью исключая неоднозначность
        public Dictionary<string, CrossPlatformUISimulator.Common.ExtrinsicComponentState> SnapshotStates { get; }

        public TreeConfigurationMemento(Dictionary<string, CrossPlatformUISimulator.Common.ExtrinsicComponentState> states)
        {
            SnapshotStates = states;
        }
    }
}