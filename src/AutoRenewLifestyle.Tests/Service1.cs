using System;

namespace AutoRenewLifestyle.Tests
{
    public class Service1
    {
        public static bool ThrowExceptionInCtor = false;
        public static int CreateCount = 0;

        public Service1()
        {
            CreateCount++;
            if (ThrowExceptionInCtor)
            {
                throw new ApplicationException("Simulated error");
            }
            InstanceNumber = CreateCount;
        }

        public int InstanceNumber { get; private set; }

        public override string ToString()
        {
            return string.Format("Service1 - InstanceNumber: {0}", InstanceNumber);
        }
    }
}