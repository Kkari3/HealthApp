using CommunityToolkit.Maui.Views;

namespace Health;

public partial class NutritionGoalReachedPopup : Popup
{
	public NutritionGoalReachedPopup()
	{
		InitializeComponent();
	}

    private async void OnClick(object sender, EventArgs e)
    {
        await CloseAsync();
    }
}