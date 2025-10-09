using Microsoft.AspNetCore.Mvc;
using Sensor_Api.Services;

namespace Sensor_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorController : ControllerBase
    {
        private readonly ISensorService _service;

        public SensorController(ISensorService service)
        {
            _service = service;
        }

        [HttpGet("batch")]
        public async Task<IActionResult> GetBatch(int startId = 0, int batchSize = 1000)
        {
            var readings = await _service.GetReadingsBatchAsync(startId, batchSize);
            return Ok(readings);
        }

    }
}
