using DentalClinicApp.Data;
using DentalClinicApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DentalClinicApp
{
    public partial class ServiceEditWindow : Window
    {
        private readonly DentalClinicContext _context;
        private readonly DentalService? _service;
        private bool _isNewService;
        public bool IsSaved { get; private set; } = false;
        public ServiceEditWindow(DentalClinicContext context, int serviceId)
        {
            InitializeComponent();
            _context = context;

            if (serviceId > 0)
            {
                _service = _context.DentalServices.Find(serviceId);
                _isNewService = false;
                Title = $"Редактирование услуги: {_service.ServiceName}";
            }
            else
            {
                // создания новой услуги
                _service = new DentalService
                {
                    DurationMinutes = 30,
                    BasePrice = 1000,
                    Category = "Общее"
                };
                _isNewService = true;
                Title = "Добавление новой услуги";
            }
            DataContext = _service;
            LoadCategories();
        }

        private void LoadCategories()
        {
            //  категории услуг
            CategoryComboBox.Items.Add("Общее");
            CategoryComboBox.Items.Add("Терапия");
            CategoryComboBox.Items.Add("Хирургия");
            CategoryComboBox.Items.Add("Ортодонтия");
            CategoryComboBox.Items.Add("Гигиена");
            CategoryComboBox.Items.Add("Протезирование");
            CategoryComboBox.Items.Add("Детская стоматология");
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_service.ServiceName))
            {
                MessageBox.Show("Название услуги обязательно для заполнения!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (_service.DurationMinutes <= 0)
            {
                MessageBox.Show("Длительность должна быть положительным числом!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (_service.BasePrice <= 0)
            {
                MessageBox.Show("Цена должна быть положительным числом!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                if (_isNewService)
                {
                    _context.DentalServices.Add(_service);
                }
                else
                {
                    _context.Entry(_service).State = EntityState.Modified;
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

        private void NumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1) && e.Text != ".")
            {
                e.Handled = true;
            }
        }
    }
}