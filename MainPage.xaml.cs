using Podsetnik.Models;
using Podsetnik.Services;

namespace Podsetnik;

public partial class MainPage : ContentPage
{
    private readonly PlannerService _service;

    public MainPage(PlannerService service)
    {
        InitializeComponent();
        _service = service;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshList();
    }

    private void RefreshList()
    {
        ItemsList.ItemsSource = null;
        ItemsList.ItemsSource = _service.GetAll();
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddItemPage(_service));
    }

    private void OnDeleteSwiped(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.BindingContext is PlanItem item)
        {
            AlarmHelper.CancelAlarm(item.Id);
            _service.Delete(item.Id);
            RefreshList();
        }
    }

    private void OnCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is PlanItem item)
        {
            _service.ToggleComplete(item.Id);
        }
    }
}
