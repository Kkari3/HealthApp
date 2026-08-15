using CommunityToolkit.Maui.Views;
using Health.Services;
using Health.ViewModels;
using Health.Models;

namespace Health;

public partial class AddMealPopup : Popup
{
    private readonly NutritionPage _nutritionPage;
    private readonly DatabaseService _db = DatabaseService.Instance;

    public AddMealPopup(NutritionPage nutritionPage)
    {
        InitializeComponent();
        _nutritionPage = nutritionPage;

        MealTimePicker.Time = DateTime.Now.TimeOfDay;
    }

    public int CountProteins(int proteins) => proteins * 4;
    public int CountFats(int fats) => fats * 9;
    public int CountCarbs(int carbs) => carbs * 4;

    public async void OnAddButtonClicked(object sender, EventArgs e)
    {
        bool proteinOk = int.TryParse(ProteinEntry.Text, out int proteins);
        bool fatsOk = int.TryParse(FatsEntry.Text, out int fats);
        bool carbsOk = int.TryParse(CarbsEntry.Text, out int carbs);

        if (string.IsNullOrWhiteSpace(ProteinEntry.Text) &&
            string.IsNullOrWhiteSpace(FatsEntry.Text) &&
            string.IsNullOrWhiteSpace(CarbsEntry.Text))
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Пожалуйста, заполните хотя бы одно поле", "ОК");
            return;
        }

        if ((!string.IsNullOrWhiteSpace(ProteinEntry.Text) && !proteinOk) ||
            (!string.IsNullOrWhiteSpace(FatsEntry.Text) && !fatsOk) ||
            (!string.IsNullOrWhiteSpace(CarbsEntry.Text) && !carbsOk))
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Можно вводить только целые числа", "ОК");
            return;
        }

        if ((proteinOk && (proteins < 0 || proteins > 300)) ||
            (fatsOk && (fats < 0 || fats > 300)) ||
            (carbsOk && (carbs < 0 || carbs > 500)))
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите реалистичные значения (белки/жиры ≤ 300г, углеводы ≤ 500г)", "ОК");
            return;
        }

        string mealName = MealNameEntry.Text;
        if (string.IsNullOrWhiteSpace(mealName))
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Пожалуйста, введите название приёма пищи", "ОК");
            return;
        }

        TimeSpan mealTime = MealTimePicker.Time;
        DateTime fullMealDateTime = DateTime.Today.Date + mealTime;

        int proteinCals = CountProteins(proteins);
        int fatCals = CountFats(fats);
        int carbCals = CountCarbs(carbs);
        int totalCals = proteinCals + fatCals + carbCals;

        
        Preferences.Set("EatenCalories", totalCals + Preferences.Get("EatenCalories", 0));
        Preferences.Set("EatenProteinGrams", proteins + Preferences.Get("EatenProteinGrams", 0));
        Preferences.Set("EatenProteinCals", proteinCals + Preferences.Get("EatenProteinCals", 0));
        Preferences.Set("EatenFatGrams", fats + Preferences.Get("EatenFatGrams", 0));
        Preferences.Set("EatenFatCals", fatCals + Preferences.Get("EatenFatCals", 0));
        Preferences.Set("EatenCarbsGrams", carbs + Preferences.Get("EatenCarbsGrams", 0));
        Preferences.Set("EatenCarbsCals", carbCals + Preferences.Get("EatenCarbsCals", 0));

        await _db.InitAsync();

        await _db.AddOrUpdateNutritionAsync(
            DateTime.Today,
            Preferences.Get("EatenCalories", 0),
            Preferences.Get("EatenProteinGrams", 0),
            Preferences.Get("EatenFatGrams", 0),
            Preferences.Get("EatenCarbsGrams", 0),
            Preferences.Get("EatenProteinCals", 0),
            Preferences.Get("EatenFatCals", 0),
            Preferences.Get("EatenCarbsCals", 0)
        );

        var newMealEntry = new MealEntry
        {
            Name = mealName.Trim(),
            MealTime = fullMealDateTime,
            ProteinGrams = proteins,
            FatGrams = fats,
            CarbGrams = carbs,
            TotalCalories = totalCals
        };
        await _db.AddMealEntryAsync(newMealEntry);

        if (_nutritionPage != null)
        {
            _nutritionPage.LoadNutritionData();
        }

        
        if (_nutritionPage?.BindingContext is ViewModel vm)
        {
            await vm.ReloadAsync();
        }

        await CloseAsync();
    }
}
