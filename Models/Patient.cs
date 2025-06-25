using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace DentalClinicApp.Models
{
    public class Patient : INotifyPropertyChanged
    {
        private int _patientID;
        private string _firstName;
        private string _middleName;
        private string _lastName;
        private DateTime _birthDate;
        private string _gender;
        private string _phone;
        private string _email;
        private string _address;
        private DateTime _registrationDate;
        private string _medicalHistory;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int PatientID
        {
            get => _patientID;
            set
            {
                if (_patientID != value)
                {
                    _patientID = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required(ErrorMessage = "Имя пациента обязательно")]
        [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
        [Column("FirstName")]
        [Display(Name = "Имя", Order = 2)]
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        [StringLength(50, ErrorMessage = "Отчество не должно превышать 50 символов")]
        [Column("MiddleName")]
        [Display(Name = "Отчество", Order = 3)]
        public string MiddleName
        {
            get => _middleName;
            set
            {
                if (_middleName != value)
                {
                    _middleName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        [Required(ErrorMessage = "Фамилия пациента обязательна")]
        [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
        [Column("LastName")]
        [Display(Name = "Фамилия", Order = 1)]
        public string LastName
        {
            get => _lastName;
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }



        [Required(ErrorMessage = "Дата рождения обязательна")]
        [Column("BirthDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата рождения", Order = 4)]
        public DateTime BirthDate
        {
            get => _birthDate;
            set
            {
                if (_birthDate != value)
                {
                    _birthDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Age));
                }
            }
        }

        [Required(ErrorMessage = "Укажите пол пациента")]
        [StringLength(10)]
        [Column("Gender")]
        [Display(Name = "Пол", Order = 5)]
        public string Gender
        {
            get => _gender;
            set
            {
                if (_gender != value)
                {
                    _gender = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required(ErrorMessage = "Телефон обязателен")]
        [StringLength(20, ErrorMessage = "Телефон не должен превышать 20 символов")]
        [Column("Phone")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [Display(Name = "Телефон", Order = 6)]
        public string Phone
        {
            get => _phone;
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    OnPropertyChanged();
                }
            }
        }

        [StringLength(100, ErrorMessage = "Email не должен превышать 100 символов")]
        [Column("Email")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [Display(Name = "Email", Order = 7)]
        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        [StringLength(200, ErrorMessage = "Адрес не должен превышать 200 символов")]
        [Column("Address")]
        [Display(Name = "Адрес", Order = 8)]
        public string Address
        {
            get => _address;
            set
            {
                if (_address != value)
                {
                    _address = value;
                    OnPropertyChanged();
                }
            }
        }

        [Column("RegistrationDate")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата регистрации", Order = 9)]
        public DateTime RegistrationDate
        {
            get => _registrationDate;
            set
            {
                if (_registrationDate != value)
                {
                    _registrationDate = value;
                    OnPropertyChanged();
                }
            }
        }

        [Column("MedicalHistory", TypeName = "nvarchar(max)")]
        [Display(Name = "Медицинская карта", Order = 10)]
        public string MedicalHistory
        {
            get => _medicalHistory;
            set
            {
                if (_medicalHistory != value)
                {
                    _medicalHistory = value;
                    OnPropertyChanged();
                }
            }
        }

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();

        [NotMapped]
        [Display(Name = "ФИО", Order = 0)]
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        [NotMapped]
        [Display(Name = "Возраст")]
        public int Age => DateTime.Now.Year - BirthDate.Year -
                         (DateTime.Now.DayOfYear < BirthDate.DayOfYear ? 1 : 0);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{FullName} ({Age} лет, тел.: {Phone})";
        }
    }
}