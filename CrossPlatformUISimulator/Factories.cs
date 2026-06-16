using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class FluentThemeFactory : IThemeFactory
    {
        public string ThemeName => "Fluent";
        public IBtn CreateButton(string text) => new WinBtn(text);
        public ICheck CreateCheckBox() => new WinCheck();
        public IFont CreateFontEngine() => new WinFont();
    }

    public class CupertinoThemeFactory : IThemeFactory
    {
        public string ThemeName => "Cupertino";
        public IBtn CreateButton(string text) => new MacBtn(text);
        public ICheck CreateCheckBox() => new MacCheck();
        public IFont CreateFontEngine() => new MacFont();
    }

    public class StandFactory : IWidgetFactory
    {
        private readonly Dictionary<WidgetType, Func<IWidget>> _registry = new();

        public StandFactory()
        {
            _registry[WidgetType.Btn] = () => new StandBtn();
            _registry[WidgetType.Txt] = () => new StandTxt();
            _registry[WidgetType.Slider] = () => new StandSlider();
        }

        public void Register(WidgetType type, Func<IWidget> builder) => _registry[type] = builder;
        public IWidget CreateWidget(WidgetConfig config) => _registry[config.Type]();
    }

    public class DbFactory : IWidgetFactory
    {
        private readonly Dictionary<WidgetType, Func<IWidget>> _registry = new();

        public DbFactory()
        {
            _registry[WidgetType.Btn] = () => new DbBtn();
            _registry[WidgetType.Txt] = () => new DbTxt();
            _registry[WidgetType.Slider] = () => new DbSlider();
        }

        public void Register(WidgetType type, Func<IWidget> builder) => _registry[type] = builder;
        public IWidget CreateWidget(WidgetConfig config) => _registry[config.Type]();
    }
}