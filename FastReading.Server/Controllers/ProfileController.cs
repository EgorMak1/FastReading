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
                return Unauthorized("Сессия истекла. Войдите снова.");
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
            var todayPoints = progresses.Sum(x => x.LastPlayedAt >= todayStart ? SafeNumber(x.LastScore) : 0);
            var totalSessions = progresses.Sum(x => x.SessionsCount);
            var overallScore = progresses.Count == 0 ? 0 : progresses.Average(x => SafeNumber(x.AverageScore));
            var strongest = progresses.OrderByDescending(x => SafeNumber(x.AverageScore)).FirstOrDefault();
            var weakest = progresses.OrderBy(x => SafeNumber(x.AverageScore)).FirstOrDefault();
            var mostStable = progresses
                .OrderByDescending(x => x.SuccessStreak)
                .ThenByDescending(x => SafeNumber(x.AverageScore))
                .FirstOrDefault();
            var needsAttention = progresses
                .OrderByDescending(x => x.FailStreak)
                .ThenBy(x => SafeNumber(x.AverageScore))
                .FirstOrDefault();
            var recommendation = BuildRecommendation(progresses);
            var readiness = BuildReadinessStatus(overallScore, progresses.Count);
            var dailyActivity = await BuildDailyActivityAsync(userId, 14);

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
                dailyActivity,
                exerciseProgress = progresses.Select(x => new
                {
                    x.ExerciseType,
                    x.CurrentLevel,
                    LastScore = SafeNumber(x.LastScore),
                    AverageScore = SafeNumber(x.AverageScore),
                    BestScore = SafeNumber(x.BestScore),
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

        private async Task<List<DailyActivityPoint>> BuildDailyActivityAsync(Guid userId, int days)
        {
            var today = DateTime.UtcNow.Date;
            var startDate = today.AddDays(-(days - 1));
            var endDate = today.AddDays(1);
            var points = new List<ActivityPoint>();

            points.AddRange(await _db.ShulteResults
                .Where(x => x.UserId == userId && x.CompletedAt >= startDate && x.CompletedAt < endDate)
                .Select(x => new ActivityPoint
                {
                    CompletedAt = x.CompletedAt,
                    Points = x.Score
                })
                .ToListAsync());

            points.AddRange(await _db.RunningWordsResults
                .Where(x => x.UserId == userId && x.CompletedAt >= startDate && x.CompletedAt < endDate)
                .Select(x => new ActivityPoint
                {
                    CompletedAt = x.CompletedAt,
                    Points = x.AccuracyPercent
                })
                .ToListAsync());

            points.AddRange(await _db.FieldOfViewResults
                .Where(x => x.UserId == userId && x.CompletedAt >= startDate && x.CompletedAt < endDate)
                .Select(x => new ActivityPoint
                {
                    CompletedAt = x.CompletedAt,
                    Points = x.AccuracyPercent
                })
                .ToListAsync());

            points.AddRange(await _db.WordErasingResults
                .Where(x => x.UserId == userId && x.CompletedAt >= startDate && x.CompletedAt < endDate)
                .Select(x => new ActivityPoint
                {
                    CompletedAt = x.CompletedAt,
                    Points = x.AccuracyPercent
                })
                .ToListAsync());

            var grouped = points
                .GroupBy(x => x.CompletedAt.Date)
                .ToDictionary(
                    x => x.Key,
                    x => new
                    {
                        Points = x.Sum(y => SafeNumber(y.Points)),
                        Sessions = x.Count()
                    });

            return Enumerable.Range(0, days)
                .Select(offset => startDate.AddDays(offset))
                .Select(date => grouped.TryGetValue(date, out var day)
                    ? new DailyActivityPoint(date, Math.Round(day.Points, 1), day.Sessions)
                    : new DailyActivityPoint(date, 0, 0))
                .ToList();
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
                .ThenBy(x => SafeNumber(x.AverageScore))
                .First();

            if (attention.FailStreak >= 2 || SafeNumber(attention.AverageScore) < 55)
            {
                return $"Рекомендуется вернуться к упражнению {ToDisplayName(attention.ExerciseType)} и закрепить текущий уровень.";
            }

            var strongest = progresses
                .OrderByDescending(x => x.SuccessStreak)
                .ThenByDescending(x => SafeNumber(x.AverageScore))
                .First();

            return $"Рекомендуется продолжить упражнение {ToDisplayName(strongest.ExerciseType)}: по нему сейчас лучшая динамика.";
        }

        private static string ToDisplayName(string exerciseType)
        {
            return exerciseType switch
            {
                "ShulteTable" => "Таблица Шульте",
                "RunningWords" => "Бегущие слова",
                "FieldOfView" => "Поле зрения",
                "WordErasing" => "Стирание слов",
                _ => exerciseType
            };
        }

        private static string BuildTrend(FastReading.Server.Models.UserExerciseProgress progress)
        {
            var lastScore = SafeNumber(progress.LastScore);
            var averageScore = SafeNumber(progress.AverageScore);

            if (lastScore >= averageScore + 8)
            {
                return "Рост";
            }

            if (lastScore <= averageScore - 8)
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

            if (SafeNumber(progress.AverageScore) >= 75)
            {
                return "Устойчиво выполняется";
            }

            return "В процессе освоения";
        }

        private static double SafeNumber(double value)
        {
            return double.IsFinite(value) ? value : 0;
        }

        private sealed class ActivityPoint
        {
            public DateTime CompletedAt { get; set; }
            public double Points { get; set; }
        }

        private sealed record DailyActivityPoint(DateTime Date, double Points, int Sessions);
    }
}
