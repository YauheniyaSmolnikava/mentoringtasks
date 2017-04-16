using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mentoring.UnmanagedCodeInterop.PowerManagement;
using System;

namespace Mentoring.UnmanagedCodeInterop.Tests
{
    [TestClass]
    public class PowerManagementInteropTest
    {
        [TestMethod]
        public void TestGetLastSleepTime()
        {
            var lastSleepTime = PowerManagementInterop.GetLastSleepTime();
            Console.WriteLine(lastSleepTime);
        }

        [TestMethod]
        public void TestGetLastWakeTime()
        {
            var lastWakeTime = PowerManagementInterop.GetLastWakeTime();
            Console.WriteLine(lastWakeTime);
        }

        [TestMethod]
        public void TestGetSystemBatteryState()
        {
            var batteryInfo = PowerManagementInterop.GetSystemBatteryState();
            Console.WriteLine(batteryInfo);
        }

        [TestMethod]
        public void TestGetSystemPowerInformation()
        {
            var lastWakeTime = PowerManagementInterop.GetSystemPowerInformation();
            Console.WriteLine(lastWakeTime);
        }

        [TestMethod]
        public void TestReserveHibernationFile()
        {
            var response = PowerManagementInterop.ReserveHibernationFile(true);
            Console.WriteLine(response);
        }

        [TestMethod]
        public void TestDeleteHibernationFile()
        {
            var response = PowerManagementInterop.ReserveHibernationFile(false);
            Console.WriteLine(response);
        }

        [TestMethod]
        public void TestForceHibernate()
        {
            PowerManagementInterop.ForceHibernate();
        }
    }
}
