using DentalClinicApp;
using DentalClinicApp.Data;
using DentalClinicApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Кожетьева_WPF;
using static Кожетьева_WPF.StatisticsWindow;

namespace DentalClinicApp
{
    public partial class MainWindow : Window
    {
        Patient_Window? Patient_Patient;
        Dentist_Window? Dentist_Dentist;
        private readonly ObservableCollection<Patient> _patients;
        private readonly ObservableCollection<Dentist> _dentists;
        private readonly ObservableCollection<Appointment> _appointments;
        private readonly ObservableCollection<DentalService> _services;
        private readonly ObservableCollection<ServiceRecord> _serviceRecords;
        private readonly ObservableCollection<Payment> _payments;
        private DentalClinicContext _context;
        private readonly List<string> _providers = ["SQLite", "SqlServer"];
        public MainWindow()
        {
            InitializeComponent();

            _patients = [];
            _dentists = [];
            _appointments = [];
            _services = [];
            _serviceRecords = [];
            _payments = [];

            StandardButtonsPanel = (StackPanel)FindName("StandardButtonsPanel");
            AnalyticsButtonsPanel = (StackPanel)FindName("AnalyticsButtonsPanel");
            DataGridPatients.ItemsSource = _patients;
            DataGridDentists.ItemsSource = _dentists;
            DataGridAppointments.ItemsSource = _appointments;
            DataGridServices.ItemsSource = _services;
            DataGridServiceRecords.ItemsSource = _serviceRecords;
            DataGridPayments.ItemsSource = _payments;

            ProviderDB.ItemsSource = _providers;
            ProviderDB.SelectedIndex = 0;
            _context = new DentalClinicContext("SQLite");

            ConfigureDataGridColumns();

            LoadData();

            // Горячие клавиши
            SetupKeyboardShortcuts();
        }
        private void ProviderDB_DropDownClosed(object sender, EventArgs e)
        {
            if (ProviderDB.SelectedItem is string selectedProvider)
            {
                ChangeDatabaseProvider(selectedProvider);
            }
        }

