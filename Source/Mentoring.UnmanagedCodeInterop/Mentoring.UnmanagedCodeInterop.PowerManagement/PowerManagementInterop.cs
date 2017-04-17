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
        public bool AcOnLine;
        public bool BatteryPresent;
        public bool Charging;
        public bool Discharging;
        public bool spare1;
        public bool spare2;
        public bool spare3;
        public bool spare4;
        public uint MaxCapacity;
        public uint RemainingCapacity;
        public uint Rate;
        public uint EstimatedTime;
        public uint DefaultAlert1;
        public uint DefaultAlert2;
    }

    #endregion

    internal class PowerManagementInterop
    {
        #region Fields

        private static int informationLevel;
        const uint STATUS_SUCCESS = 0;

        #endregion

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

        [DllImport("powrprof.dll")]
        private static extern uint CallNtPowerInformation(
             int InformationLevel,
             IntPtr lpInputBuffer,
             int nInputBufferSize,
             IntPtr lpOutputBuffer,
             int nOutputBufferSize
         );

        [DllImport("powrprof.dll")]
        private static extern uint SetSuspendState(
            bool hibernate,
            bool forceCritical,
            bool disableWakeEvent);

        #endregion

        #region API Methods

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
                    return "Computer didn't wake up after sleep since last boot time";
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
                if (sbs.BatteryPresent)
                {
                    if (sbs.AcOnLine)
                    {
                        return "Battery is currently charging";
                    }
                    else
                    {
                        return String.Format("Estimated time remaining on the battery: {0}", new TimeSpan(0, 0, (int)(sbs.EstimatedTime)));
                    }
                }

                return "Computer is on the powerline";
            }

            return "Error occured";
        }

        public static string GetSystemPowerInformation()
        {
            informationLevel = 12;
            SYSTEM_POWER_INFORMATION spi;

            var retval = CallNtPowerInformation(
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

        public static string ReserveHibernationFile(bool reserve)
        {
            informationLevel = 10;

            int hiberParam = reserve ? 1 : 0;
            var pointer = Marshal.AllocHGlobal(sizeof(int));
            Marshal.WriteInt32(pointer, hiberParam);

            ulong retval = CallNtPowerInformation(
                informationLevel,
                pointer,
                Marshal.SizeOf(typeof(int)),
                (IntPtr)null,
                0);

            Marshal.FreeHGlobal(pointer);

            if (retval == STATUS_SUCCESS)
            {
                return "Operation successfully completed";
            }

            return "Error occured";
        }

        public static void ForceHibernate()
        {
            SetSuspendState(true, true, false);
        }

        #endregion

        #region Private Methods

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

        #endregion
    }
}
