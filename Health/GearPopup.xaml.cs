using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;

namespace Health;

public partial class GearPopup : Popup
{
    private readonly List<string> _options = new()
    {
        "Никакого (только вес тела)",
        "Гантели",
        "Эспандеры / резинки",
        "Скакалка",
        "Турник",
        "Фитбол",
        "Коврик для тренировок"
    };

    private readonly List<CheckBox> _checkBoxes = new();

    public GearPopup(List<string> preselected = null)
    {
        InitializeComponent();

        for (int i = 0; i < _options.Count; i++)
        {
            var option = _options[i];
            var checkbox = new CheckBox { IsChecked = preselected?.Contains(option) ?? false };
            checkbox.CheckedChanged += OnCheckboxChanged;

            var label = new Label
            {
                Text = option,
                VerticalOptions = LayoutOptions.Center
            };

            var layout = new HorizontalStackLayout
            {
                Spacing = 10,
                Children = { checkbox, label }
            };

            _checkBoxes.Add(checkbox);
            CheckboxContainer.Children.Add(layout);
        }

        ApplySelectionRules();
    }

    private void OnCheckboxChanged(object sender, CheckedChangedEventArgs e)
    {
        ApplySelectionRules();
    }

    private void ApplySelectionRules()
    {
        var noEquipmentIndex = 0;
        var noEquipmentCheckbox = _checkBoxes[noEquipmentIndex];

        if (noEquipmentCheckbox.IsChecked)
        {
            for (int i = 1; i < _checkBoxes.Count; i++)
            {
                _checkBoxes[i].IsChecked = false;
                _checkBoxes[i].IsEnabled = false;
            }
        }
        else
        {
            for (int i = 1; i < _checkBoxes.Count; i++)
            {
                _checkBoxes[i].IsEnabled = true;
            }
        }

        if (_checkBoxes.Skip(1).Any(cb => cb.IsChecked))
        {
            noEquipmentCheckbox.IsChecked = false;
        }
    }
    private async void DisplayAlert(string title, string message, string cancel)
    {
        await Application.Current.MainPage.DisplayAlert(title, message, cancel);
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        var selectedOptions = _checkBoxes
            .Select((cb, i) => (cb, _options[i]))
            .Where(x => x.cb.IsChecked)
            .Select(x => x.Item2)
            .ToList();

        if (selectedOptions.Count == 0)
        {
            this.DisplayAlert("Ошибка", "Пожалуйста, выберите хотя бы одну опцию", "ОК");
            return;
        }

        Close(selectedOptions);
    }
}
