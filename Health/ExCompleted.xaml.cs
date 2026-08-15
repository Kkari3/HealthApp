using CommunityToolkit.Maui.Views;

namespace Health;

public partial class ExCompleted : Popup
{
	public ExCompleted()
	{
		InitializeComponent();
	}
	private async void OnClick(object sender, EventArgs e)
	{
        await CloseAsync();
    }
}