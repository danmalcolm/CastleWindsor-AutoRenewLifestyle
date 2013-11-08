using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Releasers;
using Castle.Windsor;
using FluentAssertions;
using Xunit;

namespace AutoRenewLifestyle.Tests
{
    public abstract class base_context : IDisposable
    {
        public void Dispose()
        {
            Service1.ThrowExceptionInCtor = false;
            Service1.CreateCount = 0;
            RenewScopeTracker.ResetSystemTimeOverride();
        }
    }

    namespace Configuration
    {
        public class when_configuring_lifestyle : base_context
        {
            [Fact]
            public void should_register_nice_api()
            {
                var container = new WindsorContainer();
                container.Register(
                    Component.For<Service1>()
                        .LifeStyle.AutoRenew(TimeSpan.FromHours(1))
                );
            }
        }
    }

    namespace ComponentCreation
    {
        public class when_resolving_component_with_refresh_lifestyle : base_context
        {
            private readonly WindsorContainer container;

            public when_resolving_component_with_refresh_lifestyle()
            {
                container = new WindsorContainer();
                container.Register(
                    Component.For<Service1>()
                        .LifeStyle.AutoRenew(TimeSpan.FromHours(1))
                );
            }

            [Fact]
            public void should_create_on_subsequent_attempts_to_resolve_if_exception_thrown_during_creation()
            {
                Service1.ThrowExceptionInCtor = true;
                try
                {
                    container.Resolve<Service1>();
                }
                catch
                {
                }
                try
                {
                    container.Resolve<Service1>();
                }
                catch
                {
                }

                Service1.ThrowExceptionInCtor = false;
                var s = container.Resolve<Service1>();
                s = container.Resolve<Service1>();
                Service1.CreateCount.Should().Be(3, "it should attempt to create component until resolved");
            }

            [Fact]
            public void should_create_single_instance_when_resolving_concurrently_on_multiple_threads()
            {
                var values = new ConcurrentDictionary<int, Service1>();
                Parallel.ForEach(Enumerable.Range(0, 1000), index =>
                    {
                        var service = container.Resolve<Service1>();
                        values.AddOrUpdate(index, service, (key, existing) => service);
                    });
                Service1.CreateCount.Should().Be(1, "it should create single component instance");
            }
        }
    }

    namespace ComponentRefresh
    {
        public class when_resolving_component_at_different_times : base_context
        {
            private readonly WindsorContainer container;
            private DateTime currentTime = new DateTime(2013, 1, 1);

            public when_resolving_component_at_different_times()
            {
                RenewScopeTracker.OverrideSystemTime(() => currentTime);

                container = new WindsorContainer();
                container.Register(
                    Component.For<Service1>().LifeStyle.AutoRenew(TimeSpan.FromHours(1))
                );
            }

            [Fact]
            public void should_resolve_same_instance_within_specified_refresh_interval()
            {
                var instance1 = container.Resolve<Service1>();
                var instance2 = container.Resolve<Service1>();

                instance1.Should().BeSameAs(instance2, "it should resolve same instance");
                Service1.CreateCount.Should().Be(1, "it should create single instance of component");
            }

            [Fact]
            public void should_resolve_a_different_instances_within_each_refresh_interval()
            {
                var instance1 = container.Resolve<Service1>();
                var instance2 = container.Resolve<Service1>();
                currentTime = currentTime.AddHours(2);
                var instance3 = container.Resolve<Service1>();
                var instance4 = container.Resolve<Service1>();
                currentTime = currentTime.AddHours(2);
                var instance5 = container.Resolve<Service1>();
                var instance6 = container.Resolve<Service1>();

                instance1.Should().BeSameAs(instance2, "it should resolve same instance during each refresh interval");
                instance3.Should().BeSameAs(instance4, "it should resolve same instance during each refresh interval");
                instance5.Should().BeSameAs(instance6, "it should resolve same instance during each refresh interval");

                instance1.Should().NotBeSameAs(instance3, "it should resolve different instance during different refresh interval");
                instance1.Should().NotBeSameAs(instance5, "it should resolve different instance during different refresh interval");
                instance3.Should().NotBeSameAs(instance5, "it should resolve different instance during different refresh interval");
                
                Service1.CreateCount.Should().Be(3, "it should create single instance of component for each interval");
            }
        }
    }

    namespace ComponentRelease
    {
        public class when_component_renewed
        {
            private readonly WindsorContainer container;
            private DateTime currentTime = new DateTime(2013, 1, 1);

            public when_component_renewed()
            {
                RenewScopeTracker.OverrideSystemTime(() => currentTime);

                // AutoRenew lifestyle components won't be tracked, but we need to 
                // ensure that tracked dependencies are released
                container = new WindsorContainer();
                container.Register(
                    Component.For<DisposableService>().LifeStyle.Transient,
                    Component.For<Service2>().LifeStyle.AutoRenew(TimeSpan.FromHours(1))
                );
            }
            
            [Fact]
            public void should_decomission_previous_instance_following_renewal()
            {
                var instance1 = container.Resolve<Service2>();
                currentTime = currentTime.AddHours(2);
                var instance2 = container.Resolve<Service2>();
                currentTime = currentTime.AddHours(2);
                var instance3 = container.Resolve<Service2>();

                container.Kernel.ReleasePolicy.HasTrack(instance1.Service).Should().BeFalse("it should release burden of previous instance");
                container.Kernel.ReleasePolicy.HasTrack(instance2.Service).Should().BeFalse("it should release burden of previous instance");
                container.Kernel.ReleasePolicy.HasTrack(instance3.Service).Should().BeTrue("it should track burden of current instance");
            }
        }
    }
}
