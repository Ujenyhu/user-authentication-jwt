 
using Microsoft.EntityFrameworkCore;
using userauthjwt.Models.Configs;
using userauthjwt.Models.User;
using userauthjwt.Helpers;

#nullable disable
namespace userauthjwt.Models
{
    public partial class AppDbContext : DbContext
    {
        public string UserId;
        public bool CallCustomMethod { get; set; } = true;
        public string Controller { get; set; }
        public string Action { get; set; }
        public string IP { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(new SoftDeleteInterceptor());
        }


        public virtual DbSet<FailedSecurityAttempt> FailedSecurityAttempt
        {
            get; set;
        }

        public virtual DbSet<Audit> AuditLogs { get; set; }
        public virtual DbSet<Country> Country { get; set; }
        public virtual DbSet<SmsConfig> SmsConfig { get; set; }
        public virtual DbSet<SysConfig> SysConfig { get; set; }
        public virtual DbSet<OtpRecord> OtpRecord { get; set; }
        public virtual DbSet<MailConfig> MailConfig { get; set; }
        public virtual DbSet<MetaDataRef> MetaDataRef { get; set; }
        public virtual DbSet<UserProfile> UserProfile { get; set; }
        public virtual DbSet<UserRegistration> UserRegistration { get; set; }


        // Override SaveChangesAsync method to intercept changes before saving
        public async override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, System.Threading.CancellationToken cancellationToken = default)
        {
            if (CallCustomMethod)
            {
                OnBeforeSaveChanges(UserId);
            }

            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        }



        // Log audit entries before saving changes
        private void OnBeforeSaveChanges(string userId)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();

            // Loop through entity entries being tracked for changes
            foreach (var entry in ChangeTracker.Entries())
            {
                // Skip processing if the entry is an Audit entity or is in a detached or unchanged state
                if (entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry); // Create an AuditEntry object for the entity change
                auditEntry.TableName = entry.Entity.GetType().Name;
                auditEntry.UserId = userId;
                auditEntry.IP = IP;
                auditEntry.Controller = Controller;
                auditEntry.Action = Action;
                auditEntries.Add(auditEntry);

                // Loop through the properties of the entity entry
                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;

                    // Check if the property is a primary key
                    if (property.Metadata.IsPrimaryKey())
                    {
                        // Handle negative numeric primary key values
                        if (property.CurrentValue is long || property.CurrentValue is int)
                        {
                            var numericValue = Convert.ToDecimal(property.CurrentValue);
                            if (numericValue < 0)
                            {
                                auditEntry.KeyValues[propertyName] = 0;
                                continue;
                            }
                        }
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    // Determine the state of the entry and populate audit entry properties accordingly
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = VarHelper.AuditType.CREATE;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.AuditType = VarHelper.AuditType.DELETE;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = VarHelper.AuditType.UPDATE;
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }

            // Add audit entries to the DbSet of AuditLogs
            foreach (var auditEntry in auditEntries)
            {
                AuditLogs.Add(auditEntry.ToAudit());
            }
        }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.UserStatus).ValueGeneratedOnAdd();
                entity.Property(u => u.UserType).ValueGeneratedOnAdd();
                entity.HasQueryFilter(x => !x.IsDeleted);

            });

            modelBuilder.Entity<UserRegistration>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.UserStatus).ValueGeneratedOnAdd();
                entity.Property(u => u.UserType).ValueGeneratedOnAdd();

                entity.HasQueryFilter(x => !x.IsDeleted);
                //entity.HasIndex(x => x.IsDeleted);
            });


        }
    }
}
