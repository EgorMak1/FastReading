using FastReading.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FastReading.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProfileController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var progresses = await _db.UserExerciseProgresses
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.LastPlayedAt)
                .ToListAsync();

            var user = await _db.Users
                .Where(x => x.Id == userId)
                .Select(x => new { x.Username })
                .FirstOrDefaultAsync();

            var todayStart = DateTime.UtcNow.Date;
            var todayPoints = progresses.Sum(x => x.LastPlayedAt >= todayStart ? x.LastScore : 0);
            var totalSessions = progresses.Sum(x => x.SessionsCount);
            var overallScore = progresses.Count == 0 ? 0 : progresses.Average(x => x.AverageScore);
            var strongest = progresses.OrderByDescending(x => x.AverageScore).FirstOrDefault();
            var weakest = progresses.OrderBy(x => x.AverageScore).FirstOrDefault();
            var mostStable = progresses
                .OrderByDescending(x => x.SuccessStreak)
                .ThenByDescending(x => x.AverageScore)
                .FirstOrDefault();
            var needsAttention = progresses
                .OrderByDescending(x => x.FailStreak)
                .ThenBy(x => x.AverageScore)
                .FirstOrDefault();
            var recommendation = BuildRecommendation(progresses);
            var readiness = BuildReadinessStatus(overallScore, progresses.Count);

            return Ok(new
            {
                username = user?.Username,
                displayName = (string?)null,
                overallScore,
                todayPoints,
                totalSessions,
                exercisesTracked = progresses.Count,
                readiness,
                strongestExercise = strongest?.ExerciseType,
                weakestExercise = weakest?.ExerciseType,
                mostStableExercise = mostStable?.ExerciseType,
                needsAttentionExercise = needsAttention?.ExerciseType,
                recommendation,
                exerciseProgress = progresses.Select(x => new
                {
                    x.ExerciseType,
                    x.CurrentLevel,
                    x.LastScore,
                    x.AverageScore,
                    x.BestScore,
                    x.SessionsCount,
                    x.SuccessStreak,
                    x.FailStreak,
                    x.LastPlayedAt,
                    trend = BuildTrend(x),
                    status = BuildExerciseStatus(x)
                })
            });
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");

            return Guid.TryParse(userIdClaim?.Value, out userId);
        }

        private static string BuildReadinessStatus(double overallScore, int exercisesTracked)
        {
            if (exercisesTracked == 0)
            {
                return "Недостаточно данных";
            }

            if (overallScore >= 80)
            {
                return "Высокая устойчивость";
            }

            if (overallScore >= 60)
            {
                return "Стабильный прогресс";
            }

            return "Нужна дополнительная практика";
        }

        private static string BuildRecommendation(List<FastReading.Server.Models.UserExerciseProgress> progresses)
        {
            if (progresses.Count == 0)
            {
                return "Начните с любой тренировки, чтобы система накопила данные.";
            }

            var attention = progresses
                .OrderByDescending(x => x.FailStreak)
                .ThenBy(x => x.AverageScore)
                .First();

            if (attention.FailStreak >= 2 || attention.AverageScore < 55)
            {
                return $"Рекомендуется вернуться к упражнению {attention.ExerciseType} и закрепить текущий уровень.";
            }

            var strongest = progresses
                .OrderByDescending(x => x.SuccessStreak)
                .ThenByDescending(x => x.AverageScore)
                .First();

            return $"Рекомендуется продолжить упражнение {strongest.ExerciseType}: по нему сейчас лучшая динамика.";
        }

        private static string BuildTrend(FastReading.Server.Models.UserExerciseProgress progress)
        {
            if (progress.LastScore >= progress.AverageScore + 8)
            {
                return "Рост";
            }

            if (progress.LastScore <= progress.AverageScore - 8)
            {
                return "Спад";
            }

            return "Стабильно";
        }

        private static string BuildExerciseStatus(FastReading.Server.Models.UserExerciseProgress progress)
        {
            if (progress.SuccessStreak >= 3)
            {
                return "Готов к усложнению";
            }

            if (progress.FailStreak >= 2)
            {
                return "Требует закрепления";
            }

            if (progress.AverageScore >= 75)
            {
                return "Устойчиво выполняется";
            }

            return "В процессе освоения";
        }
    }
}
