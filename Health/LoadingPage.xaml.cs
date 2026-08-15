namespace Health;

public partial class LoadingPage : ContentPage
{
    private readonly string[] loadingMessages =
    {
        "Подбираем тренировки...",
        "Анализируем цель...",
        "Считаем БЖУ...",
        "Оптимизируем программу...",
        "Готовим персональные рекомендации..."
    };

    public LoadingPage()
    {
        InitializeComponent();
        StartLoading();
    }

    private async void StartLoading()
    {
        foreach (string message in loadingMessages)
        {
            LoadingLabel.Text = message;

            // Анимация появления
            await LoadingLabel.FadeTo(1, 500);

            // Пауза на экране
            await Task.Delay(1000);

            // Анимация исчезновения
            await LoadingLabel.FadeTo(0, 500);
        }

        Application.Current.Windows[0].Page = new AppShell();
    }
}

