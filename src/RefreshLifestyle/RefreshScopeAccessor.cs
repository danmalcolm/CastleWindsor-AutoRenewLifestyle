using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle.Scoped;

namespace ClassLibrary1
{
    public class RefreshScopeAccessor : IScopeAccessor
    {
        private TimeSpan lifeTime = TimeSpan.FromMinutes(5);
        
        public void Dispose()
        {
           
        }

        public ILifetimeScope GetScope(CreationContext context)
        {
            return new RefreshLifetimeScope(TimeSpan.FromMinutes(10));
        }
    }
}
