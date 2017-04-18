using System;
using System.Runtime.InteropServices;
using System.Management;

namespace Mentoring.UnmanagedCodeInterop.PowerManagement
{
    #region Structures
    public struct SYSTEM_POWER_INFORMATION
    {
        public UInt32 MaxIdlenessAllowed;
        public UInt32 Idleness;
        public UInt32 TimeRemaining;
        public Char CoolingMode;
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

        const uint STATUS_SUCCESS = 0;

        #endregion

        #region PowerManagement Functions

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
            var informationLevel = 15;
            ulong lpOutputBuffer;

            var retval = GetEventTime(informationLevel, out lpOutputBuffer);

            if (retval == STATUS_SUCCESS)
            {
                return lpOutputBuffer > 0
                    ? String.Format("Last time computer went to sleep mode was at: {0}", GetLastBootTime().AddSeconds(lpOutputBuffer / 10000000))
                    : "Computer didn't go to sleep mode since last boot time";
            }

            return "Error occured";
        }

        public static string GetLastWakeTime()
        {
            var informationLevel = 14;
            ulong lpOutputBuffer;

            var retval = GetEventTime(informationLevel, out lpOutputBuffer);

            if (retval == STATUS_SUCCESS)
            {
                return lpOutputBuffer > 0
                    ? String.Format("Last time computer woke up was at: {0}", GetLastBootTime().AddSeconds(lpOutputBuffer / 10000000))
                    : "Computer didn't wake up after sleep since last boot time";
            }

            return "Error occured";
        }

        public static string GetSystemBatteryState()
        {
            var informationLevel = 5;

            var pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SYSTEM_BATTERY_STATE)));

            uint retval = CallNtPowerInformation(
                informationLevel,
                IntPtr.Zero,
                0,
                pointer,
                Marshal.SizeOf(typeof(SYSTEM_BATTERY_STATE)));

            var sbsResult = Marshal.PtrToStructure<SYSTEM_BATTERY_STATE>(pointer);
            Marshal.FreeHGlobal(pointer);

            if (retval == STATUS_SUCCESS)
            {
                if (sbsResult.BatteryPresent)
                {
                    if (sbsResult.AcOnLine)
                    {
                        return "Battery is currently charging";
                    }
                    else
                    {
                        return String.Format("Estimated time remaining on the battery: {0}", new TimeSpan(0, 0, (int)(sbsResult.EstimatedTime)));
                    }
                }

                return "Computer is on the powerline";
            }

            return "Error occured";
        }

        public static string GetSystemPowerInformation()
        {
            var informationLevel = 12;

            var pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SYSTEM_POWER_INFORMATION)));

            var retval = CallNtPowerInformation(
                informationLevel,
                IntPtr.Zero,
                0,
                pointer,
                Marshal.SizeOf(typeof(SYSTEM_POWER_INFORMATION)));

            var spiResult = Marshal.PtrToStructure<SYSTEM_POWER_INFORMATION>(pointer);
            Marshal.FreeHGlobal(pointer);

            if (retval == STATUS_SUCCESS)
            {
                return String.Format("Idleness: {0}", spiResult.Idleness);
            }

            return "Error occured";
        }

        public static string ReserveHibernationFile(bool reserve)
        {
            var informationLevel = 10;

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

        private static uint GetEventTime(int informationLevel, out ulong lpOutputBuffer)
        {
            var pointer = Marshal.AllocHGlobal(sizeof(ulong));

            uint retval = CallNtPowerInformation(
                informationLevel,
                IntPtr.Zero,
                0,
                pointer,
                Marshal.SizeOf(typeof(ulong)));

            lpOutputBuffer = Marshal.PtrToStructure<ulong>(pointer);
            Marshal.FreeHGlobal(pointer);

            return retval;
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

        #endregion
    }
}
