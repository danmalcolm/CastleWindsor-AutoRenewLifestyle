using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using ClassLibrary1;
using Xunit;

namespace CachedLifestyle.Tests
{
    public class RefreshLifestyleTests
    {
        [Fact]
        public void should_register_nice()
        {
            var container = new WindsorContainer();
            container.Register(
                Component.For<IService1>()
                    .ImplementedBy<Service1>()
                    .LifestyleScoped<RefreshScopeAccessor>()
            );
        }

        [Fact]
        public void should_attempt_refresh_on_subsequent_attempts_to_resolve_if_creation_fails()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void should_create_single_instance_when_creating_concurrently()
        {
            throw new NotImplementedException();
        }


    }

    public interface IService1
    {
    }

    class Service1 : IService1
    {
    }
}
