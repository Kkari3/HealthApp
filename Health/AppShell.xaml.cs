namespace Health
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("profile", typeof(ProfilePage));
            Routing.RegisterRoute("nutritionstatistics", typeof(NutritionStatisticsPage));
            Routing.RegisterRoute("mealjournal", typeof(MealJournal));
        }
    }
}
