using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Behavioral.Mediator
{
    public abstract class BaseValidationHandler : IValidationHandler
    {
        private IValidationHandler? _next;

        public IValidationHandler SetNext(IValidationHandler handler)
        {
            _next = handler;
            return handler;
        }

        public virtual bool Validate(UIEvent @event)
        {
            if (_next == null) return true;
            return _next.Validate(@event);
        }
    }

   
    public class SpamProtectionHandler : BaseValidationHandler
    {
        public override bool Validate(UIEvent @event)
        {
            if (@event.EventType == "SpamClick")
            {
                return false; 
            }
            return base.Validate(@event); 
        }
    }
}