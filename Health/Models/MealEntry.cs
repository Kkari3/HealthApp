using SQLite;
using System;

namespace Health.Models
{
    public class MealEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime MealTime { get; set; }

        public int ProteinGrams { get; set; }

        public int FatGrams { get; set; }

        public int CarbGrams { get; set; }

        public int TotalCalories { get; set; }

        public string MealTimeDisplay => MealTime.ToString("g"); // Формат "17.11.2025 14:30"
        public string MacrosDisplay => $"Б: {ProteinGrams}г | Ж: {FatGrams}г | У: {CarbGrams}г";
    }
}