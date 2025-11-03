using Microsoft.AspNetCore.Mvc;

namespace Inventory_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "API is working!",
                timestamp = DateTime.UtcNow,
                status = "OK"
            });
        }

        [HttpGet("public")]
        public IActionResult GetPublic()
        {
            return Ok(new
            {
                message = "Public API endpoint is working!",
                timestamp = DateTime.UtcNow
            });
        }
    }
}