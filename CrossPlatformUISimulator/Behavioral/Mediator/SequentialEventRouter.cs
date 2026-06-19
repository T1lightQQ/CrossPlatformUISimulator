using System.Collections.Generic;
using CrossPlatformUISimulator.Common;
using CrossPlatformUISimulator.Abstractions;

namespace CrossPlatformUISimulator.Behavioral.Mediator
{
    public class SequentialEventRouter : IEventRouter
    {
        public void Route(UIEvent e, IEnumerable<Subscription> subs)
        {
            foreach (var s in subs)
            {
                if (s.Filter(e))
                {
                    s.Handler(e);
                }
            }
        }
    }
}