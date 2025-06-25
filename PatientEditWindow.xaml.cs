using DentalClinicApp.Data;
using DentalClinicApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DentalClinicApp
{
    public partial class PatientEditWindow : Window
    {
        private readonly DentalClinicContext _context;
        private readonly Patient? _patient;
        private bool _isNewPatient;
        public bool IsSaved { get; private set; } = false;
        public PatientEditWindow(DentalClinicContext context, int patientId)
        {
            InitializeComponent();
            _context = context;

            if (patientId > 0)
            {
                _patient = _context.Patients.Find(patientId);

                _patient.Gender = _patient.Gender switch
                {
                    "М" => "Мужской",
                    "Ж" => "Женский",
                    _ => "Мужской"
                };

                _isNewPatient = false;
            }
            else
            {
                // Для SQLite 
                _patient = new Patient
                {
                    Gender = "Мужской",
                    RegistrationDate = DateTime.Now,
                    BirthDate = DateTime.Now
                };
                _isNewPatient = true;
            }

            DataContext = _patient;
        }
        private bool ValidatePatient()
        {
            if (string.IsNullOrWhiteSpace(_patient.LastName))
            {
                MessageBox.Show("Фамилия пациента обязательна для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_patient.FirstName))
            {
                MessageBox.Show("Имя пациента обязательно для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_patient.BirthDate > DateTime.Now)
            {
                MessageBox.Show("Дата рождения не может быть в будущем", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //CheckPatientTableSchema();
                _patient.Gender = _patient.Gender switch
                {
                    "Мужской" => "М",
                    "Женский" => "Ж",
                    _ => "М"
                };
                // Добавляем валидацию 
                if (!ValidatePatient()) return;

                if (_isNewPatient)
                {
                    // Логирование перед добавлением
                    //Debug.WriteLine($"Добавление нового пациента. ID перед сохранением: {_patient.PatientID}");
                    _context.Patients.Add(_patient);
                }
                else
                {
                    _context.Entry(_patient).State = EntityState.Modified;
                }

                _context.SaveChanges();
                IsSaved = true;
                Close();
            }
            catch (DbUpdateException ex)
            {
                // Получаем полную цепочку исключений
                var errorMessage = new System.Text.StringBuilder();
                errorMessage.AppendLine("Ошибка при сохранении пациента:");

                Exception currentEx = ex;
                while (currentEx != null)
                {
                    errorMessage.AppendLine($"- {currentEx.GetType().Name}: {currentEx.Message}");
                    currentEx = currentEx.InnerException;
                }

                // Добавляем информацию о состоянии сущности
                errorMessage.AppendLine("\nСостояние сущности Patient:");
                errorMessage.AppendLine($"ID: {_patient.PatientID}");
                errorMessage.AppendLine($"ФИО: {_patient.LastName} {_patient.FirstName}");
                errorMessage.AppendLine($"Пол: {_patient.Gender}");

                // Для SQLite получаем дополнительные детали
                if (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx)
                {
                    errorMessage.AppendLine("\nДетали SQLite:");
                    errorMessage.AppendLine($"Код ошибки: {sqliteEx.SqliteErrorCode}");
                    errorMessage.AppendLine($"Расширенный код: {sqliteEx.SqliteExtendedErrorCode}");
                    errorMessage.AppendLine($"SQL: {sqliteEx.SqliteExtendedErrorCode}");
                }

                // Выводим полную информацию
                MessageBox.Show(errorMessage.ToString(), "Ошибка сохранения",
                               MessageBoxButton.OK, MessageBoxImage.Error);

                // Логируем в отладочный вывод
                Debug.WriteLine(errorMessage.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.ToString()}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Tag?.ToString() == "numeric")
                {
                    e.Handled = !char.IsDigit(e.Text, 0);
                }
            }
        }

        private void CheckPatientTableSchema()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "PRAGMA table_info(Patient);";
                    var reader = command.ExecuteReader();

                    var schemaInfo = new System.Text.StringBuilder();
                    schemaInfo.AppendLine("Структура таблицы Patient:");

                    while (reader.Read())
                    {
                        schemaInfo.AppendLine($"Колонка: {reader["name"]}, Тип: {reader["type"]}, NULL: {reader["notnull"]}, PK: {reader["pk"]}");
                    }

                    MessageBox.Show(schemaInfo.ToString(), "Схема таблицы Patient");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке схемы: {ex.Message}");
            }
            finally
            {
                _context.Database.CloseConnection(); // Важно закрыть соединение
            }
        }

    }
}