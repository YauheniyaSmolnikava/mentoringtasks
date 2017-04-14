using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mentoring.UnmanagedCodeInterop.PowerManagement
{
    public interface IPowerManagement
    {
        string GetLastSleepTime();

        string GetLastWakeTime();

        string GetSystemBatteryState();

        string GetSystemPowerInformation();

        string ReserveHibernationFile();

        void DeleteHibernationFile();

        void ForceHibernate();
    }
}
