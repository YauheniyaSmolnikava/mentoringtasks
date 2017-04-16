using System.Runtime.InteropServices;

namespace Mentoring.UnmanagedCodeInterop.PowerManagement
{
    [ComVisible(true)]
    [Guid("E3D713B4-2D10-4868-8CF6-ED694A501437")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IPowerManagement
    {
        string GetLastSleepTime();

        string GetLastWakeTime();

        string GetSystemBatteryState();

        string GetSystemPowerInformation();

        string ReserveHibernationFile(bool reserve);

        void ForceHibernate();
    }
}
