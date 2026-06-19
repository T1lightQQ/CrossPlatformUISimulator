using System;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Abstractions
{
    // Часть 32: Каркас процесса с жестко зафиксированной последовательностью шагов
    // Внимание: Данный жизненный цикл не является потокобезопасным из соображений производительности. 
    // Синхронизация вызовов должна обеспечиваться вызывающим потоком.
    public abstract class UIComponentLifecycleBase
    {
        protected readonly IUIComponent TargetComponent;

        protected UIComponentLifecycleBase(IUIComponent targetComponent)
        {
            TargetComponent = targetComponent ?? throw new ArgumentNullException(nameof(targetComponent));
        }

        // Исправлено: убран модификатор sealed. Метод не virtual, поэтому переопределить его и так нельзя.
        public void ExecuteLifecycle(UIContext ctx)
        {
            Initialize(ctx);
            NotifyStep("Initialized", ctx);

            LoadResources(ctx);
            NotifyStep("ResourcesLoaded", ctx);

            ApplyTheme(ctx);
            NotifyStep("ThemeApplied", ctx);

            if (!Validate(ctx))
            {
                OnValidationFailed(ctx);
                NotifyStep("ValidationFailed", ctx);
                return;
            }
            NotifyStep("ValidationPassed", ctx);

            PreRender(ctx);
            Render(ctx);
            PostRender(ctx);
            NotifyStep("RenderCompleted", ctx);

            Cleanup(ctx);
            NotifyStep("CleanedUp", ctx);
        }

        protected abstract void Initialize(UIContext ctx);
        protected virtual void LoadResources(UIContext ctx) { /* Базовый пустой хук */ }
        protected virtual void ApplyTheme(UIContext ctx) { /* Базовый пустой хук */ }
        protected abstract bool Validate(UIContext ctx);
        protected virtual void OnValidationFailed(UIContext ctx) { /* Хук обработки ошибок валидации */ }
        protected virtual void PreRender(UIContext ctx) { /* Хук подготовки рендеринга */ }
        protected abstract void Render(UIContext ctx);
        protected virtual void PostRender(UIContext ctx) { /* Хук пост-обработки изображений */ }
        protected virtual void Cleanup(UIContext ctx) { /* Базовый хук очистки памяти */ }

        private void NotifyStep(string stepName, UIContext ctx)
        {
            TargetComponent.Notify(new UIStateChangeData("LifecycleStep", "Processing", stepName, DateTime.UtcNow));
            if (ctx.SharedMetrics.ContainsKey(stepName))
                ctx.SharedMetrics[stepName] = (int)ctx.SharedMetrics[stepName] + 1;
            else
                ctx.SharedMetrics[stepName] = 1;
        }
    }
}