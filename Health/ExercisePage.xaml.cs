using System.Text.Json;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Views; 

namespace Health;

public partial class ExercisePage : ContentPage
{
    private List<Exercise> allExercises;

    public ExercisePage()
    {
        InitializeComponent();
        LoadExercises();
    }

    private async void LoadExercises()
    {
        try
        {
            ResetDailyIfNeeded();

            using var stream = await FileSystem.OpenAppPackageFileAsync("exercises.json");
            using var reader = new StreamReader(stream);
            string json = await reader.ReadToEndAsync();
            allExercises = JsonSerializer.Deserialize<List<Exercise>>(json);

            var preferences = Preferences.Get("Preferences", "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var equipment = Preferences.Get("Gear", "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var selectedDuration = Preferences.Get("Duration", "");
            var userFitnessLevel = Preferences.Get("FitnessLevel", "Я новичок");

            int maxMinutes = selectedDuration switch
            {
                "10–15 минут" => 15,
                "20–30 минут" => 30,
                "45–60 минут" => 60,
                "Более 1 часа" => 999,
                _ => 999
            };

            int userRank = GetDifficultyRank(userFitnessLevel);

            var filtered = allExercises
                .Where(ex =>
                    IsPreferenceCompatible(preferences, ex.Type) &&
                    IsEquipmentCompatible(ex.Gear, equipment) &&
                    ex.Duration <= maxMinutes &&
                    GetDifficultyRank(ex.Difficulty) <= userRank
                )
                .OrderBy(e => e.Type)
                .ThenBy(e => e.Duration)
                .ToList();

            filtered = FilterExercisesByTotalTime(filtered, maxMinutes);

            foreach (var ex in filtered)
                ex.SetsReps = CalculateSetsReps(ex.Type, selectedDuration, userFitnessLevel);

            var completed = Preferences.Get("CompletedExercises", "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var toDo = filtered.Where(e => !completed.Contains(e.Id)).ToList();
            var done = filtered.Where(e => completed.Contains(e.Id)).ToList();

            ToDoList.ItemsSource = toDo;
            DoneList.ItemsSource = done;

            if (filtered.Count > 0 && toDo.Count == 0)
            {
                var popup = new ExCompleted();
                await this.ShowPopupAsync(popup);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private void ResetDailyIfNeeded()
    {
        var lastDate = Preferences.Get("LastOpenDate", "");
        var today = DateTime.Now.ToString("yyyy-MM-dd");

        if (lastDate != today)
        {
            Preferences.Set("CompletedExercises", "");
            Preferences.Set("LastOpenDate", today);
        }
    }

    private void OnExerciseDoneClicked(object sender, EventArgs e)
    {
        if (sender is not ImageButton btn) return;

        int id = Convert.ToInt32(btn.CommandParameter);

        var completed = Preferences.Get("CompletedExercises", "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        if (!completed.Contains(id))
            completed.Add(id);

        Preferences.Set("CompletedExercises", string.Join(",", completed));

        LoadExercises();
    }

    private bool IsPreferenceCompatible(List<string> preferences, string exerciseType)
        => preferences.Contains("Без разницы") || preferences.Contains(exerciseType);

    private bool IsEquipmentCompatible(List<string> exerciseGear, List<string> userEquipment)
    {
        if (userEquipment.Contains("Никакого (только вес тела)"))
            return exerciseGear.Contains("Никакого (только вес тела)");
        return exerciseGear.Any(g => userEquipment.Contains(g));
    }

    private List<Exercise> FilterExercisesByTotalTime(List<Exercise> exercises, int maxMinutes)
    {
        var result = new List<Exercise>();
        int totalTime = 0;

        foreach (var exercise in exercises)
        {
            if (totalTime + exercise.Duration <= maxMinutes)
            {
                result.Add(exercise);
                totalTime += exercise.Duration;
            }
        }

        return result;
    }

    private string CalculateSetsReps(string exerciseType, string duration, string fitnessLevel)
    {
        return (exerciseType, duration, fitnessLevel) switch
        {
            ("Кардио", "10–15 минут", "Я новичок") => "2 подхода по 30 секунд",
            ("Кардио", "10–15 минут", _) => "3 подхода по 30 секунд",
            ("Кардио", _, "Я новичок") => "3 подхода по 45 секунд",
            ("Кардио", _, "Иногда тренируюсь") => "4 подхода по 45 секунд",
            ("Кардио", _, "Регулярно тренируюсь") => "5 подходов по 60 секунд",
            ("Йога / растяжка", _, _) => "2 подхода по 30 секунд на сторону",
            (_, "10–15 минут", "Я новичок") => "2×8–10 повторений",
            (_, "10–15 минут", "Иногда тренируюсь") => "3×10–12 повторений",
            (_, "10–15 минут", "Регулярно тренируюсь") => "4×12–15 повторений",
            (_, "20–30 минут", "Я новичок") => "3×10 повторений",
            (_, "20–30 минут", "Иногда тренируюсь") => "3×12–15 повторений",
            (_, "20–30 минут", "Регулярно тренируюсь") => "4×15–20 повторений",
            (_, "45–60 минут", "Я новичок") => "3×12 повторений",
            (_, "45–60 минут", "Иногда тренируюсь") => "4×15 повторений",
            (_, "45–60 минут", "Регулярно тренируюсь") => "5×20 повторений",
            (_, "Более 1 часа", _) => "5×20 повторений",
            _ => "3×10 повторений"
        };
    }

    private int GetDifficultyRank(string difficulty)
    {
        return difficulty switch
        {
            "Я новичок" => 0,
            "Иногда тренируюсь" => 1,
            "Регулярно тренируюсь" => 2,
            _ => 0
        };
    }

    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public List<string> Gear { get; set; }
        public int Duration { get; set; }
        public string Image { get; set; }
        public string Difficulty { get; set; }
        public string SetsReps { get; set; }
        public string GearString => Gear != null ? string.Join(", ", Gear) : "";
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("profile");
    }

    private async void OnExerciseImageTapped(object sender, EventArgs e)
    {
        if (sender is Image image && image.BindingContext is Exercise exercise)
        {
            var popup = new ExerciseImagePopup(exercise.Image, exercise.Name, exercise.Type);
            await this.ShowPopupAsync(popup);
        }
    }
}