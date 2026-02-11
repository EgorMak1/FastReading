using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FastReading.Server.Contracts.Auth;
using FastReading.Server.Data;
using FastReading.Server.Models;

namespace FastReading.Server.Controllers
{
    // Контроллер для авторизации/регистрации
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Базовая валидация входных данных (минимум, чтобы не падать)
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Email обязателен.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Пароль обязателен.");
            }

            // Упрощение на старте:
            // - пока используем Email как Username
            // - чтобы не менять контракт и UI прямо сейчас
            // Важно: Username ограничен 50 символами в настройке EF.
            if (request.Email.Length > 50)
            {
                return BadRequest("Email слишком длинный (максимум 50 символов на текущем этапе).");
            }

            // Нормализуем email (чтобы не было дублей вида Test@Mail и test@mail)
            var email = request.Email.Trim().ToLowerInvariant();

            // Проверяем, нет ли уже такого пользователя
            var exists = await _db.Users.AnyAsync(x => x.Email.ToLower() == email);
            if (exists)
            {
                return Conflict("Пользователь с таким Email уже существует.");
            }

            // Хешируем пароль (в базу НИКОГДА не сохраняем пароль в открытом виде)
            var passwordHash = PasswordHashing.HashPassword(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = email, // временно
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Возвращаем минимум информации (без пароля и без хеша)
            return Ok(new
            {
                userId = user.Id,
                user.Email,
                user.Username
            });
        }

        // Внутренний helper для хеширования пароля через PBKDF2
        // Формат хранения: base64(salt):base64(hash)
        private static class PasswordHashing
        {
            public static string HashPassword(string password)
            {
                // Соль (salt) нужна, чтобы одинаковые пароли давали разные хеши
                var salt = RandomNumberGenerator.GetBytes(16);

                // PBKDF2 — стандартный и безопасный вариант для хеширования паролей
                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password: password,
                    salt: salt,
                    iterations: 100_000,
                    hashAlgorithm: HashAlgorithmName.SHA256);

                var hash = pbkdf2.GetBytes(32);

                return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
            }
        }
    }
}
