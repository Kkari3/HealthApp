using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;

namespace Health;

public partial class RegistrationPage2 : ContentPage
{
	private UserInputData _user;
	public RegistrationPage2(UserInputData userData)
	{
		InitializeComponent();
		_user = userData;
	}
    private List<string> selectedGear = new();

    private async void OnSelectGearClicked(object sender, EventArgs e)
    {
        var popup = new GearPopup(selectedGear);
        var result = await this.ShowPopupAsync(popup);

        if (result is List<string> gearList)
        {
            selectedGear = gearList;
            SelectedGearLabel.Text = "Выбрано: " + string.Join(", ", selectedGear);
        }
    }
    private void OnAnyCheckboxChanged(object sender, CheckedChangedEventArgs e)
	{
        if (AnyCheckbox.IsChecked)
        {
            CardioCheckbox.IsChecked = false;
            StrengthCheckbox.IsChecked = false;
            YogaCheckbox.IsChecked = false;
            HomeCheckbox.IsChecked = false;
            OutdoorCheckbox.IsChecked = false;
        }
    }
	private void OnOtherCheckboxChanged(object sender, CheckedChangedEventArgs e)
	{
        if (((CheckBox)sender).IsChecked)
        {
            AnyCheckbox.IsChecked = false;
        }
    }
    private async void OnFinishButtonClicked(object sender, EventArgs e)
	{
        if (LifestylePicker.SelectedItem == null ||
        FitnessLevelPicker.SelectedItem == null ||
        DurationPicker.SelectedItem == null)
        {
            await DisplayAlert("Ошибка", "Пожалуйста, заполните все поля", "ОК");
            return;
        }
        string lifestyle = LifestylePicker.SelectedItem.ToString();
        string fitnessLevel = FitnessLevelPicker.SelectedItem.ToString();
        string duration = DurationPicker.SelectedItem.ToString();;

        if (selectedGear.Count == 0)
        {
            await DisplayAlert("Ошибка", "Выберите хотя бы одно оборудование", "ОК");
            return;
        }

        var preferences = new List<string>();
        if (CardioCheckbox.IsChecked) preferences.Add("Кардио");
        if (StrengthCheckbox.IsChecked) preferences.Add("Силовые");
        if (YogaCheckbox.IsChecked) preferences.Add("Йога / растяжка");
        if (HomeCheckbox.IsChecked) preferences.Add("Домашние");
        if (OutdoorCheckbox.IsChecked) preferences.Add("На свежем воздухе");
        if (AnyCheckbox.IsChecked) preferences.Add("Без разницы");

        if (preferences.Count == 0)
        {
            await DisplayAlert("Ошибка", "Выберите хотя бы один тип тренировки", "ОК");
            return;
        }

        Preferences.Set("HeightCm", (int)Math.Round(_user.HeightCm));
        Preferences.Set("WeightKg", (int)Math.Round(_user.WeightKg));
        Preferences.Set("Gender", _user.Gender);
        Preferences.Set("DateOfBirth", _user.DateOfBirth.ToString("dd.MM.yyyy"));
        Preferences.Set("Goal", _user.Goal);

        Preferences.Set("Lifestyle", lifestyle);
        Preferences.Set("FitnessLevel", fitnessLevel);
        Preferences.Set("Duration", duration);
        Preferences.Set("Gear", string.Join(",", selectedGear));
        Preferences.Set("Preferences", string.Join(",", preferences));
       
        Preferences.Set("IsUserRegistered", true);
        await Navigation.PushModalAsync(new LoadingPage());
    }
}