using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Core.Proxies;
using System;

namespace CrossPlatformUISimulator.Behavioral.TemplateMethod
{
    
    public class StandardComponentLifecycle : UIComponentLifecycleBase
    {
        public StandardComponentLifecycle(IUIComponent component) : base(component) { }

        protected override void Initialize(UIContext ctx)
        {
            
        }

        protected override bool Validate(UIContext ctx)
        {
            
            return TargetComponent.CurrentState.StateName != "ErrorState";
        }

        protected override void Render(UIContext ctx)
        {
            
            if (TargetComponent.CurrentState.StateName == "NormalState")
            {
                
            }
        }
    }

    
    public class ComplexContainerLifecycle : UIComponentLifecycleBase
    {
        private readonly IContainerComponent _container;

        public ComplexContainerLifecycle(IContainerComponent container) : base(container)
        {
            _container = container;
        }

        protected override void Initialize(UIContext ctx)
        {
            
        }

        protected override void LoadResources(UIContext ctx)
        {
            
            foreach (var child in _container.Children)
            {
                if (child is ILazyComponentProxy proxy && !proxy.IsMaterialized)
                {
                    
                    var text = child.TextContent;
                }
            }
        }

        protected override bool Validate(UIContext ctx)
        {
            
            if (_container.CurrentState.StateName == "ErrorState") return false;

            foreach (var child in _container.Children)
            {
                if (child.CurrentState.StateName == "ErrorState") return false;
            }
            return true;
        }

        protected override void Render(UIContext ctx)
        {
            
            var layoutContext = new LayoutContext(10, 5, 800, 600, 1.0);
            _container.CalculateLayout(layoutContext);
        }
    }
}