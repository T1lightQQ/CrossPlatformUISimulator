using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Core.Components;
using CrossPlatformUISimulator.Behavioral.Mediator;
using CrossPlatformUISimulator.Behavioral.Command;
using CrossPlatformUISimulator.DSL;

namespace CrossPlatformUISimulator.Facade
{
    public class UISystemFacade
    {

       

        private readonly EventDrivenMediator _mediator;
        private readonly CommandManager _commandManager;

        public PanelComponent Root { get; }
        
        // Фасад
        public UISystemFacade(EventDrivenMediator mediator, CommandManager commandManager, PanelComponent root)
        {
            _mediator = mediator;
            _commandManager = commandManager;
            Root = root;
        }

        // Упрощенный доступ к медиатору
        public void Subscribe(Predicate<UIEvent> filter, Action<UIEvent> handler) =>
            _mediator.AddSub(filter, handler);

        // Упрощенный доступ к Обсерверу
        public void AttachObserverTo(string componentId, IUIStateObserver observer)
        {
            var comp = Root.FindById<IUIComponent>(componentId);
            comp?.Attach(observer);
        }

        public void ApplyLayout(string containerId, ILayoutStrategy strategy, LayoutContext context)
        {
            var container = Root.FindById<IContainerComponent>(containerId);
            if (container != null)
            {
                var cmd = new ApplyLayoutCommand(container, strategy, context);
                _commandManager.Execute(cmd);
            }
        }

        
        public void RunVisitor(IUIComponentVisitor visitor)
        {
            Root.Accept(visitor);
        }

        public void ExecuteDsl(string script)
        {
            var tokens = new Scanner(script).ScanTokens();
            var astNode = new ScriptParser(tokens).Parse();
            var context = new UIInterpreterContext(this, _commandManager);

            astNode.Interpret(context);
        }
    }
}