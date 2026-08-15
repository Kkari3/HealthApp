using SQLite;

namespace Health.Models;

public class NutritionEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public double Calories { get; set; }
    public double ProteinsCals { get; set; }
    public double FatsCals { get; set; }
    public double CarbsCals { get; set; }
    public double ProteinsGrams { get; set; }   
    public double FatsGrams { get; set; }
    public double CarbsGrams { get; set; }
    public bool GoalReached { get; set; }
}