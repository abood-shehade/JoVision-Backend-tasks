using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoVision_Backend_tasks.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GreeterController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromQuery] string name = "anonymous")
        {
            string greeting = $"hello {name}";
            return Ok(greeting);
        }
    }
}
