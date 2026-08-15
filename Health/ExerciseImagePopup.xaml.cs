using CommunityToolkit.Maui.Views;

namespace Health;

public partial class ExerciseImagePopup : Popup
{
	public ExerciseImagePopup(string imagePath, string name, string type)
	{
		InitializeComponent();
		
		ExerciseImage.Source = imagePath;
		ExerciseName.Text = name;
		ExerciseType.Text = type;
	}

	private void OnCloseClicked(object sender, EventArgs e)
	{
		Close();
	}
}