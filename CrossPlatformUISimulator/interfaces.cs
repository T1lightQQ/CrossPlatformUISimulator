using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public interface IWidget
    {
        void Show();
    }

    public interface IBtn
    {
        string Theme { get; }
        void Click();
    }

    public interface ICheck
    {
        string Theme { get; }
        void Toggle();
    }

    public interface IDlg
    {
        string Theme { get; }
        void Open(IBtn btn, ICheck check, IWidget extra, IFont font);
    }

    public interface IFont
    {
        string Theme { get; }
        void Print();
    }

    public interface IWidgetFactory
    {
        IWidget CreateWidget(WidgetConfig config);
    }

    public interface IThemeFactory
    {
        string ThemeName { get; }
        IBtn CreateButton();
        ICheck CreateCheckBox();
        IDlg CreateDialogRenderer();
        IFont CreateFontEngine();
    }
}