using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace DentalClinicApp.Models
{
    public class Payment : INotifyPropertyChanged
    {
        private int _paymentID;
        private int _recordID;
        private DateTime _paymentDate;
        private decimal _amount;
        private string _paymentMethod;
        private string _status;
        private ServiceRecord _serviceRecord;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        [Display(Name = "ID платежа", Order = 0)]
        public int PaymentID
        {
            get => _paymentID;
            set
            {
                if (_paymentID != value)
                {
                    _paymentID = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required(ErrorMessage = "Ссылка на запись услуги обязательна")]
        [Column("RecordID")]
        [ForeignKey("ServiceRecord")]
        [Display(Name = "ID записи", Order = 1)]
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

        [Required(ErrorMessage = "Дата платежа обязательна")]
        [Column("PaymentDate")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата платежа", Order = 2)]
        public DateTime PaymentDate
        {
            get => _paymentDate;
            set
            {
                if (_paymentDate != value)
                {
                    _paymentDate = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required(ErrorMessage = "Сумма платежа обязательна")]
        [Column("Amount", TypeName = "decimal(18,2)")]
        [Range(0.01, 1000000, ErrorMessage = "Сумма должна быть от 0.01 до 1 000 000")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Display(Name = "Сумма", Order = 3)]
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required(ErrorMessage = "Способ оплаты обязателен")]
        [StringLength(50, ErrorMessage = "Способ оплаты не должен превышать 50 символов")]
        [Column("PaymentMethod")]
        [Display(Name = "Способ оплаты", Order = 4)]
        public string PaymentMethod
        {
            get => _paymentMethod;
            set
            {
                if (_paymentMethod != value)
                {
                    _paymentMethod = value;
                    OnPropertyChanged();
                }
            }
        }

        [Required]
        [StringLength(20)]
        [Column("Status")]
        [Display(Name = "Статус", Order = 5)]
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

        [Display(Name = "Запись услуги", Order = 6)]
        public virtual ServiceRecord ServiceRecord
        {
            get => _serviceRecord;
            set
            {
                if (_serviceRecord != value)
                {
                    _serviceRecord = value;
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
            return $"{PaymentDate:dd.MM.yyyy} - {Amount:N2} ({PaymentMethod}, статус: {Status})";
        }

        [NotMapped]
        [Display(Name = "Валиден")]
        public bool IsValid => ValidatePayment();

        public bool ValidatePayment()
        {
            return Amount > 0 && !string.IsNullOrEmpty(PaymentMethod);
        }
    }
}