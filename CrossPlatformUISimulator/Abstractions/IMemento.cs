using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator.Abstractions
{
    
    public interface IMemento { }

    
    public interface IOriginator
    {
        IMemento CreateMemento();
        void Restore(IMemento memento);
    }
}