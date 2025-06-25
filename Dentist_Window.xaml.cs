using DentalClinicApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DentalClinicApp.Data;

namespace Кожетьева_WPF
{
    public partial class Dentist_Window : Window
    {
        private DentalClinicContext _context;
        public bool IsSaved { get; private set; } = false;

        public Dentist_Window(DentalClinicContext context)
        {
            InitializeComponent();
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            var items = _context.Dentists.ToList();
            dataGrid.ItemsSource = items;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var items = (List<Dentist>)dataGrid.ItemsSource;

            foreach (var dentist in items)
            {
                if (string.IsNullOrWhiteSpace(dentist.LastName) ||
                    string.IsNullOrWhiteSpace(dentist.FirstName))
                {
                    MessageBox.Show("Фамилия и имя стоматолога обязательны для заполнения!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(dentist.LicenseNumber))
                {
                    MessageBox.Show("Номер лицензии обязателен для заполнения!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            try
            {
                foreach (var item in items)
                {
                    _context.Entry(item).State = item.DentistID == 0 ? EntityState.Added : EntityState.Modified;
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

        public Dentist_Window()
        {
            InitializeComponent();
        }
    }
}