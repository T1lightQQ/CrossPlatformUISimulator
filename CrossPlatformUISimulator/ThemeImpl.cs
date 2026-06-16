using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class WinBtn : IBtn
    {
        public string Text { get; }
        public string Theme => "Fluent";

        public WinBtn(string text) => Text = text;

        public void Click() => Console.WriteLine($"[Fluent Кнопка]: {Text}");

        public IBtn Clone()
        {
            var sw = Stopwatch.StartNew();
            var clone = new WinBtn(Text);
            sw.Stop();
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", sw.Elapsed, "WinBtn");
            return clone;
        }

        public IBtn WithText(string text) => new WinBtn(text);
    }

    public class WinCheck : ICheck
    {
        public string Theme => "Fluent";
        public void Toggle() => Console.WriteLine("[Fluent Флажок] переключен");
        public ICheck Clone()
        {
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", TimeSpan.Zero, "WinCheck");
            return new WinCheck();
        }
    }

    public class WinFont : IFont
    {
        public string Theme => "Fluent";
        public void Print() => Console.WriteLine("Шрифт: Segoe UI");
    }

    public class MacBtn : IBtn
    {
        public string Text { get; }
        public string Theme => "Cupertino";

        public MacBtn(string text) => Text = text;

        public void Click() => Console.WriteLine($"[Cupertino Кнопка]: {Text}");

        public IBtn Clone()
        {
            var sw = Stopwatch.StartNew();
            var clone = new MacBtn(Text);
            sw.Stop();
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", sw.Elapsed, "MacBtn");
            return clone;
        }

        public IBtn WithText(string text) => new MacBtn(text);
    }

    public class MacCheck : ICheck
    {
        public string Theme => "Cupertino";
        public void Toggle() => Console.WriteLine("[Cupertino Флажок] переключен");
        public ICheck Clone()
        {
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", TimeSpan.Zero, "MacCheck");
            return new MacCheck();
        }
    }

    public class MacFont : IFont
    {
        public string Theme => "Cupertino";
        public void Print() => Console.WriteLine("Шрифт: San Francisco");
    }
}