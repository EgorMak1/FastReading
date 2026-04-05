using FastReading.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace FastReading.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        // Новая таблица для статистики
        public DbSet<TrainingResult> TrainingResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Username).IsUnique();
                entity.HasIndex(x => x.Email).IsUnique();
                entity.Property(x => x.Username)
                      .HasMaxLength(50)
                      .IsRequired();
                entity.Property(x => x.Email)
                      .HasMaxLength(255)
                      .IsRequired();
                entity.Property(x => x.PasswordHash)
                      .IsRequired();
                entity.Property(x => x.CreatedAt)
                      .IsRequired();
            });

            // Настройка таблицы TrainingResults
            modelBuilder.Entity<TrainingResult>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ExerciseType)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(x => x.DurationSeconds)
                      .IsRequired();

                entity.Property(x => x.CompletedAt)
                      .IsRequired();

                // Связь: один User -> много TrainingResults
                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}