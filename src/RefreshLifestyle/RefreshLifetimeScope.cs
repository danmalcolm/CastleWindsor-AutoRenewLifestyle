using System;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle.Scoped;

namespace ClassLibrary1
{
    public class RefreshLifetimeScope : ILifetimeScope
    {
        private Burden instance;
        private readonly TimeSpan interval;
        private DateTime lastRefresh = DateTime.MinValue;
        private DateTime nextRefresh;

        private Func<DateTime> getCurrentDate = () => DateTime.Now; 

        private readonly object @lock = new object();
        
        public RefreshLifetimeScope(TimeSpan interval)
        {
            this.interval = interval;
        }

        public void Dispose()
        {
            
        }

        public Burden GetCachedInstance(ComponentModel model, ScopedInstanceActivationCallback createInstance)
        {
            var current = getCurrentDate();
            if (current >= nextRefresh)
            {
                lock (@lock)
                {
                    if (current >= nextRefresh)
                    {
                        RefreshInstance(createInstance, current);
                    }
                }
            }
            return instance;
        }

        private void RefreshInstance(ScopedInstanceActivationCallback createInstance, DateTime current)
        {
            nextRefresh = GetNextRefreshDate(current);
            lastRefresh = current;
            instance = createInstance(b => { });
        }

        private DateTime GetNextRefreshDate(DateTime current)
        {
            if (lastRefresh == DateTime.MinValue)
            {
                lastRefresh = getCurrentDate().Date;
            }
            var next = lastRefresh;
            while (next < current)
            {
                next = next + interval;
            }
            return next;
        }
    }
}