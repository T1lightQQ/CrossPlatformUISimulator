using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace CrossPlatformUISimulator
{
    public class WinBtn : IBtn
    {
        public string Text { get; }
        public string Theme => "Fluent";

        public WinBtn(string text)
        {
            Text = text;
            Counter.HeavyCount++;
        }

        public void Click() => Console.WriteLine($"[Fluent Кнопка]: {Text}");
        public IBtn Clone() { Counter.CloneCount++; return new WinBtn(Text); }
        public IBtn WithText(string text) => new WinBtn(text);
    }

    public class WinCheck : ICheck
    {
        public string Theme => "Fluent";
        public WinCheck() => Counter.HeavyCount++;
        public void Toggle() => Console.WriteLine("[Fluent Флажок] переключен");
        public ICheck Clone() { Counter.CloneCount++; return new WinCheck(); }
    }

    public class WinFont : IFont
    {
        public string Theme => "Fluent";
        public WinFont() => Counter.HeavyCount++;
        public void Print() => Console.WriteLine("Шрифт: Segoe UI");
    }

    public class MacBtn : IBtn
    {
        public string Text { get; }
        public string Theme => "Cupertino";

        public MacBtn(string text)
        {
            Text = text;
            Counter.HeavyCount++;
        }

        public void Click() => Console.WriteLine($"[Cupertino Кнопка]: {Text}");
        public IBtn Clone() { Counter.CloneCount++; return new MacBtn(Text); }
        public IBtn WithText(string text) => new MacBtn(text);
    }

    public class MacCheck : ICheck
    {
        public string Theme => "Cupertino";
        public MacCheck() => Counter.HeavyCount++;
        public void Toggle() => Console.WriteLine("[Cupertino Флажок] переключен");
        public ICheck Clone() { Counter.CloneCount++; return new MacCheck(); }
    }

    public class MacFont : IFont
    {
        public string Theme => "Cupertino";
        public MacFont() => Counter.HeavyCount++;
        public void Print() => Console.WriteLine("Шрифт: San Francisco");
    }
}