using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace Podsetnik.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class AlarmReceiver : BroadcastReceiver
{
    private static PowerManager.WakeLock? _wakeLock;

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;

        var id = intent.GetIntExtra("id", 0);
        var description = intent.GetStringExtra("description") ?? "Podsetnik";

        // Acquire wake lock immediately to prevent device going back to sleep
        try
        {
            var powerManager = (PowerManager?)context.GetSystemService(Context.PowerService);
            _wakeLock = powerManager?.NewWakeLock(
                WakeLockFlags.ScreenBright | WakeLockFlags.AcquireCausesWakeup | WakeLockFlags.Full,
                "Podsetnik::AlarmReceiverWakeLock");
            _wakeLock?.Acquire(120 * 1000L); // 2 minutes
        }
        catch { }

        // Launch full-screen alarm activity with sound
        var alarmIntent = new Intent(context, typeof(AlarmActivity));
        alarmIntent.PutExtra("id", id);
        alarmIntent.PutExtra("description", description);
        alarmIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.ReorderToFront);

        var fullScreenPendingIntent = PendingIntent.GetActivity(
            context, id, alarmIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        CreateNotificationChannel(context);

        // Show notification with full-screen intent - bypasses DND
        var notification = new NotificationCompat.Builder(context, "podsetnik_alarm_channel")
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
            .SetContentTitle("PODSETNIK - ALARM")
            .SetContentText(description)
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(description))
            .SetPriority(NotificationCompat.PriorityMax)
            .SetCategory(NotificationCompat.CategoryAlarm)
            .SetFullScreenIntent(fullScreenPendingIntent, true)
            .SetAutoCancel(true)
            .SetOngoing(true)
            .SetVisibility(NotificationCompat.VisibilityPublic)
            .Build();

        var notificationManager = NotificationManagerCompat.From(context);
        notificationManager.Notify(id, notification);

        // Start alarm activity directly
        try
        {
            context.StartActivity(alarmIntent);
        }
        catch { }
    }

    public static void ReleaseWakeLock()
    {
        try
        {
            if (_wakeLock?.IsHeld == true)
                _wakeLock.Release();
        }
        catch { }
    }

    private void CreateNotificationChannel(Context context)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                "podsetnik_alarm_channel",
                "Alarm podsetnika",
                NotificationImportance.Max)
            {
                Description = "Alarm zvuk za podsetnike"
            };
            channel.EnableVibration(true);
            channel.EnableLights(true);
            channel.SetBypassDnd(true);
            channel.LockscreenVisibility = NotificationVisibility.Public;

            var manager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
            manager?.CreateNotificationChannel(channel);
        }
    }
}
