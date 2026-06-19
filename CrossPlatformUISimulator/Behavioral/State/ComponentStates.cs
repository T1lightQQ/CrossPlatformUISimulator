using System;
using CrossPlatformUISimulator.Abstractions;
using CrossPlatformUISimulator.Common;

namespace CrossPlatformUISimulator.Behavioral.State
{
    
    public record NormalState : IComponentState
    {
        public static NormalState Instance { get; } = new();
        private NormalState() { }

        public string StateName => "NormalState";

        public void Enter(IUIComponent context) => context.Enabled = true;
        public void Exit(IUIComponent context) { }

        public void HandleClick(IUIComponent context)
        {
            
            if (context is Core.Components.ButtonComponent btn)
            {
                btn.SimulateClick();
            }
        }

        public void HandleRender(IUIComponent context, IRenderingContext ctx)
        {
            
        }
    }

    
    public record LoadingState : IComponentState
    {
        public static LoadingState Instance { get; } = new();
        private LoadingState() { }

        public string StateName => "LoadingState";

        public void Enter(IUIComponent context) => context.Enabled = false;
        public void Exit(IUIComponent context) { }

        public void HandleClick(IUIComponent context)
        {
            
        }

        public void HandleRender(IUIComponent context, IRenderingContext ctx)
        {
            
        }
    }

    
    public record ErrorState : IComponentState
    {
        public static ErrorState Instance { get; } = new();
        private ErrorState() { }

        public string StateName => "ErrorState";

        public void Enter(IUIComponent context)
        {
            context.Enabled = false;
            context.TextContent = "[ERROR] " + context.TextContent;
        }

        public void Exit(IUIComponent context)
        {
            if (context.TextContent.StartsWith("[ERROR] "))
            {
                context.TextContent = context.TextContent.Substring(8);
            }
        }

        public void HandleClick(IUIComponent context)
        {
            
        }

        public void HandleRender(IUIComponent context, IRenderingContext ctx)
        {
            
        }
    }
}