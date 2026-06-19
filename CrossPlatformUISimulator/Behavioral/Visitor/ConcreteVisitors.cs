using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Core.Components;
using CrossPlatformUISimulator.Core.Decorators;
using CrossPlatformUISimulator.Core.Proxies;
using CrossPlatformUISimulator.Infrastructure;

namespace CrossPlatformUISimulator.Behavioral.Visitor
{
    // Обоснование рекурсивного обхода: Посетитель самостоятельно управляет вызовами Accept для Children.
    // Это изолирует структуру Composite от логики фильтрации (например, пропуск скрытых веток) внутри алгоритмов.

    
    public class MetricsCollectorVisitor : IUIComponentVisitor
    {
        private int _totalNodes;
        private int _proxyCount;
        private int _materializedProxies;
        private readonly HashSet<string> _distinctStyles = new();

        public MetricsReport GetReport() => new(_totalNodes, _proxyCount, _materializedProxies, _distinctStyles.Count);


        public void Visit(LabelComponent label)
        {
            
            System.Reflection.FieldInfo? field = typeof(MetricsCollectorVisitor).GetField("_totalNodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(this, (int)field.GetValue(this)! + 1);
        }

        public void Visit(ButtonComponent button)
        {
            _totalNodes++;
            var impl = button.Flyweight as UIStyleFlyweightImpl;
            if (impl != null) _distinctStyles.Add(impl.Key.FontName);
        }

        public void Visit(PanelComponent panel)
        {
            _totalNodes++;
            var impl = panel.Flyweight as UIStyleFlyweightImpl;
            if (impl != null) _distinctStyles.Add(impl.Key.FontName);

            
            foreach (var child in panel.Children)
            {
                child.Accept(this);
            }
        }

        public void Visit(UIComponentDecorator decorator)
        {
            
            _totalNodes++;
        }

        public void Visit(VirtualComponentProxy proxy)
        {
            _totalNodes++;
            _proxyCount++;
            if (proxy.IsMaterialized)
            {
                _materializedProxies++;
            }
        }
    }

    
    public class AccessibilityTreeVisitor : IUIComponentVisitor
    {
        public List<AccessibleNode> AccessibleNodes { get; } = new();

        public void Visit(ButtonComponent button)
        {
            if (button.Enabled)
            {
                AccessibleNodes.Add(new AccessibleNode(button.Id, "Button", button.TextContent, true));
            }
        }

        public void Visit(LabelComponent label)
        {
            if (label.Enabled)
            {
                AccessibleNodes.Add(new AccessibleNode(label.Id, "StaticText", label.TextContent, false));
            }
        }
        public void Visit(PanelComponent panel)
        {
            if (panel.Enabled)
            {
                AccessibleNodes.Add(new AccessibleNode(panel.Id, "LayoutContainer", panel.TextContent ?? "Panel", false));
                foreach (var child in panel.Children)
                {
                    child.Accept(this);
                }
            }
        }

        public void Visit(UIComponentDecorator decorator) {  }
        public void Visit(VirtualComponentProxy proxy) {  }
    }

    
    public class DependencyValidatorVisitor : IUIComponentVisitor
    {

        public void Visit(LabelComponent label)
        {
            if (label.Flyweight == null)
            {
                ValidationErrors.Add($"[Error] Текстовая метка ID='{label.Id}' нарушает паттерн Flyweight.");
            }
        }

        public List<string> ValidationErrors { get; } = new();
        private readonly HashSet<string> _visitedIds = new();

        public void Visit(ButtonComponent button)
        {
            CheckCycleAndStyle(button);
        }

        public void Visit(PanelComponent panel)
        {
            if (!CheckCycleAndStyle(panel)) return;

            foreach (var child in panel.Children)
            {
                child.Accept(this);
            }
        }

        public void Visit(UIComponentDecorator decorator)
        {
            if (decorator.GetWrappedComponent() == null)
            {
                ValidationErrors.Add($"[Error] Декоратор содержит пустую ссылку на целевой компонент.");
            }
        }

        public void Visit(VirtualComponentProxy proxy)
        {
            if (string.IsNullOrEmpty(proxy.Id))
            {
                ValidationErrors.Add("[Error] Обнаружен прокси-компонент с поврежденным глобальным идентификатором.");
            }
        }

        private bool CheckCycleAndStyle(IUIComponent component)
        {
            if (!_visitedIds.Add(component.Id))
            {
                ValidationErrors.Add($"[Error] Обнаружен циклический граф! Компонент ID='{component.Id}' вызван повторно.");
                return false;
            }
            if (component.Flyweight == null)
            {
                ValidationErrors.Add($"[Error] Компонент ID='{component.Id}' нарушает паттерн Flyweight (отсутствует ссылка на разделяемый стиль).");
            }
            return true;
        }
    }
}