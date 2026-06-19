using System;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Core.Components;
using CrossPlatformUISimulator.Behavioral.Mediator;
using CrossPlatformUISimulator.Behavioral.Command;
using CrossPlatformUISimulator.DSL;

namespace CrossPlatformUISimulator.Facade
{
    // Паттерн Facade (Фасад)
    public class UISystemFacade
    {
        private readonly EventDrivenMediator _mediator;
        private readonly CommandManager _commandManager;

        public PanelComponent Root { get; }

        public UISystemFacade(EventDrivenMediator mediator, CommandManager commandManager, PanelComponent root)
        {
            _mediator = mediator;
            _commandManager = commandManager;
            Root = root;
        }

        public void Subscribe(Predicate<UIEvent> filter, Action<UIEvent> handler) =>
            _mediator.AddSub(filter, handler);

        public void ExecuteDsl(string script)
        {
            var tokens = new Scanner(script).ScanTokens();
            var astNode = new ScriptParser(tokens).Parse();
            var context = new UIInterpreterContext(this, _commandManager);

            astNode.Interpret(context);
        }
    }
}