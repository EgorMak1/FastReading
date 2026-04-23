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

            var todayStart = DateTime.UtcNow.Date;
            var todayPoints = progresses.Sum(x => x.LastPlayedAt >= todayStart ? x.LastScore : 0);
            var totalSessions = progresses.Sum(x => x.SessionsCount);
            var overallScore = progresses.Count == 0 ? 0 : progresses.Average(x => x.AverageScore);
            var strongest = progresses.OrderByDescending(x => x.AverageScore).FirstOrDefault();
            var weakest = progresses.OrderBy(x => x.AverageScore).FirstOrDefault();

            return Ok(new
            {
                overallScore,
                todayPoints,
                totalSessions,
                exercisesTracked = progresses.Count,
                strongestExercise = strongest?.ExerciseType,
                weakestExercise = weakest?.ExerciseType,
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
                    x.LastPlayedAt
                })
            });
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");

            return Guid.TryParse(userIdClaim?.Value, out userId);
        }
    }
}
