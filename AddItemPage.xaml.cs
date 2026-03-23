using Podsetnik.Services;

namespace Podsetnik;

public partial class AddItemPage : ContentPage
{
    private readonly PlannerService _service;

    public AddItemPage(PlannerService service)
    {
        InitializeComponent();
        _service = service;

        AlarmDate.Date = DateTime.Today;
        AlarmTime.Time = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(5));
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var description = DescriptionEditor.Text?.Trim();
        if (string.IsNullOrEmpty(description))
        {
            await DisplayAlert("Greška", "Unesite opis podsetnika.", "OK");
            return;
        }

        var alarmDateTime = AlarmDate.Date.Add(AlarmTime.Time);
        if (alarmDateTime <= DateTime.Now)
        {
            await DisplayAlert("Greška", "Vreme alarma mora biti u budućnosti.", "OK");
            return;
        }

        var item = _service.Add(description, alarmDateTime);
        AlarmHelper.ScheduleAlarm(item.Id, item.Description, alarmDateTime);

        await Navigation.PopAsync();
    }
}
