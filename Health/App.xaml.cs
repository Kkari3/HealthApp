using System.Diagnostics;
namespace Health
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Debug.WriteLine($"AppData Directory: {FileSystem.AppDataDirectory}");
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", e.ExceptionObject.ToString(), "OK");
                });
            };
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", e.Exception.ToString(), "OK");
                });
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Page startPage;

            if (Preferences.Get("IsUserRegistered", false))
            {
                startPage = new AppShell();
            }
            else
            {
                startPage = new NavigationPage(new RegistrationPage());
            }

            return new Window(startPage);
        }
    }
}