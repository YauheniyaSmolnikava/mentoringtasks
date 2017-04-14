using System;
using System.Runtime.InteropServices;
using System.Management;

namespace Mentoring.UnmanagedCodeInterop.PowerManagement
{
    #region Structures
    public struct SYSTEM_POWER_INFORMATION
    {
        public ulong MaxIdlenessAllowed;
        public ulong Idleness;
        public ulong TimeRemaining;
        public byte CoolingMode;
    }

    public struct SYSTEM_BATTERY_STATE
    {
        public byte AcOnLine;
        public byte BatteryPresent;
        public byte Charging;
        public byte Discharging;
        public byte spare1;
        public byte spare2;
        public byte spare3;
        public byte spare4;
        public uint MaxCapacity;
        public uint RemainingCapacity;
        public uint Rate;
        public uint EstimatedTime;
        public uint DefaultAlert1;
        public uint DefaultAlert2;
    }

    #endregion

    public class PowerManagementInterop
    {
        private static int informationLevel;
        const uint STATUS_SUCCESS = 0;

        #region PowerManagement Functions

        [DllImport("powrprof.dll")]
        private static extern uint CallNtPowerInformation(
            int InformationLevel,
            IntPtr lpInputBuffer,
            int nInputBufferSize,
            out SYSTEM_POWER_INFORMATION spi,
            int nOutputBufferSize
        );

        [DllImport("powrprof.dll")]
        private static extern uint CallNtPowerInformation(
            int InformationLevel,
            IntPtr lpInputBuffer,
            int nInputBufferSize,
            out SYSTEM_BATTERY_STATE sbs,
            int nOutputBufferSize
        );

        [DllImport("powrprof.dll")]
        private static extern uint CallNtPowerInformation(
             int InformationLevel,
             IntPtr lpInputBuffer,
             int nInputBufferSize,
             out ulong lpOutputBuffer,
             int nOutputBufferSize
         );

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint SetSuspendState(
            bool hibernate,
            bool forceCritical,
            bool disableWakeEvent);

        #endregion

        public static string GetLastSleepTime()
        {
            informationLevel = 15;
            ulong lpOutputBuffer;

            DateTime lastSleep = new DateTime();

            uint retval = CallNtPowerInformation(
                informationLevel,
                IntPtr.Zero,
                0,
                out lpOutputBuffer,
                Marshal.SizeOf(typeof(ulong)));

            if (retval == STATUS_SUCCESS)
            {
                if (lpOutputBuffer > 0)
                {
                    lastSleep = GetLastBootTime().AddSeconds(lpOutputBuffer / 10000000);
                    return String.Format("Last time computer went to sleep mode was at: {0}", lastSleep);
                }
                else
                {
                    return "Computer didn't go to sleep mode since last boot time";
                }
            }

            return "Error occured";
        }

        public static string GetLastWakeTime()
        {
            informationLevel = 14;
            ulong lpOutputBuffer;

            DateTime lastSleep = new DateTime();

            uint retval = CallNtPowerInformation(
                informationLevel,
                IntPtr.Zero,
                0,
                out lpOutputBuffer,
                Marshal.SizeOf(typeof(ulong)));

            if (retval == STATUS_SUCCESS)
            {
                if (lpOutputBuffer > 0)
                {
                    lastSleep = GetLastBootTime().AddSeconds(lpOutputBuffer / 10000000);
                    return String.Format("Last time computer woke up was at: {0}", lastSleep);
                }
                else
                {
                    return "Computer didn't go to sleep mode since last boot time";
                }
            }

            return "Error occured";
        }

        public static string GetSystemBatteryState()
        {
            informationLevel = 5;

            SYSTEM_BATTERY_STATE sbs;

            uint retval = CallNtPowerInformation(
                informationLevel,
                IntPtr.Zero,
                0,
                out sbs,
                Marshal.SizeOf(typeof(SYSTEM_BATTERY_STATE)));

            if (retval == STATUS_SUCCESS)
            {
                return String.Format("Is battery currently charging: {0}. Estimated time remaining on the battery: {1}", sbs.Charging == 1 ? "Yes" : "No", new TimeSpan(0, 0, (int)(sbs.EstimatedTime)));
            }

            return "Error occured";
        }

        public static string GetSystemPowerInformation()
        {
            informationLevel = 12;
            SYSTEM_POWER_INFORMATION spi;

            uint retval = CallNtPowerInformation(
                informationLevel,
                IntPtr.Zero,
                0,
                out spi,
                Marshal.SizeOf(typeof(SYSTEM_POWER_INFORMATION)));

            if (retval == STATUS_SUCCESS)
            {
                return String.Format("Idleness: {0}", spi.Idleness);
            }

            return "Error occured";
        }

        public static string ReserveHibernationFile()
        {
            informationLevel = 10;
            ulong hibernate = 0; 

            uint retval = CallNtPowerInformation(
                informationLevel,
                (IntPtr)hibernate,
                IntPtr.Size,
                out hibernate,
                Marshal.SizeOf(typeof(byte)));

            if (retval == STATUS_SUCCESS)
            {
                return String.Empty;
            }

            return "Error occured";
        }

        public static void DeleteHibernationFile()
        {
            informationLevel = 10;
            return;
        }

        public static void ForceHibernate()
        {
            var x = SetSuspendState(true, true, false);
            return;
        }

        private static DateTime GetLastBootTime()
        {
            SelectQuery query =
                new SelectQuery(@"SELECT LastBootUpTime FROM Win32_OperatingSystem WHERE Primary='true'");

            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher(query);

            DateTime bootTime = new DateTime();
            foreach (ManagementObject mo in searcher.Get())
            {
                bootTime =
                    ManagementDateTimeConverter.ToDateTime(
                        mo.Properties["LastBootUpTime"].Value.ToString());
            }

            return bootTime;
        }
    }
}
