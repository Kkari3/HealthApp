using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Health.Services;

namespace Health
{
    public partial class NutritionPage : ContentPage
    {
        private const string GoalPopupShownDateKey = "GoalPopupShownDate";
        public NutritionPage()
        {
            InitializeComponent();
            try
            {
                LoadNutritionData();
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    bool copy = await Application.Current.MainPage.DisplayAlert("Ошибка", ex.ToString(), "Скопировать", "OK");
                    if (copy)
                    {
                        await Clipboard.Default.SetTextAsync(ex.ToString());
                        await Application.Current.MainPage.DisplayAlert("Скопировано", "Текст ошибки скопирован в буфер обмена.", "OK");
                    }
                });
            }
        }
        private async void OnProfileClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("profile");
        }
        public enum Gender { Male, Female }
        public enum Goal { Loss, Maintain, Gain }
        public enum Lifestyle { Sedentary, Moderate, Active }
        public enum FitnessLevel { Beginner, Sometimes, Regular }

        public record UserData
        (
            int HeightCm,
            int WeightKg,
            int Age,
            Gender Gender,
            Goal Goal,
            Lifestyle Lifestyle,
            FitnessLevel FitnessLevel
        );
        public record NutritionResult
        (
            float Calories,
            float ProteinGrams, float ProteinCals,
            float FatGrams, float FatCals,
            float CarbsGrams, float CarbsCals
        );
        public static class NutritionCalculator
        {
            private const float MinCaloriesFemale = 1200;
            private const float MinCaloriesMale = 1400;

            public static NutritionResult Calculate(UserData user)
            {
                float bmr = user.Gender == Gender.Male
                    ? 10 * user.WeightKg + 6.25f * user.HeightCm - 5 * user.Age + 5
                    : 10 * user.WeightKg + 6.25f * user.HeightCm - 5 * user.Age - 161;

                float activityFactor = user.Lifestyle switch
                {
                    Lifestyle.Sedentary => 1.2f,
                    Lifestyle.Moderate => 1.35f,
                    Lifestyle.Active => 1.55f,
                    _ => 1.2f
                };

                float tdee = bmr * activityFactor;

                float calories = user.Goal switch
                {
                    Goal.Loss => tdee * 0.85f,
                    Goal.Maintain => tdee,
                    Goal.Gain => tdee * 1.10f,
                    _ => tdee
                };

                if (user.Gender == Gender.Female && calories < MinCaloriesFemale) calories = MinCaloriesFemale;
                if (user.Gender == Gender.Male && calories < MinCaloriesMale) calories = MinCaloriesMale;

                float proteinGrams = user.FitnessLevel switch
                {
                    FitnessLevel.Beginner => user.WeightKg * 1.4f,
                    FitnessLevel.Sometimes => user.WeightKg * 1.6f,
                    FitnessLevel.Regular => user.WeightKg * 1.9f,
                    _ => user.WeightKg * 1.4f
                };
                float proteinCals = proteinGrams * 4;

                float fatCals = user.Goal switch
                {
                    Goal.Loss => calories * 0.25f,
                    Goal.Maintain => calories * 0.27f,
                    Goal.Gain => calories * 0.30f,
                    _ => calories * 0.27f
                };
                float fatGrams = fatCals / 9;

                float carbsCals = calories - (proteinCals + fatCals);
                float carbsGrams = carbsCals / 4;

                return new NutritionResult(calories, proteinGrams, proteinCals, fatGrams, fatCals, carbsGrams, carbsCals);
            }
        }

        private void UpdateEatenInfo()
        {
            string lastDate = Preferences.Get("NutritionLastDate", "");
            if (lastDate != DateTime.Today.ToString("yyyyMMdd"))
            {
                // Новый день - сбрасываем
                Preferences.Set("EatenCalories", 0);
                Preferences.Set("EatenProteinGrams", 0);
                Preferences.Set("EatenProteinCals", 0);
                Preferences.Set("EatenFatGrams", 0);
                Preferences.Set("EatenFatCals", 0);
                Preferences.Set("EatenCarbsGrams", 0);
                Preferences.Set("EatenCarbsCals", 0);
                Preferences.Set("NutritionLastDate", DateTime.Today.ToString("yyyyMMdd"));
            }
        }

        private async Task CheckAndShowGoalPopup()
        {
            // 1. Проверяем, показывалось ли всплывающее окно сегодня
            string lastShownDate = Preferences.Get(GoalPopupShownDateKey, string.Empty);
            string todayDate = DateTime.Today.ToString("yyyyMMdd");

            if (lastShownDate == todayDate)
            {
                // Уже показывали сегодня, выходим
                return;
            }

            // Загружаем все целевые и съеденные значения (Ккал и БЖУ)
            int targetCals = Preferences.Get("Calories", 0);
            int eatenCals = Preferences.Get("EatenCalories", 0);

            int targetP = Preferences.Get("ProteinGrams", 0);
            int eatenP = Preferences.Get("EatenProteinGrams", 0);

            int targetF = Preferences.Get("FatGrams", 0);
            int eatenF = Preferences.Get("EatenFatGrams", 0);

            int targetC = Preferences.Get("CarbsGrams", 0);
            int eatenC = Preferences.Get("EatenCarbsGrams", 0);

            // Проверяем, что цель по калориям установлена (> 0) и что все цели достигнуты
            bool allGoalsMet = targetCals > 0 &&
                               eatenCals >= targetCals &&
                               eatenP >= targetP &&
                               eatenF >= targetF &&
                               eatenC >= targetC;

            if (allGoalsMet)
            {
                // 2. Показываем всплывающее окно
                var popup = new NutritionGoalReachedPopup();
                await this.ShowPopupAsync(popup);

                // 3. Сохраняем дату, чтобы не показывать снова до завтра
                Preferences.Set(GoalPopupShownDateKey, todayDate);
            }
        }

        public async void LoadNutritionData()
        {
            var birthDate = DateTime.ParseExact(Preferences.Get("DateOfBirth", "01.01.2000"), "dd.MM.yyyy", null);
            int age = DateTime.Today.Year - birthDate.Year;
            if (birthDate.Date > DateTime.Today.AddYears(-age)) age--;

            var user = new UserData(
                HeightCm: Preferences.Get("HeightCm", 0),
                WeightKg: Preferences.Get("WeightKg", 0),
                Age: age,
                Gender: Preferences.Get("Gender", "") == "Мужской" ? Gender.Male : Gender.Female,
                Goal: Preferences.Get("Goal", "") switch
                {
                    "Похудение" => Goal.Loss,
                    "Поддержание веса" => Goal.Maintain,
                    "Набор массы" => Goal.Gain,
                    _ => Goal.Maintain
                },
                Lifestyle: Preferences.Get("Lifestyle", "") switch
                {
                    "Сидячий" => Lifestyle.Sedentary,
                    "Умеренно активный" => Lifestyle.Moderate,
                    "Активный" => Lifestyle.Active,
                    _ => Lifestyle.Sedentary
                },
                FitnessLevel: Preferences.Get("FitnessLevel", "") switch
                {
                    "Я новичок" => FitnessLevel.Beginner,
                    "Иногда тренируюсь" => FitnessLevel.Sometimes,
                    "Регулярно тренируюсь" => FitnessLevel.Regular,
                    _ => FitnessLevel.Beginner
                }
            );
            string currentHash = $"{user.HeightCm}-{user.WeightKg}-{user.Age}-{user.Gender}-{user.Goal}-{user.Lifestyle}-{user.FitnessLevel}";
            UpdateEatenInfo();
            string savedHash = Preferences.Get("NutritionHash", "");
            if (savedHash == currentHash)
            {
                CaloriesLabel.Text = $"{Preferences.Get("EatenCalories", 0)} / {Preferences.Get("Calories", 0)} ккал";
                ProteinGramsLabel.Text = $"{Preferences.Get("EatenProteinGrams", 0)} / {Preferences.Get("ProteinGrams", 0)} г";
                ProteinCalsLabel.Text = $"{Preferences.Get("EatenProteinCals", 0)} / {Preferences.Get("ProteinCals", 0)} ккал";
                FatGramsLabel.Text = $"{Preferences.Get("EatenFatGrams", 0)} / {Preferences.Get("FatGrams", 0)} г";
                FatCalsLabel.Text = $"{Preferences.Get("EatenFatCals", 0)} / {Preferences.Get("FatCals", 0)} ккал";
                CarbsGramsLabel.Text = $"{Preferences.Get("EatenCarbsGrams", 0)} / {Preferences.Get("CarbsGrams", 0)} г";
                CarbsCalsLabel.Text = $"{Preferences.Get("EatenCarbsCals", 0)} / {Preferences.Get("CarbsCals", 0)} ккал";
                await CheckAndShowGoalPopup();
                return;
            }

            var result = NutritionCalculator.Calculate(user);

            CaloriesLabel.Text = $"{Preferences.Get("EatenCalories", 0)} / {Math.Round(result.Calories)} ккал";
            ProteinGramsLabel.Text = $"{Preferences.Get("EatenProteinGrams", 0)} / {Math.Round(result.ProteinGrams)} г";
            ProteinCalsLabel.Text = $"{Preferences.Get("EatenProteinCals", 0)} / {Math.Round(result.ProteinCals)} ккал";
            FatGramsLabel.Text = $"{Preferences.Get("EatenFatGrams", 0)} / {Math.Round(result.FatGrams)} г";
            FatCalsLabel.Text = $"{Preferences.Get("EatenFatCals", 0)} / {Math.Round(result.FatCals)} ккал";
            CarbsGramsLabel.Text = $"{Preferences.Get("EatenCarbsGrams", 0)} / {Math.Round(result.CarbsGrams)} г";
            CarbsCalsLabel.Text = $"{Preferences.Get("EatenCarbsCals", 0)} / {Math.Round(result.CarbsCals)} ккал";

            Preferences.Set("Calories", (int)Math.Round(result.Calories));
            Preferences.Set("ProteinGrams", (int)Math.Round(result.ProteinGrams));
            Preferences.Set("ProteinCals", (int)Math.Round(result.ProteinCals));
            Preferences.Set("FatGrams", (int)Math.Round(result.FatGrams));
            Preferences.Set("FatCals", (int)Math.Round(result.FatCals));
            Preferences.Set("CarbsGrams", (int)Math.Round(result.CarbsGrams));
            Preferences.Set("CarbsCals", (int)Math.Round(result.CarbsCals));
            Preferences.Set("NutritionHash", currentHash);
        }
        
        private void OnStatisticsClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("nutritionstatistics");
        }
        private void OnJournalClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("mealjournal");
        }
        private async void OnAddCaloriesClicked(object sender, EventArgs e)
        {
            var popup = new AddMealPopup(this);
            await this.ShowPopupAsync(popup);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadNutritionData();
        }

    }
}
