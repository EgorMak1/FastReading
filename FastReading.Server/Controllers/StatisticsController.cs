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
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

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
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

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
        public async Task<IActionResult> SaveRunningWordsResult([FromBody] RunningWordsResultRequest request)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            if (request.TotalAttempts <= 0)
            {
                return BadRequest("TotalAttempts must be greater than 0.");
            }

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
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

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

        [HttpPost("field-of-view")]
        public async Task<IActionResult> SaveFieldOfViewResult([FromBody] FieldOfViewResultRequest request)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            if (request.TotalRounds <= 0)
            {
                return BadRequest("TotalRounds must be greater than 0.");
            }

            var result = new FieldOfViewResult
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TotalRounds = request.TotalRounds,
                CorrectRounds = request.CorrectRounds,
                DetectedMismatchCount = request.DetectedMismatchCount,
                MissedMismatchCount = request.MissedMismatchCount,
                FalseAlarmCount = request.FalseAlarmCount,
                AccuracyPercent = request.AccuracyPercent,
                FinalLevel = request.FinalLevel,
                FinalIntervalMilliseconds = request.FinalIntervalMilliseconds,
                CompletedAt = DateTime.UtcNow
            };

            _db.FieldOfViewResults.Add(result);
            await _db.SaveChangesAsync();

            return Ok(new { result.Id, result.CompletedAt });
        }

        [HttpGet("field-of-view")]
        public async Task<IActionResult> GetFieldOfViewResults()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var results = await _db.FieldOfViewResults
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CompletedAt)
                .Select(x => new
                {
                    x.Id,
                    x.TotalRounds,
                    x.CorrectRounds,
                    x.DetectedMismatchCount,
                    x.MissedMismatchCount,
                    x.FalseAlarmCount,
                    x.AccuracyPercent,
                    x.FinalLevel,
                    x.FinalIntervalMilliseconds,
                    x.CompletedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpPost("word-erasing")]
        public async Task<IActionResult> SaveWordErasingResult([FromBody] WordErasingResultRequest request)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            if (request.SpeedBeforeWpm <= 0 || request.SpeedAfterWpm <= 0)
            {
                return BadRequest("Speed values must be greater than 0.");
            }

            var result = new WordErasingResult
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TextId = request.TextId,
                TextTitle = request.TextTitle,
                SpeedBeforeWpm = request.SpeedBeforeWpm,
                SpeedAfterWpm = request.SpeedAfterWpm,
                SpeedDelta = request.SpeedDelta,
                CompletionType = request.CompletionType,
                CorrectAnswers = request.CorrectAnswers,
                TotalQuestions = request.TotalQuestions,
                QuestionsSkipped = request.QuestionsSkipped,
                AccuracyPercent = request.AccuracyPercent,
                ErasedWords = request.ErasedWords,
                TotalWords = request.TotalWords,
                CompletedAt = DateTime.UtcNow
            };

            _db.WordErasingResults.Add(result);
            await _db.SaveChangesAsync();

            return Ok(new { result.Id, result.CompletedAt });
        }

        [HttpGet("word-erasing")]
        public async Task<IActionResult> GetWordErasingResults()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var results = await _db.WordErasingResults
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CompletedAt)
                .Select(x => new
                {
                    x.Id,
                    x.TextId,
                    x.TextTitle,
                    x.SpeedBeforeWpm,
                    x.SpeedAfterWpm,
                    x.SpeedDelta,
                    x.CompletionType,
                    x.CorrectAnswers,
                    x.TotalQuestions,
                    x.QuestionsSkipped,
                    x.AccuracyPercent,
                    x.ErasedWords,
                    x.TotalWords,
                    x.CompletedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");

            return Guid.TryParse(userIdClaim?.Value, out userId);
        }
    }

    public class SaveResultRequest
    {
        public string ExerciseType { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
    }

    public class RunningWordsResultRequest
    {
        public int TotalAttempts { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public double AccuracyPercent { get; set; }
        public int FinalLevel { get; set; }
        public int FinalSpeedMilliseconds { get; set; }
    }

    public class FieldOfViewResultRequest
    {
        public int TotalRounds { get; set; }
        public int CorrectRounds { get; set; }
        public int DetectedMismatchCount { get; set; }
        public int MissedMismatchCount { get; set; }
        public int FalseAlarmCount { get; set; }
        public double AccuracyPercent { get; set; }
        public int FinalLevel { get; set; }
        public int FinalIntervalMilliseconds { get; set; }
    }

    public class WordErasingResultRequest
    {
        public string TextId { get; set; } = string.Empty;
        public string TextTitle { get; set; } = string.Empty;
        public int SpeedBeforeWpm { get; set; }
        public int SpeedAfterWpm { get; set; }
        public int SpeedDelta { get; set; }
        public string CompletionType { get; set; } = string.Empty;
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public bool QuestionsSkipped { get; set; }
        public double AccuracyPercent { get; set; }
        public int ErasedWords { get; set; }
        public int TotalWords { get; set; }
    }
}
