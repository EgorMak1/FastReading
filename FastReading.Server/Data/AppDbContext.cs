using FastReading.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace FastReading.Server.Data
{
    // DbContext — главный класс EF Core для работы с базой данных.
    // Через него происходит создание таблиц, запросы, сохранение данных.
    public class AppDbContext : DbContext
    {
        // Конструктор получает настройки подключения к базе (connection string)
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet<User> — это таблица Users в базе данных.
        // EF создаст таблицу автоматически при миграции.
        public DbSet<User> Users { get; set; }

        // Метод для дополнительной настройки таблиц и ограничений
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                // Указываем первичный ключ
                entity.HasKey(x => x.Id);

                // Username должен быть уникальным
                entity.HasIndex(x => x.Username).IsUnique();

                // Email тоже должен быть уникальным
                entity.HasIndex(x => x.Email).IsUnique();

                // Ограничение длины и обязательность поля Username
                entity.Property(x => x.Username)
                      .HasMaxLength(50)
                      .IsRequired();

                // Ограничение длины и обязательность поля Email
                entity.Property(x => x.Email)
                      .HasMaxLength(255)
                      .IsRequired();

                // Пароль обязателен
                entity.Property(x => x.PasswordHash)
                      .IsRequired();

                // Дата создания обязательна
                entity.Property(x => x.CreatedAt)
                      .IsRequired();
            });
        }
    }
}
