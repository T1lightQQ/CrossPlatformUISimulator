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

    // Расширенный интерфейс UI-компонента, разделяющий внутреннее и внешнее состояние
    public interface IUIComponent : IPrototypical<IUIComponent>
    {
        string Id { get; }
        Rectangle BoundingBox { get; set; } // Изменяемый Extrinsic State
        string TextContent { get; set; }   // Изменяемый Extrinsic State
        bool Enabled { get; set; }         // Изменяемый Extrinsic State
        int ZIndex { get; set; }           // Изменяемый Extrinsic State
        IUIStyleFlyweight Flyweight { get; } // Ссылка на Intrinsic State

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
    }

    // Легковесный неизменяемый стиль (Flyweight)
    public interface IUIStyleFlyweight
    {
        Guid StyleId { get; }
        FontMetrics Font { get; }
        ColorPalette Palette { get; }
        byte[]? IconBytes { get; }
    }

    // Контракт для ленивых виртуальных прокси-серверов
    public interface ILazyComponentProxy : IUIComponent
    {
        bool IsMaterialized { get; }
        void Materialize();
        IUIComponent GetRealSubject();
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
        IContainerComponent CreateDialog(DialogPreset preset, ThemeType theme);
        void ApplyGlobalTheme(ThemeType theme);
        void RenderAllToContext(IRenderingContext ctx);
        void LogCurrentMetrics();
    }
}