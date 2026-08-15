using Microsoft.Maui.Controls;
using System.Windows;

namespace Health;

public partial class RegistrationPage : ContentPage
{
    public RegistrationPage()
    {
        InitializeComponent();
    }

    private async void OnNavigateButtonClicked(object sender, EventArgs e)
    {
        string? dateOfBirthText = DateOfBirthEntry?.Text?.Trim();

        if (string.IsNullOrEmpty(dateOfBirthText) ||
            !DateTime.TryParseExact(dateOfBirthText, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dateOfBirth))
        {
            await DisplayAlert("Ошибка", "Введите корректную дату рождения в формате ДД.ММ.ГГГГ.", "ОК");
            return;
        }

        if (dateOfBirth > DateTime.Today || dateOfBirth.Year < 1900)
        {
            await DisplayAlert("Ошибка", "Введите реальную дату рождения.", "ОК");
            return;
        }

        if (double.TryParse(HeightEntry.Text, out double heightCm))
        {
            if (heightCm < 50 || heightCm > 300)
            {
                await DisplayAlert("Ошибка", "Введите рост от 50 до 300 см", "ОК");
                return;
            }
        }
        else
        {
            await DisplayAlert("Ошибка", "Введите корректное число для роста", "ОК");
            return;
        }

        if(double.TryParse(WeightEntry.Text, out double weightKg))
        {
            if (weightKg < 20 || weightKg > 500)
            {
                await DisplayAlert("Ошибка", "Введите вес от 20 до 500 кг", "ОК");
                return;
            }
        }
        else
        {
            await DisplayAlert("Ошибка", "Введите корректное число для веса", "ОК");
            return;
        }

        if(GenderPicker.SelectedItem == null)
        {
            await DisplayAlert("Ошибка", "Выберите пол", "ОК");
            return;
        }

        if(GoalPicker.SelectedItem == null)
        {
            await DisplayAlert("Ошибка", "Выберите цель", "ОК");
            return;
        }
        double height = double.Parse(HeightEntry.Text);
        double weight = double.Parse(WeightEntry.Text);
        string gender = GenderPicker.SelectedItem as string;
        string goal = GoalPicker.SelectedItem as string;
        DateTime dateOfBirthValue = DateTime.ParseExact(DateOfBirthEntry.Text, "dd.MM.yyyy", null);

        var user = new UserInputData
        {
            HeightCm = height,
            WeightKg = weight,
            Gender = gender,
            DateOfBirth = dateOfBirth,
            Goal = goal
        };
        await Navigation.PushAsync(new RegistrationPage2(user));
    }
}