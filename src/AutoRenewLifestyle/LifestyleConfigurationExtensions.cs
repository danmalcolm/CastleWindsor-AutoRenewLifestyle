using System;
using Castle.Core.Configuration;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Registration.Lifestyle;

namespace AutoRenewLifestyle
{
    public static class LifestyleConfigurationExtensions
    {
         public static ComponentRegistration<TService> AutoRenew<TService>(
             this LifestyleGroup<TService> lifestyle, TimeSpan interval) where TService : class
         {
             return lifestyle.Scoped<AutoRenewScopeAccessor>()
                 .Configuration(new MutableConfiguration("interval", TimeSpan.FromHours(1).ToString()));
         }
    }
}