using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoVision_Backend_tasks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BirthDate_Task46Controller : ControllerBase
    {
        [HttpPost]
        public IActionResult Post([FromForm] string name = "anonymous", [FromForm] int? years = null, [FromForm] int? months = null, [FromForm] int? days = null)
        {
            if (years == null || months == null || days == null)
            {
                return Ok($"Hello {name}, I can’t calculate your age without knowing your birthdate!");
            }

            DateTime birthDate = new DateTime(years.Value, months.Value, days.Value);
            int age = CalculateAge(birthDate);

            return Ok($"Hello {name}, your age is {age}");
        }

        private int CalculateAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age)) age--;

            return age;
        }
    }
}
