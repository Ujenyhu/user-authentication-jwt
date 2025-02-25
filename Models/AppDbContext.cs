using userauthjwt.Models;  
using Microsoft.EntityFrameworkCore;
using userauthjwt.Models.Configs;
using userauthjwt.Models.User;

#nullable disable
namespace userauthjwt.Models
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {

        }

        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
        }

        public virtual DbSet<FailedSecurityAttempt> FailedSecurityAttempt
        {
            get; set;
        }

        public virtual DbSet<Country> Country { get; set; }
        public virtual DbSet<SmsConfig> SmsConfig { get; set; }
        public virtual DbSet<SysConfig> SysConfig { get; set; }
        public virtual DbSet<OtpRecord> OtpRecord { get; set; }
        public virtual DbSet<MailConfig> MailConfig { get; set; }
        public virtual DbSet<MetaDataRef> MetaDataRef { get; set; }
        public virtual DbSet<UserProfile> UserProfile { get; set; }
        public virtual DbSet<UserRegistration> UserRegistration { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.UserStatus).ValueGeneratedOnAdd();
                entity.Property(u => u.UserType).ValueGeneratedOnAdd();

            });

            modelBuilder.Entity<UserRegistration>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.UserStatus).ValueGeneratedOnAdd();
                entity.Property(u => u.UserType).ValueGeneratedOnAdd();
            });
        }
    }
}
