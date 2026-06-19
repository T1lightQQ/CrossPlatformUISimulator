using System;
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
        int ZIndex { get; set; }
        IUIStyleFlyweight Flyweight { get; }

        void Render(IRenderingContext ctx);
        void SetPosition(Point position);
        T? FindById<T>(string id) where T : class, IUIComponent;
    }

    public interface ILeafComponent : IUIComponent { }

    public interface IContainerComponent : IUIComponent
    {
        IReadOnlyList<IUIComponent> Children { get; }
        void AddChild(IUIComponent child);
        void RemoveChild(IUIComponent child);
        void ReplaceChild(string id, IUIComponent newChild); // Требуется для динамической инжекции декораторов командами
    }

    public interface IUIStyleFlyweight
    {
        Guid StyleId { get; }
        FontMetrics Font { get; }
        ColorPalette Palette { get; }
        byte[]? IconBytes { get; }
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
        void DrawBorder(Rectangle rect, Color stroke, float thickness);
        void DrawText(string text, FontMetrics font, Point position, Color color);
        bool HitTest(Rectangle bounds, Point cursor);
        void DisposeResources();
    }

    public interface IWidgetFactory
    {
        IUIComponent CreateWidget(WidgetConfig config, IRenderingStrategy strategy, IUIStyleFlyweight flyweight);
    }

    public interface IThemeFactory
    {
        string ThemeName { get; }
        IRenderingStrategy CreateRenderingStrategy();
    }

    public interface IContainerBuilder
    {
        IContainerBuilder SetId(string id);
        IContainerBuilder SetBounds(Rectangle bounds);
        IContainerBuilder AddButton(BtnConfig config);
        IContainerBuilder ConfigureTheme(IThemeFactory theme);
        IContainerBuilder AddComponent(IUIComponent component);
        IContainerBuilder ApplyDecorator(Func<IUIComponent, UIComponentDecorator> decoratorFactory);
        IContainerComponent Build();
    }

    public interface IApplicationTelemetry
    {
        void LogOperation(string category, string action, TimeSpan duration, string? metadata = null);
        IReadOnlyDictionary<string, int> GetOperationCounts();
        GlobalUiSettings GetCurrentSettings();
        void ResetForTesting();
    }

    public interface IUISystemFacade
    {
        IContainerComponent RootTree { get; }
        IContainerComponent CreateDialog(DialogPreset preset, ThemeType theme);
        void ApplyGlobalTheme(ThemeType theme);
        void RenderAllToContext(IRenderingContext ctx);
        void LogCurrentMetrics();
    }

    // Паттерн Chain of Responsibility
    public interface IUIEventHandler
    {
        IUIEventHandler SetNext(IUIEventHandler next);
        bool Handle(UIEvent @event);
    }

    // Паттерн Command
    public interface IUICommand
    {
        void Execute();
        void Undo();
        string Description { get; }
    }
}