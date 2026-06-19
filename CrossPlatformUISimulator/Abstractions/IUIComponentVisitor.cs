using CrossPlatformUISimulator.Core.Components;
using CrossPlatformUISimulator.Core.Decorators;
using CrossPlatformUISimulator.Core.Proxies;

namespace CrossPlatformUISimulator.Abstractions
{
    public interface IUIComponentVisitor
    {
        void Visit(ButtonComponent button);
        void Visit(PanelComponent panel);
        void Visit(UIComponentDecorator decorator);
        void Visit(VirtualComponentProxy proxy);
        void Visit(LabelComponent label); 
    }
}