using Microsoft.AspNetCore.Mvc;

namespace PolicyService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController() : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public IActionResult GetHealth()
        {
            var response = new HealthResponse
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Service = "PolicyService",
                Version = "1.0.0"
            };

            return Ok(response);
        }

        private class HealthResponse
        {
            public string Status { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public string Service { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
        }
    }
}