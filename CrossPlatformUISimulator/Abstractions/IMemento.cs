using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator.Abstractions
{
    // Opaque Token (Маркерный интерфейс "хранителя" без публичных методов)
    public interface IMemento { }

    // Интерфейс создателя состояния (Originator)
    public interface IOriginator
    {
        IMemento CreateMemento();
        void Restore(IMemento memento);
    }
}