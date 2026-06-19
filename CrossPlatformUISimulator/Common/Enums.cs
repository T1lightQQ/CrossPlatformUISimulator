using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator.Common
{
    public enum WidgetType
    {
        Button,
        Label,
        Checkbox,
        Slider,
        Panel
    }

    public enum ThemeType
    {
        Fluent,
        Cupertino
    }

    
    public enum TokenType
    {
        Select,
        Execute,
        Where,
        Arrow,
        Selector,
        Predicate,
        Action,
        EOF
    }
}