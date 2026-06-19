using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator.Abstractions
{
    // Паттерн Command для инкрементальных дельта-изменений
    public interface IUICommand
    {
        void Execute();
        void Undo();
        string Description { get; }
    }
}