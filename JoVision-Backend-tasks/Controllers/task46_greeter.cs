using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoVision_Backend_tasks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Greeter_Task46Controller : ControllerBase
    {
        [HttpPost]
        public IActionResult Post([FromForm] string name = "anonymous")
        {
            string greeting = $"Hello {name}";
            return Ok(greeting);
        }
    }
}
