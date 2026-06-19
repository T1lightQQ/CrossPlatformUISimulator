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

        public IUIStyleFlyweight Flyweight { get; set; } // Разделяемое внутреннее состояние
        public string Id { get; protected set; }

        // Внешние состояния (Extrinsic States)
        public Rectangle BoundingBox { get; set; }
        public string TextContent { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public int ZIndex { get; set; } = 0;

        protected UIComponentBase(string id, Rectangle bounds, IRenderingStrategy strategy, IUIStyleFlyweight flyweight)
        {
            Id = id;
            BoundingBox = bounds;
            _renderingStrategy = strategy;
            Flyweight = flyweight;
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
                throw new InvalidOperationException("Критическая ошибка совместимости рендереров.");
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
                else if (current is ILazyComponentProxy proxy)
                {
                    var real = proxy.GetRealSubject();
                    stack.Push(real);
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
        public ButtonComponent(string id, Rectangle bounds, string text, IRenderingStrategy strategy, IUIStyleFlyweight flyweight)
            : base(id, bounds, strategy, flyweight) => TextContent = text;

        public override void Render(IRenderingContext ctx)
        {
            _renderingStrategy.DrawBackground(BoundingBox, Flyweight.Palette.Background);
            _renderingStrategy.DrawText(TextContent, Flyweight.Font, new Point(BoundingBox.X + 5, BoundingBox.Y + 5), Flyweight.Palette.Text);
        }

        public override IUIComponent Clone()
        {
            var sw = Stopwatch.StartNew();
            var clone = new ButtonComponent(Id, BoundingBox, TextContent, _renderingStrategy, Flyweight);
            ApplicationTelemetrySingleton.Instance.LogOperation("Prototype", "Clone", sw.Elapsed, $"Button:{Id}");
            return clone;
        }
    }

    public class LabelComponent : UIComponentBase, ILeafComponent
    {
        public LabelComponent(string id, Rectangle bounds, string text, IRenderingStrategy strategy, IUIStyleFlyweight flyweight)
            : base(id, bounds, strategy, flyweight) => TextContent = text;

        public override void Render(IRenderingContext ctx) =>
            _renderingStrategy.DrawText(TextContent, Flyweight.Font, new Point(BoundingBox.X, BoundingBox.Y), Flyweight.Palette.Text);

        public override IUIComponent Clone() => new LabelComponent(Id, BoundingBox, TextContent, _renderingStrategy, Flyweight);
    }

    public class SliderComponent : UIComponentBase, ILeafComponent
    {
        public int Value { get; set; } = 50;

        public SliderComponent(string id, Rectangle bounds, IRenderingStrategy strategy, IUIStyleFlyweight flyweight)
            : base(id, bounds, strategy, flyweight) { }

        public override void Render(IRenderingContext ctx)
        {
            _renderingStrategy.DrawBackground(BoundingBox, Flyweight.Palette.Background);
        }

        public override IUIComponent Clone() => new SliderComponent(Id, BoundingBox, _renderingStrategy, Flyweight) { Value = this.Value };
    }

    public class PanelComponent : UIComponentBase, IContainerComponent
    {
        private readonly List<IUIComponent> _children = new();
        public IReadOnlyList<IUIComponent> Children => _children;

        public PanelComponent(string id, Rectangle bounds, IRenderingStrategy strategy, IUIStyleFlyweight flyweight)
            : base(id, bounds, strategy, flyweight) { }

        public void AddChild(IUIComponent child) => _children.Add(child);
        public void RemoveChild(IUIComponent child) => _children.Remove(child);

        public override void Render(IRenderingContext ctx)
        {
            _renderingStrategy.DrawBackground(BoundingBox, Flyweight.Palette.Background);
            foreach (var child in _children) child.Render(ctx);
        }

        public override IUIComponent Clone()
        {
            var clonedPanel = new PanelComponent(Id, BoundingBox, _renderingStrategy, Flyweight);
            foreach (var child in _children) clonedPanel.AddChild(child.Clone());
            return clonedPanel;
        }
    }

    // --- ДЕКОРАТОРЫ ---
    public abstract class UIComponentDecorator : IUIComponent, IContainerComponent
    {
        protected readonly IUIComponent _component;
        protected UIComponentDecorator(IUIComponent component) => _component = component;

        public virtual string Id => _component.Id;
        public virtual Rectangle BoundingBox { get => _component.BoundingBox; set => _component.BoundingBox = value; }
        public virtual string TextContent { get => _component.TextContent; set => _component.TextContent = value; }
        public virtual bool Enabled { get => _component.Enabled; set => _component.Enabled = value; }
        public virtual int ZIndex { get => _component.ZIndex; set => _component.ZIndex = value; }
        public IUIStyleFlyweight Flyweight => _component.Flyweight;

        public virtual void Render(IRenderingContext ctx) => _component.Render(ctx);
        public virtual void SetPosition(Point position) => _component.SetPosition(position);

        public virtual T? FindById<T>(string id) where T : class, IUIComponent
        {
            if (Id == id && this is T target) return target;
            return _component.FindById<T>(id);
        }

        public IUIComponent GetWrappedComponent() => _component;
        public abstract IUIComponent Clone();

        public IReadOnlyList<IUIComponent> Children => (_component as IContainerComponent)?.Children ?? new List<IUIComponent>().AsReadOnly();
        public void AddChild(IUIComponent child) => (_component as IContainerComponent)?.AddChild(child);
        public void RemoveChild(IUIComponent child) => (_component as IContainerComponent)?.RemoveChild(child);
    }

    public class BorderDecorator : UIComponentDecorator
    {
        public BorderDecorator(IUIComponent component) : base(component) { }
        public override void Render(IRenderingContext ctx) => _component.Render(ctx);
        public override IUIComponent Clone() => new BorderDecorator(_component.Clone());
    }

    public class RenderLogDecorator : UIComponentDecorator
    {
        public RenderLogDecorator(IUIComponent component) : base(component) { }
        public override void Render(IRenderingContext ctx) => _component.Render(ctx);
        public override IUIComponent Clone() => new RenderLogDecorator(_component.Clone());
    }

    public class CachedRenderDecorator : UIComponentDecorator
    {
        private RenderCacheKey? _cacheKey;
        private bool _isCacheValid = false;

        public CachedRenderDecorator(IUIComponent component) : base(component) { }

        public override void Render(IRenderingContext ctx)
        {
            var currentKey = new RenderCacheKey(BoundingBox, Id);
            if (_isCacheValid && _cacheKey == currentKey) return;

            _cacheKey = currentKey;
            _isCacheValid = true;
            _component.Render(ctx);
        }

        public override void SetPosition(Point position)
        {
            _isCacheValid = false;
            base.SetPosition(position);
        }

        public override IUIComponent Clone() => new CachedRenderDecorator(_component.Clone());
    }

    // --- ПАТТЕРН PROXY: VIRTUAL PROXY ---
    public class VirtualComponentProxy : ILazyComponentProxy
    {
        private readonly WidgetConfig _config;
        private readonly IWidgetFactory _factory;
        private IRenderingStrategy _strategy;
        private IUIComponent? _realSubject;
        private readonly object _lock = new();

        public bool IsMaterialized => _realSubject != null;

        public VirtualComponentProxy(WidgetConfig config, IWidgetFactory factory, IRenderingStrategy strategy)
        {
            _config = config;
            _factory = factory;
            _strategy = strategy;
            Id = config.Id;
            BoundingBox = config.Bounds;
        }

        public string Id { get; }
        public Rectangle BoundingBox { get; set; }
        public string TextContent { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public int ZIndex { get; set; } = 0;
        public IUIStyleFlyweight Flyweight => _factory.CreateWidget(_config, _strategy, null!).Flyweight; // Быстрый доступ при необходимости

        public void Materialize()
        {
            if (_realSubject != null) return;
            lock (_lock)
            {
                if (_realSubject == null)
                {
                    // Получаем общий флайвейт через доменную фабрику при материализации
                    var flyweight = FlyweightFactory.Instance.GetFlyweight(_config.Style);
                    _realSubject = _factory.CreateWidget(_config, _strategy, flyweight);
                    _realSubject.BoundingBox = BoundingBox;
                    _realSubject.TextContent = TextContent;
                    _realSubject.Enabled = Enabled;
                    _realSubject.ZIndex = ZIndex;
                }
            }
        }

        public IUIComponent GetRealSubject()
        {
            Materialize();
            return _realSubject!;
        }

        public void Render(IRenderingContext ctx)
        {
            Materialize();
            _realSubject!.Render(ctx);
        }

        public void SetPosition(Point position)
        {
            BoundingBox = new Rectangle(position.X, position.Y, BoundingBox.Width, BoundingBox.Height);
            if (IsMaterialized) _realSubject!.SetPosition(position);
        }

        public T? FindById<T>(string id) where T : class, IUIComponent
        {
            if (Id == id && this is T self) return self;
            Materialize();
            return _realSubject!.FindById<T>(id);
        }

        public IUIComponent Clone()
        {
            return new VirtualComponentProxy(_config, _factory, _strategy)
            {
                BoundingBox = BoundingBox,
                TextContent = TextContent,
                Enabled = Enabled,
                ZIndex = ZIndex
            };
        }

        public void UpdateStrategy(IRenderingStrategy strategy)
        {
            _strategy = strategy;
            if (IsMaterialized && _realSubject is UIComponentBase baseComp)
            {
                baseComp.SwitchRenderingStrategy(strategy);
            }
        }
    }

    // --- ПАТТЕРН PROXY: PROTECTION PROXY ---
    public class ProtectionComponentProxy : IUIComponent
    {
        private readonly IUIComponent _component;
        private bool _isLocked = false;

        public ProtectionComponentProxy(IUIComponent component) => _component = component;

        public void LockComponent() => _isLocked = true;

        public string Id => _component.Id;
        public IUIStyleFlyweight Flyweight => _component.Flyweight;

        public Rectangle BoundingBox
        {
            get => _component.BoundingBox;
            set { ThrowIfLocked(); _component.BoundingBox = value; }
        }
        public string TextContent
        {
            get => _component.TextContent;
            set { ThrowIfLocked(); _component.TextContent = value; }
        }
        public bool Enabled
        {
            get => _component.Enabled;
            set { ThrowIfLocked(); _component.Enabled = value; }
        }
        public int ZIndex
        {
            get => _component.ZIndex;
            set { ThrowIfLocked(); _component.ZIndex = value; }
        }

        public void Render(IRenderingContext ctx) => _component.Render(ctx);
        public void SetPosition(Point position) { ThrowIfLocked(); _component.SetPosition(position); }

        public T? FindById<T>(string id) where T : class, IUIComponent
        {
            if (Id == id && this is T self) return self;
            return _component.FindById<T>(id);
        }

        public IUIComponent Clone()
        {
            var clone = new ProtectionComponentProxy(_component.Clone());
            if (_isLocked) clone.LockComponent();
            return clone;
        }

        private void ThrowIfLocked()
        {
            if (_isLocked) throw new InvalidOperationException("Защитная ошибка прокси: Компонент заблокирован для изменений!");
        }
    }
}