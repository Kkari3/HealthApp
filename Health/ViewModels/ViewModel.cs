using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Health.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Enums;
using Microsoft.Maui.Graphics;
using Health.Models;

namespace Health.ViewModels
{
    public class CalendarEventModel
    {
        public string Color { get; set; }
    }
    public class ViewModel : INotifyPropertyChanged
    {
        public DatabaseService DatabaseService { get; }

        // -----------------------------
        //  ВЕС
        // -----------------------------
        private ISeries[] _series = Array.Empty<ISeries>();
        public ISeries[] Series
        {
            get => _series;
            set
            {
                if (_series != value)
                {
                    _series = value;
                    OnPropertyChanged(nameof(Series));
                }
            }
        }

        // -----------------------------
        // КАЛЕНДАРЬ
        // -----------------------------
        private EventCollection _events = new();
        public EventCollection Events
        {
            get => _events;
            set { _events = value; OnPropertyChanged(nameof(Events)); }
        }

        private List<Axis> _xAxes = new() { new Axis() };
        public List<Axis> XAxes
        {
            get => _xAxes;
            set
            {
                if (_xAxes != value)
                {
                    _xAxes = value;
                    OnPropertyChanged(nameof(XAxes));
                }
            }
        }

        private List<Axis> _yAxes = new() { new Axis() };
        public List<Axis> YAxes
        {
            get => _yAxes;
            set
            {
                if (_yAxes != value)
                {
                    _yAxes = value;
                    OnPropertyChanged(nameof(YAxes));
                }
            }
        }

        // -----------------------------
        //  ПИТАНИЕ
        // -----------------------------
        private ISeries[] _nutritionSeries = Array.Empty<ISeries>();
        public ISeries[] NutritionSeries
        {
            get => _nutritionSeries;
            set
            {
                if (_nutritionSeries != value)
                {
                    _nutritionSeries = value;
                    OnPropertyChanged(nameof(NutritionSeries));
                }
            }
        }

        private List<Axis> _nutritionXAxes = new() { new Axis() };
        public List<Axis> NutritionXAxes
        {
            get => _nutritionXAxes;
            set
            {
                if (_nutritionXAxes != value)
                {
                    _nutritionXAxes = value;
                    OnPropertyChanged(nameof(NutritionXAxes));
                }
            }
        }

        private List<Axis> _nutritionYAxes = new() { new Axis() };
        public List<Axis> NutritionYAxes
        {
            get => _nutritionYAxes;
            set
            {
                if (_nutritionYAxes != value)
                {
                    _nutritionYAxes = value;
                    OnPropertyChanged(nameof(NutritionYAxes));
                }
            }
        }

        // -----------------------------
        //  РЕЖИМ ОТОБРАЖЕНИЯ (граммы / ккал)
        // -----------------------------
        private bool _isCaloriesMode = false;
        public bool IsCaloriesMode
        {
            get => _isCaloriesMode;
            set
            {
                if (_isCaloriesMode != value)
                {
                    _isCaloriesMode = value;
                    OnPropertyChanged(nameof(IsCaloriesMode));
                }
            }
        }

        public IRelayCommand ToggleNutritionModeCommand { get; }

        // -----------------------------
        //  СОБЫТИЯ
        // -----------------------------
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // -----------------------------
        //  КОНСТРУКТОР
        // -----------------------------
        public ViewModel()
        {
            DatabaseService = DatabaseService.Instance;

            ToggleNutritionModeCommand = new RelayCommand(async () =>
            {
                IsCaloriesMode = !IsCaloriesMode;
                await LoadNutritionChartAsync();
            });

            Task.Run(async () =>
            {
                await DatabaseService.InitAsync();
                await DatabaseService.EnsureFirstWeightAsync();
                await DatabaseService.EnsureNutritionTableAsync();
                await LoadWeightChartAsync();
                await LoadNutritionChartAsync();
                //await LoadCalendarEventsAsync();
            });
        }

