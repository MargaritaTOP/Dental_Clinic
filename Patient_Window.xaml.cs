using DentalClinicApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using DentalClinicApp.Data;
using System.Collections.Generic;


namespace Кожетьева_WPF
{
    /// <summary>
    /// Логика взаимодействия для Patient_Window.xaml
    /// </summary>
    public partial class Patient_Window : Window
    {
        DentalClinicContext? _context;
        public bool IsSaved { get; private set; } = false;

        public Patient_Window(DentalClinicContext context)
        {
            InitializeComponent();
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            var items = _context.Patients.ToList();
            dataGrid.ItemsSource = items;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var items = (List<Patient>)dataGrid.ItemsSource;

            foreach (var patient in items)
            {
                if (string.IsNullOrWhiteSpace(patient.LastName) ||
                    string.IsNullOrWhiteSpace(patient.FirstName))
                {
                    MessageBox.Show("Фамилия и имя пациента обязательны для заполнения!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            try
            {
                // Обновляем контекст
                foreach (var item in items)
                {
                    _context.Entry(item).State = item.PatientID == 0 ? EntityState.Added : EntityState.Modified;
                }

                _context.SaveChanges();
                IsSaved = true;
                MessageBox.Show("Изменения сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public Patient_Window()
        {
            InitializeComponent();
        }
    }
}