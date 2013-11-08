using System;

namespace AutoRenewLifestyle.Tests
{
    public class Service2
    {
        public static int CreateCount = 0;

        public Service2(DisposableService service1)
        {
            CreateCount++;
            InstanceNumber = CreateCount;
            Service = service1;
        }

        public int InstanceNumber { get; private set; }

        public DisposableService Service { get; private set; }

        public override string ToString()
        {
            return string.Format("Service1 - InstanceNumber: {0}", InstanceNumber);
        }
    }

    public class DisposableService : IDisposable
    {
        public void Dispose()
        {
            
        }
    }
}