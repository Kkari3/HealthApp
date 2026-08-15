using CommunityToolkit.Maui.Views;
using Health.Services;
using Health.ViewModels;

namespace Health;

public partial class NutritionStatisticsPage : ContentPage
{
    private readonly ViewModel _vm;
    public NutritionStatisticsPage()
    {
        Shell.SetTabBarIsVisible(this, false);
        InitializeComponent();

        _vm = new ViewModel();
        BindingContext = _vm;
    }
    private void OnWeightChange(object sender, EventArgs e)
    {
        var popup = new WeightChangePopup(async () =>
        {
            await _vm.ReloadAsync(); // после изменени€ веса Ч обновл€ем график
        });

        this.ShowPopup(popup);
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ViewModel vm)
            await vm.ReloadAsync();
    }

}
