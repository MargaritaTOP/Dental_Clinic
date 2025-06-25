using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using DentalClinicApp.Data;
using DentalClinicApp.Models;

namespace DentalClinicApp
{
    public partial class AppointmentEditWindow : Window
    {
        private readonly DentalClinicContext _context;
        private readonly Appointment? _appointment;
        private bool _isNewAppointment;

        public bool IsSaved { get; private set; } = false;

        public AppointmentEditWindow(DentalClinicContext context, int appointmentId)
        {
            InitializeComponent();
            _context = context;
            //this.MouseDown += Window_MouseDown;


            if (appointmentId > 0)
            {
                // Редактирования существующей записи
                _appointment = _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Dentist)
                    .FirstOrDefault(a => a.AppointmentID == appointmentId);
                _isNewAppointment = false;
                Title = $"Редактирование записи (ID: {appointmentId})";
            }
            else
            {
                // Создание новой записи
                _appointment = new Appointment
                {
                    ScheduledDate = DateTime.Now.AddHours(1),
                    DurationMinutes = 30,
                    Status = "Запланирован"
                };
                _isNewAppointment = true;
                Title = "Новая запись на прием";
            }

            DataContext = _appointment;
            LoadComboBoxData();
        }
        private void LoadComboBoxData()
        {
            // Загрузка пациентов и стоматологов
            PatientComboBox.ItemsSource = _context.Patients.OrderBy(p => p.LastName).ToList();
            DentistComboBox.ItemsSource = _context.Dentists.OrderBy(d => d.LastName).ToList();

            // Установка выбранных значений
            if (!_isNewAppointment)
            {
                PatientComboBox.SelectedValue = _appointment.PatientID;
                DentistComboBox.SelectedValue = _appointment.DentistID;
                StatusComboBox.SelectedItem = _appointment.Status;
            }
            else
            {
                StatusComboBox.SelectedItem = "Запланирован";
            }

            // Загрузка статусов
            StatusComboBox.Items.Add("Запланирован");
            StatusComboBox.Items.Add("Подтвержден");
            StatusComboBox.Items.Add("Завершен");
            StatusComboBox.Items.Add("Отменен");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_appointment.PatientID == 0)
            {
                MessageBox.Show("Выберите пациента!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_appointment.DentistID == 0)
            {
                MessageBox.Show("Выберите стоматолога!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(_appointment.Purpose))
            {
                MessageBox.Show("Укажите цель визита!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_appointment.ScheduledDate < DateTime.Now)
            {
                MessageBox.Show("Дата приема не может быть в прошлом!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (_isNewAppointment)
                {
                    _context.Appointments.Add(_appointment);
                }
                else
                {
                    _context.Entry(_appointment).State = EntityState.Modified;
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

        private void DurationTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }
    }
}