using System.Collections.Generic;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Behavioral.Command;
using CrossPlatformUISimulator.Facade;

namespace CrossPlatformUISimulator.DSL
{
    
    public class UIInterpreterContext
    {
        public UISystemFacade Facade { get; }
        public CommandManager CommandManager { get; }
        public List<IUIComponent> SelectedComponents { get; set; } = new();

        public UIInterpreterContext(UISystemFacade facade, CommandManager cm)
        {
            Facade = facade;
            CommandManager = cm;
        }
    }
}