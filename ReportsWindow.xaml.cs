using DentalClinicApp.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Кожетьева_WPF
{
    public partial class ReportsWindow : Window
    {
        private string _dbProvider;

        public DateTime StartDate { get; set; } = DateTime.Today.AddMonths(-1);
        public DateTime EndDate { get; set; } = DateTime.Today;
        public bool UseDateRange { get; set; }
        public ReportsWindow(string dbProvider)
        {
            _dbProvider = dbProvider;
            InitializeComponent();
            DataContext = this;
            this.Loaded += async (s, e) => await LoadReports(); 
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
            LoadReports();
            StatusText.Text = $"Данные за период с {StartDate:dd.MM.yyyy} по {EndDate:dd.MM.yyyy}";
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

            // Перезагружаем отчеты
            LoadReports();

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
                Style = (System.Windows.Style)FindResource(typeof(Window))
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
            var contentGrid = new Grid { Margin = new Thickness(15) };
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var titleText = new TextBlock
            {
                Text = "Экспорт отчетов в JSON",
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
                Style = (System.Windows.Style)FindResource("ModernButtonStyle"),
                Margin = new Thickness(10, 0, 10, 0),
                MinWidth = 100
            };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Style = (System.Windows.Style)FindResource("ModernButtonStyle"),
                Margin = new Thickness(10, 0, 10, 0),
                MinWidth = 100
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);

            var spacer = new Border { Background = Brushes.Transparent };
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
                Style = (System.Windows.Style)FindResource(typeof(Window))
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

            var contentGrid = new Grid { Margin = new Thickness(15) };
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var titleText = new TextBlock
            {
                Text = "Импорт отчетов из JSON",
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
                Style = (System.Windows.Style)FindResource("ModernButtonStyle"),
                Margin = new Thickness(10, 0, 10, 0),
                MinWidth = 100
            };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Style = (System.Windows.Style)FindResource("ModernButtonStyle"),
                Margin = new Thickness(10, 0, 10, 0),
                MinWidth = 100
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);

            var spacer = new Border { Background = Brushes.Transparent };
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

        private void ExportToJson(string filePath, dynamic patientReport, dynamic serviceReport)
        {
            var exportData = new
            {
                ОтчетПоПациентам = patientReport,
                ОтчетПоУслугам = serviceReport,
                ДатаЭкспорта = DateTime.Now
            };

            string json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private (IEnumerable<dynamic> patientReport, IEnumerable<dynamic> serviceReport) ImportFromJson(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                dynamic data = JsonConvert.DeserializeObject(json);

                // Проверка структуры импортированных данных
                if (data?.ОтчетПоПациентам == null || data?.ОтчетПоУслугам == null)
                {
                    throw new InvalidDataException("Некорректный формат файла");
                }

                StatusText.Text = $"Данные успешно импортированы из {filePath}";
                return (data.ОтчетПоПациентам, data.ОтчетПоУслугам);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте JSON: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return (null, null);
            }
        }
        public async Task LoadReports(dynamic importedPatientReport = null, dynamic importedServiceReport = null)
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    StatusText.Text = "Загрузка данных...";
                    PatientsReportGrid.ItemsSource = null;
                    ServicesReportGrid.ItemsSource = null;
                });

                List<PatientReportItem> patientReport = new List<PatientReportItem>();
                List<ServiceReportItem> serviceReport = new List<ServiceReportItem>();

                await Task.Run(async () =>
                {
                    using (var context = new DentalClinicContext(_dbProvider))
                    {
                        if (!await context.Database.CanConnectAsync())
                        {
                            return;
                        }

                        var endDatePlusOne = EndDate.AddDays(1);

                        // Загрузка отчета по пациентам
                        if (importedPatientReport == null)
                        {
                            var patientsQuery = context.Patients.AsNoTracking();

                            if (UseDateRange)
                            {
                                patientsQuery = patientsQuery.Where(p => p.ServiceRecords
                                    .Any(sr => sr.ServiceDate >= StartDate && sr.ServiceDate < endDatePlusOne));
                            }

                            var patientsWithData = await patientsQuery
                                .Select(p => new PatientReportItem
                                {
                                    LastName = p.LastName,
                                    FirstName = p.FirstName,
                                    MiddleName = p.MiddleName,
                                    VisitCount = UseDateRange ?
                                        p.ServiceRecords.Count(sr => sr.ServiceDate >= StartDate && sr.ServiceDate < endDatePlusOne) :
                                        p.ServiceRecords.Count(),
                                    TotalSpent = UseDateRange ?
                                        p.ServiceRecords
                                            .Where(sr => sr.ServiceDate >= StartDate && sr.ServiceDate < endDatePlusOne)
                                            .Sum(sr => (double?)sr.ActualPrice) ?? 0 :
                                        p.ServiceRecords.Sum(sr => (double?)sr.ActualPrice) ?? 0
                                })
                                .Where(p => p.VisitCount > 0) // Только пациенты с визитами
                                .OrderByDescending(p => p.TotalSpent)
                                .ToListAsync();

                            // Добавляем пациентов без визитов с нулевыми значениями
                            var allPatients = await context.Patients.AsNoTracking().ToListAsync();
                            patientReport = allPatients
                                .Select(p => new PatientReportItem
                                {
                                    LastName = p.LastName,
                                    FirstName = p.FirstName,
                                    MiddleName = p.MiddleName,
                                    VisitCount = patientsWithData.FirstOrDefault(pat =>
                                        pat.LastName == p.LastName &&
                                        pat.FirstName == p.FirstName)?.VisitCount ?? 0,
                                    TotalSpent = patientsWithData.FirstOrDefault(pat =>
                                        pat.LastName == p.LastName &&
                                        pat.FirstName == p.FirstName)?.TotalSpent ?? 0
                                })
                                .OrderByDescending(p => p.TotalSpent)
                                .ToList();
                        }

                        // Загрузка отчета по услугам
                        if (importedServiceReport == null)
                        {
                            var servicesQuery = context.DentalServices.AsNoTracking();

                            if (UseDateRange)
                            {
                                servicesQuery = servicesQuery.Where(s => s.ServiceRecords
                                    .Any(sr => sr.ServiceDate >= StartDate && sr.ServiceDate < endDatePlusOne));
                            }

                            var servicesWithData = await servicesQuery
                                .Select(s => new ServiceReportItem
                                {
                                    ServiceName = s.ServiceName,
                                    BasePrice = s.BasePrice,
                                    UsageCount = UseDateRange ?
                                        s.ServiceRecords.Count(sr => sr.ServiceDate >= StartDate && sr.ServiceDate < endDatePlusOne) :
                                        s.ServiceRecords.Count(),
                                    TotalIncome = UseDateRange ?
                                        s.ServiceRecords
                                            .Where(sr => sr.ServiceDate >= StartDate && sr.ServiceDate < endDatePlusOne)
                                            .Sum(sr => (decimal?)sr.ActualPrice) ?? 0 :
                                        s.ServiceRecords.Sum(sr => (decimal?)sr.ActualPrice) ?? 0
                                })
                                .Where(s => s.UsageCount > 0) // Только услуги с использованием
                                .OrderByDescending(s => s.UsageCount)
                                .ToListAsync();

                            // Добавляем все услуги, даже с нулевым использованием
                            var allServices = await context.DentalServices.AsNoTracking().ToListAsync();
                            serviceReport = allServices
                                .Select(s => new ServiceReportItem
                                {
                                    ServiceName = s.ServiceName,
                                    BasePrice = s.BasePrice,
                                    UsageCount = servicesWithData.FirstOrDefault(serv =>
                                        serv.ServiceName == s.ServiceName)?.UsageCount ?? 0,
                                    TotalIncome = servicesWithData.FirstOrDefault(serv =>
                                        serv.ServiceName == s.ServiceName)?.TotalIncome ?? 0
                                })
                                .OrderByDescending(s => s.UsageCount)
                                .ToList();
                        }
                    }
                });

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    PatientsReportGrid.ItemsSource = patientReport;
                    ServicesReportGrid.ItemsSource = serviceReport;

                    StatusText.Text = UseDateRange ?
                        $"Данные за период с {StartDate:dd.MM.yyyy} по {EndDate:dd.MM.yyyy} успешно загружены" :
                        "Все данные успешно загружены";
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки отчетов: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Ошибка загрузки отчетов";
                });
            }
        }
        // Классы для отчетов
        public class PatientReportItem
        {
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string MiddleName { get; set; }  
            public int VisitCount { get; set; }
            public double TotalSpent { get; set; }
        }

        public class ServiceReportItem
        {
            public string ServiceName { get; set; }
            public decimal BasePrice { get; set; }
            public int UsageCount { get; set; }
            public decimal TotalIncome { get; set; }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
        //private void RefreshButton_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        PatientsReportGrid.ItemsSource = null;
        //        ServicesReportGrid.ItemsSource = null;
        //        LoadReports();
        //        StatusText.Text = "Отчеты успешно обновлены";
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при обновлении отчетов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadReports();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    StatusText.Text = "Отчеты успешно обновлены";
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при обновлении отчетов: {ex.Message}",
                                 "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PatientsReportGrid.ItemsSource == null || ServicesReportGrid.ItemsSource == null)
                {
                    MessageBox.Show("Нет данных для экспорта", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ShowExportDialog()) return;

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON файл (*.json)|*.json",
                    DefaultExt = ".json",
                    FileName = $"Отчеты_клиники_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var patientReport = PatientsReportGrid.ItemsSource as IEnumerable<PatientReportItem>;
                    var serviceReport = ServicesReportGrid.ItemsSource as IEnumerable<ServiceReportItem>;

                    ExportToJson(saveFileDialog.FileName, patientReport, serviceReport);
                    StatusText.Text = $"Отчеты успешно экспортированы в {saveFileDialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте отчетов: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void ImportButton_Click(object sender, RoutedEventArgs e)
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
                    var (patientReport, serviceReport) = ImportFromJson(openFileDialog.FileName);
                    if (patientReport != null && serviceReport != null)
                    {
                        await LoadReports(patientReport, serviceReport);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте отчетов: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
