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

        CreateNotificationChannel(context);

        var notificationIntent = new Intent(context, typeof(MainActivity));
        notificationIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
        var pendingIntent = PendingIntent.GetActivity(
            context, id, notificationIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var notification = new NotificationCompat.Builder(context, "podsetnik_channel")
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
            .SetContentTitle("Podsetnik")
            .SetContentText(description)
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(description))
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetDefaults((int)NotificationDefaults.All)
            .SetAutoCancel(true)
            .SetContentIntent(pendingIntent)
            .Build();

        var notificationManager = NotificationManagerCompat.From(context);
        notificationManager.Notify(id, notification);
    }

    private void CreateNotificationChannel(Context context)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                "podsetnik_channel",
                "Podsetnici",
                NotificationImportance.High)
            {
                Description = "Obaveštenja za podsetnike"
            };
            channel.EnableVibration(true);
            channel.EnableLights(true);

            var manager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
            manager?.CreateNotificationChannel(channel);
        }
    }
}