        //горячие клавиши
        private void SetupKeyboardShortcuts()
        {
            // F5 - обновление
            this.InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => RefreshClick(null, null)),
                new KeyGesture(Key.F5)));

            // Ctrl+N - добавление
            this.InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => AddClick(null, null)),
                new KeyGesture(Key.N, ModifierKeys.Control)));

            // Ctrl+E - редактирование
            this.InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => EditClick(null, null)),
                new KeyGesture(Key.E, ModifierKeys.Control)));

            // Delete - удаление
            this.InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => DeleteClick(null, null)),
                new KeyGesture(Key.Delete)));

            // Ctrl+Shift+S - статистика
            this.InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => StatisticsButton_Click(null, null)),
                new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift)));

            // Ctrl+Shift+R - отчетность
            this.InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ReportsButton_Click(null, null)),
                new KeyGesture(Key.R, ModifierKeys.Control | ModifierKeys.Shift)));

            // Ctrl+Shift+G - графики
            this.InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ChartsButton_Click(null, null)),
                new KeyGesture(Key.G, ModifierKeys.Control | ModifierKeys.Shift)));

            // Ctrl+Shift+L - переключение на SQLite
            this.InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ChangeDatabaseProvider("SQLite")),
                new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Shift)));

            // Ctrl+Shift+M - переключение на SQL Server
            this.InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ChangeDatabaseProvider("SqlServer")),
                new KeyGesture(Key.M, ModifierKeys.Control | ModifierKeys.Shift)));
        }
        private void ChangeDatabaseProvider(string provider)
        {
            _context?.Dispose();

            _context = new DentalClinicContext(provider);

            LoadData();

            StatusText.Text = $"База данных изменена на: {provider}";
        }
        //загрузка
        private void LoadData(bool loadAll = true)
        {
            try
            {
                var currentTab = GetCurrentTab();
                if (_context == null) return;

                // Отключаем автоматическое отслеживание изменений 
                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                if (loadAll || currentTab == "Пациенты")
                {
                    var patients = _context.Patients.AsNoTracking().ToList();
                    SyncCollections(_patients, patients, (p1, p2) => p1.PatientID == p2.PatientID);
                }

                if (loadAll || currentTab == "Стоматологи")
                {
                    var dentists = _context.Dentists.AsNoTracking().ToList();
                    SyncCollections(_dentists, dentists, (d1, d2) => d1.DentistID == d2.DentistID);
                }

                if (loadAll || currentTab == "Записи на прием")
                {
                    var appointments = _context.Appointments
                        .Include(a => a.Patient)
                        .Include(a => a.Dentist)
                        .AsNoTracking()
                        .ToList();
                    SyncCollections(_appointments, appointments, (a1, a2) => a1.AppointmentID == a2.AppointmentID);
                }

                if (loadAll || currentTab == "Услуги")
                {
                    var services = _context.DentalServices.AsNoTracking().ToList();
                    SyncCollections(_services, services, (s1, s2) => s1.ServiceID == s2.ServiceID);
                }

                if (loadAll || currentTab == "Оказанные услуги")
                {
                    var serviceRecords = _context.ServiceRecords
                        .Include(sr => sr.Patient)
                        .Include(sr => sr.Dentist)
                        .Include(sr => sr.DentalService)
                        .AsNoTracking()
                        .ToList();
                    SyncCollections(_serviceRecords, serviceRecords, (sr1, sr2) => sr1.RecordID == sr2.RecordID);
                }

                if (loadAll || currentTab == "Платежи")
                {
                    var payments = _context.Payments
                        .Include(p => p.ServiceRecord)
                        .AsNoTracking()
                        .ToList();
                    SyncCollections(_payments, payments, (p1, p2) => p1.PaymentID == p2.PaymentID);
                }

                // Включаем обратно отслеживание изменений
                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                StatusText.Text = "Данные успешно загружены";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        // Метод для синхронизации коллекций
        private void SyncCollections<T>(ObservableCollection<T> target, List<T> source, Func<T, T, bool> compareFunc) where T : class
        {
            // Создаем временный список для новых элементов
            var newItems = new List<T>();

            // Обновляем существующие элементы и находим новые
            foreach (var sourceItem in source)
            {
                var existingItem = target.FirstOrDefault(t => compareFunc(t, sourceItem));
                if (existingItem != null)
                {
                    
                    _context.Entry(existingItem).CurrentValues.SetValues(sourceItem);
                }
                else
                {
                    newItems.Add(sourceItem);
                }
            }

            
            for (int i = target.Count - 1; i >= 0; i--)
            {
                var targetItem = target[i];
                if (!source.Any(s => compareFunc(s, targetItem)))
                {
                    target.RemoveAt(i);
                }
            }

            // Добавляем новые элементы
            foreach (var newItem in newItems)
            {
                target.Add(newItem);
            }

            // обновляем привязку данных
            if (target.Count > 0)
            {
                var collectionView = CollectionViewSource.GetDefaultView(target);
                collectionView.Refresh();
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            _context?.Dispose();
            base.OnClosing(e);
        }
        private void ConfigureDataGridColumns()
        {
            // таблица пациенты
            DataGridPatients.Columns.Clear();
            AddColumn(DataGridPatients, "ID", "PatientID", 50);
            AddColumn(DataGridPatients, "Фамилия", "LastName", 100);
            AddColumn(DataGridPatients, "Имя", "FirstName", 100);
            AddColumn(DataGridPatients, "Отчество", "MiddleName", 100);
            AddColumn(DataGridPatients, "Дата рождения", "BirthDate", 120, "dd.MM.yyyy");
            AddColumn(DataGridPatients, "Пол", "Gender", 50);
            AddColumn(DataGridPatients, "Телефон", "Phone", 100);
            AddColumn(DataGridPatients, "Email", "Email", 150);
            AddColumn(DataGridPatients, "Адрес", "Address", 170);
            AddColumn(DataGridPatients, "Дата регистрации", "RegistrationDate", 120, "dd.MM.yyyy");
            AddColumn(DataGridPatients, "Медицинская карта", "MedicalHistory", 200);

            // таблица стоматологи
            DataGridDentists.Columns.Clear();
            AddColumn(DataGridDentists, "ID", "DentistID", 50);
            AddColumn(DataGridDentists, "Фамилия", "LastName", 100);
            AddColumn(DataGridDentists, "Имя", "FirstName", 100);
            AddColumn(DataGridDentists, "Отчество", "MiddleName", 100);
            AddColumn(DataGridDentists, "Специализация", "Specialization", 200);
            AddColumn(DataGridDentists, "Лицензия", "LicenseNumber", 100);
            AddColumn(DataGridDentists, "Дата приёма на работу", "HireDate", 160, "dd.MM.yyyy");
            AddColumn(DataGridDentists, "Номер телефона", "Phone", 120);
            AddColumn(DataGridDentists, "Email", "Email", 150);

            // таблица записи на прием
            DataGridAppointments.Columns.Clear();
            AddColumn(DataGridAppointments, "ID", "AppointmentID", 50);
            AddColumn(DataGridAppointments, "Пациент", "Patient.LastName", 120);
            AddColumn(DataGridAppointments, "Стоматолог", "Dentist.LastName", 120);
            AddColumn(DataGridAppointments, "Дата", "ScheduledDate", 120, "dd.MM.yyyy HH:mm");
            AddColumn(DataGridAppointments, "Длительность", "DurationMinutes", 100);
            AddColumn(DataGridAppointments, "Цель", "Purpose", 150);
            AddColumn(DataGridAppointments, "Статус", "Status", 80);

            // таблица услуги
            DataGridServices.Columns.Clear();
            AddColumn(DataGridServices, "ID", "ServiceID", 50);
            AddColumn(DataGridServices, "Название", "ServiceName", 150);
            AddColumn(DataGridServices, "Описание", "Description", 240);
            AddColumn(DataGridServices, "Длительность", "DurationMinutes", 100);
            AddColumn(DataGridServices, "Цена", "BasePrice", 80, "N2");
            AddColumn(DataGridServices, "Категория", "Category", 100);

            // таблица записи об услугах
            DataGridServiceRecords.Columns.Clear();
            AddColumn(DataGridServiceRecords, "ID", "RecordID", 50);
            AddColumn(DataGridServiceRecords, "Пациент", "Patient.LastName", 120);
            AddColumn(DataGridServiceRecords, "Стоматолог", "Dentist.LastName", 120);
            AddColumn(DataGridServiceRecords, "Услуга", "DentalService.ServiceName", 150);
            AddColumn(DataGridServiceRecords, "Дата", "ServiceDate", 120, "dd.MM.yyyy HH:mm");
            AddColumn(DataGridServiceRecords, "Цена", "ActualPrice", 100, "N2");
            AddColumn(DataGridServiceRecords, "Комментарий", "Notes", 280);
            AddColumn(DataGridServiceRecords, "Статус", "Status", 80);

            // таблица платежи
            DataGridPayments.Columns.Clear();
            AddColumn(DataGridPayments, "ID", "PaymentID", 50);
            AddColumn(DataGridPayments, "Запись", "ServiceRecord.RecordID", 50);
            AddColumn(DataGridPayments, "Дата", "PaymentDate", 100, "dd.MM.yyyy");
            AddColumn(DataGridPayments, "Сумма", "Amount", 80, "N2");
            AddColumn(DataGridPayments, "Метод", "PaymentMethod", 100);
            AddColumn(DataGridPayments, "Статус", "Status", 130);
        }
        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedItem is TabItem selectedTab)
            {
                var tabName = selectedTab.Header.ToString();
                bool showAnalytics = tabName == "Оказанные услуги" || tabName == "Платежи";

                StandardButtonsPanel.Visibility = showAnalytics ? Visibility.Collapsed : Visibility.Visible;
                AnalyticsButtonsPanel.Visibility = showAnalytics ? Visibility.Visible : Visibility.Collapsed;

                UpdateContextMenuVisibility(showAnalytics);
            }
        }
        private void UpdateContextMenuVisibility(bool showAnalytics)
        {
            // Обновляем видимость пунктов основного контекстного меню
            if (MainContextMenu != null)
            {
                StatsMenuItem.Visibility = showAnalytics ? Visibility.Visible : Visibility.Collapsed;
                ReportsMenuItem.Visibility = showAnalytics ? Visibility.Visible : Visibility.Collapsed;
                ChartsMenuItem.Visibility = showAnalytics ? Visibility.Visible : Visibility.Collapsed;

                AddMenuItem.Visibility = Visibility.Visible;
                EditMenuItem.Visibility = Visibility.Visible;
                DeleteMenuItem.Visibility = Visibility.Visible;
            }

            // Обновляем контекстные меню для DataGrid
            if (DataGridServiceRecords.ContextMenu is ContextMenu serviceRecordsMenu)
            {
                SetMenuItemsVisibility(serviceRecordsMenu, showAnalytics);
            }

            if (DataGridPayments.ContextMenu is ContextMenu paymentsMenu)
            {
                SetMenuItemsVisibility(paymentsMenu, showAnalytics);
            }
        }
        private void SetMenuItemsVisibility(ContextMenu menu, bool showAnalytics)
        {
            foreach (var item in menu.Items)
            {
                if (item is MenuItem menuItem)
                {
                    switch (menuItem.Header)
                    {
                        case "Статистика":
                        case "Отчётность":
                        case "Графики":
                            menuItem.Visibility = showAnalytics ? Visibility.Visible : Visibility.Collapsed;
                            break;
                        case "Добавить":
                        case "Редактировать":
                        case "Удалить":
                            menuItem.Visibility = showAnalytics ? Visibility.Collapsed : Visibility.Visible;
                            break;
                    }
                }
                else if (item is Separator separator)
                {
                    
                }
            }
        }
        private string GetCurrentTab()
        {
            if (MainTabControl.SelectedItem is TabItem selectedTab)
            {
                return selectedTab.Header.ToString();
            }
            return string.Empty;
        }
        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            var currentTab = GetCurrentTab();
            if (currentTab != "Оказанные услуги" && currentTab != "Платежи")
            {
                MessageBox.Show("Статистика доступна только на вкладках 'Оказанные услуги' и 'Платежи'");
                return;
            }
            if (currentTab == "Оказанные услуги")
            {
                ShowServiceRecordsStatistics();
            }
            else if (currentTab == "Платежи")
            {
                ShowPaymentsStatistics();
            }
        }
        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            var currentTab = GetCurrentTab();
            if (currentTab != "Оказанные услуги" && currentTab != "Платежи")
            {
                MessageBox.Show("Отчетность доступна только на вкладках 'Оказанные услуги' и 'Платежи'");
                return;
            }
            var reportsWindow = new ReportsWindow(ProviderDB.Text)
            {
                Owner = this
            };
            reportsWindow.ShowDialog();
        }
        private void ChartsButton_Click(object sender, RoutedEventArgs e)
        {
            var currentTab = GetCurrentTab();
            if (currentTab != "Оказанные услуги" && currentTab != "Платежи")
            {
                MessageBox.Show("Графики доступны только на вкладках 'Оказанные услуги' и 'Платежи'");
                return;
            }
            var chartsWindow = new ChartsWindow(ProviderDB.Text)
            {
                Owner = this
            };
            chartsWindow.ShowDialog();
        }
        private void ShowServiceRecordsStatistics()
        {
            var statsWindow = new StatisticsWindow(ProviderDB.Text);

            using (var context = new DentalClinicContext(ProviderDB.Text))
            {
                var mainStats = new List<StatisticItem>
        {
            new()
            {
                Title = "Всего оказано услуг",
                Value = context.ServiceRecords.Count().ToString(),
                AdditionalInfo = ""
            },
            new()
            {
                Title = "Средняя цена услуги",
                Value = context.ServiceRecords.Average(sr => sr.ActualPrice).ToString("N2"),
                AdditionalInfo = ""
            },
            new()
            {
                Title = "Общий доход от услуг",
                Value = context.ServiceRecords.Sum(sr => sr.ActualPrice).ToString("N2"),
                AdditionalInfo = ""
            },
            new()
            {
                Title = "Самая популярная услуга",
                Value = context.ServiceRecords
                    .GroupBy(sr => sr.DentalService.ServiceName)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Select(x => x.Name)
                    .FirstOrDefault() ?? "Нет данных",
                AdditionalInfo = ""
            }
        };

                var serviceStats = context.ServiceRecords
                    .Include(sr => sr.DentalService)
                    .Include(sr => sr.Patient)
                    .GroupBy(sr => sr.DentalService.ServiceName)
                    .Select(g => new
                    {
                        Услуга = g.Key,
                        Количество = g.Count(),
                        Средняя_цена = g.Average(sr => sr.ActualPrice),
                        Общий_доход = g.Sum(sr => sr.ActualPrice),
                        Пациенты = g.Select(sr => sr.Patient.FullName).Distinct().Count()
                    })
                    .OrderByDescending(x => x.Количество)
                    .ToList();

                statsWindow.LoadStatistics(mainStats, serviceStats);
            }

            statsWindow.ShowDialog(); 
        }
        private void ShowPaymentsStatistics()
        {
            var statsWindow = new StatisticsWindow(ProviderDB.Text);

            using (var context = new DentalClinicContext(ProviderDB.Text))
            {
                var mainStats = new List<StatisticItem>
        {
            new()
            {
                Title = "Всего платежей",
                Value = context.Payments.Count().ToString(),
                AdditionalInfo = ""
            },
            new()
            {
                Title = "Общая сумма платежей",
                Value = context.Payments.Sum(p => p.Amount).ToString("N2"),
                AdditionalInfo = ""
            },
            new()
            {
                Title = "Средний платеж",
                Value = context.Payments.Average(p => p.Amount).ToString("N2"),
                AdditionalInfo = ""
            },
            new()
            {
                Title = "Самый частый метод оплаты",
                Value = context.Payments
                    .GroupBy(p => p.PaymentMethod)
                    .Select(g => new { Method = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Select(x => x.Method)
                    .FirstOrDefault() ?? "Нет данных",
                AdditionalInfo = ""
            }
        };

                var paymentStats = context.Payments
                    .Include(p => p.ServiceRecord)
                        .ThenInclude(sr => sr.Patient)
                    .GroupBy(p => p.PaymentMethod)
                    .Select(g => new
                    {
                        Метод_оплаты = g.Key,
                        Количество = g.Count(),
                        Сумма = g.Sum(p => p.Amount),
                        Средний_платеж = g.Average(p => p.Amount),
                        Уникальные_пациенты = g.Select(p => p.ServiceRecord.Patient.FullName).Distinct().Count()
                    })
                    .OrderByDescending(x => x.Количество)
                    .ToList();

                statsWindow.LoadStatistics(mainStats, paymentStats);
            }

            statsWindow.ShowDialog(); 
        }
        private void ShowGeneralStatistics()
        {
            var statsWindow = new StatisticsWindow(ProviderDB.Text);

            using (var context = new DentalClinicContext(ProviderDB.Text))
            {
                var mainStats = new List<StatisticItem>
        {
            new StatisticItem
            {
                Title = "Общая статистика",
                Value = "Данные не загружены",
                AdditionalInfo = ""
            }
        };

                var serviceStats = new List<dynamic>(); 

                statsWindow.LoadStatistics(mainStats, serviceStats);
            }

            statsWindow.ShowDialog();
        }
        private void AddColumn(DataGrid dataGrid, string header, string bindingPath, double width, string stringFormat = null)
        {
            var column = new DataGridTextColumn
            {
                Header = header,
                Width = width,
                Binding = stringFormat != null
                    ? new Binding(bindingPath) { StringFormat = stringFormat }
                    : new Binding(bindingPath)
            };
            dataGrid.Columns.Add(column);
        }
        private void AddClick(object sender, RoutedEventArgs e)
        {
            var currentTab = MainTabControl.SelectedIndex;

            try
            {
                using (var context = new DentalClinicContext(ProviderDB.Text))
                {
                    switch (currentTab)
                    {
                        case 0: // Пациенты
                            var patientWindow = new PatientEditWindow(context, 0);
                            if (patientWindow.ShowDialog() == true) LoadData();
                            break;
                        case 1: // Стоматологи
                            var dentistWindow = new DentistEditWindow(context, 0);
                            if (dentistWindow.ShowDialog() == true) LoadData();
                            break;
                        case 2: // Записи на прием
                            var appointmentWindow = new AppointmentEditWindow(context, 0);
                            if (appointmentWindow.ShowDialog() == true) LoadData();
                            break;
                        case 3: // Услуги
                            var serviceWindow = new ServiceEditWindow(context, 0);
                            if (serviceWindow.ShowDialog() == true) LoadData();
                            break;
                        case 4: // Оказанные услуги
                            var serviceRecordWindow = new ServiceRecordEditWindow(context, 0);
                            if (serviceRecordWindow.ShowDialog() == true) LoadData();
                            break;
                        case 5: // Платежи
                            var paymentWindow = new PaymentEditWindow(context, 0);
                            if (paymentWindow.ShowDialog() == true) LoadData();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}");
            }
        }
        //кнопка редактирования
        private void EditClick(object sender, RoutedEventArgs e)
        {
            var currentTab = MainTabControl.SelectedIndex;

            try
            {
                using (var context = new DentalClinicContext(ProviderDB.Text))
                {
                    switch (currentTab)
                    {
                        case 0: // пациенты
                            if (DataGridPatients.SelectedItem is Patient selectedPatient)
                            {
                                var patientWindow = new PatientEditWindow(context, selectedPatient.PatientID);
                                if (patientWindow.ShowDialog() == true) LoadData();
                            }
                            break;
                        case 1: // стоматологи
                            if (DataGridDentists.SelectedItem is Dentist selectedDentist)
                            {
                                var dentistWindow = new DentistEditWindow(context, selectedDentist.DentistID);
                                if (dentistWindow.ShowDialog() == true) LoadData();
                            }
                            break;
                        case 2: // Записи на прием
                            if (DataGridAppointments.SelectedItem is Appointment selectedAppointment)
                            {
                                var appointmentWindow = new AppointmentEditWindow(context, selectedAppointment.AppointmentID);
                                if (appointmentWindow.ShowDialog() == true) LoadData();
                            }
                            break;
                        case 3: // Услуги
                            if (DataGridServices.SelectedItem is DentalService selectedService)
                            {
                                var serviceWindow = new ServiceEditWindow(context, selectedService.ServiceID);
                                if (serviceWindow.ShowDialog() == true) LoadData();
                            }
                            break;
                        case 4: // Оказанные услуги
                            if (DataGridServiceRecords.SelectedItem is ServiceRecord selectedRecord)
                            {
                                var serviceRecordWindow = new ServiceRecordEditWindow(context, selectedRecord.RecordID);
                                if (serviceRecordWindow.ShowDialog() == true) LoadData();
                            }
                            break;
                        case 5: // Платежи
                            if (DataGridPayments.SelectedItem is Payment selectedPayment)
                            {
                                var paymentWindow = new PaymentEditWindow(context, selectedPayment.PaymentID);
                                if (paymentWindow.ShowDialog() == true) LoadData();
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}");
            }
        }
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            var currentTab = MainTabControl.SelectedIndex;

            try
            {
                using (var context = new DentalClinicContext(ProviderDB.Text))
                {
                    switch (currentTab)
                    {
                        case 0: // Пациенты
                            if (DataGridPatients.SelectedItem is Patient patient)
                            {

                                if (MessageBox.Show($"Удалить пациента {patient.FullName}?", "Подтверждение удаления",
                                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    context.Patients.Remove(patient);
                                    context.SaveChanges();
                                    _patients.Remove(patient);
                                }
                            }
                            break;
                        case 1: // Стоматологи
                            if (DataGridDentists.SelectedItem is Dentist dentist)
                            {
                                if (MessageBox.Show($"Удалить стоматолога {dentist.LastName}?", "Подтверждение",
                                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    context.Dentists.Remove(dentist);
                                    context.SaveChanges();
                                    _dentists.Remove(dentist);
                                }
                            }
                            break;
                        case 2: // Записи на прием
                            if (DataGridAppointments.SelectedItem is Appointment appointment)
                            {
                                if (MessageBox.Show($"Удалить запись на {appointment.ScheduledDate:dd.MM.yyyy}?",
                                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    context.Appointments.Remove(appointment);
                                    context.SaveChanges();
                                    _appointments.Remove(appointment);
                                }
                            }
                            break;
                        case 3: // Услуги
                            if (DataGridServices.SelectedItem is DentalService service)
                            {
                                if (MessageBox.Show($"Удалить услугу {service.ServiceName}?", "Подтверждение",
                                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    context.DentalServices.Remove(service);
                                    context.SaveChanges();
                                    _services.Remove(service);
                                }
                            }
                            break;
                        case 4: // Оказанные услуги
                            if (DataGridServiceRecords.SelectedItem is ServiceRecord record)
                            {
                                if (MessageBox.Show($"Удалить запись об услуге от {record.ServiceDate:dd.MM.yyyy}?", "Подтверждение",
                                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    context.ServiceRecords.Remove(record);
                                    context.SaveChanges();
                                    _serviceRecords.Remove(record);
                                }
                            }
                            break;
                        case 5: // Платежи
                            if (DataGridPayments.SelectedItem is Payment payment)
                            {
                                if (MessageBox.Show($"Удалить платеж от {payment.PaymentDate:dd.MM.yyyy}?", "Подтверждение",
                                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    context.Payments.Remove(payment);
                                    context.SaveChanges();
                                    _payments.Remove(payment);
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close(); 
        }
        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentTab = GetCurrentTab();

                using (var context = new DentalClinicContext(ProviderDB.Text))
                {
                    switch (currentTab)
                    {
                        case "Услуги":
                            var services = context.DentalServices.ToList();
                            _services.Clear();
                            foreach (var service in services) _services.Add(service);

                            var updatedServiceRecords = context.ServiceRecords
                                .Include(sr => sr.Patient)
                                .Include(sr => sr.Dentist)
                                .Include(sr => sr.DentalService)
                                .ToList();
                            _serviceRecords.Clear();
                            foreach (var record in updatedServiceRecords) _serviceRecords.Add(record);

                            DataGridServices.Items.Refresh();
                            DataGridServiceRecords.Items.Refresh();
                            break;

                        case "Оказанные услуги":
                            var serviceRecords = context.ServiceRecords
                                .Include(sr => sr.Patient)
                                .Include(sr => sr.Dentist)
                                .Include(sr => sr.DentalService)
                                .ToList();
                            _serviceRecords.Clear();
                            foreach (var record in serviceRecords) _serviceRecords.Add(record);
                            DataGridServiceRecords.Items.Refresh();
                            break;

                        case "Платежи":
                            var payments = context.Payments
                                .Include(p => p.ServiceRecord)
                                .ThenInclude(sr => sr.DentalService) 
                                .ToList();
                            _payments.Clear();
                            foreach (var payment in payments) _payments.Add(payment);
                            DataGridPayments.Items.Refresh();
                            break;

                        default:
                            LoadData();
                            break;
                    }
                }
                StatusText.Text = "Данные успешно обновлены";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
        private void AboutClick(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new About();
            aboutWindow.Owner = this;  
            aboutWindow.ShowDialog(); 
        }
        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Predicate<object> _canExecute;

            public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }
            public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
            public void Execute(object parameter) => _execute(parameter);
            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }
        }
        private void AddPatient_Click(object sender, RoutedEventArgs e)
        {
            AddClick(sender, e);
        }
        private void EditPatient_Click(object sender, RoutedEventArgs e)
        {
            EditClick(sender, e);
        }
        private void DeletePatient_Click(object sender, RoutedEventArgs e)
        {
            DeleteClick(sender, e);
        }
        private void ShowPatientStats_Click(object sender, RoutedEventArgs e)
        {
           
        }
        private void EditPatient(object sender, RoutedEventArgs e)
        {
            Patient_Patient = new Patient_Window(_context);
            Patient_Patient?.Show();
        }

        private void EditDentist(object sender, RoutedEventArgs e)
        {
            Dentist_Dentist = new Dentist_Window(_context);
            Dentist_Dentist?.Show();
        }

        private void DataGridPatients_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var isItemSelected = DataGridPatients.SelectedItem != null;

            if (((ContextMenu)e.Source).FindName("EditPatientMenu") is MenuItem editItem)
            {
                editItem.IsEnabled = isItemSelected;
            }
            if (((ContextMenu)e.Source).FindName("DeletePatientMenu") is MenuItem deleteItem)
            {
                deleteItem.IsEnabled = isItemSelected;
            }
            if (DataGridPatients.SelectedItem == null)
            {
                foreach (var item in ((ContextMenu)e.Source).Items)
                {
                    if (item is MenuItem menuItem &&
                        (menuItem.Header.ToString() == "Редактировать пациента" ||
                         menuItem.Header.ToString() == "Удалить пациента"))
                    {
                        menuItem.IsEnabled = false;
                    }
                }
            }
            else
            {
                //  пункты меню при выборе
                foreach (var item in ((ContextMenu)e.Source).Items)
                {
                    if (item is MenuItem menuItem)
                    {
                        menuItem.IsEnabled = true;
                    }
                }
            }
        }
        private void DataGridServiceRecords_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var isItemSelected = DataGridServiceRecords.SelectedItem != null;
            var currentTab = GetCurrentTab();
            bool showAnalytics = currentTab == "Оказанные услуги" || currentTab == "Платежи";

            foreach (var item in ((ContextMenu)e.Source).Items)
            {
                if (item is MenuItem menuItem)
                {
                    // Управление доступностью в зависимости от выбора элемента
                    if (menuItem.Header.ToString() == "Статистика" ||
                        menuItem.Header.ToString() == "Отчётность" ||
                        menuItem.Header.ToString() == "Графики")
                    {
                        menuItem.IsEnabled = isItemSelected && showAnalytics;
                    }
                    else if (menuItem.Header.ToString() == "Редактировать" ||
                             menuItem.Header.ToString() == "Удалить")
                    {
                        menuItem.IsEnabled = isItemSelected && !showAnalytics;
                    }
                }
            }
        }
        private void DataGridPayments_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var isItemSelected = DataGridPayments.SelectedItem != null;
            var currentTab = GetCurrentTab();
            bool showAnalytics = currentTab == "Оказанные услуги" || currentTab == "Платежи";

            foreach (var item in ((ContextMenu)e.Source).Items)
            {
                if (item is MenuItem menuItem)
                {
                    if (menuItem.Header.ToString() == "Статистика" ||
                        menuItem.Header.ToString() == "Отчётность" ||
                        menuItem.Header.ToString() == "Графики")
                    {
                        menuItem.IsEnabled = isItemSelected && showAnalytics;
                    }
                    else if (menuItem.Header.ToString() == "Редактировать" ||
                             menuItem.Header.ToString() == "Удалить")
                    {
                        menuItem.IsEnabled = isItemSelected && !showAnalytics;
                    }
                }
            }
        }
        private void ChartsPayment_Click(object sender, RoutedEventArgs e)
        {
            var chartsWindow = new ChartsWindow(ProviderDB.Text);
            chartsWindow.Owner = this;
            chartsWindow.ShowDialog();
        }
        private void ChangeDatabaseProvider_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string provider = menuItem.Header.ToString();
                ProviderDB.SelectedItem = provider; 

                _context?.Dispose();

                _context = new DentalClinicContext(provider);

                LoadData();

                StatusText.Text = $"База данных изменена на: {provider}";
            }
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Редактирование для всех DataGrid
            DataGridPatients.RowEditEnding += DataGrid_RowEditEnding;
            DataGridDentists.RowEditEnding += DataGrid_RowEditEnding;
            DataGridAppointments.RowEditEnding += DataGrid_RowEditEnding;
            DataGridServices.RowEditEnding += DataGrid_RowEditEnding;
            DataGridServiceRecords.RowEditEnding += DataGrid_RowEditEnding;
            DataGridPayments.RowEditEnding += DataGrid_RowEditEnding;

            DataGridPatients.LostFocus += DataGrid_LostFocus;
            DataGridDentists.LostFocus += DataGrid_LostFocus;
            DataGridAppointments.LostFocus += DataGrid_LostFocus;
            DataGridServices.LostFocus += DataGrid_LostFocus;
            DataGridServiceRecords.LostFocus += DataGrid_LostFocus;
            DataGridPayments.LostFocus += DataGrid_LostFocus;
        }
        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            
        }
        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                CommitChanges();
            }
        }
        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                CommitChanges();
            }
        }
        private void DataGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            CommitChanges();
        }

        private bool _isCommitting = false; 
        private async void CommitChanges()
        {
            if (_isCommitting || _context == null)
                return;

            _isCommitting = true;

            try
            {
                // Фиксируем изменения для всех DataGrid (без проверки IsEditing)
                DataGridPatients.CommitEdit(DataGridEditingUnit.Row, true);
                DataGridDentists.CommitEdit(DataGridEditingUnit.Row, true);
                DataGridAppointments.CommitEdit(DataGridEditingUnit.Row, true);
                DataGridServices.CommitEdit(DataGridEditingUnit.Row, true);
                DataGridServiceRecords.CommitEdit(DataGridEditingUnit.Row, true);
                DataGridPayments.CommitEdit(DataGridEditingUnit.Row, true);

                // Проверяем изменения и сохраняем
                if (_context.ChangeTracker.HasChanges())
                {
                    int savedCount = await _context.SaveChangesAsync();
                    StatusText.Text = $"Сохранено изменений: {savedCount}";

                    // Обновляем только текущую вкладку
                    LoadData(false);
                }
            }
            catch (DbUpdateException dbEx)
            {
                string errorMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (errorMessage.Contains("UNIQUE constraint") || errorMessage.Contains("IX_"))
                {
                    MessageBox.Show("Ошибка: нарушение уникальности данных.\nВозможно, такая запись уже существует.",
                                  "Ошибка сохранения",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }
                else if (errorMessage.Contains("FOREIGN KEY constraint"))
                {
                    MessageBox.Show("Ошибка: нарушение ссылочной целостности.\nПроверьте связанные данные.",
                                  "Ошибка сохранения",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных: {errorMessage}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }

                LoadData(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                LoadData(false);
            }
            finally
            {
                _isCommitting = false;
            }
        }
    }
}