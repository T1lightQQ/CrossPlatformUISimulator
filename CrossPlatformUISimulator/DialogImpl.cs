using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class DlgObj : IDialog
    {
        public required string Title { get; init; }
        public required IconSrc? Icon { get; init; }
        public required List<IBtn> Buttons { get; init; }
        public required List<IWidget> Widgets { get; init; }
        public required string ThemeName { get; init; }

        public void ShowDialog()
        {
            Console.WriteLine($"--- Диалог [{ThemeName}]: {Title} ---");
            if (Icon != null) Console.WriteLine($"Иконка: {Icon.Path}");
            foreach (var b in Buttons) b.Click();
            foreach (var w in Widgets) w.Show();
        }

        public IDialog AddWidget(IWidget widget)
        {
            var newWidgets = new List<IWidget>(Widgets) { widget };
            return new DlgObj
            {
                Title = this.Title,
                Icon = this.Icon,
                Buttons = this.Buttons,
                ThemeName = this.ThemeName,
                Widgets = newWidgets
            };
        }

        public IDialog Clone()
        {
            var sw = Stopwatch.StartNew();

            var clonedButtons = new List<IBtn>();
            foreach (var b in Buttons)
            {
                clonedButtons.Add(b.Clone());
            }

            var clonedWidgets = new List<IWidget>();
            foreach (var w in Widgets)
            {
                clonedWidgets.Add(w.Clone());
            }

            IconSrc? clonedIcon = Icon != null ? new IconSrc { Path = Icon.Path } : null;

            var executionResult = new DlgObj
            {
                Title = this.Title,
                Icon = clonedIcon,
                ThemeName = this.ThemeName,
                Buttons = clonedButtons,
                Widgets = clonedWidgets
            };

            sw.Stop();
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", sw.Elapsed, "DialogObject");

            return executionResult;
        }
    }
}