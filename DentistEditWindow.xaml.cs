using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DentalClinicApp.Models;
using DentalClinicApp.Data;

namespace DentalClinicApp
{
    public partial class DentistEditWindow : Window
    {
        private readonly DentalClinicContext _context;
        private readonly Dentist? _dentist;
        private bool _isNewDentist;

        public bool IsSaved { get; private set; } = false;

        public DentistEditWindow(DentalClinicContext context, int dentistId)
        {
            InitializeComponent();
            _context = context;

            if (dentistId > 0)
            {
                // для редактир-я стоматолога
                _dentist = _context.Dentists
                    .Include(d => d.ServiceRecords)
                    .FirstOrDefault(d => d.DentistID == dentistId);
                _isNewDentist = false;
                Title = $"Редактирование стоматолога (ID: {dentistId})";
            }
            else
            {
                // для созд-я нового стоматолога
                _dentist = new Dentist
                {
                    HireDate = DateTime.Now
                };
                _isNewDentist = true;
                Title = "Добавление нового стоматолога";
            }

            DataContext = _dentist;
            LoadSpecializations();
        }

        private void LoadSpecializations()
        {
            // загрузка списока специализаций из базы 
            SpecializationComboBox.Items.Add("Терапевтическая стоматология");
            SpecializationComboBox.Items.Add("Ортодонтия");
            SpecializationComboBox.Items.Add("Хирургическая стоматология");
            SpecializationComboBox.Items.Add("Детская стоматология");
            SpecializationComboBox.Items.Add("Ортопедическая стоматология");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_dentist.LastName) ||
                string.IsNullOrWhiteSpace(_dentist.FirstName))
            {
                MessageBox.Show("Фамилия и имя стоматолога обязательны для заполнения!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(_dentist.LicenseNumber))
            {
                MessageBox.Show("Номер лицензии обязателен для заполнения!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (_isNewDentist)
                {
                    _context.Dentists.Add(_dentist);
                }
                else
                {
                    _context.Entry(_dentist).State = EntityState.Modified;
                }

                _context.SaveChanges();
                IsSaved = true;
                Close();
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

        private void LicenseNumberTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && !char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '-')
            {
                e.Handled = true;
            }
        }
    }
}