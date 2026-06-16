using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class UIScenarioRunner
    {
        public void Run()
        {
            IThemeFactory theme = new FluentThemeFactory();
            StandFactory widgetFactory = new StandFactory();

            DialogBuilder builder = new DialogBuilder();
            MultiDirector director = new MultiDirector();

            director.Construct(builder, theme);
            IDialog original = builder.Build();

            Console.WriteLine("=== Шаг 1: Оригинальный диалог ===");
            original.ShowDialog();

            IDialog clone = original.Clone();

            WidgetConfig txtCfg = new WidgetConfig { Type = WidgetType.Txt, Theme = "Fluent" };
            IWidget txtWidget = widgetFactory.CreateWidget(txtCfg);

            IDialog modifiedClone = clone.AddWidget(txtWidget);

            Console.WriteLine("\n=== Шаг 2: Измененный клон диалога (Добавлен виджет) ===");
            modifiedClone.ShowDialog();

            Console.WriteLine("\n=== Шаг 3: Проверка глубокой изоляции (Оригинал не изменился) ===");
            original.ShowDialog();

            Counter.Reset();

            DialogBuilder massBuilder = new DialogBuilder();
            massBuilder.SetTitle("Шаблон")
                       .AddButton(new BtnConfig { Text = "Кнопка" })
                       .ConfigureTheme(theme);

            IDialog template = massBuilder.Build();

            int totalCount = 10;
            for (int i = 0; i < totalCount; i++)
            {
                IDialog instance = template.Clone();
            }

            int heavyWithoutPrototype = totalCount * 2;
            int heavyWithPrototype = Counter.HeavyCount;
            double savings = ((double)(heavyWithoutPrototype - heavyWithPrototype) / heavyWithoutPrototype) * 100;

            Console.WriteLine($"\n=== Тест производительности ({totalCount} созданий) ===");
            Console.WriteLine($"Тяжелых вызовов без паттерна Prototype: {heavyWithoutPrototype}");
            Console.WriteLine($"Тяжелых вызовов с паттерном Prototype: {heavyWithPrototype}");
            Console.WriteLine($"Сокращение тяжелых вызовов конструкторов/фабрик на: {savings}%");
            Console.WriteLine("Примечание по клонированию: Делегаты и события сбрасываются для предотвращения утечек памяти.");
        }
    }
}