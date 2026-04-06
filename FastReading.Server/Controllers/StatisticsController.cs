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
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public StatisticsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> SaveResult([FromBody] SaveResultRequest request)
        {
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

        [HttpPost("running-words")]
        public async Task<IActionResult> SaveRunningWordsResult([FromBody] RunningWordsResult request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var result = new RunningWordsResult
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TotalAttempts = request.TotalAttempts,
                CorrectAnswers = request.CorrectAnswers,
                WrongAnswers = request.WrongAnswers,
                AccuracyPercent = request.AccuracyPercent,
                FinalLevel = request.FinalLevel,
                FinalSpeedMilliseconds = request.FinalSpeedMilliseconds,
                CompletedAt = DateTime.UtcNow
            };

            _db.RunningWordsResults.Add(result);
            await _db.SaveChangesAsync();

            return Ok(new { result.Id, result.CompletedAt });
        }

        [HttpGet("running-words")]
        public async Task<IActionResult> GetRunningWordsResults()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);

            var results = await _db.RunningWordsResults
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CompletedAt)
                .Select(x => new
                {
                    x.Id,
                    x.TotalAttempts,
                    x.CorrectAnswers,
                    x.WrongAnswers,
                    x.AccuracyPercent,
                    x.FinalLevel,
                    x.FinalSpeedMilliseconds,
                    x.CompletedAt
                })
                .ToListAsync();

            return Ok(results);
        }
    }

    public class SaveResultRequest
    {
        public string ExerciseType { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
    }
}