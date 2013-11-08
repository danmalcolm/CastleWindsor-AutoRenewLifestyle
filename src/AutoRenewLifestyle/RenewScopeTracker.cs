using System;
using Castle.Core;
using Castle.MicroKernel.Lifestyle.Scoped;

namespace AutoRenewLifestyle
{
    /// <summary>
    /// Manages lifetime of the scope of a single Component
    /// </summary>
    public class RenewScopeTracker
    {
        #region Current date hook
        private static Func<DateTime> getCurrentDate;

        static RenewScopeTracker()
        {
            ResetSystemTimeOverride();
        }
        
        public static void OverrideSystemTime(Func<DateTime> getDate)
        {
            getCurrentDate = getDate;
        }

        public static void ResetSystemTimeOverride()
        {
            getCurrentDate = () => DateTime.Now; 
        }

        #endregion

        private readonly RenewConfiguration configuration;
        private DateTime lastRefreshDate = DateTime.MinValue;
        private DateTime nextRefreshDate;
        private readonly object @lock = new object();
        private ILifetimeScope current;

        public RenewScopeTracker(ComponentModel component)
        {
            this.configuration = RenewConfiguration.ForComponent(component);
        }

        public ILifetimeScope GetCurrent()
        {
            var currentDate = getCurrentDate();
            if (currentDate >= nextRefreshDate)
            {
                lock (@lock)
                {
                    // Ensure refresh hasn't happened on another thread while
                    // current thread awaiting lock
                    if (currentDate >= nextRefreshDate)
                    {
                        RefreshScope(currentDate);
                        lastRefreshDate = currentDate;
                    }
                }
            }
            return current;
        }

        private void RefreshScope(DateTime currentDate)
        {
            if (current != null)
            {
                current.Dispose();
            }
            nextRefreshDate = GetNextRefreshDate(currentDate);
            current = new ThreadSafeDefaultLifetimeScope();
        }

        private DateTime GetNextRefreshDate(DateTime currentDate)
        {
            if (lastRefreshDate == DateTime.MinValue)
            {
                lastRefreshDate = getCurrentDate().Date;
            }
            var refreshDate = lastRefreshDate;
            while (refreshDate <= currentDate)
            {
                refreshDate = refreshDate + configuration.Interval;
            }
            return refreshDate;
        }

        public void Dispose()
        {
            if(current != null)
                current.Dispose();
        }

    }
}