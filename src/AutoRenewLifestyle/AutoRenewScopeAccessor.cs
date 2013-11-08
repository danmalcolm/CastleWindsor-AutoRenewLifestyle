using System;
using System.Collections.Concurrent;
using Castle.Core;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle.Scoped;

namespace AutoRenewLifestyle
{
    public class AutoRenewScopeAccessor : IScopeAccessor
    {
        private readonly ConcurrentDictionary<ComponentModel, RenewScopeTracker> trackers = new ConcurrentDictionary<ComponentModel, RenewScopeTracker>();

        public ILifetimeScope GetScope(CreationContext context)
        {
            var component = context.Handler.ComponentModel;
            var tracker = trackers.GetOrAdd(component, c => new RenewScopeTracker(c));
            return tracker.GetCurrent();
        }

        public void Dispose()
        {
            foreach (var item in trackers)
            {
                item.Value.Dispose();
            }
            trackers.Clear();
        }
    }
}
