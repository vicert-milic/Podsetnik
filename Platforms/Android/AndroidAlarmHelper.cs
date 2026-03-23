using Android.App;
using Android.Content;
using Android.OS;

namespace Podsetnik.Platforms.Android;

public static class AndroidAlarmHelper
{
    public static void Schedule(int id, string description, DateTime alarmTime)
    {
        var context = global::Android.App.Application.Context;
        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        if (alarmManager == null) return;

        var intent = new Intent(context, typeof(AlarmReceiver));
        intent.PutExtra("id", id);
        intent.PutExtra("description", description);

        var pendingIntent = PendingIntent.GetBroadcast(
            context, id, intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var triggerTime = new DateTimeOffset(alarmTime).ToUnixTimeMilliseconds();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            if (alarmManager.CanScheduleExactAlarms())
            {
                alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerTime, pendingIntent!);
            }
            else
            {
                alarmManager.SetAndAllowWhileIdle(AlarmType.RtcWakeup, triggerTime, pendingIntent!);
            }
        }
        else if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerTime, pendingIntent!);
        }
        else
        {
            alarmManager.SetExact(AlarmType.RtcWakeup, triggerTime, pendingIntent!);
        }
    }

    public static void Cancel(int id)
    {
        var context = global::Android.App.Application.Context;
        var intent = new Intent(context, typeof(AlarmReceiver));
        var pendingIntent = PendingIntent.GetBroadcast(
            context, id, intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        alarmManager?.Cancel(pendingIntent!);
    }
}
