using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace DentalClinicApp.Models
{
    public class DentalService : INotifyPropertyChanged
    {
        private int _serviceID;
        private string _serviceName;
        private string _description;
        private int _durationMinutes;
        private decimal _basePrice;
        private string _category;
        private ICollection<ServiceRecord> _serviceRecords;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Display(Name = "ID услуги", Order = 0)]
        public int ServiceID
        {
            get => _serviceID;
            set
            {
                if (_serviceID != value)
                {
                    _serviceID = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required(ErrorMessage = "Название услуги обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        [Column("ServiceName")]
        [Display(Name = "Название услуги", Order = 1)]
        public string ServiceName
        {
            get => _serviceName;
            set
            {
                if (_serviceName != value)
                {
                    _serviceName = value;
                    OnPropertyChanged();
                }
            }
        }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        [Column("Description")]
        [Display(Name = "Описание", Order = 2)]
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required(ErrorMessage = "Длительность услуги обязательна")]
        [Column("DurationMinutes")]
        [Range(1, 1000, ErrorMessage = "Длительность должна быть от 1 до 1000 минут")]
        [Display(Name = "Длительность (мин.)", Order = 3)]
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

        [Required(ErrorMessage = "Базовая цена обязательна")]
        [Column("BasePrice", TypeName = "decimal(18,2)")]
        [Range(0.01, 1000000, ErrorMessage = "Цена должна быть от 0.01 до 1 000 000")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Display(Name = "Базовая цена", Order = 4)]
        public decimal BasePrice
        {
            get => _basePrice;
            set
            {
                if (_basePrice != value)
                {
                    _basePrice = value;
                    OnPropertyChanged();
                }
            }
        }

        [StringLength(50, ErrorMessage = "Категория не должна превышать 50 символов")]
        [Column("Category")]
        [Display(Name = "Категория", Order = 5)]
        public string Category
        {
            get => _category;
            set
            {
                if (_category != value)
                {
                    _category = value;
                    OnPropertyChanged();
                }
            }
        }

        [Display(Name = "Записи об услугах", AutoGenerateField = false)]
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{ServiceName} ({BasePrice:N2}, {DurationMinutes} мин.)";
        }
    }
}