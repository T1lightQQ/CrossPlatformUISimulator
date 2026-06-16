using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class WinBtn : IBtn { public string Theme => "Fluent"; public void Click() => Console.WriteLine("Клик по кнопке Windows Fluent"); }
    public class WinCheck : ICheck { public string Theme => "Fluent"; public void Toggle() => Console.WriteLine("Переключение флажка Windows Fluent"); }
    public class WinFont : IFont { public string Theme => "Fluent"; public void Print() => Console.WriteLine("Использование шрифта Segoe UI"); }
    public class WinDlg : IDlg
    {
        public string Theme => "Fluent";
        public void Open(IBtn btn, ICheck check, IWidget extra, IFont font)
        {
            if (btn.Theme != "Fluent" || check.Theme != "Fluent")
                throw new InvalidOperationException("Ошибка! Нельзя смешивать темы в контексте Windows.");

            Console.WriteLine("=== Диалоговое окно Windows Fluent ===");
            font.Print();
            btn.Click();
            check.Toggle();
            extra.Show();
        }
    }

    public class FluentThemeFactory : IThemeFactory
    {
        public string ThemeName => "Fluent";
        public IBtn CreateButton() => new WinBtn();
        public ICheck CreateCheckBox() => new WinCheck();
        public IDlg CreateDialogRenderer() => new WinDlg();
        public IFont CreateFontEngine() => new WinFont();
    }

    public class MacBtn : IBtn { public string Theme => "Cupertino"; public void Click() => Console.WriteLine("Клик по кнопке Mac Cupertino"); }
    public class MacCheck : ICheck { public string Theme => "Cupertino"; public void Toggle() => Console.WriteLine("Переключение флажка Mac Cupertino"); }
    public class MacFont : IFont { public string Theme => "Cupertino"; public void Print() => Console.WriteLine("Использование шрифта San Francisco"); }
    public class MacDlg : IDlg
    {
        public string Theme => "Cupertino";
        public void Open(IBtn btn, ICheck check, IWidget extra, IFont font)
        {
            if (btn.Theme != "Cupertino" || check.Theme != "Cupertino")
                throw new InvalidOperationException("Ошибка! Нельзя смешивать темы в контексте Mac.");

            Console.WriteLine("=== Диалоговое окно Mac Cupertino ===");
            font.Print();
            btn.Click();
            check.Toggle();
            extra.Show();
        }
    }

    public class CupertinoThemeFactory : IThemeFactory
    {
        public string ThemeName => "Cupertino";
        public IBtn CreateButton() => new MacBtn();
        public ICheck CreateCheckBox() => new MacCheck();
        public IDlg CreateDialogRenderer() => new MacDlg();
        public IFont CreateFontEngine() => new MacFont();
    }
}