using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace DentalClinicApp.Models
{
    public class ServiceRecord : INotifyPropertyChanged
    {
        private int _recordID;
        private int _patientID;
        private int _dentistID;
        private int _serviceID;
        private DateTime _serviceDate;
        private decimal _actualPrice;
        private string _notes;
        private string _status;
        private Patient _patient;
        private Dentist _dentist;
        private DentalService _dentalService;
        private ICollection<Payment> _payments;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Display(Name = "ID записи", Order = 0)]
        public int RecordID
        {
            get => _recordID;
            set
            {
                if (_recordID != value)
                {
                    _recordID = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required(ErrorMessage = "Пациент обязателен")]
        [Column("PatientID")]
        [ForeignKey("Patient")]
        [Display(Name = "ID пациента", Order = 1)]
        public int PatientID
        {
            get => _patientID;
            set
            {
                if (_patientID != value)
                {
                    _patientID = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PatientInfo));
                }
            }
        }

        [Required(ErrorMessage = "Стоматолог обязателен")]
        [Column("DentistID")]
        [ForeignKey("Dentist")]
        [Display(Name = "ID стоматолога", Order = 2)]
        public int DentistID
        {
            get => _dentistID;
            set
            {
                if (_dentistID != value)
                {
                    _dentistID = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DentistInfo));
                }
            }
        }

        [Required(ErrorMessage = "Услуга обязательна")]
        [Column("ServiceID")]
        [ForeignKey("DentalService")]
        [Display(Name = "ID услуги", Order = 3)]
        public int ServiceID
        {
            get => _serviceID;
            set
            {
                if (_serviceID != value)
                {
                    _serviceID = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ServiceInfo));
                    OnPropertyChanged(nameof(IsPaid));
                    OnPropertyChanged(nameof(RemainingAmount));
                }
            }
        }

        [Required(ErrorMessage = "Дата оказания услуги обязательна")]
        [Column("ServiceDate")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата услуги", Order = 4)]
        public DateTime ServiceDate
        {
            get => _serviceDate;
            set
            {
                if (_serviceDate != value)
                {
                    _serviceDate = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required(ErrorMessage = "Стоимость обязательна")]
        [Column("ActualPrice", TypeName = "decimal(18,2)")]
        [Range(0.01, 1000000, ErrorMessage = "Стоимость должна быть от 0.01 до 1 000 000")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Display(Name = "Стоимость", Order = 5)]
        public decimal ActualPrice
        {
            get => _actualPrice;
            set
            {
                if (_actualPrice != value)
                {
                    _actualPrice = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsPaid));
                    OnPropertyChanged(nameof(RemainingAmount));
                }
            }
        }

        [Column("Notes", TypeName = "nvarchar(max)")]
        [Display(Name = "Примечания", Order = 6)]
        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required]
        [Column("Status")]
        [StringLength(20)]
        [Display(Name = "Статус", Order = 7)]
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        [Display(Name = "Пациент", Order = 8)]
        public virtual Patient Patient
        {
            get => _patient;
            set
            {
                if (_patient != value)
                {
                    _patient = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PatientInfo));
                }
            }
        }

        [Display(Name = "Стоматолог", Order = 9)]
        public virtual Dentist Dentist
        {
            get => _dentist;
            set
            {
                if (_dentist != value)
                {
                    _dentist = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DentistInfo));
                }
            }
        }

        [Display(Name = "Услуга", Order = 10)]
        public virtual DentalService DentalService
        {
            get => _dentalService;
            set
            {
                if (_dentalService != value)
                {
                    _dentalService = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ServiceInfo));
                }
            }
        }

        [Display(Name = "Платежи", Order = 11)]
        public virtual ICollection<Payment> Payments
        {
            get => _payments ??= new List<Payment>();
            set
            {
                if (_payments != value)
                {
                    _payments = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsPaid));
                    OnPropertyChanged(nameof(RemainingAmount));
                }
            }
        }

        [NotMapped]
        [Display(Name = "Оплачено")]
        public bool IsPaid
        {
            get => Payments?.Sum(p => p.Amount) >= ActualPrice;
        }

        [NotMapped]
        [Display(Name = "Остаток", AutoGenerateField = false)]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal RemainingAmount
        {
            get => ActualPrice - (Payments?.Sum(p => p.Amount) ?? 0);
        }

        [NotMapped]
        [Display(Name = "Информация об услуге", AutoGenerateField = false)]
        public string ServiceInfo
        {
            get => $"{DentalService?.ServiceName}";
        }

        [NotMapped]
        [Display(Name = "Информация о пациенте", AutoGenerateField = false)]
        public string PatientInfo
        {
            get => $"{Patient?.FullName}";
        }

        [NotMapped]
        [Display(Name = "Информация о стоматологе", AutoGenerateField = false)]
        public string DentistInfo
        {
            get => $"{Dentist?.FullName}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{ServiceDate:dd.MM.yyyy HH:mm} - {DentalService?.ServiceName} ({ActualPrice:N2})";
        }

        public void AddPayment(Payment payment)
        {
            payment.RecordID = RecordID;
            Payments.Add(payment);
            OnPropertyChanged(nameof(Payments));
            OnPropertyChanged(nameof(IsPaid));
            OnPropertyChanged(nameof(RemainingAmount));
        }
    }
}