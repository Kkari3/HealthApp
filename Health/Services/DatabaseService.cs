using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Health.Models;
using SQLite;

namespace Health.Services
{
    public class DatabaseService
    {
        private const string DB_NAME = "NutritionData.db3";
        private readonly SQLiteAsyncConnection _connection;

        private static readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        private static DatabaseService _instance;
        public static DatabaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DatabaseService();
                }
                return _instance;
            }
        }

        private DatabaseService()
        {
            _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, DB_NAME));
        }

        public async Task InitAsync()
        {
            await _connection.CreateTableAsync<WeightEntry>();
            await _connection.CreateTableAsync<NutritionEntry>();
            await _connection.CreateTableAsync<MealEntry>();
        }

        // Последние 7 весов по дате (старые -> новые)
        public async Task<List<int>> GetLast7WeightsAsync()
        {
            var entries = await _connection.Table<WeightEntry>()
                                           .OrderByDescending(x => x.Date) // новые -> старые
                                           .Take(7)
                                           .ToListAsync();

            return entries.OrderBy(x => x.Date)
                          .Select(e => e.Weight)
                          .ToList();
        }

        public async Task<List<DateTime>> GetLast7WeightDatesAsync()
        {
            var entries = await _connection.Table<WeightEntry>()
                                           .OrderByDescending(x => x.Date)
                                           .Take(7)
                                           .ToListAsync();

            return entries.OrderBy(x => x.Date)
                          .Select(e => e.Date)
                          .ToList();
        }

        public async Task AddOrUpdateDayAsync(DateTime dateTime, int weight)
        {
            var date = dateTime.Date;

            var entry = await _connection.Table<WeightEntry>()
                                         .Where(x => x.Date == date)
                                         .FirstOrDefaultAsync();

            if (entry != null)
            {
                entry.Weight = weight;
                await _connection.UpdateAsync(entry);
            }
            else
            {
                await _connection.InsertAsync(new WeightEntry
                {
                    Date = date,
                    Weight = weight
                });
            }
        }


        public async Task EnsureFirstWeightAsync()
        {
            await _initLock.WaitAsync();
            try
            {
                var count = await _connection.Table<WeightEntry>().CountAsync();
                if (count == 0)
                {
                    var weight = Preferences.Default.Get("WeightKg", 0);
                    if (weight > 0)
                    {
                        await _connection.InsertAsync(new WeightEntry
                        {
                            Date = DateTime.Today,
                            Weight = weight
                        });
                    }
                }
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task EnsureNutritionTableAsync()
        {
            await _initLock.WaitAsync();
            try
            {
                var count = await _connection.Table<NutritionEntry>().CountAsync();
                if (count == 0)
                {
                    var Calories = Preferences.Default.Get("EatenCalories", 0);
                    var ProteinsGrams = Preferences.Default.Get("EatenProteinGrams", 0);
                    var FatsGrams = Preferences.Default.Get("EatenFatGrams", 0);
                    var CarbsGrams = Preferences.Default.Get("EatenCarbsGrams", 0);
                    var ProteinsCals = Preferences.Default.Get("EatenProteinCals", 0);
                    var FatsCals = Preferences.Default.Get("EatenFatCals", 0);
                    var CarbsCals = Preferences.Default.Get("EatenCarbsCals", 0);
                    if (Calories > 0 || ProteinsGrams > 0 || FatsGrams > 0 || CarbsGrams > 0 || ProteinsCals > 0 || FatsCals > 0 || CarbsCals > 0)
                    {
                        var goalReached = Calories >= Preferences.Default.Get("Calories", 0);
                        await _connection.InsertAsync(new NutritionEntry
                        {
                            Date = DateTime.Today,
                            Calories = Calories,
                            ProteinsGrams = ProteinsGrams,
                            FatsGrams = FatsGrams,
                            CarbsGrams = CarbsGrams,
                            ProteinsCals = ProteinsCals,
                            FatsCals = FatsCals,
                            CarbsCals = CarbsCals,
                            GoalReached = goalReached
                        });
                    }
                }
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task<List<NutritionEntry>> GetNutritionDataFor7days()
        {
            var entries = await _connection.Table<NutritionEntry>()
                                           .OrderByDescending(x => x.Date) // новые -> старые
                                           .Take(7)
                                           .ToListAsync();

            return entries.OrderBy(x => x.Date).ToList(); // старые -> новые
        }

        public async Task AddOrUpdateNutritionAsync(DateTime date, double calories, double proteinsgrams, double fatsgrams, double carbsgrams, double proteinscals, double fatscals, double carbscals)
        {
            var dateOnly = date.Date;

            var existing = await _connection.Table<NutritionEntry>()
                                           .Where(x => x.Date == dateOnly)
                                           .FirstOrDefaultAsync();

            var requiredCalories = Preferences.Default.Get("Calories", 0);
            var goalReached = calories >= requiredCalories;

            if (existing != null)
            {
                existing.Calories = calories;
                existing.ProteinsGrams = proteinsgrams;
                existing.FatsGrams = fatsgrams;
                existing.CarbsGrams = carbsgrams;
                existing.ProteinsCals = proteinscals;
                existing.FatsCals = fatscals;
                existing.CarbsCals = carbscals;
                existing.GoalReached = goalReached;
                await _connection.UpdateAsync(existing);
            }
            else
            {
                await _connection.InsertAsync(new NutritionEntry
                {
                    Date = dateOnly,
                    Calories = calories,
                    ProteinsGrams = proteinsgrams,
                    FatsGrams = fatsgrams,
                    CarbsGrams = carbsgrams,
                    ProteinsCals = proteinscals,
                    FatsCals = fatscals,
                    CarbsCals = carbscals,
                    GoalReached = goalReached
                });
            }
        }
        // ---------------------------------------------------------
        // КАЛЕНДАРЬ 
        // ---------------------------------------------------------
        public async Task<List<NutritionEntry>> GetAllNutritionEntriesAsync()
        {
            return await _connection.Table<NutritionEntry>()
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        // Норма выполнена или нет
        public bool IsCalorieNormMet(NutritionEntry entry)
        {
            int requiredCalories = Preferences.Get("Calories", 0);
            return entry.Calories >= requiredCalories;
        }

        // Записи за конкретный месяц
        public async Task<List<NutritionEntry>> GetNutritionForMonth(int year, int month)
        {
            return await _connection.Table<NutritionEntry>()
                .Where(x => x.Date.Year == year && x.Date.Month == month)
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        // ---------------------------------------------------------
        // ЖУРНАЛ ПИТАНИЯ 
        // ---------------------------------------------------------
        // Добавить новую запись о приёме пищи
        public async Task AddMealEntryAsync(MealEntry meal)
        {
            await _connection.InsertAsync(meal);
        }

        // Получить приёмы пищи за последние 7 дней (от новых к старым)
        public async Task<List<MealEntry>> GetMealsForLast7DaysAsync()
        {
            var sevenDaysAgo = DateTime.Today.AddDays(-7);
            return await _connection.Table<MealEntry>()
                                    .Where(x => x.MealTime >= sevenDaysAgo)
                                    .OrderByDescending(x => x.MealTime)
                                    .ToListAsync();
        }
    }
}

