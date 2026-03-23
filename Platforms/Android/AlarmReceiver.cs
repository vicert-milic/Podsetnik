using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace Podsetnik.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class AlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;

        var id = intent.GetIntExtra("id", 0);
        var description = intent.GetStringExtra("description") ?? "Podsetnik";

        // Launch full-screen alarm activity with sound
        var alarmIntent = new Intent(context, typeof(AlarmActivity));
        alarmIntent.PutExtra("id", id);
        alarmIntent.PutExtra("description", description);
        alarmIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);

        var fullScreenPendingIntent = PendingIntent.GetActivity(
            context, id, alarmIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        CreateNotificationChannel(context);

        // Also show notification with full-screen intent (for when screen is locked)
        var notification = new NotificationCompat.Builder(context, "podsetnik_alarm_channel")
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
            .SetContentTitle("Podsetnik - ALARM")
            .SetContentText(description)
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(description))
            .SetPriority(NotificationCompat.PriorityMax)
            .SetCategory(NotificationCompat.CategoryAlarm)
            .SetFullScreenIntent(fullScreenPendingIntent, true)
            .SetAutoCancel(true)
            .SetOngoing(true)
            .Build();

        var notificationManager = NotificationManagerCompat.From(context);
        notificationManager.Notify(id, notification);

        // Also try to start the alarm activity directly
        try
        {
            context.StartActivity(alarmIntent);
        }
        catch
        {
            // If can't start activity directly, full-screen intent on notification will handle it
        }
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
