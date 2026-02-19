using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FastReading.Server.Contracts.Auth;
using FastReading.Server.Data;
using FastReading.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace FastReading.Server.Controllers
{
    // Контроллер для авторизации/регистрации
    [ApiController]
    [Route("api/[controller]")]

    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;


        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
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

            // Нормализуем email:
            // - убираем пробелы по краям
            // - приводим к нижнему регистру
            // Это нужно, чтобы не было дублей вида Test@Mail.com и test@mail.com
            var email = request.Email.Trim().ToLowerInvariant();

            // Проверяем, нет ли уже пользователя с таким Email
            // (Email — уникальный бизнес-идентификатор)
            var exists = await _db.Users.AnyAsync(x => x.Email.ToLower() == email);
            if (exists)
            {
                return Conflict("Пользователь с таким Email уже существует.");
            }

            // Генерируем username автоматически.
            // Почему так:
            // - сейчас пользователь вводит только Email и Password
            // - Username в БД обязателен (IsRequired) и должен быть уникальным (IsUnique)
            // Формат: user_XXXXXXXX (8 символов), чтобы уложиться в лимит 50 символов.
            string? username = null;

            for (var i = 0; i < 5; i++)
            {
                // Кандидат username
                var candidate = "user_" + Guid.NewGuid().ToString("N")[..8];

                // Проверяем уникальность username
                var usernameExists = await _db.Users.AnyAsync(x => x.Username == candidate);
                if (!usernameExists)
                {
                    username = candidate;
                    break;
                }
            }

            // Если за несколько попыток не смогли сгенерировать уникальный username — отдаём 500
            // (на практике вероятность очень низкая, но обработка обязана быть)
            if (username == null)
            {
                return StatusCode(500, "Не удалось сгенерировать уникальное имя пользователя. Повторите попытку.");
            }

            // Хешируем пароль (в базу НИКОГДА не сохраняем пароль в открытом виде)
            var passwordHash = PasswordHashing.HashPassword(request.Password);

            // Создаём пользователя
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = username, // автогенерированное уникальное имя пользователя
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            // Сохраняем в базу
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

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email и пароль обязательны.");
            }

            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _db.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (user == null)
            {
                return Unauthorized("Неверный email или пароль.");
            }

            var isValid = PasswordHashing.VerifyPassword(request.Password, user.PasswordHash);

            if (!isValid)
            {
                return Unauthorized("Неверный email или пароль.");
            }

            var jwt = _config.GetSection("Jwt");
            var key = jwt["Key"]!;
            var issuer = jwt["Issuer"]!;
            var audience = jwt["Audience"]!;
            var expiresMinutes = int.Parse(jwt["ExpiresMinutes"]!);

            var claims = new[]
            {
    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    new Claim(JwtRegisteredClaimNames.Email, user.Email),
    new Claim("username", user.Username)
};

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                accessToken,
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

            public static bool VerifyPassword(string password, string storedHash)
            {
                var parts = storedHash.Split(':');
                if (parts.Length != 2)
                {
                    return false;
                }

                var salt = Convert.FromBase64String(parts[0]);
                var expectedHash = Convert.FromBase64String(parts[1]);

                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password: password,
                    salt: salt,
                    iterations: 100_000,
                    hashAlgorithm: HashAlgorithmName.SHA256);

                var actualHash = pbkdf2.GetBytes(32);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }

        }
    }
}
