using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator.Abstractions
{
    
    public interface IUICommand
    {
        void Execute();
        void Undo();
        string Description { get; }
    }
}