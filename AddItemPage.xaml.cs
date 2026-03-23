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

        LoadCategories();
    }

    private void LoadCategories()
    {
        var categories = _service.GetCategories();
        CategoryPicker.ItemsSource = categories;
        CategoryPicker.SelectedIndex = categories.Count > 0 ? 0 : -1;
    }

    private async void OnAddCategoryClicked(object? sender, EventArgs e)
    {
        var name = NewCategoryEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlert("Greška", "Unesite naziv kategorije.", "OK");
            return;
        }

        _service.AddCategory(name);
        NewCategoryEntry.Text = string.Empty;
        LoadCategories();
        CategoryPicker.SelectedItem = name;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var description = DescriptionEditor.Text?.Trim();
        if (string.IsNullOrEmpty(description))
        {
            await DisplayAlert("Greška", "Unesite opis podsetnika.", "OK");
            return;
        }

        if (CategoryPicker.SelectedItem is not string category)
        {
            await DisplayAlert("Greška", "Izaberite kategoriju.", "OK");
            return;
        }

        var alarmDateTime = AlarmDate.Date.Add(AlarmTime.Time);
        if (alarmDateTime <= DateTime.Now)
        {
            await DisplayAlert("Greška", "Vreme alarma mora biti u budućnosti.", "OK");
            return;
        }

        var item = _service.Add(description, alarmDateTime, category);
        AlarmHelper.ScheduleAlarm(item.Id, item.Description, alarmDateTime);

        await Navigation.PopAsync();
    }
}
