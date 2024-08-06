using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoVision_Backend_tasks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GreeterrController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromQuery] string name = "anonymous")
        {
            string greeting = $"Hello {name}";
            return Ok(greeting);
        }
    }
}
