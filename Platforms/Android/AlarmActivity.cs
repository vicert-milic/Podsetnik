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
    private MediaPlayer? _mediaPlayer;
    private Vibrator? _vibrator;
    private AudioManager? _audioManager;
    private int _originalVolume;
    private int _originalRingerMode;

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
        // Force alarm volume to max - this bypasses silent/vibration mode
        try
        {
            _audioManager = (AudioManager?)GetSystemService(AudioService);
            if (_audioManager != null)
            {
                _originalVolume = _audioManager.GetStreamVolume(global::Android.Media.Stream.Alarm);
                _originalRingerMode = (int)_audioManager.RingerMode;

                int maxVolume = _audioManager.GetStreamMaxVolume(global::Android.Media.Stream.Alarm);
                _audioManager.SetStreamVolume(global::Android.Media.Stream.Alarm, maxVolume, 0);
            }
        }
        catch { }

        // Use MediaPlayer with STREAM_ALARM - plays even in silent/vibration mode
        try
        {
            var alarmUri = RingtoneManager.GetDefaultUri(RingtoneType.Alarm)
                           ?? RingtoneManager.GetDefaultUri(RingtoneType.Ringtone)
                           ?? RingtoneManager.GetDefaultUri(RingtoneType.Notification);

            _mediaPlayer = new MediaPlayer();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var audioAttributes = new AudioAttributes.Builder()!
                    .SetUsage(AudioUsageKind.Alarm)!
                    .SetContentType(AudioContentType.Sonification)!
                    .SetFlags(AudioFlags.AudibilityEnforced)!
                    .Build();
                _mediaPlayer.SetAudioAttributes(audioAttributes);
            }
            else
            {
#pragma warning disable CS0618
                _mediaPlayer.SetAudioStreamType(global::Android.Media.Stream.Alarm);
#pragma warning restore CS0618
            }

            _mediaPlayer.SetDataSource(this, alarmUri!);
            _mediaPlayer.Looping = true;
            _mediaPlayer.Prepare();
            _mediaPlayer.Start();
        }
        catch
        {
            // fallback - at least vibrate
        }

        // Always vibrate regardless of mode
        try
        {
            _vibrator = (Vibrator?)GetSystemService(VibratorService);
            if (_vibrator != null)
            {
                var pattern = new long[] { 0, 1000, 500, 1000, 500, 1000 };
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var effect = VibrationEffect.CreateWaveform(pattern, 0);
                    var audioAttributes = new AudioAttributes.Builder()!
                        .SetUsage(AudioUsageKind.Alarm)!
                        .Build();
                    _vibrator.Vibrate(effect, audioAttributes);
                }
                else
                {
#pragma warning disable CS0618
                    _vibrator.Vibrate(pattern, 0);
#pragma warning restore CS0618
                }
            }
        }
        catch { }

        // Acquire wake lock to keep device awake
        try
        {
            var powerManager = (PowerManager?)GetSystemService(PowerService);
            var wakeLock = powerManager?.NewWakeLock(WakeLockFlags.ScreenBright | WakeLockFlags.AcquireCausesWakeup, "Podsetnik::AlarmWakeLock");
            wakeLock?.Acquire(60 * 1000L); // 60 seconds
        }
        catch { }
    }

    private void StopAlarm()
    {
        try
        {
            if (_mediaPlayer != null)
            {
                if (_mediaPlayer.IsPlaying)
                    _mediaPlayer.Stop();
                _mediaPlayer.Release();
                _mediaPlayer = null;
            }
        }
        catch { }

        _vibrator?.Cancel();

        // Restore original alarm volume
        try
        {
            if (_audioManager != null)
            {
                _audioManager.SetStreamVolume(global::Android.Media.Stream.Alarm, _originalVolume, 0);
            }
        }
        catch { }
    }

    protected override void OnDestroy()
    {
        StopAlarm();
        base.OnDestroy();
    }

    public override void OnBackPressed()
    {
        // Don't allow back to dismiss - must press the button
    }
}
