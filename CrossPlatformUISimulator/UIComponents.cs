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
        protected IRenderingStrategy Strategy;
        public string Id { get; }
        public Rectangle BoundingBox { get; set; }
        public string TextContent { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public IUIStyleFlyweight Flyweight { get; set; }

        protected UIComponentBase(string id, Rectangle bounds, IRenderingStrategy strategy, IUIStyleFlyweight flyweight)
        {
            Id = id;
            BoundingBox = bounds;
            Strategy = strategy;
            Flyweight = flyweight;
        }

        public abstract void Render(IRenderingContext ctx);
        public void SetPosition(Point pos) => BoundingBox = new Rectangle(pos.X, pos.Y, BoundingBox.Width, BoundingBox.Height);

        public T? FindById<T>(string id) where T : class, IUIComponent
        {
            if (Id == id && this is T target) return target;
            return null;
        }

        public abstract IUIComponent Clone();
    }

    public class ButtonComponent : UIComponentBase
    {
        public ButtonComponent(string id, Rectangle bounds, IRenderingStrategy strategy, IUIStyleFlyweight flyweight)
            : base(id, bounds, strategy, flyweight) { }

        public override void Render(IRenderingContext ctx) => Strategy.DrawBackground(BoundingBox, Flyweight.Palette.Background);
        public override IUIComponent Clone() => new ButtonComponent(Id, BoundingBox, Strategy, Flyweight) { Enabled = Enabled, TextContent = TextContent };
    }

    public class LabelComponent : UIComponentBase
    {
        public LabelComponent(string id, Rectangle bounds, IRenderingStrategy strategy, IUIStyleFlyweight flyweight)
            : base(id, bounds, strategy, flyweight) { }

        public override void Render(IRenderingContext ctx) { }
        public override IUIComponent Clone() => new LabelComponent(Id, BoundingBox, Strategy, Flyweight) { Enabled = Enabled };
    }

    public class PanelComponent : UIComponentBase, IContainerComponent
    {
        private readonly List<IUIComponent> _children = new();
        public IReadOnlyList<IUIComponent> Children => _children;

        public PanelComponent(string id, Rectangle bounds, IRenderingStrategy strategy, IUIStyleFlyweight flyweight)
            : base(id, bounds, strategy, flyweight) { }

        public void AddChild(IUIComponent child) => _children.Add(child);
        public void RemoveChild(IUIComponent child) => _children.Remove(child);
        public void ReplaceChild(string id, IUIComponent newChild)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i].Id == id) { _children[i] = newChild; return; }
            }
        }

        public override void Render(IRenderingContext ctx)
        {
            foreach (var child in _children) child.Render(ctx);
        }

        public override IUIComponent Clone()
        {
            var clone = new PanelComponent(Id, BoundingBox, Strategy, Flyweight);
            foreach (var child in _children) clone.AddChild(child.Clone());
            return clone;
        }
    }

    // --- ДЕКОРАТОР КОМПОНЕНТОВ ---
    public abstract class UIComponentDecorator : IUIComponent
    {
        protected IUIComponent Component;
        protected UIComponentDecorator(IUIComponent component) => Component = component;

        public string Id => Component.Id;
        public Rectangle BoundingBox { get => Component.BoundingBox; set => Component.BoundingBox = value; }
        public string TextContent { get => Component.TextContent; set => Component.TextContent = value; }
        public bool Enabled { get => Component.Enabled; set => Component.Enabled = value; }
        public IUIStyleFlyweight Flyweight => Component.Flyweight;

        public virtual void Render(IRenderingContext ctx) => Component.Render(ctx);
        public void SetPosition(Point position) => Component.SetPosition(position);
        public T? FindById<T>(string id) where T : class, IUIComponent => Component.FindById<T>(id);
        public IUIComponent GetWrappedComponent() => Component;
        public abstract IUIComponent Clone();
    }

    public class BorderDecorator : UIComponentDecorator
    {
        public BorderDecorator(IUIComponent component) : base(component) { }
        public override IUIComponent Clone() => new BorderDecorator(Component.Clone());
    }

    // --- ЛЕНИВЫЙ ВИРТУАЛЬНЫЙ ПРОКСИ ---
    public class VirtualComponentProxy : ILazyComponentProxy
    {
        private IUIComponent? _realSubject;
        public string Id { get; }
        public bool IsMaterialized => _realSubject != null;
        public Rectangle BoundingBox { get; set; }
        public string TextContent { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public IUIStyleFlyweight Flyweight => _realSubject?.Flyweight!;

        public VirtualComponentProxy(string id, Rectangle bounds)
        {
            Id = id;
            BoundingBox = bounds;
        }

        public void Materialize()
        {
            if (_realSubject == null)
            {
                _realSubject = new ButtonComponent(Id, BoundingBox, new DummyRasterStrategy(), FlyweightFactory.Instance.GetFlyweight(new StyleKey("Arial", 12, 0, 0, 0)));
            }
        }

        public IUIComponent GetRealSubject() { Materialize(); return _realSubject!; }
        public void Render(IRenderingContext ctx) => GetRealSubject().Render(ctx);
        public void SetPosition(Point pos) => BoundingBox = new Rectangle(pos.X, pos.Y, BoundingBox.Width, BoundingBox.Height);
        public T? FindById<T>(string id) where T : class, IUIComponent => Id == id ? this as T : _realSubject?.FindById<T>(id);
        public IUIComponent Clone() => new VirtualComponentProxy(Id, BoundingBox);
    }
}