using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossPlatformUISimulator
{
    public class UISystemFacade : IUISystemFacade
    {
        private readonly IContainerBuilder _builder;
        private readonly CommandManager _commandManager;
        private IContainerComponent? _root;

        public IContainerComponent RootTree => _root ?? throw new InvalidOperationException("Древо компонентов не сформировано.");

        public UISystemFacade(IContainerBuilder builder, CommandManager cmdManager)
        {
            _builder = builder;
            _commandManager = cmdManager;
        }

        public IContainerComponent CreateDialog(DialogPreset preset, ThemeType theme)
        {
            _root = _builder.SetId("MainWindow").SetBounds(preset.Bounds).Build();
            return _root;
        }

        public void ExecuteDsl(string script)
        {
            var scanner = new Scanner(script);
            var tokens = scanner.ScanTokens();
            var parser = new ScriptParser(tokens);

            var astRoot = parser.ParseExpressionTree();
            var context = new UIInterpreterContext(this, _commandManager, TelemetrySingleton.Instance);

            astRoot.Interpret(context);
        }
    }
}