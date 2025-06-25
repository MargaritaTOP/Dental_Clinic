using DentalClinicApp.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Кожетьева_WPF
{
    public partial class StatisticsWindow : Window
    {
        private string _dbProvider;
        public DateTime StartDate { get; set; } = DateTime.Today.AddMonths(-1);
        public DateTime EndDate { get; set; } = DateTime.Today;
        public bool UseDateRange { get; set; }
        public StatisticsWindow(string dbProvider)
        {
            _dbProvider = dbProvider;
            InitializeComponent();
            DataContext = this;
            LoadStatistics();
        }

        private void ApplyDateRange_Click(object sender, RoutedEventArgs e)
        {
            if (StartDate > EndDate)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UseDateRange = true;
            LoadStatistics();
            StatusText.Text = $"Данные за период с {StartDate:dd.MM.yyyy} по {EndDate:dd.MM.yyyy}";
        }

        private void LoadStatistics()
        {
            try
            {
                using (var context = new DentalClinicContext(_dbProvider))
                {
                    var serviceRecordsQuery = context.ServiceRecords
                        .Include(sr => sr.Patient)
                        .Include(sr => sr.Dentist)
                        .Include(sr => sr.DentalService)
                        .AsQueryable();

                    var paymentsQuery = context.Payments
                        .Include(p => p.ServiceRecord)
                        .AsQueryable();

                    var appointmentsQuery = context.Appointments
                        .Include(a => a.Patient)
                        .Include(a => a.Dentist)
                        .AsQueryable();

                    if (UseDateRange)
                    {
                        var endDatePlusOne = EndDate.AddDays(1);
                        serviceRecordsQuery = serviceRecordsQuery.Where(sr =>
                            sr.ServiceDate >= StartDate && sr.ServiceDate < endDatePlusOne);
                        paymentsQuery = paymentsQuery.Where(p =>
                            p.PaymentDate >= StartDate && p.PaymentDate < endDatePlusOne);
                        appointmentsQuery = appointmentsQuery.Where(a =>
                            a.ScheduledDate >= StartDate && a.ScheduledDate < endDatePlusOne);

                        StatusText.Text = $"Данные за период: {StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";
                    }
                    else
                    {
                        StatusText.Text = "Показаны все данные (без фильтрации по дате)";
                    }

                    var periodInfo = UseDateRange ? $"с {StartDate:dd.MM.yyyy} по {EndDate:dd.MM.yyyy}" : "все данные";

                    var mainStats = new List<StatisticItem>
                {
                        new StatisticItem
                {
                    Title = UseDateRange ? "Количество услуг (за период)" : "Количество услуг (всего)",
                    Value = serviceRecordsQuery.Count().ToString(),
                    AdditionalInfo = periodInfo
                },
                        new StatisticItem
                {
                    Title = UseDateRange ? "Общий доход (за период)" : "Общий доход (всего)",
                    Value = paymentsQuery.Sum(p => p.Amount).ToString("N2") + " руб.",
                    AdditionalInfo = periodInfo
                },
                        new StatisticItem
                {
                    Title = "Средний чек",
                    Value = (paymentsQuery.Any() ? paymentsQuery.Average(p => p.Amount) : 0).ToString("N2") + " руб.",
                    AdditionalInfo = periodInfo
                },
                        new StatisticItem
                {
                    Title = "Количество пациентов",
                    Value = (UseDateRange ?
                        context.Patients.Count(p => p.Appointments.Any(a =>
                            a.ScheduledDate >= StartDate && a.ScheduledDate < EndDate.AddDays(1))) :
                        context.Patients.Count()).ToString(),
                    AdditionalInfo = periodInfo
                },
                        new StatisticItem
                {
                    Title = "Количество стоматологов",
                    Value = (UseDateRange ?
                        context.Dentists.Count(d => d.Appointments.Any(a =>
                            a.ScheduledDate >= StartDate && a.ScheduledDate < EndDate.AddDays(1))) :
                        context.Dentists.Count()).ToString(),
                    AdditionalInfo = periodInfo
                },                
                        new StatisticItem
                {
                    Title = UseDateRange ? "Записей на прием (за период)" : "Записей на прием (всего)",
                    Value = appointmentsQuery.Count().ToString(),
                    AdditionalInfo = periodInfo
                }
                };

                    MainStatsDataGrid.ItemsSource = mainStats;

                    var serviceStats = context.DentalServices
                        .Select(ds => new
                        {
                            Услуга = ds.ServiceName,
                            Категория = ds.Category,
                            Количество = UseDateRange ?
                                ds.ServiceRecords.Count(sr =>
                                    sr.ServiceDate >= StartDate && sr.ServiceDate < EndDate.AddDays(1)) :
                                ds.ServiceRecords.Count(),
                            Средняя_цена = UseDateRange ?
                                ds.ServiceRecords
                                    .Where(sr => sr.ServiceDate >= StartDate && sr.ServiceDate < EndDate.AddDays(1))
                                    .Average(sr => sr.ActualPrice).ToString("N2") :
                                ds.ServiceRecords.Average(sr => sr.ActualPrice).ToString("N2"),
                            Общий_доход = UseDateRange ?
                                ds.ServiceRecords
                                    .Where(sr => sr.ServiceDate >= StartDate && sr.ServiceDate < EndDate.AddDays(1))
                                    .Sum(sr => sr.ActualPrice).ToString("N2") :
                                ds.ServiceRecords.Sum(sr => sr.ActualPrice).ToString("N2"),
                            Доля_от_общего_дохода = (UseDateRange ?
                                (double)ds.ServiceRecords
                                    .Where(sr => sr.ServiceDate >= StartDate && sr.ServiceDate < EndDate.AddDays(1))
                                    .Sum(sr => sr.ActualPrice) :
                                (double)ds.ServiceRecords.Sum(sr => sr.ActualPrice)) /
                                (paymentsQuery.Any() ? (double)paymentsQuery.Sum(p => p.Amount) : 1) * 100
                        })
                        .Select(x => new
                        {
                            x.Услуга,
                            x.Категория,
                            x.Количество,
                            Средняя_цена = x.Средняя_цена + " руб.",
                            Общий_доход = x.Общий_доход + " руб.",
                            Доля_от_общего_дохода = x.Доля_от_общего_дохода.ToString("N2") + "%"
                        })
                        .OrderByDescending(x => x.Количество)
                        .ToList();

                    ServiceStatsDataGrid.ItemsSource = serviceStats;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка загрузки данных";
            }
        }
        private void ResetDateRange_Click(object sender, RoutedEventArgs e)
        {
            
            UseDateRange = false;

            // Устанавливаем даты по умолчанию (текущий месяц)
            StartDate = DateTime.Today.AddMonths(-1);
            EndDate = DateTime.Today;

            // Обновляем привязки дат
            StartDatePicker.SelectedDate = StartDate;
            EndDatePicker.SelectedDate = EndDate;

            // Перезагружаем статистику
            LoadStatistics();

            // Обновляем статус
            StatusText.Text = "Показаны все данные (без фильтрации по дате)";
        }
        private List<Dictionary<string, object>> ConvertDynamicToDictionary(IEnumerable<dynamic> dynamicList)
        {
            var result = new List<Dictionary<string, object>>();

            foreach (var item in dynamicList)
            {
                var dict = new Dictionary<string, object>();
                var props = item.GetType().GetProperties();

                foreach (var prop in props)
                {
                    dict[prop.Name] = prop.GetValue(item);
                }

                result.Add(dict);
            }
            return result;
        }
        private bool ShowExportDialog()
        {
            var dialog = new Window
            {
                Title = "Экспорт данных в JSON",
                Width = 350,
                Height = 200,
                MinWidth = 300,
                MinHeight = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.CanResize,
                Style = (Style)FindResource(typeof(Window))
            };
            dialog.Effect = new DropShadowEffect
            {
                Color = Colors.Gray,
                Direction = 270,
                ShadowDepth = 5,
                Opacity = 0.5
            };
            var mainGrid = new Grid();
            mainGrid.Background = new LinearGradientBrush(
                Color.FromRgb(224, 247, 250),
                Color.FromRgb(178, 235, 242),
                new Point(0, 0),
                new Point(1, 1));

            var contentGrid = new Grid
            {
                Margin = new Thickness(15)
            };
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var titleText = new TextBlock
            {
                Text = "Экспорт данных в JSON",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = Brushes.DarkSlateGray,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(titleText, 0);
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            var okButton = new Button
            {
                Content = "Экспорт",
                Style = (Style)FindResource("ModernButtonStyle"),
                Margin = new Thickness(10, 0, 10, 0),
                MinWidth = 100
            };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Style = (Style)FindResource("ModernButtonStyle"),
                Margin = new Thickness(10, 0, 10, 0),
                MinWidth = 100
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);

            var spacer = new Border
            {
                Background = Brushes.Transparent
            };
            Grid.SetRow(spacer, 1);
            contentGrid.Children.Add(titleText);
            contentGrid.Children.Add(spacer);
            contentGrid.Children.Add(buttonPanel);
            mainGrid.Children.Add(contentGrid);
            dialog.Content = mainGrid;

            bool result = false;

            okButton.Click += (s, e) => { result = true; dialog.Close(); };
            cancelButton.Click += (s, e) => dialog.Close();

            dialog.ShowDialog();

            return result;
        }

        private bool ShowImportDialog()
        {
            var dialog = new Window
            {
                Title = "Импорт данных из JSON",
                Width = 350,
                Height = 200,
                MinWidth = 300,
                MinHeight = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.CanResize,
                Style = (Style)FindResource(typeof(Window))
            };

            dialog.Effect = new DropShadowEffect
            {
                Color = Colors.Gray,
                Direction = 270,
                ShadowDepth = 5,
                Opacity = 0.5
            };

            var mainGrid = new Grid();
            mainGrid.Background = new LinearGradientBrush
                (Color.FromRgb(224, 247, 250),
                Color.FromRgb(178, 235, 242),
                new Point(0, 0),
                new Point(1, 1));

            var contentGrid = new Grid
            {
                Margin = new Thickness(15)
            };

            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var titleText = new TextBlock
            {
                Text = "Импорт данных из JSON",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = Brushes.DarkSlateGray,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(titleText, 0);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var okButton = new Button
            {
                Content = "Импорт",
                Style = (Style)FindResource("ModernButtonStyle"),
                Margin = new Thickness(10, 0, 10, 0),
                MinWidth = 100
            };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Style = (Style)FindResource("ModernButtonStyle"),
                Margin = new Thickness(10, 0, 10, 0),
                MinWidth = 100
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);

            var spacer = new Border
            {
                Background = Brushes.Transparent
            };
            Grid.SetRow(spacer, 1);

            contentGrid.Children.Add(titleText);
            contentGrid.Children.Add(spacer);
            contentGrid.Children.Add(buttonPanel);
            mainGrid.Children.Add(contentGrid);
            dialog.Content = mainGrid;

            bool result = false;

            okButton.Click += (s, e) => { result = true; dialog.Close(); };
            cancelButton.Click += (s, e) => dialog.Close();

            dialog.ShowDialog();

            return result;
        }
        private void ExportToJson(string filePath, List<StatisticItem> mainStats, List<Dictionary<string, object>> serviceStats)
        {
            var exportData = new
            {
                ОсновнаяСтатистика = mainStats,
                СтатистикаУслуг = serviceStats,
                ДатаЭкспорта = DateTime.Now
            };

            string json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private (List<StatisticItem> mainStats, dynamic serviceStats) ImportFromJson(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                dynamic data = JsonConvert.DeserializeObject(json);

                List<StatisticItem> mainStats = new List<StatisticItem>();
                if (data.ОсновнаяСтатистика != null)
                {
                    foreach (var item in data.ОсновнаяСтатистика)
                    {
                        mainStats.Add(new StatisticItem
                        {
                            Title = item.Title,
                            Value = item.Value,
                            AdditionalInfo = item.AdditionalInfo
                        });
                    }
                }

                dynamic serviceStats = data.СтатистикаУслуг;

                StatusText.Text = $"Данные успешно импортированы из {filePath}";
                return (mainStats, serviceStats);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте JSON: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return (null, null);
            }
        }
        public void LoadStatistics(List<StatisticItem> mainStats, dynamic serviceStats)
        {
            MainStatsDataGrid.ItemsSource = mainStats;
            ServiceStatsDataGrid.ItemsSource = serviceStats;
        }
        public class StatisticItem
        {
            public string? Title { get; set; }
            public string? Value { get; set; }
            public string? AdditionalInfo { get; set; }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_KeyDown(object sender, KeyEventArgs e) => Close();
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainStatsDataGrid.ItemsSource = null;
                ServiceStatsDataGrid.ItemsSource = null;
                LoadStatistics();
                StatusText.Text = "Данные успешно обновлены";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ShowExportDialog()) return;

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON файл (*.json)|*.json",
                    DefaultExt = ".json",
                    FileName = $"Статистика_клиники_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var mainStats = MainStatsDataGrid.Items.Cast<StatisticItem>().ToList();
                    var dynamicServiceStats = ServiceStatsDataGrid.Items.Cast<dynamic>().ToList();
                    var serviceStats = ConvertDynamicToDictionary(dynamicServiceStats);

                    ExportToJson(saveFileDialog.FileName, mainStats, serviceStats);
                    StatusText.Text = $"Данные успешно экспортированы в {saveFileDialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ShowImportDialog()) return;

                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON файлы (*.json)|*.json"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var (mainStats, serviceStats) = ImportFromJson(openFileDialog.FileName);
                    if (mainStats != null || serviceStats != null)
                    {
                        // Заменяем вызов LoadStatistics на прямое присвоение источников данных
                        MainStatsDataGrid.ItemsSource = mainStats;
                        ServiceStatsDataGrid.ItemsSource = serviceStats;

                        StatusText.Text = $"Данные успешно импортированы из {openFileDialog.FileName}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}