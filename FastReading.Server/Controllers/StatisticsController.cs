using FastReading.Server.Data;
using FastReading.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FastReading.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // все endpoints требуют JWT токен
    public class StatisticsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public StatisticsController(AppDbContext db)
        {
            _db = db;
        }

        // POST /api/statistics
        // Сохраняем результат тренировки
        [HttpPost]
        public async Task<IActionResult> SaveResult([FromBody] SaveResultRequest request)
        {
            // Получаем ID пользователя из JWT токена
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var result = new TrainingResult
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExerciseType = request.ExerciseType,
                DurationSeconds = request.DurationSeconds,
                CompletedAt = DateTime.UtcNow
            };

            _db.TrainingResults.Add(result);
            await _db.SaveChangesAsync();

            return Ok(new { result.Id, result.CompletedAt });
        }

        // GET /api/statistics
        // Получаем статистику текущего пользователя
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var results = await _db.TrainingResults
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CompletedAt)
                .Select(x => new
                {
                    x.Id,
                    x.ExerciseType,
                    x.DurationSeconds,
                    x.CompletedAt
                })
                .ToListAsync();

            return Ok(results);
        }
    }

    // Модель запроса для сохранения результата
    public class SaveResultRequest
    {
        public string ExerciseType { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
    }
}