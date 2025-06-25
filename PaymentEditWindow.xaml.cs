using DentalClinicApp.Data;
using DentalClinicApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace DentalClinicApp
{
    public partial class PaymentEditWindow : Window
    {
        private readonly DentalClinicContext _context;
        private readonly Payment? _payment;
        private bool _isNewPayment;

        public PaymentEditWindow(DentalClinicContext context, int paymentId)
        {
            InitializeComponent();
            _context = context;

            if (paymentId > 0)
            {
                _payment = _context.Payments
                    .Include(p => p.ServiceRecord)
                    .ThenInclude(sr => sr.DentalService)
                    .FirstOrDefault(p => p.PaymentID == paymentId);
                _isNewPayment = false;
            }
            else
            {
                _payment = new Payment
                {
                    PaymentDate = DateTime.Now,
                    Status = "Pending"
                };
                _isNewPayment = true;
            }

            // Заполняем ComboBox'ы
            ServiceRecordComboBox.ItemsSource = _context.ServiceRecords
                .Include(sr => sr.DentalService) 
                .ToList();
            PaymentMethodComboBox.ItemsSource = new[] { "Наличные", "Карта", "Перевод" };

            DataContext = _payment;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем обязательные поля
            if (_payment.RecordID == 0)
            {
                MessageBox.Show("Необходимо выбрать запись об услуге!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_payment.Amount <= 0)
            {
                MessageBox.Show("Сумма платежа должна быть больше нуля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(_payment.PaymentMethod))
            {
                MessageBox.Show("Необходимо указать способ оплаты!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_isNewPayment)
            {
                _context.Payments.Add(_payment);
            }
            else
            {
                _context.Entry(_payment).State = EntityState.Modified;
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