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
            await UpdateExerciseProgressAsync(
                userId,
                "RunningWords",
                request.FinalLevel,
                CalculateRunningWordsScore(request),
                result.CompletedAt);

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

        [HttpPost("shulte")]
        public async Task<IActionResult> SaveShulteResult([FromBody] ShulteResultRequest request)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            if (request.GridSize <= 0 || request.NumbersCount <= 0 || request.DurationSeconds <= 0)
            {
                return BadRequest("GridSize, NumbersCount and DurationSeconds must be greater than 0.");
            }

            var result = new ShulteResult
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GridSize = request.GridSize,
                NumbersCount = request.NumbersCount,
                LevelBefore = request.LevelBefore,
                LevelAfter = request.LevelAfter,
                DurationSeconds = request.DurationSeconds,
                ErrorsCount = request.ErrorsCount,
                Score = request.Score,
                CompletedAt = DateTime.UtcNow
            };

            _db.ShulteResults.Add(result);
            await _db.SaveChangesAsync();
            await UpdateExerciseProgressAsync(
                userId,
                "ShulteTable",
                request.LevelAfter,
                request.Score,
                result.CompletedAt);

            return Ok(new { result.Id, result.CompletedAt });
        }

        [HttpGet("shulte")]
        public async Task<IActionResult> GetShulteResults()
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var results = await _db.ShulteResults
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CompletedAt)
                .Select(x => new
                {
                    x.Id,
                    x.GridSize,
                    x.NumbersCount,
                    x.LevelBefore,
                    x.LevelAfter,
                    x.DurationSeconds,
                    x.ErrorsCount,
                    x.Score,
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
            await UpdateExerciseProgressAsync(
                userId,
                "FieldOfView",
                request.FinalLevel,
                CalculateFieldOfViewScore(request),
                result.CompletedAt);

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
            await UpdateExerciseProgressAsync(
                userId,
                "WordErasing",
                CalculateWordErasingLevel(request.SpeedAfterWpm),
                CalculateWordErasingScore(request),
                result.CompletedAt);

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

        private async Task UpdateExerciseProgressAsync(Guid userId, string exerciseType, int currentLevel, double score, DateTime playedAt)
        {
            var progress = await _db.UserExerciseProgresses
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ExerciseType == exerciseType);

            if (progress == null)
            {
                progress = new UserExerciseProgress
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ExerciseType = exerciseType,
                    CurrentLevel = currentLevel,
                    LastScore = score,
                    AverageScore = score,
                    BestScore = score,
                    SessionsCount = 1,
                    SuccessStreak = score >= 80 ? 1 : 0,
                    FailStreak = score < 55 ? 1 : 0,
                    LastPlayedAt = playedAt
                };

                _db.UserExerciseProgresses.Add(progress);
            }
            else
            {
                progress.CurrentLevel = currentLevel;
                progress.LastScore = score;
                progress.AverageScore = ((progress.AverageScore * progress.SessionsCount) + score) / (progress.SessionsCount + 1);
                progress.BestScore = Math.Max(progress.BestScore, score);
                progress.SessionsCount++;
                progress.LastPlayedAt = playedAt;

                if (score >= 80)
                {
                    progress.SuccessStreak++;
                    progress.FailStreak = 0;
                }
                else if (score < 55)
                {
                    progress.FailStreak++;
                    progress.SuccessStreak = 0;
                }
                else
                {
                    progress.SuccessStreak = 0;
                    progress.FailStreak = 0;
                }
            }

            await _db.SaveChangesAsync();
        }

        private static double CalculateRunningWordsScore(RunningWordsResultRequest request)
        {
            var levelBonus = Math.Min(20, request.FinalLevel * 4);
            return Math.Clamp(request.AccuracyPercent * 0.8 + levelBonus, 0, 100);
        }

        private static double CalculateFieldOfViewScore(FieldOfViewResultRequest request)
        {
            var penalty = request.FalseAlarmCount * 4 + request.MissedMismatchCount * 5;
            var levelBonus = Math.Min(20, request.FinalLevel * 4);
            return Math.Clamp(request.AccuracyPercent * 0.75 + levelBonus - penalty, 0, 100);
        }

        private static double CalculateWordErasingScore(WordErasingResultRequest request)
        {
            var speedBonus = Math.Clamp(request.SpeedDelta + 15, 0, 30);
            var skippedPenalty = request.QuestionsSkipped ? 20 : 0;
            return Math.Clamp(request.AccuracyPercent * 0.7 + speedBonus - skippedPenalty, 0, 100);
        }

        private static int CalculateWordErasingLevel(int speedAfterWpm)
        {
            return speedAfterWpm switch
            {
                <= 160 => 1,
                <= 220 => 2,
                <= 280 => 3,
                <= 340 => 4,
                _ => 5
            };
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

    public class ShulteResultRequest
    {
        public int GridSize { get; set; }
        public int NumbersCount { get; set; }
        public int LevelBefore { get; set; }
        public int LevelAfter { get; set; }
        public int DurationSeconds { get; set; }
        public int ErrorsCount { get; set; }
        public double Score { get; set; }
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
