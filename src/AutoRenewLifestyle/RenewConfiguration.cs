using System;
using Castle.Core;
using Castle.Core.Configuration;

namespace AutoRenewLifestyle
{
    public class RenewConfiguration
    {
        private static readonly TimeSpan DefaultRefreshInterval = TimeSpan.FromHours(1);
        public static readonly string IntervalConfigurationKey = "interval";


        public static RenewConfiguration ForComponent(ComponentModel component)
        {
            return new RenewConfiguration(component.Configuration);
        }

        public RenewConfiguration(IConfiguration configuration)
        {
            if (configuration != null && configuration.Name == IntervalConfigurationKey)
            {
                Interval = TimeSpan.Parse(configuration.Value);
            }
            else
            {
                Interval = DefaultRefreshInterval;
            }
        }

        public TimeSpan Interval { get; private set; }
    }
}