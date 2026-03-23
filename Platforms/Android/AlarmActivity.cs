using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Podsetnik.Platforms.Android;

[Activity(Theme = "@android:style/Theme.DeviceDefault.Light.NoActionBar",
    ShowOnLockScreen = true,
    TurnScreenOn = true,
    LaunchMode = global::Android.Content.PM.LaunchMode.SingleInstance)]
public class AlarmActivity : global::Android.App.Activity
{
    private Ringtone? _ringtone;
    private Vibrator? _vibrator;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.OMr1)
        {
            SetShowWhenLocked(true);
            SetTurnScreenOn(true);
        }

        Window?.AddFlags(WindowManagerFlags.KeepScreenOn |
                         WindowManagerFlags.ShowWhenLocked |
                         WindowManagerFlags.TurnScreenOn |
                         WindowManagerFlags.Fullscreen);

        var description = Intent?.GetStringExtra("description") ?? "Podsetnik!";

        var layout = new LinearLayout(this)
        {
            Orientation = global::Android.Widget.Orientation.Vertical
        };
        layout.SetGravity(GravityFlags.Center);
        layout.SetBackgroundColor(global::Android.Graphics.Color.ParseColor("#2E7D32"));
        layout.SetPadding(60, 60, 60, 60);

        var alarmIcon = new TextView(this)
        {
            Text = "\u23F0",
            TextSize = 80,
            Gravity = GravityFlags.Center
        };
        layout.AddView(alarmIcon);

        var titleText = new TextView(this)
        {
            Text = "PODSETNIK",
            TextSize = 32,
            Gravity = GravityFlags.Center
        };
        titleText.SetTextColor(global::Android.Graphics.Color.White);
        titleText.SetPadding(0, 40, 0, 20);
        layout.AddView(titleText);

        var descText = new TextView(this)
        {
            Text = description,
            TextSize = 22,
            Gravity = GravityFlags.Center
        };
        descText.SetTextColor(global::Android.Graphics.Color.White);
        descText.SetPadding(0, 0, 0, 60);
        layout.AddView(descText);

        var dismissButton = new global::Android.Widget.Button(this)
        {
            Text = "UGASI ALARM"
        };
        dismissButton.SetBackgroundColor(global::Android.Graphics.Color.White);
        dismissButton.SetTextColor(global::Android.Graphics.Color.ParseColor("#2E7D32"));
        dismissButton.TextSize = 20;
        dismissButton.Click += (s, e) =>
        {
            StopAlarm();
            Finish();
        };
        layout.AddView(dismissButton);

        SetContentView(layout);

        StartAlarm();
    }

    private void StartAlarm()
    {
        try
        {
            var alarmUri = RingtoneManager.GetDefaultUri(RingtoneType.Alarm)
                           ?? RingtoneManager.GetDefaultUri(RingtoneType.Ringtone)
                           ?? RingtoneManager.GetDefaultUri(RingtoneType.Notification);

            _ringtone = RingtoneManager.GetRingtone(this, alarmUri);
            if (_ringtone != null)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
                {
                    _ringtone.Looping = true;
                }
                _ringtone.Play();
            }
        }
        catch
        {
            // fallback - at least vibrate
        }

        try
        {
            _vibrator = (Vibrator?)GetSystemService(VibratorService);
            if (_vibrator != null)
            {
                var pattern = new long[] { 0, 1000, 500, 1000, 500, 1000 };
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    _vibrator.Vibrate(VibrationEffect.CreateWaveform(pattern, 0));
                }
                else
                {
#pragma warning disable CS0618
                    _vibrator.Vibrate(pattern, 0);
#pragma warning restore CS0618
                }
            }
        }
        catch
        {
            // vibration not available
        }
    }

    private void StopAlarm()
    {
        _ringtone?.Stop();
        _vibrator?.Cancel();
    }

    protected override void OnDestroy()
    {
        StopAlarm();
        base.OnDestroy();
    }

    public override void OnBackPressed()
    {
        StopAlarm();
        base.OnBackPressed();
    }
}
