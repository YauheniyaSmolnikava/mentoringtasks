Sub TestPowerManagement_GetLastSleepTime()

Dim manager As New PowerManagement

lastSleepTime = manager.GetLastSleepTime()

MsgBox lastSleepTime

End Sub

Sub TestPowerManagement_GetLastWakeTime()

Dim manager As New PowerManagement

lastWakeTime = manager.GetLastWakeTime()

MsgBox lastWakeTime

End Sub


Sub TestPowerManagement_GetSystemBatteryState()

Dim manager As New PowerManagement

batterState = manager.GetSystemBatteryState()

MsgBox batterState

End Sub

Sub TestPowerManagement_GetSystemPowerInformation()

Dim manager As New PowerManagement

powerInfo = manager.GetSystemPowerInformation()

MsgBox powerInfo

End Sub

Sub TestPowerManagement_ReserveHibernationFile()

Dim manager As New PowerManagement

reserveInfo = manager.ReserveHibernationFile(True)

MsgBox reserveInfo

End Sub

Sub TestPowerManagement_RemoveHibernationFile()

Dim manager As New PowerManagement

reserveInfo = manager.ReserveHibernationFile(False)

MsgBox reserveInfo

End Sub

Sub TestPowerManagement_ForceHibernate()

Dim manager As New PowerManagement

manager.ForceHibernate

End Sub
