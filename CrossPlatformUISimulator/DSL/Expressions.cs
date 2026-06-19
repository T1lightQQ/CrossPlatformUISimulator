using System;
using System.Collections.Generic;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Behavioral.Command;
using CrossPlatformUISimulator.Behavioral.Iterator;
using CrossPlatformUISimulator.Core.Components;

namespace CrossPlatformUISimulator.DSL
{
    // Интерфейс выражения в паттерне Interpreter
    public interface IExpression
    {
        void Interpret(UIInterpreterContext context);
    }

    public readonly record struct TypeSelectorExpression(string Selector) : IExpression
    {
        public void Interpret(UIInterpreterContext context) { }
    }

    public readonly record struct PredicateExpression(string Predicate) : IExpression
    {
        public void Interpret(UIInterpreterContext context) { }
    }

    public readonly record struct ActionExpression(string ActionText) : IExpression
    {
        public void Interpret(UIInterpreterContext context) { }
    }

    // Выражение SELECT
    public record SelectExpression(TypeSelectorExpression TypeSelector, WhereExpression? WhereExpr) : IExpression
    {
        public void Interpret(UIInterpreterContext context)
        {
            var iteratorFactory = new IteratorFactory();
            var iterator = iteratorFactory.CreateDfs(context.Facade.Root);
            var results = new List<IUIComponent>();
            string targetType = TypeSelector.Selector;

            while (iterator.MoveNext())
            {
                var comp = (IUIComponent)iterator.Current;
                bool isMatch = targetType == "<*>" ||
                              (targetType == "<Button>" && comp is ButtonComponent) ||
                              (targetType == "<Label>" && comp is LabelComponent);

                if (isMatch) results.Add(comp);
            }

            context.SelectedComponents = results;
            WhereExpr?.Interpret(context);
        }
    }

    // Выражение WHERE
    public record WhereExpression(PredicateExpression PredicateExpr) : IExpression
    {
        public void Interpret(UIInterpreterContext context)
        {
            var filtered = new List<IUIComponent>();
            string condition = PredicateExpr.Predicate;

            foreach (var comp in context.SelectedComponents)
            {
                bool matches = condition == "Visible=true" || (condition == "Enabled=false" && !comp.Enabled);
                if (matches) filtered.Add(comp);
            }
            context.SelectedComponents = filtered;
        }
    }

    // Выражение EXECUTE
    public record ExecuteExpression(ChainExpression Chain) : IExpression
    {
        public void Interpret(UIInterpreterContext context) => Chain.Interpret(context);
    }

    // Выражение цепочки действий (Action1 -> Action2)
    public record ChainExpression(List<ActionExpression> Actions) : IExpression
    {
        public void Interpret(UIInterpreterContext context)
        {
            var aggregateCommands = new List<IUICommand>();

            foreach (var comp in context.SelectedComponents)
            {
                var innerCommands = new List<IUICommand>();
                foreach (var action in Actions)
                {
                    if (action.ActionText.StartsWith("ApplyTheme"))
                    {
                        innerCommands.Add(new ApplyThemeCommand(comp, new StyleKey("Calibri", 14, 30, 30, 30)));
                    }
                    else if (action.ActionText.StartsWith("SetPosition"))
                    {
                        innerCommands.Add(new MoveComponentCommand(comp, new Point(10, 20)));
                    }
                }

                if (innerCommands.Count > 0)
                    aggregateCommands.Add(new MacroCommand(innerCommands.ToArray()));
            }

            if (aggregateCommands.Count > 0)
                context.CommandManager.Execute(new MacroCommand(aggregateCommands.ToArray()));
        }
    }

    // Корневой конвейер (SELECT -> EXECUTE)
    public record PipelineExpression(SelectExpression Select, ExecuteExpression Execute) : IExpression
    {
        public void Interpret(UIInterpreterContext context)
        {
            Select.Interpret(context);
            Execute.Interpret(context);
        }
    }
}