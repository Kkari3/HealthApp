using CommunityToolkit.Maui.Views;
using Health.Services;

namespace Health;

public partial class WeightChangePopup : Popup
{
    private readonly Func<Task>? _onWeightChanged;
    private readonly DatabaseService _db = DatabaseService.Instance;

    public WeightChangePopup(Func<Task>? onWeightChanged = null)
    {
        InitializeComponent();
        _onWeightChanged = onWeightChanged;
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(WeightEntry.Text, out int newWeight))
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите число.", "ОК");
            return;
        }

        if (newWeight < 20 || newWeight > 500)
        {
            await Application.Current.MainPage.DisplayAlert(
              "Неверное значение",
              "Вес должен быть в диапазоне от 20 до 500 кг.",
              "ОК"
            );
            return;
        }

        Preferences.Default.Set("WeightKg", newWeight);


        await _db.AddOrUpdateDayAsync(DateTime.Today, newWeight);

        if (_onWeightChanged is not null)
            await _onWeightChanged();

        Close();
    }
}