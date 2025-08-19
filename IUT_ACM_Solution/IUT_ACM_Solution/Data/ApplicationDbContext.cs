using IUT_ACM_Solution.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IUT_ACM_Solution.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Event> Events { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<UserEvent> UserEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Event Configuration
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Configure Sponsors as JSON column (EF Core 7+)
                entity.Property(e => e.Sponsors)
                .HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                ));

                // Indexes for better query performance
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.EventType);

                // Check constraints
                entity.HasCheckConstraint("CK_Event_DateRange", "[EndDate] > [StartDate]");
                entity.HasCheckConstraint("CK_Event_Capacity", "[Capacity] > 0");
                entity.HasCheckConstraint("CK_Event_BasePrice", "[BasePrice] >= 0");
                entity.HasCheckConstraint("CK_Event_CurrentParticipants", "[CurrentParticipants] >= 0");
            });

            // Team Configuration
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(t => t.Id);

                // Configure TeamMemberEmails as JSON
                entity.Property(t => t.TeamMemberEmails)
                .HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                ));

                // Foreign key relationship
                entity.HasOne(t => t.Event)
                    .WithMany(e => e.ParticipatingTeams)
                    .HasForeignKey(t => t.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index
                entity.HasIndex(t => t.EventId);
                entity.HasIndex(t => t.TeamName);
            });

            // UserEvent Configuration (Many-to-Many)
            modelBuilder.Entity<UserEvent>(entity =>
            {
                // Composite primary key
                entity.HasKey(ue => new { ue.UserId, ue.EventId });

                // Relationships
                entity.HasOne(ue => ue.User)
                    .WithMany() // Add navigation property to User if needed
                    .HasForeignKey(ue => ue.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ue => ue.Event)
                    .WithMany(e => e.UserEvents)
                    .HasForeignKey(ue => ue.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index
                entity.HasIndex(ue => ue.RegisteredAt);
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries().Where(e =>
                e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                // Handle User timestamps
                if (entry.Entity is User user)
                {
                    if (entry.State == EntityState.Added)
                    {
                        user.CreatedAt = DateTime.UtcNow;
                    }
                    user.LastUpdatedAt = DateTime.UtcNow;
                }

                // Handle Event timestamps
                if (entry.Entity is Event eventEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        eventEntity.CreatedAt = DateTime.UtcNow;
                    }
                    eventEntity.LastUpdated = DateTime.UtcNow;
                }
            }
        }
    }

}
