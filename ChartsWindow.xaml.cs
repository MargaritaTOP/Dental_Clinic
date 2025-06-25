using DentalClinicApp.Data;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Кожетьева_WPF
{
    public partial class ChartsWindow : Window
    {
        public SeriesCollection PaymentSeries { get; set; }
        public SeriesCollection ServiceSeries { get; set; }
        public SeriesCollection DentistWorkloadSeries { get; set; }
        public SeriesCollection CategorySeries { get; set; }
        public SeriesCollection PaymentMethodSeries { get; set; }
        public SeriesCollection RegistrationSeries { get; set; }
        public SeriesCollection SpecializationSeries { get; set; }
        public SeriesCollection IncomeSeries { get; set; } = new SeriesCollection();
        public string[] IncomeDates { get; set; } = Array.Empty<string>();
        private string[] _months = Array.Empty<string>();
        public string[] Months


        {
            get => _months;
            set
            {
                _months = value ?? Array.Empty<string>();
                OnPropertyChanged(nameof(Months));
            }
        }

        public string[] DentistNames { get; set; } = Array.Empty<string>();
        public Func<double, string> YFormatter { get; } = value => value.ToString("N0") + " руб";
        private string _dbProvider;

        public ChartsWindow(string dbProvider)
        {
            _dbProvider = dbProvider;
            InitializeComponent();

            // Инициализация форматтера для оси Y
            YFormatter = value => value.ToString("N0") + " руб";
            // Инициализация коллекций
            PaymentSeries = new SeriesCollection();
            ServiceSeries = new SeriesCollection();
            DentistWorkloadSeries = new SeriesCollection();
            CategorySeries = new SeriesCollection();
            PaymentMethodSeries = new SeriesCollection();
            RegistrationSeries = new SeriesCollection();
            SpecializationSeries = new SeriesCollection();

            if (DataContext == null)
            {
                DataContext = this;
            }
            LoadChartData();
        }

        private void LoadChartData(dynamic importedData = null)
        {
            try
            {
                // Очистка всех коллекций перед загрузкой новых данных
                ClearAllSeries();

                if (importedData != null)
                {
                    LoadImportedData(importedData);
                    return;
                }

                // Загрузка данных из базы данных
                using (var context = new DentalClinicContext(_dbProvider))
                {
                    LoadIncomeData(context);
                    // Устанавливаем период для мая
                    int targetMonth = 5; // Май
                    int targetYear = DateTime.Now.Year; // Текущий год

                    // Первый день мая
                    DateTime mayStart = new DateTime(targetYear, targetMonth, 1);

                    // Последний день мая
                    DateTime mayEnd = new DateTime(targetYear, targetMonth,
                                                DateTime.DaysInMonth(targetYear, targetMonth));

                    // 1. График платежей по дням (за май)
                    LoadPaymentData(context, mayStart, mayEnd);

                    // 2. График загруженности стоматологов 
                    LoadDentistWorkloadData(context);

                    // 3. График по категориям услуг 
                    LoadServiceCategoriesData(context);

                    // 4. График регистраций пациентов 
                    LoadPatientRegistrationsData(context, mayStart, mayEnd);

                    // 5. График методов оплаты 
                    LoadPaymentMethodsData(context);

                    // 6. График услуг по специализациям 
                    LoadSpecializationData(context);

                    // 7. Круговой график популярных услуг 
                    LoadPopularServicesData(context);
                }

                StatusText.Text = $"Данные графиков успешно загружены (май {DateTime.Now.Year})";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных графиков: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка при загрузке данных графиков";

                // Инициализация пустых данных в если буд. ошибки
                InitializeEmptyData();
            }
            finally
            {
                // Обновляем все коллекции с графиками
                OnPropertyChanged(nameof(PaymentSeries));
                OnPropertyChanged(nameof(ServiceSeries));
                OnPropertyChanged(nameof(DentistWorkloadSeries));
                OnPropertyChanged(nameof(CategorySeries));
                OnPropertyChanged(nameof(PaymentMethodSeries));
                OnPropertyChanged(nameof(RegistrationSeries));
                OnPropertyChanged(nameof(SpecializationSeries));

                // Обновление вспомогательных свойств
                OnPropertyChanged(nameof(Months));
                OnPropertyChanged(nameof(DentistNames));
            }
        }
        private void LoadIncomeData(DentalClinicContext context)
        {
            // Получаем данные за последние 30 дней
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-40);

            var dailyIncome = context.Payments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .GroupBy(p => p.PaymentDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(p => p.Amount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Если нет данных, создаем пустые значения
            if (!dailyIncome.Any())
            {
                dailyIncome = Enumerable.Range(0, 30)
                    .Select(i => new
                    {
                        Date = startDate.AddDays(i),
                        TotalAmount = 0m
                    })
                    .ToList();
            }

            IncomeDates = dailyIncome.Select(x => x.Date.ToString("dd.MM")).ToArray();

            IncomeSeries.Add(new LineSeries
            {
                Title = "Доходы",
                Values = new ChartValues<decimal>(dailyIncome.Select(x => x.TotalAmount)),
                Stroke = Brushes.DodgerBlue,
                Fill = Brushes.Transparent,
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10,
                StrokeThickness = 3
            });
            // вторая линия
            //IncomeSeries.Add(new LineSeries
            //{
            //    Title = "Прошлый месяц",
            //    Values = new ChartValues<decimal>(prevMonthData),
            //    Stroke = Brushes.Orange,
            //    Fill = Brushes.Transparent,
            //    StrokeDashArray = new DoubleCollection { 5 }
            //});
        }
        private void ClearAllSeries()
        {
            Debug.WriteLine($"RegistrationSeries is null (before clear): {RegistrationSeries == null}");

            if (RegistrationSeries == null)
            {
                RegistrationSeries = new SeriesCollection(); // Пересоздаём, если null
            }

            PaymentSeries?.Clear();
            ServiceSeries?.Clear();
            DentistWorkloadSeries?.Clear();
            CategorySeries?.Clear();
            PaymentMethodSeries?.Clear();
            RegistrationSeries?.Clear(); 
            SpecializationSeries?.Clear();
            IncomeSeries?.Clear();
        }

        private void LoadImportedData(dynamic importedData)
        {
            try
            {
                Months = importedData.Months?.ToObject<string[]>() ?? Array.Empty<string>();

                if (importedData.PaymentSeries != null)
                {
                    foreach (var series in importedData.PaymentSeries)
                    {
                        PaymentSeries.Add(new LineSeries
                        {
                            Title = series.Title?.ToString() ?? "Без названия",
                            Values = new ChartValues<double>(series.Values?.ToObject<double[]>() ?? Array.Empty<double>())
                        });
                    }
                }

                if (importedData.ServiceSeries != null)
                {
                    foreach (var series in importedData.ServiceSeries)
                    {
                        ServiceSeries.Add(new PieSeries
                        {
                            Title = series.Title?.ToString() ?? "Без названия",
                            Values = new ChartValues<double>(series.Values?.ToObject<double[]>() ?? Array.Empty<double>()),
                            DataLabels = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке импортированных данных: {ex.Message}");
                throw;
            }
        }

        private (DateTime startDate, DateTime endDate) GetMayDateRange()
        {
            int year = DateTime.Now.Year; // Текущий год
            DateTime startDate = new DateTime(year, 5, 1); // 1 мая
            DateTime endDate = new DateTime(year, 5, 31); // 31 мая
            return (startDate, endDate);
        }
        private void LoadPaymentData(DentalClinicContext context, DateTime startDate, DateTime endDate)
        {
            // Группировка платежей по дням с детализацией по типам услуг
            var dailyData = context.Payments
                .Include(p => p.ServiceRecord)
                    .ThenInclude(sr => sr.DentalService)
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate) 
                .AsEnumerable()
                .GroupBy(p => new
                {
                    Date = p.PaymentDate.Date,
                    Category = p.ServiceRecord?.DentalService?.Category ?? "Без категории"
                })
                .Select(g => new
                {
                    g.Key.Date,
                    g.Key.Category,
                    TotalAmount = g.Sum(p => p.Amount)
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Получаем даты для оси X
            Months = dailyData.Select(d => d.Date.ToString("dd.MM.yyyy"))
                             .Distinct()
                             .ToArray();

            // Группируем по категориям для создания серий
            var categories = dailyData.Select(d => d.Category).Distinct();

            foreach (var category in categories)
            {
                var categoryData = dailyData.Where(d => d.Category == category)
                                           .OrderBy(d => d.Date)
                                           .ToList();

                PaymentSeries.Add(new LineSeries
                {
                    Title = category,
                    Values = new ChartValues<decimal>(categoryData.Select(d => d.TotalAmount)),
                    Stroke = GetCategoryBrush(category),
                    Fill = Brushes.Transparent,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    StrokeThickness = 3
                });
            }
        }

        private Brush GetCategoryBrush(string category)
        {
            // Словарь цветов для разных категорий
            var categoryColors = new Dictionary<string, Brush>
            {
                ["Терапия"] = Brushes.DodgerBlue,
                ["Хирургия"] = Brushes.IndianRed,
                ["Ортодонтия"] = Brushes.Goldenrod,
                ["Имплантация"] = Brushes.MediumSeaGreen,
                ["Без категории"] = Brushes.Gray
            };

            return categoryColors.TryGetValue(category, out var brush)
                   ? brush
                   : Brushes.AliceBlue; 
        }

        private void LoadDentistWorkloadData(DentalClinicContext context)
        {
            var dentistWorkload = context.Appointments
                .Include(a => a.Dentist)
                .Where(a => a.Status == "Завершен")
                .GroupBy(a => new { a.Dentist.LastName, a.Dentist.FirstName })
                .Select(g => new {
                    Dentist = $"{g.Key.LastName} {g.Key.FirstName}",
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            DentistNames = dentistWorkload.Any()
                ? dentistWorkload.Select(d => d.Dentist).ToArray()
                : new[] { "Нет данных" };

            foreach (var item in dentistWorkload)
            {
                DentistWorkloadSeries.Add(new ColumnSeries
                {
                    Title = item.Dentist,
                    Values = new ChartValues<double> { item.Count },
                    Fill = GetServiceColor(item.Dentist), 
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                });
            }
        }

        private void LoadServiceCategoriesData(DentalClinicContext context)
        {
            var serviceCategories = context.ServiceRecords
                .Include(sr => sr.DentalService)
                .Where(sr => sr.DentalService != null)
                .GroupBy(sr => sr.DentalService.Category)
                .Select(g => new {
                    Category = g.Key ?? "Без категории",
                    Total = g.Sum(sr => sr.ActualPrice)
                })
                .ToList();

            foreach (var item in serviceCategories)
            {
                CategorySeries.Add(new PieSeries
                {
                    Title = item.Category,
                    Values = new ChartValues<double> { (double)item.Total },
                    DataLabels = true,
                    Fill = GetCategoryColor(item.Category),
                    Stroke = Brushes.Transparent
                });
            }
        }

        private void LoadPatientRegistrationsData(DentalClinicContext context, DateTime startDate, DateTime endDate)
        {
            try
            {
                var patientRegistrations = context.Patients
                    .Where(p => p.RegistrationDate >= startDate && p.RegistrationDate <= endDate)
                    .GroupBy(p => p.RegistrationDate.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new {
                        Date = g.Key.ToString("dd.MM.yyyy"),
                        Count = g.Count()
                    })
                    .ToList();

                Months = patientRegistrations.Select(x => x.Date).ToArray();

                var registrationValues = patientRegistrations.Any()
                    ? patientRegistrations.Select(x => (double)x.Count).ToList()
                    : new List<double> { 0 };

                if (RegistrationSeries == null)
                {
                    RegistrationSeries = new SeriesCollection();
                }

                RegistrationSeries.Add(new LineSeries
                {
                    Title = $"Регистрации пациентов ({startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy})",
                    Values = new ChartValues<double>(registrationValues),
                    Stroke = Brushes.Green,
                    StrokeThickness = 2,
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 8
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в LoadPatientRegistrationsData: {ex.Message}");
                throw;
            }
        }

        private void LoadPaymentMethodsData(DentalClinicContext context)
        {
            var paymentMethods = context.Payments
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new {
                    Method = g.Key ?? "Не указан",
                    Total = g.Sum(p => p.Amount)
                })
                .ToList();

            foreach (var item in paymentMethods)
            {
                PaymentMethodSeries.Add(new PieSeries
                {
                    Title = item.Method,
                    Values = new ChartValues<double> { (double)item.Total },
                    DataLabels = true,
                    Fill = GetPaymentMethodColor(item.Method),
                    Stroke = Brushes.Transparent
                });
            }
        }

        private void LoadSpecializationData(DentalClinicContext context)
        {
            var serviceBySpecialization = context.ServiceRecords
                .Include(sr => sr.Dentist)
                .Where(sr => sr.Dentist != null)
                .GroupBy(sr => sr.Dentist.Specialization)
                .Select(g => new {
                    Specialization = g.Key ?? "Без специализации",
                    AvgPrice = g.Average(sr => sr.ActualPrice)
                })
                .ToList();

            foreach (var item in serviceBySpecialization)
            {
                SpecializationSeries.Add(new ColumnSeries
                {
                    Title = item.Specialization,
                    Values = new ChartValues<double> { (double)item.AvgPrice },
                    Fill = GetGradientBrush()
                });
            }
        }

        private void LoadPopularServicesData(DentalClinicContext context)
        {
            var services = context.ServiceRecords
                .Include(sr => sr.DentalService)
                .Where(sr => sr.DentalService != null)
                .GroupBy(sr => sr.DentalService.ServiceName)
                .Select(g => new {
                    Name = g.Key ?? "Без названия",
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            foreach (var service in services)
            {
                ServiceSeries.Add(new PieSeries
                {
                    Title = service.Name,
                    Values = new ChartValues<double> { service.Count },
                    DataLabels = true,
                    Fill = GetServiceColor(service.Name),
                    Stroke = Brushes.Transparent
                });
            }
        }

        // Палитра для услуг 
        private Brush GetServiceColor(string serviceName)
        {
            var colors = new Dictionary<string, Color>
            {
                ["Чистка"] = Color.FromRgb(93, 173, 226),
                ["Пломбирование"] = Color.FromRgb(88, 214, 141),
                ["Удаление зуба"] = Color.FromRgb(236, 112, 99),
                ["Имплантация"] = Color.FromRgb(241, 196, 83),
                ["Отбеливание"] = Color.FromRgb(84, 153, 199),
                ["Диагностика"] = Color.FromRgb(155, 204, 80),
                ["Рентген"] = Color.FromRgb(175, 122, 197),
                ["Первичный осмотр"] = Color.FromRgb(242, 121, 53),
                ["Лечение кариеса"] = Color.FromRgb(102, 204, 204),
                ["Установка брекетов"] = Color.FromRgb(220, 118, 51)
            };

            return new SolidColorBrush(colors.TryGetValue(serviceName, out var color)
                ? color
                : GenerateColorFromName(serviceName));
        }

        // Палитра для категорий
        private Brush GetCategoryColor(string category)
        {
            var colors = new Dictionary<string, Color>
            {
                ["Терапия"] = Color.FromRgb(65, 179, 163),
                ["Хирургия"] = Color.FromRgb(219, 84, 97),
                ["Ортодонтия"] = Color.FromRgb(246, 189, 96),
                ["Имплантация"] = Color.FromRgb(140, 109, 180)
            };

            return new SolidColorBrush(colors.TryGetValue(category, out var color)
                ? color
                : Color.FromRgb(149, 165, 166));
        }

        // Палитра для методов оплаты
        private Brush GetPaymentMethodColor(string method)
        {
            var colors = new Dictionary<string, Color>
            {
                ["Наличные"] = Color.FromRgb(46, 204, 113),
                ["Карта"] = Color.FromRgb(52, 152, 219),
                ["Онлайн"] = Color.FromRgb(155, 89, 182),
                ["Страховка"] = Color.FromRgb(26, 188, 156)
            };

            return new SolidColorBrush(colors.TryGetValue(method, out var color)
                ? color
                : Color.FromRgb(189, 195, 199));
        }

        // Генератор цвета на основе названия 
        private Color GenerateColorFromName(string name)
        {
            var hash = name.GetHashCode();
            return Color.FromRgb(
                (byte)((hash & 0xFF0000) >> 16),
                (byte)((hash & 0x00FF00) >> 8),
                (byte)(hash & 0x0000FF));
        }

        private Brush GetGradientBrush()
        {
            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Colors.DarkSlateBlue, 0),
            new GradientStop(Colors.LightSkyBlue, 1)
        }
            };
        }
        private void InitializeEmptyData()
        {
            Months = new[] { "Нет данных" };
            DentistNames = new[] { "Нет данных" };

            PaymentSeries.Add(new LineSeries
            {
                Title = "Нет данных",
                Values = new ChartValues<double> { 0 }
            });

            ServiceSeries.Add(new PieSeries
            {
                Title = "Нет данных",
                Values = new ChartValues<double> { 1 },
                DataLabels = true
            });
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearAllSeries();
                LoadChartData();
                StatusText.Text = "Данные графиков успешно обновлены";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка при обновлении данных графиков";
            }
        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON файл (*.json)|*.json",
                    DefaultExt = ".json",
                    FileName = $"Графики_клиники_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExportToJson(saveFileDialog.FileName);
                    StatusText.Text = $"Данные графиков успешно экспортированы в {saveFileDialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка при экспорте данных графиков";
            }
        }
        private void ExportToJson(string filePath)
        {
            try
            {
                var exportData = new
                {
                    Months = Months,
                    PaymentSeries = PaymentSeries.Select(s => new {
                        Title = s.Title,
                        Values = s.Values.Cast<double>().ToArray()
                    }),
                    ServiceSeries = ServiceSeries.Select(s => new {
                        Title = s.Title,
                        Values = s.Values.Cast<double>().ToArray()
                    }),
                    ДатаЭкспорта = DateTime.Now
                };

                string json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON файлы (*.json)|*.json"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var importedData = ImportFromJson(openFileDialog.FileName);
                    if (importedData != null)
                    {
                        LoadChartData(importedData);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка при импорте данных графиков";
            }
        }
        private dynamic ImportFromJson(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте JSON: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}