        // -----------------------------
        //  ГРАФИК ВЕСА
        // -----------------------------
        private async Task LoadWeightChartAsync()
        {
            var weights = await DatabaseService.GetLast7WeightsAsync();
            var dates = await DatabaseService.GetLast7WeightDatesAsync();
            var dateLabels = dates.Select(d => d.ToString("dd.MM")).ToArray();

            Series = new ISeries[]
            {
                new LineSeries<int>
                {
                    Values = weights,
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.LightBlue) { StrokeThickness = 6 },
                    GeometryStroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 6 },
                    GeometrySize = 10
                }
            };

            XAxes = new List<Axis>
            {
                new Axis
                {
                    Labels = dateLabels,
                    LabelsRotation = 0
                }
            };

            if (weights.Count > 0)
            {
                var min = weights.Min();
                var max = weights.Max();

                YAxes = new List<Axis>
                {
                    new Axis
                    {
                        MinLimit = min - 1,
                        MaxLimit = max + 1,
                        Name = "Вес (кг)"
                    }
                };
            }
        }

        // -----------------------------
        //  ГРАФИК ПИТАНИЯ
        // -----------------------------
        private async Task LoadNutritionChartAsync()
        {

            var entries = await DatabaseService.GetNutritionDataFor7days();

            if (entries == null || entries.Count == 0)
            {
                NutritionSeries = Array.Empty<ISeries>();
                return;
            }

            var dateLabels = entries.Select(e => e.Date.ToString("dd.MM")).ToArray();

            var proteinValues = IsCaloriesMode
                ? entries.Select(e => e.ProteinsCals).ToArray()
                : entries.Select(e => e.ProteinsGrams).ToArray();

            var fatValues = IsCaloriesMode
                ? entries.Select(e => e.FatsCals).ToArray()
                : entries.Select(e => e.FatsGrams).ToArray();

            var carbValues = IsCaloriesMode
                ? entries.Select(e => e.CarbsCals).ToArray()
                : entries.Select(e => e.CarbsGrams).ToArray();

            NutritionSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Белки",
                    Values = proteinValues,
                    Fill = new SolidColorPaint(SKColors.MediumSeaGreen),
                    MaxBarWidth = 30,
                },
                new ColumnSeries<double>
                {
                    Name = "Жиры",
                    Values = fatValues,
                    Fill = new SolidColorPaint(SKColors.LightCoral),
                    MaxBarWidth = 30,
                },
                new ColumnSeries<double>
                {
                    Name = "Углеводы",
                    Values = carbValues,
                    Fill = new SolidColorPaint(SKColors.SkyBlue),
                    MaxBarWidth = 30,
                }
            };

            NutritionXAxes = new List<Axis>
            {
                new Axis
                {
                    Labels = dateLabels,
                    LabelsRotation = 0,
                    TextSize = 12
                }
            };

            NutritionYAxes = new List<Axis>
            {
                new Axis
                {
                    Name = IsCaloriesMode ? "ккал" : "граммы",
                    TextSize = 12
                }
            };
        }


        //private async Task LoadCalendarEventsAsync()
        //{
        //    var allEntries = await DatabaseService.GetAllNutritionEntriesAsync();

        //    var eventCollection = new EventCollection();

        //    foreach (var entry in allEntries)
        //    {
        //        bool ok = DatabaseService.IsCalorieNormMet(entry);

        //        eventCollection[entry.Date.Date] = new List<CalendarEventModel>
        //{
        //    new CalendarEventModel
        //    {
        //        Color = ok ? "#4CAF50" : "#FF5252" 
        //    }
        //};
        //    }

        //    Events = eventCollection;
        //}



        // -----------------------------
        //  ОБНОВЛЕНИЕ ДАННЫХ
        // -----------------------------
        public async Task ReloadAsync()
        {
            await LoadWeightChartAsync();
            await LoadNutritionChartAsync();
            //await LoadCalendarEventsAsync();
        }
    }
}
