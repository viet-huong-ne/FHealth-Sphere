using Microsoft.EntityFrameworkCore;
//using Contract.Repositories.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Contract.Repositories.Entity;

namespace Repositories.Base
{
    public class FHealthSphereDBContext : IdentityDbContext<Account, ApplicationRole, int, ApplicationUserClaims, ApplicationUserRoles, ApplicationUserLogins, ApplicationRoleClaims, ApplicationUserTokens>
    {
        public FHealthSphereDBContext() { }
        public FHealthSphereDBContext(DbContextOptions<FHealthSphereDBContext> options) : base(options) { }
        public virtual DbSet<Account> Users => Set<Account>();
        public virtual DbSet<ApplicationRole> Roles => Set<ApplicationRole>();
        public virtual DbSet<ApplicationUserClaims> UserClaims => Set<ApplicationUserClaims>();
        public virtual DbSet<ApplicationUserRoles> UserRoles => Set<ApplicationUserRoles>();
        public virtual DbSet<ApplicationUserLogins> UserLogins => Set<ApplicationUserLogins>();
        public virtual DbSet<ApplicationRoleClaims> RoleClaims => Set<ApplicationRoleClaims>();
        public virtual DbSet<ApplicationUserTokens> UserTokens => Set<ApplicationUserTokens>();

        public DbSet<NotificationWatcher> NotificationWatchers { get; set; }
        public DbSet<PatientInformation> PatientInformations { get; set; }
        public DbSet<Watcher> Watchers { get; set; }
        public DbSet<Band> Bands { get; set; }
        public DbSet<BandBrand> BandBrands { get; set; }
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<HealthRecord> HealthRecords { get; set; }
        public DbSet<RecordMetricItem> RecordMetricItems { get; set; }
        public DbSet<MetricGroup> MetricGroup { get; set; }
        public DbSet<NotificationSystem> NotificationSystems { get; set; }
        public DbSet<BloodPressureClassification> BloodPressureClassifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableAnnotation = entityType.GetAnnotation("Relational:TableName");
                string tableName = tableAnnotation?.Value?.ToString() ?? "";
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }
            modelBuilder.Entity<BloodPressureClassification>().Property(a => a.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Watcher>().Property(w => w.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<PatientInformation>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<NotificationSystem>().Property(n => n.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<NotificationWatcher>().Property(nw => nw.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Metric>().Property(m => m.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<MetricGroup>().Property(mg => mg.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<HealthRecord>().Property(hr => hr.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Band>().Property(b => b.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<BandBrand>().Property(bb => bb.Id).ValueGeneratedOnAdd();
            //quan hệ cho HealthRecord
            modelBuilder.Entity<HealthRecord>()
                .HasOne(hr => hr.Patient)
                .WithMany(a => a.HealthRecords)
                .HasForeignKey(hr => hr.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Thiết lập quan hệ cho PatientInformation
            modelBuilder.Entity<PatientInformation>()
                .HasOne(p => p.Account)
                .WithMany(a => a.PatientInformation)
                .HasForeignKey(p => p.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Thiết lập quan hệ cho Notification
            modelBuilder.Entity<NotificationSystem>()
                .HasOne(n => n.Accounts)
                .WithMany(a => a.NotificationSystems)
                .HasForeignKey(n => n.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Thiết lập quan hệ cho NotificationWatcher
            //modelBuilder.Entity<NotificationWatcher>()
            //    .HasOne(nw => nw.Watcher)
            //    .WithMany()
            //    .HasForeignKey(nw => nw.ID)
            //    .OnDelete(DeleteBehavior.Restrict);  // Không xóa tự động nếu Watcher bị xóa

            // Liên kết giữa NotificationWatcher và Notification
            //modelBuilder.Entity<NotificationWatcher>()
            //    .HasOne(nw => nw.NotificationSystem)
            //    .WithMany()
            //    .HasForeignKey(nw => nw.NotificationID)
            //    .OnDelete(DeleteBehavior.Cascade);  // Nếu Notification bị xóa, các NotificationWatcher cũng bị xóa

            // Liên kết giữa Watcher và Account (Relative & Patient)
            modelBuilder.Entity<Watcher>()
                .HasOne(w => w.Relative)
                .WithMany()
                .HasForeignKey(w => w.RelativeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Watcher>()
                .HasOne(w => w.Patient)
                .WithMany()
                .HasForeignKey(w => w.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
