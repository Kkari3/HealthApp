namespace Health;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();
		Shell.SetTabBarIsVisible(this, false);
		Shell.SetNavBarIsVisible(this, false);

        try
        {
            LoadProfileData();
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                bool copy = await Application.Current.MainPage.DisplayAlert("Ошибка", ex.ToString(), "Скопировать", "OK");
                if (copy)
                {
                    await Clipboard.Default.SetTextAsync(ex.ToString());
                    await Application.Current.MainPage.DisplayAlert("Скопировано", "Текст ошибки скопирован в буфер обмена.", "OK");
                }
            });
        }
    }
    private void LoadProfileData()
    {
        HeightLabel.Text = $"Рост: {Preferences.Get("HeightCm", 0)} см";
        WeightLabel.Text = $"Вес: {Preferences.Get("WeightKg", 0)} кг";
        GenderLabel.Text = $"Пол: {Preferences.Get("Gender", "не указано")}";
        GoalLabel.Text = $"{Preferences.Get("Goal", "не указано")}";

        DateOfBirthLabel.Text = $"Дата рожения: {Preferences.Get("DateOfBirth", null)}";
        string birthDateStr = Preferences.Get("DateOfBirth", null);
        if (!string.IsNullOrEmpty(birthDateStr) &&
            DateTime.TryParseExact(birthDateStr, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime birthDate))
        {
            AgeLabel.Text = $"Возраст: {CalculateAge(birthDate)} {GetAgeSuffix(CalculateAge(birthDate))}";
        }
        else
        {
            AgeLabel.Text = "Дата рождения: не указано";
        }


        LifestyleLabel.Text = $"{Preferences.Get("Lifestyle", "не указано")}";
        FitnessLevelLabel.Text = $"{Preferences.Get("FitnessLevel", "не указано")}";
        PreferencesLabel.Text = $"{Preferences.Get("Preferences", "не выбраны")}";
        GearLabel.Text = $"{Preferences.Get("Gear", "не выбрано")}";
    }

    private int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        int age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--; 
        return age;
    }

    private string GetAgeSuffix(int age)
    {
        if (age % 10 == 1 && age % 100 != 11) return "год";
        if (age % 10 >= 2 && age % 10 <= 4 && (age % 100 < 10 || age % 100 >= 20)) return "года";
        return "лет";
    }
}