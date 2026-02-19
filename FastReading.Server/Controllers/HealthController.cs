using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace FastReading.Server.Controllers
{
    // Атрибут [ApiController] включает автоматическую обработку HTTP-запросов
    [ApiController]

    // Маршрут: /api/health
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        // GET /api/health
        [HttpGet]
        public IActionResult Get()
        {
            // Возвращаем статус 200 OK и JSON-объект
            return Ok(new
            {
                status = "ok",
                environment = "Production"
            });
        }

        // GET /api/health/secure
        [Authorize]
        [HttpGet("secure")]
        public IActionResult Secure()
        {
            return Ok(new { status = "ok", secure = true });
        }

    }
}
