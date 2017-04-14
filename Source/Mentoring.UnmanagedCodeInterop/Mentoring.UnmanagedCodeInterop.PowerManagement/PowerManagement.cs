using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mentoring.UnmanagedCodeInterop.PowerManagement
{
    [ComVisible(true)]
    [Guid("4C4FE3DE-7461-435B-80BA-08EF415D2923")]
    [ClassInterface(ClassInterfaceType.None)]
    public class PowerManagement : IPowerManagement
    {
        public string GetLastSleepTime()
        {
            return PowerManagementInterop.GetLastSleepTime();
        }

        public string GetLastWakeTime()
        {
            return PowerManagementInterop.GetLastWakeTime();
        }

        public string GetSystemBatteryState()
        {
            return PowerManagementInterop.GetSystemBatteryState();
        }

        public string GetSystemPowerInformation()
        {
            return PowerManagementInterop.GetSystemPowerInformation();
        }

        public string ReserveHibernationFile()
        {
            return PowerManagementInterop.ReserveHibernationFile();
        }

        public void DeleteHibernationFile()
        {
            PowerManagementInterop.DeleteHibernationFile();
        }

        public void ForceHibernate()
        {
            PowerManagementInterop.ForceHibernate();
        }
    }
}
