namespace Podsetnik;

public static class AlarmHelper
{
    public static void ScheduleAlarm(int id, string description, DateTime alarmTime)
    {
#if ANDROID
        Platforms.Android.AndroidAlarmHelper.Schedule(id, description, alarmTime);
#endif
    }

    public static void CancelAlarm(int id)
    {
#if ANDROID
        Platforms.Android.AndroidAlarmHelper.Cancel(id);
#endif
    }
}
