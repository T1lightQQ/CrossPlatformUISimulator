using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class StandBtn : IWidget
    {
        public StandBtn() => Counter.HeavyCount++;
        public void Show() => Console.WriteLine("Виджет: Стандартная кнопка");
        public IWidget Clone() { Counter.CloneCount++; return new StandBtn(); }
    }

    public class StandTxt : IWidget
    {
        public StandTxt() => Counter.HeavyCount++;
        public void Show() => Console.WriteLine("Виджет: Текстовое поле");
        public IWidget Clone() { Counter.CloneCount++; return new StandTxt(); }
    }

    public class StandSlider : IWidget
    {
        public StandSlider() => Counter.HeavyCount++;
        public void Show() => Console.WriteLine("Виджет: Стандартный слайдер");
        public IWidget Clone() { Counter.CloneCount++; return new StandSlider(); }
    }

    public class DbBtn : IWidget
    {
        public DbBtn() => Counter.HeavyCount++;
        public void Show() => Console.WriteLine("Виджет: [Отладка] Кнопка + Рамка (1px)");
        public IWidget Clone() { Counter.CloneCount++; return new DbBtn(); }
    }

    public class DbTxt : IWidget
    {
        public DbTxt() => Counter.HeavyCount++;
        public void Show() => Console.WriteLine("Виджет: [Отладка] Текстовое поле + Лог системы");
        public IWidget Clone() { Counter.CloneCount++; return new DbTxt(); }
    }

    public class DbSlider : IWidget
    {
        public DbSlider() => Counter.HeavyCount++;
        public void Show() => Console.WriteLine("Виджет: [Отладка] Слайдер + Таймер");
        public IWidget Clone() { Counter.CloneCount++; return new DbSlider(); }
    }
}