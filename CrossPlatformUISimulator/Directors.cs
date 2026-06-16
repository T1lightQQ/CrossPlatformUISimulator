using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class ErrDirector
    {
        public void Construct(IContainerBuilder builder, IThemeFactory defaultTheme)
        {
            builder.SetTitle("Ошибка системы")
                   .SetIcon(new IconSrc { Path = "error.png" })
                   .AddButton(new BtnConfig { Text = "OK" })
                   .ConfigureTheme(defaultTheme);
        }
    }

    public class MultiDirector
    {
        public void Construct(IContainerBuilder builder, IThemeFactory theme)
        {
            builder.SetTitle("Выберите действие")
                   .AddButton(new BtnConfig { Text = "Да" })
                   .AddButton(new BtnConfig { Text = "Нет" })
                   .AddButton(new BtnConfig { Text = "Отмена" })
                   .ConfigureTheme(theme);
        }
    }
}