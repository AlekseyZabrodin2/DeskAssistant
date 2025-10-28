using Microsoft.EntityFrameworkCore;

namespace DeskAssistant.DataBase
{
    public class TasksDbContext : DbContext
    {
        public DbSet<CalendarTaskEntity> Tasks { get; set; }

        public TasksDbContext(DbContextOptions<TasksDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CalendarTaskEntity>(entity =>
            {
                entity.ToTable("Tasks");

                entity.HasKey(task => task.Id).HasName("PK_Task");

                entity.Property(task => task.Name).HasMaxLength(128).IsRequired();
                entity.Property(task => task.Description).HasColumnType("text");

                entity.Property(task => task.CreatedDate).HasDefaultValueSql("NOW()").IsRequired();
                entity.Property(task => task.DueDate).IsRequired();
                entity.Property(task => task.DueTime);
                entity.Property(task => task.ReminderTime);
                entity.Property(task => task.CompletedDate);

                entity.Property(task => task.Priority).HasMaxLength(16).IsRequired();
                entity.Property(task => task.Category).HasMaxLength(64).IsRequired();
                entity.Property(task => task.Status).HasMaxLength(32);
                entity.Property(task => task.Tags).HasMaxLength(64).IsRequired(false);

                entity.Property(task => task.IsCompleted).HasDefaultValue(false);
                entity.Property(task => task.IsRecurring).HasDefaultValue(false);
                entity.Property(task => task.RecurrencePattern).HasMaxLength(32);

                entity.Property(task => task.Duration);

                entity.HasIndex(task => task.DueDate).HasDatabaseName("Idx_Tasks_DueDate");
                entity.HasIndex(task => task.IsCompleted).HasDatabaseName("Idx_Tasks_IsCompleted");
            });
        }
    }
}
