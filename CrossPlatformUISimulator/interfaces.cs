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

    public interface IWidget : IPrototypical<IWidget>
    {
        void Show();
    }

    public interface IBtn : IPrototypical<IBtn>
    {
        string Text { get; }
        string Theme { get; }
        void Click();
        IBtn WithText(string text);
    }

    public interface ICheck : IPrototypical<ICheck>
    {
        string Theme { get; }
        void Toggle();
    }

    public interface IFont
    {
        string Theme { get; }
        void Print();
    }

    public interface IDialog : IPrototypical<IDialog>
    {
        string Title { get; }
        IconSrc? Icon { get; }
        List<IBtn> Buttons { get; }
        List<IWidget> Widgets { get; }
        string ThemeName { get; }
        void ShowDialog();
        IDialog AddWidget(IWidget widget);
    }

    public interface IWidgetFactory
    {
        IWidget CreateWidget(WidgetConfig config);
    }

    public interface IThemeFactory
    {
        string ThemeName { get; }
        IBtn CreateButton(string text);
        ICheck CreateCheckBox();
        IFont CreateFontEngine();
    }

    public interface IContainerBuilder
    {
        IContainerBuilder SetTitle(string title);
        IContainerBuilder AddButton(BtnConfig config);
        IContainerBuilder SetIcon(IconSrc source);
        IContainerBuilder ConfigureTheme(IThemeFactory theme);
        IContainerBuilder AddCustomWidget(IWidget widget);
        IDialog Build();
    }

    public interface IApplicationTelemetry
    {
        void LogOperation(string category, string action, TimeSpan duration, string? metadata = null);
        IReadOnlyDictionary<string, int> GetOperationCounts();
        GlobalUiSettings GetCurrentSettings();
        void ResetForTesting();
    }
}