using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mentoring.UnmanagedCodeInterop.PowerManagement;
using System;
using System.Runtime.InteropServices;

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
            var lastWakeTime = PowerManagementInterop.GetSystemBatteryState();
            Console.WriteLine(lastWakeTime);
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
            var lastWakeTime = PowerManagementInterop.ReserveHibernationFile();
            Console.WriteLine(lastWakeTime);
        }

        [TestMethod]
        public void TestDeleteHibernationFile()
        {
             PowerManagementInterop.DeleteHibernationFile();
            //Console.WriteLine(lastWakeTime);
        }

        [TestMethod]
        public void TestForceHibernate()
        {
            PowerManagementInterop.ForceHibernate();
        }
    }
}
