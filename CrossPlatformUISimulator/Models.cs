using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public enum WidgetType
    {
        Btn,
        Txt,
        Slider
    }

    public class WidgetConfig
    {
        public required WidgetType Type { get; init; }
        public required string Theme { get; init; }
    }

    public class BtnConfig
    {
        public required string Text { get; init; }
    }

    public class IconSrc
    {
        public required string Path { get; init; }
    }
}