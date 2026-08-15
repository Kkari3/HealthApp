using CommunityToolkit.Maui.Views;

namespace Health;

public partial class WaterPage : ContentPage
{
    private int _remainingGlasses;
    private int _totalGlasses;

    public WaterPage()
    {
        InitializeComponent();
        LoadWaterData();
    }

    public record UserData(int WeightKg, int Age);
    public record WaterIntakeResult(float DailyIntakeLiters, int DailyGlassCount);

    public static class WaterIntakeCalculator
    {
        private const float MinWaterIntake = 1.35f;

        public static WaterIntakeResult Calculate(UserData user)
        {
            float baseIntake = user.WeightKg * 0.033f;
            float ageFactor = user.Age > 55 ? 0.1f : 0f;
            float dailyIntake = Math.Max(baseIntake - ageFactor, MinWaterIntake);
            int dailyGlass = (int)Math.Ceiling(dailyIntake / 0.25f);
            return new WaterIntakeResult(dailyIntake, dailyGlass);
        }
    }

    private void LoadWaterData()
    {
        var birthDate = DateTime.ParseExact(
            Preferences.Get("DateOfBirth", "01.01.2000"), "dd.MM.yyyy", null);
        int age = DateTime.Today.Year - birthDate.Year;
        if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;

        var userData = new UserData(
            WeightKg: Preferences.Get("WeightKg", 70),
            Age: age
        );

        var waterIntakeResult = WaterIntakeCalculator.Calculate(userData);
        _totalGlasses = waterIntakeResult.DailyGlassCount;

        WaterNormLabel.Text = $"~ {waterIntakeResult.DailyIntakeLiters:F2} л.";

        // Проверка даты
        string lastDate = Preferences.Get("WaterLastDate", "");
        if (lastDate != DateTime.Today.ToString("yyyyMMdd"))
        {
            // Новый день → сбрасываем
            _remainingGlasses = _totalGlasses;
            Preferences.Set("WaterRemaining", _remainingGlasses);
            Preferences.Set("WaterLastDate", DateTime.Today.ToString("yyyyMMdd"));
        }
        else
        {
            // Загружаем сохранённое значение
            _remainingGlasses = Preferences.Get("WaterRemaining", _totalGlasses);
        }

        UpdateGlassesUI();
    }

    private void UpdateGlassesUI()
    {
        GlassOfWater.Children.Clear();
        for (int i = 0; i < _remainingGlasses; i++)
        {
            GlassOfWater.Children.Add(new Image
            {
                Source = "glassofwater_2.png",
                Margin = new Thickness(2),
                WidthRequest = 30,
                HeightRequest = 30,
                VerticalOptions = LayoutOptions.Center
            });
        }
    }

    private async void OnDrinkGlassClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;

        if (_remainingGlasses > 0 && !button.IsEnabled)
            return;

        if (_remainingGlasses > 0)
        {
            _remainingGlasses--;
            Preferences.Set("WaterRemaining", _remainingGlasses);
            UpdateGlassesUI();
        }

        // Кулдаун 5 секунд
        button.IsEnabled = false;
        for (int i = 5; i > 0; i--)
        {
            button.Text = $"Жди {i} сек.";
            await Task.Delay(1000);
        }

        button.Text = "Выпить стакан";
        button.IsEnabled = true;
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("profile");
    }
}
