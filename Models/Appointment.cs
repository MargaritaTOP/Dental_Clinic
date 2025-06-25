using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace DentalClinicApp.Models
{
    public class Appointment : INotifyPropertyChanged
    {
        private int _appointmentID;
        private int _patientID;
        private int _dentistID;
        private DateTime _scheduledDate;
        private int _durationMinutes;
        private string _purpose;
        private string _status;
        private Patient _patient;
        private Dentist _dentist;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Display(Name = "ID записи", Order = 0)]
        public int AppointmentID
        {
            get => _appointmentID;
            set
            {
                if (_appointmentID != value)
                {
                    _appointmentID = value;
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

        [Required(ErrorMessage = "Дата и время приема обязательны")]
        [Column("ScheduledDate")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата и время", Order = 3)]
        public DateTime ScheduledDate
        {
            get => _scheduledDate;
            set
            {
                if (_scheduledDate != value)
                {
                    _scheduledDate = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required(ErrorMessage = "Длительность приема обязательна")]
        [Column("DurationMinutes")]
        [Range(1, 1000, ErrorMessage = "Длительность должна быть от 1 до 1000 минут")]
        [Display(Name = "Длительность (мин.)", Order = 4)]
        public int DurationMinutes
        {
            get => _durationMinutes;
            set
            {
                if (_durationMinutes != value)
                {
                    _durationMinutes = value;
                    OnPropertyChanged();
                }
            }
        }

        [Column("Purpose", TypeName = "nvarchar(500)")]
        [StringLength(500, ErrorMessage = "Цель визита не должна превышать 500 символов")]
        [Display(Name = "Цель визита", Order = 5)]
        public string Purpose
        {
            get => _purpose;
            set
            {
                if (_purpose != value)
                {
                    _purpose = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required]
        [Column("Status")]
        [StringLength(20)]
        [Display(Name = "Статус", Order = 6)]
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

        [Display(Name = "Пациент", Order = 7)]
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

        [Display(Name = "Стоматолог", Order = 8)]
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

        [NotMapped]
        [Display(Name = "Информация о пациенте")]
        public string PatientInfo => Patient?.FullName;

        [NotMapped]
        [Display(Name = "Информация о стоматологе")]
        public string DentistInfo => Dentist?.FullName;

        [NotMapped]
        [Display(Name = "Дата окончания")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        public DateTime EndDate => ScheduledDate.AddMinutes(DurationMinutes);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{ScheduledDate:dd.MM.yyyy HH:mm} - {PatientInfo} с {DentistInfo} ({Status})";
        }

        public bool IsValid()
        {
            return PatientID > 0 &&
                   DentistID > 0 &&
                   ScheduledDate > DateTime.MinValue &&
                   DurationMinutes > 0 &&
                   !string.IsNullOrWhiteSpace(Status);
        }
    }
}