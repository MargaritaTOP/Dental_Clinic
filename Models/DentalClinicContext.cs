using DentalClinicApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Diagnostics;

namespace DentalClinicApp.Data
{
    public class DentalClinicContext : DbContext
    {
        private readonly string _provider;

        public DentalClinicContext(string provider)
        {
            _provider = provider;
        }

        public DentalClinicContext() : this("SQLite") 
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Dentist> Dentists { get; set; }
        public DbSet<DentalService> DentalServices { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ServiceRecord> ServiceRecords { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = ConfigurationManager.ConnectionStrings[$"DentalClinic_{_provider}"]?.ConnectionString;

                switch (_provider.ToLower())
                {
                    case "sqlite":
                        optionsBuilder.UseSqlite(connectionString);
                        break;
                    case "sqlserver":
                        optionsBuilder.UseSqlServer(connectionString);
                        break;
                    default:
                        optionsBuilder.UseSqlite(connectionString);
                        break;
                }

                // Оптимизация для редактирования в ячейках
                optionsBuilder
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .LogTo(message => Debug.WriteLine(message), LogLevel.Information)  
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация Patient
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Patient");
                entity.HasKey(p => p.PatientID).HasName("PK_Patient");

                //  SQLite
                if (_provider.ToLower() == "sqlite")
                {
                    entity.Property(p => p.PatientID)
                        .HasColumnName("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .IsRequired();
                }
                else
                {
                    entity.Property(p => p.PatientID)
                        .HasColumnName("Id")
                        .ValueGeneratedOnAdd();
                }

                entity.Property(p => p.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.MiddleName)
                    .HasMaxLength(100);

                entity.Property(p => p.BirthDate)
                    .IsRequired();

                entity.Property(p => p.Gender)
                    .IsRequired()
                    .HasMaxLength(1);

                entity.Property(p => p.Phone)
                    .HasMaxLength(20);

                entity.Property(p => p.Email)
                    .HasMaxLength(100);

                entity.Property(p => p.Address)
                    .HasMaxLength(200);

                entity.Property(p => p.RegistrationDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(p => p.MedicalHistory)
                    .HasColumnType("nvarchar(max)");

                entity.HasMany(p => p.Appointments)
                    .WithOne(a => a.Patient)
                    .HasForeignKey(a => a.PatientID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.ServiceRecords)
                    .WithOne(sr => sr.Patient)
                    .HasForeignKey(sr => sr.PatientID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Конфигурация Dentist
            modelBuilder.Entity<Dentist>(entity =>
            {
                entity.ToTable("Dentist");
                entity.HasKey(d => d.DentistID).HasName("PK_Dentist");
                entity.Property(d => d.DentistID).HasColumnName("Id");

                entity.Property(d => d.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(d => d.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(d => d.MiddleName)
                    .HasMaxLength(100);

                entity.Property(d => d.Specialization)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(d => d.LicenseNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(d => d.HireDate)
                    .IsRequired();

                entity.Property(d => d.Phone)
                    .HasMaxLength(20);

                entity.Property(d => d.Email)
                    .HasMaxLength(100);

                entity.HasMany(d => d.Appointments)
                    .WithOne(a => a.Dentist)
                    .HasForeignKey(a => a.DentistID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(d => d.ServiceRecords)
                    .WithOne(sr => sr.Dentist)
                    .HasForeignKey(sr => sr.DentistID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Конфигурация DentalService
            modelBuilder.Entity<DentalService>(entity =>
            {
                entity.ToTable("DentalService");
                entity.HasKey(ds => ds.ServiceID).HasName("PK_DentalService");
                entity.Property(ds => ds.ServiceID).HasColumnName("Id");

                entity.Property(ds => ds.ServiceName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(ds => ds.Description)
                    .HasColumnType("nvarchar(max)");

                entity.Property(ds => ds.DurationMinutes)
                    .IsRequired();

                entity.Property(ds => ds.BasePrice)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(ds => ds.Category)
                    .HasMaxLength(50);

                entity.HasMany(ds => ds.ServiceRecords)
                    .WithOne(sr => sr.DentalService)
                    .HasForeignKey(sr => sr.ServiceID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Конфигурация Appointment
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("Appointment");
                entity.HasKey(a => a.AppointmentID).HasName("PK_Appointment");
                entity.Property(a => a.AppointmentID).HasColumnName("Id");

                entity.Property(a => a.PatientID)
                    .IsRequired();

                entity.Property(a => a.DentistID)
                    .IsRequired();

                entity.Property(a => a.ScheduledDate)
                    .IsRequired();

                entity.Property(a => a.DurationMinutes)
                    .IsRequired();

                entity.Property(a => a.Purpose)
                    .HasMaxLength(200);

                entity.Property(a => a.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Scheduled");

                entity.HasOne(a => a.Patient)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(a => a.PatientID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Dentist)
                    .WithMany(d => d.Appointments)
                    .HasForeignKey(a => a.DentistID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Конфигурация ServiceRecord
            modelBuilder.Entity<ServiceRecord>(entity =>
            {
                entity.ToTable("ServiceRecord");
                entity.HasKey(sr => sr.RecordID).HasName("PK_ServiceRecord");
                entity.Property(sr => sr.RecordID).HasColumnName("Id");

                entity.Property(sr => sr.PatientID)
                    .IsRequired();

                entity.Property(sr => sr.DentistID)
                    .IsRequired();

                entity.Property(sr => sr.ServiceID)
                    .IsRequired();

                entity.Property(sr => sr.ServiceDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(sr => sr.ActualPrice)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(sr => sr.Notes)
                    .HasColumnType("nvarchar(max)");

                entity.Property(sr => sr.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Completed");

                entity.HasOne(sr => sr.Patient)
                    .WithMany(p => p.ServiceRecords)
                    .HasForeignKey(sr => sr.PatientID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.Dentist)
                    .WithMany(d => d.ServiceRecords)
                    .HasForeignKey(sr => sr.DentistID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.DentalService)
                    .WithMany(ds => ds.ServiceRecords)
                    .HasForeignKey(sr => sr.ServiceID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(sr => sr.Payments)
                    .WithOne(p => p.ServiceRecord)
                    .HasForeignKey(p => p.RecordID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payment");
                entity.HasKey(p => p.PaymentID).HasName("PK_Payment");
                entity.Property(p => p.PaymentID).HasColumnName("Id");

                entity.Property(p => p.RecordID)
                    .IsRequired();

                entity.Property(p => p.PaymentDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(p => p.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.PaymentMethod)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(p => p.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Pending");

                entity.HasOne(p => p.ServiceRecord)
                    .WithMany(sr => sr.Payments)
                    .HasForeignKey(p => p.RecordID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Добавляем начальные данные
            SeedInitialData(modelBuilder);
        }
        private void SeedInitialData(ModelBuilder modelBuilder)
        {
            // Добавление начальных данных для тестирования
            modelBuilder.Entity<DentalService>().HasData(
                new DentalService
                {
                    ServiceID = 1,
                    ServiceName = "Консультация",
                    Description = "Первичная консультация стоматолога",
                    DurationMinutes = 30,
                    BasePrice = 1500,
                    Category = "Диагностика"
                },
                new DentalService
                {
                    ServiceID = 2,
                    ServiceName = "Лечение кариеса",
                    Description = "Пломбирование одного кариозного зуба",
                    DurationMinutes = 60,
                    BasePrice = 4500,
                    Category = "Терапия"
                }
            );

            modelBuilder.Entity<Dentist>().HasData(
                new Dentist
                {
                    DentistID = 1,
                    FirstName = "Иван",
                    MiddleName = "Иванович",
                    LastName = "Петров",
                    Email = "ivan.petrov@clinic.com",
                    Phone = "+71234567890",
                    Specialization = "Терапевт",
                    LicenseNumber = "ST-12345",
                    HireDate = new DateTime(2020, 1, 15)
                }
            );
        }

        public async Task<int> SaveChangesWithValidationAsync()
        {
            try
            {
                return await SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new ApplicationException("Ошибка сохранения данных", ex);
            }
        }
    }
}