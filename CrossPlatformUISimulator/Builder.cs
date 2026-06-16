using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class DialogBuilder : IContainerBuilder
    {
        private string? _title;
        private IconSrc? _icon;
        private IThemeFactory? _theme;
        private readonly List<BtnConfig> _btnConfigs = new();
        private readonly List<IWidget> _widgets = new();
        private int _buttonCount = 0;
        private bool _hasTitle = false;
        private bool _hasTheme = false;

        public IContainerBuilder SetTitle(string title)
        {
            _title = title;
            _hasTitle = !string.IsNullOrEmpty(title);
            return this;
        }

        public IContainerBuilder AddButton(BtnConfig config)
        {
            _btnConfigs.Add(config);
            _buttonCount++;
            return this;
        }

        public IContainerBuilder SetIcon(IconSrc source)
        {
            _icon = source;
            return this;
        }

        public IContainerBuilder ConfigureTheme(IThemeFactory theme)
        {
            _theme = theme;
            _hasTheme = theme != null;
            return this;
        }

        public IContainerBuilder AddCustomWidget(IWidget widget)
        {
            _widgets.Add(widget);
            return this;
        }

        public IDialog Build()
        {
            if (!_hasTheme || _theme == null)
            {
                throw new InvalidOperationException("Ошибка валидации: Тема оформления не задана.");
            }
            if (!_hasTitle || string.IsNullOrEmpty(_title))
            {
                throw new InvalidOperationException("Ошибка валидации: Отсутствует обязательный заголовок.");
            }
            if (_buttonCount == 0)
            {
                throw new InvalidOperationException("Ошибка валидации: Необходимо добавить хотя бы одну кнопку.");
            }

            Counter.HeavyCount++;
            var buttons = new List<IBtn>();
            foreach (var cfg in _btnConfigs)
            {
                buttons.Add(_theme.CreateButton(cfg.Text));
            }

            return new DlgObj
            {
                Title = _title,
                Icon = _icon,
                ThemeName = _theme.ThemeName,
                Buttons = buttons,
                Widgets = new List<IWidget>(_widgets)
            };
        }
    }
}