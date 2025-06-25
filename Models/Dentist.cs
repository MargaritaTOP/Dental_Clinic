using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace DentalClinicApp.Models
{
    public class Dentist : INotifyPropertyChanged
    {
        private int _dentistID;
        private string _firstName;
        private string _middleName;
        private string _lastName;
        private string _specialization;
        private string _licenseNumber;
        private DateTime _hireDate;
        private string _phone;
        private string _email;
        private ICollection<Appointment> _appointments;
        private ICollection<ServiceRecord> _serviceRecords;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Display(Name = "ID", Order = 0)]
        public int DentistID
        {
            get => _dentistID;
            set
            {
                if (_dentistID != value)
                {
                    _dentistID = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required(ErrorMessage = "Имя стоматолога обязательно")]
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

        [Required(ErrorMessage = "Фамилия стоматолога обязательна")]
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

        [StringLength(100, ErrorMessage = "Специализация не должна превышать 100 символов")]
        [Column("Specialization")]
        [Display(Name = "Специализация", Order = 4)]
        public string Specialization
        {
            get => _specialization;
            set
            {
                if (_specialization != value)
                {
                    _specialization = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required(ErrorMessage = "Номер лицензии обязателен")]
        [StringLength(50, ErrorMessage = "Номер лицензии не должен превышать 50 символов")]
        [Column("LicenseNumber")]
        [Display(Name = "Лицензия", Order = 5)]
        public string LicenseNumber
        {
            get => _licenseNumber;
            set
            {
                if (_licenseNumber != value)
                {
                    _licenseNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required(ErrorMessage = "Дата приема на работу обязательна")]
        [Column("HireDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата приема", Order = 6)]
        public DateTime HireDate
        {
            get => _hireDate;
            set
            {
                if (_hireDate != value)
                {
                    _hireDate = value;
                    OnPropertyChanged();
                }
            }
        }

        [StringLength(20, ErrorMessage = "Телефон не должен превышать 20 символов")]
        [Column("Phone")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [Display(Name = "Телефон", Order = 7)]
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
        [Display(Name = "Email", Order = 8)]
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

        [Display(Name = "Записи на прием", AutoGenerateField = false)]
        public virtual ICollection<Appointment> Appointments
        {
            get => _appointments ??= new List<Appointment>();
            set
            {
                if (_appointments != value)
                {
                    _appointments = value;
                    OnPropertyChanged();
                }
            }
        }

        [Display(Name = "Оказанные услуги", AutoGenerateField = false)]
        public virtual ICollection<ServiceRecord> ServiceRecords
        {
            get => _serviceRecords ??= new List<ServiceRecord>();
            set
            {
                if (_serviceRecords != value)
                {
                    _serviceRecords = value;
                    OnPropertyChanged();
                }
            }
        }

        [NotMapped]
        [Display(Name = "ФИО", Order = 9)]
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{FullName} ({Specialization})";
        }
    }
}