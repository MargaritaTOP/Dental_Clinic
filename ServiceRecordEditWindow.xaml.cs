using DentalClinicApp.Data;
using DentalClinicApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace DentalClinicApp
{
    public partial class ServiceRecordEditWindow : Window
    {
        private readonly DentalClinicContext _context;
        private readonly ServiceRecord? _serviceRecord;
        private bool _isNewRecord;

        public ServiceRecordEditWindow(DentalClinicContext context, int recordId)
        {
            InitializeComponent();
            _context = context;

            if (recordId > 0)
            {
                _serviceRecord = _context.ServiceRecords
                    .Include(sr => sr.Patient)
                    .Include(sr => sr.Dentist)
                    .Include(sr => sr.DentalService)
                    .FirstOrDefault(sr => sr.RecordID == recordId);
                _isNewRecord = false;
            }
            else
            {
                _serviceRecord = new ServiceRecord
                {
                    ServiceDate = DateTime.Now,
                    Status = "Completed"
                };
                _isNewRecord = true;
            }

            // Заполняем ComboBox'ы
            PatientComboBox.ItemsSource = _context.Patients.ToList();
            DentistComboBox.ItemsSource = _context.Dentists.ToList();
            ServiceComboBox.ItemsSource = _context.DentalServices.ToList();

            DataContext = _serviceRecord;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем обязательные поля
            if (_serviceRecord.PatientID == 0 || _serviceRecord.DentistID == 0 || _serviceRecord.ServiceID == 0)
            {
                MessageBox.Show("Необходимо заполнить все обязательные поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_serviceRecord.ActualPrice <= 0)
            {
                MessageBox.Show("Стоимость услуги должна быть больше нуля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_isNewRecord)
            {
                _context.ServiceRecords.Add(_serviceRecord);
            }
            else
            {
                _context.Entry(_serviceRecord).State = EntityState.Modified;
            }

            try
            {
                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}