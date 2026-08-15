using Health.Services; 
using Health.Models;
namespace Health;

public partial class MealJournal : ContentPage
{
    private readonly DatabaseService _db = DatabaseService.Instance;

    public MealJournal()
    {
        Shell.SetTabBarIsVisible(this, false);
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadMealsAsync();
    }

    private async Task LoadMealsAsync()
    {
        await _db.InitAsync(); 
        var meals = await _db.GetMealsForLast7DaysAsync();

        var groups = meals
            .GroupBy(m => m.MealTime.Date)
            .OrderByDescending(g => g.Key)
            .Select(g =>
            {
                string label =
                    g.Key == DateTime.Today ? "Сегодня" :
                    g.Key == DateTime.Today.AddDays(-1) ? "Вчера" :
                    g.Key.ToString("dd MMMM");

                return new MealGroup(label, g);
            })
            .ToList();

        MealsCollectionView.ItemsSource = groups;
    }
}
