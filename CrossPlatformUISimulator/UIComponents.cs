using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public abstract class UIComponentBase : IUIComponent
    {
        protected IRenderingStrategy _renderingStrategy;
        public string Id { get; protected set; }
        public Rectangle BoundingBox { get; protected set; }

        protected UIComponentBase(string id, Rectangle bounds, IRenderingStrategy strategy)
        {
            Id = id;
            BoundingBox = bounds;
            _renderingStrategy = strategy;
        }

        public abstract void Render(IRenderingContext ctx);

        public virtual void SetPosition(Point position)
        {
            BoundingBox = new Rectangle(position.X, position.Y, BoundingBox.Width, BoundingBox.Height);
        }

        public void SwitchRenderingStrategy(IRenderingStrategy newStrategy)
        {
            if (_renderingStrategy.StrategyName == "VectorSvg" && newStrategy.StrategyName == "Raster")
            {
                throw new InvalidOperationException("Критическая ошибка совместимости: Запрещена подмена векторного рендерера на растровый в runtime.");
            }
            _renderingStrategy = newStrategy;
        }

        public T? FindById<T>(string id) where T : class, IUIComponent
        {
            var stack = new Stack<IUIComponent>();
            stack.Push(this);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current.Id == id && current is T target) return target;

                if (current is UIComponentDecorator decorator)
                {
                    stack.Push(decorator.GetWrappedComponent());
                }
                else if (current is IContainerComponent container)
                {
                    for (int i = container.Children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(container.Children[i]);
                    }
                }
            }
            return null;
        }

        public abstract IUIComponent Clone();
    }

    public class ButtonComponent : UIComponentBase, ILeafComponent
    {
        public string Text { get; set; }

        public ButtonComponent(string id, Rectangle bounds, string text, IRenderingStrategy strategy)
            : base(id, bounds, strategy) => Text = text;

        public override void Render(IRenderingContext ctx)
        {
            _renderingStrategy.DrawBackground(BoundingBox, new Color(200, 200, 200));
            _renderingStrategy.DrawText(Text, new FontMetrics("Arial", 12), new Point(BoundingBox.X + 5, BoundingBox.Y + 5), new Color(0, 0, 0));
        }

        public override IUIComponent Clone()
        {
            var sw = Stopwatch.StartNew();
            var clone = new ButtonComponent(Id, BoundingBox, Text, _renderingStrategy);
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", sw.Elapsed, $"Button:{Id}");
            return clone;
        }
    }

    public class LabelComponent : UIComponentBase, ILeafComponent
    {
        public string Text { get; set; }

        public LabelComponent(string id, Rectangle bounds, string text, IRenderingStrategy strategy)
            : base(id, bounds, strategy) => Text = text;

        public override void Render(IRenderingContext ctx) =>
            _renderingStrategy.DrawText(Text, new FontMetrics("Arial", 11), new Point(BoundingBox.X, BoundingBox.Y), new Color(0, 0, 0));

        public override IUIComponent Clone() => new LabelComponent(Id, BoundingBox, Text, _renderingStrategy);
    }

    public class SliderComponent : UIComponentBase, ILeafComponent
    {
        public int Value { get; set; }

        public SliderComponent(string id, Rectangle bounds, int val, IRenderingStrategy strategy)
            : base(id, bounds, strategy) => Value = val;

        public override void Render(IRenderingContext ctx)
        {
            _renderingStrategy.DrawBackground(BoundingBox, new Color(100, 100, 250));
            _renderingStrategy.DrawBorder(BoundingBox, new Color(0, 0, 0), 1.0f);
        }

        public override IUIComponent Clone() => new SliderComponent(Id, BoundingBox, Value, _renderingStrategy);
    }

    public class PanelComponent : UIComponentBase, IContainerComponent
    {
        private readonly List<IUIComponent> _children = new();
        public IReadOnlyList<IUIComponent> Children => _children;

        public PanelComponent(string id, Rectangle bounds, IRenderingStrategy strategy)
            : base(id, bounds, strategy) { }

        public void AddChild(IUIComponent child) => _children.Add(child);
        public void RemoveChild(IUIComponent child) => _children.Remove(child);

        public override void Render(IRenderingContext ctx)
        {
            _renderingStrategy.DrawBackground(BoundingBox, new Color(255, 255, 255));
            foreach (var child in _children) child.Render(ctx);
        }

        public override IUIComponent Clone()
        {
            var sw = Stopwatch.StartNew();
            var clonedPanel = new PanelComponent(Id, BoundingBox, _renderingStrategy);
            foreach (var child in _children)
            {
                clonedPanel.AddChild(child.Clone());
            }
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", sw.Elapsed, $"Panel:{Id}");
            return clonedPanel;
        }
    }

    // --- ПАТТЕРН ДЕКОРАТОР ---
    public abstract class UIComponentDecorator : IUIComponent, IContainerComponent
    {
        protected readonly IUIComponent _component;
        protected UIComponentDecorator(IUIComponent component) => _component = component;

        public virtual string Id => _component.Id;
        public virtual Rectangle BoundingBox => _component.BoundingBox;

        public virtual void Render(IRenderingContext ctx) => _component.Render(ctx);

        public virtual void SetPosition(Point position) => _component.SetPosition(position);

        public virtual T? FindById<T>(string id) where T : class, IUIComponent
        {
            if (Id == id && this is T target) return target;
            return _component.FindById<T>(id);
        }

        public IUIComponent GetWrappedComponent() => _component;
        public abstract IUIComponent Clone();

        // Прозрачная переадресация интерфейса контейнера для Composite
        public IReadOnlyList<IUIComponent> Children => (_component as IContainerComponent)?.Children ?? new List<IUIComponent>().AsReadOnly();
        public void AddChild(IUIComponent child) => (_component as IContainerComponent)?.AddChild(child);
        public void RemoveChild(IUIComponent child) => (_component as IContainerComponent)?.RemoveChild(child);
    }

    public class BorderDecorator : UIComponentDecorator
    {
        public BorderDecorator(IUIComponent component) : base(component) { }

        public override void Render(IRenderingContext ctx)
        {
            Console.WriteLine($"[BorderDecorator] >>> Отрисовка рамки вокруг {BoundingBox} <<<");
            _component.Render(ctx);
        }

        public override IUIComponent Clone() => new BorderDecorator(_component.Clone());
    }

    public class RenderLogDecorator : UIComponentDecorator
    {
        public RenderLogDecorator(IUIComponent component) : base(component) { }

        public override void Render(IRenderingContext ctx)
        {
            var sw = Stopwatch.StartNew();
            _component.Render(ctx);
            sw.Stop();
            ApplicationTelemetrySingleton.Instance.LogOperation("Decorator", "RenderLog", sw.Elapsed, Id);
        }

        public override IUIComponent Clone() => new RenderLogDecorator(_component.Clone());
    }

    public class CachedRenderDecorator : UIComponentDecorator
    {
        // СТРАТЕГИЯ ИНВАЛИДАЦИИ КЭША:
        // Кэш считается валидным, пока текущий BoundingBox и Id компонента эквивалентны record-ключу RenderCacheKey.
        // Любой вызов изменения позиции SetPosition() сбрасывает валидность кэша.
        private RenderCacheKey? _cacheKey;
        private string? _cachedOutput;
        private bool _isCacheValid = false;

        public CachedRenderDecorator(IUIComponent component) : base(component) { }

        public override void Render(IRenderingContext ctx)
        {
            var currentKey = new RenderCacheKey(BoundingBox, Id);
            if (_isCacheValid && _cacheKey == currentKey)
            {
                Console.WriteLine($"[CachedRenderDecorator] (КЭШ ПОПАДАНИЕ) Возврат сохраненного представления для {Id}");
                return;
            }

            _cacheKey = currentKey;
            _cachedOutput = $"[Растровый Слепок Элемента {Id}]";
            _isCacheValid = true;
            _component.Render(ctx);
        }

        public override void SetPosition(Point position)
        {
            _isCacheValid = false; // Инвалидация кэша при перемещении
            base.SetPosition(position);
        }

        public override IUIComponent Clone() => new CachedRenderDecorator(_component.Clone());
    }
}