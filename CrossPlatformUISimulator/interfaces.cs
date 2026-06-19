using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public interface IPrototypical<T> where T : class
    {
        T Clone();
    }

    public interface IUIComponent : IPrototypical<IUIComponent>
    {
        string Id { get; }
        Rectangle BoundingBox { get; set; }
        string TextContent { get; set; }
        bool Enabled { get; set; }
        IUIStyleFlyweight Flyweight { get; }
        void Render(IRenderingContext ctx);
        void SetPosition(Point position);
        T? FindById<T>(string id) where T : class, IUIComponent;
    }

    public interface IContainerComponent : IUIComponent
    {
        IReadOnlyList<IUIComponent> Children { get; }
        void AddChild(IUIComponent child);
        void RemoveChild(IUIComponent child);
        void ReplaceChild(string id, IUIComponent newChild);
    }

    public interface IUIStyleFlyweight
    {
        FontMetrics Font { get; }
        ColorPalette Palette { get; }
    }

    public interface ILazyComponentProxy : IUIComponent
    {
        bool IsMaterialized { get; }
        void Materialize();
        IUIComponent GetRealSubject();
    }

    public interface IProtectionProxy : IUIComponent
    {
        bool IsLocked { get; }
        void LockComponent();
    }

    public interface IRenderingStrategy
    {
        string StrategyName { get; }
        void DrawBackground(Rectangle rect, Color fill);
    }

    public interface IContainerBuilder
    {
        IContainerBuilder SetId(string id);
        IContainerBuilder SetBounds(Rectangle bounds);
        IContainerBuilder ConfigureTheme(IThemeFactory theme);
        IContainerComponent Build();
    }

    public interface IThemeFactory
    {
        IRenderingStrategy CreateRenderingStrategy();
    }

    public interface IApplicationTelemetry
    {
        void LogOperation(string category, string action, TimeSpan duration, string? metadata = null);
    }

    public interface IUISystemFacade
    {
        IContainerComponent RootTree { get; }
        IContainerComponent CreateDialog(DialogPreset preset, ThemeType theme);
        void ExecuteDsl(string script);
    }

    // --- ИНТЕРФЕЙСЫ ПАТТЕРНА ITERATOR ---
    public interface IUIComponentIterator : IEnumerator
    {
        // Каноническое сужение контракта IEnumerator под типизированный UI-компонент
    }

    public interface IIteratorFactory
    {
        IUIComponentIterator CreateDfs(IUIComponent root, Func<IUIComponent, bool>? predicate = null);
        IUIComponentIterator CreateBfs(IUIComponent root, Func<IUIComponent, bool>? predicate = null);
        IUIComponentIterator CreateProxyAware(IUIComponent root);
    }

    // --- ИНТЕРФЕЙС ПАТТЕРНА INTERPRETER ---
    public interface IExpression
    {
        void Interpret(UIInterpreterContext context);
    }

    public interface IUICommand
    {
        void Execute();
        void Undo();
        string Description { get; }
    }
}