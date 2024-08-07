using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
namespace JoVision_Backend_tasks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreateController : ControllerBase
    {
        private readonly ILogger<CreateController> _logger;
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

        public CreateController(ILogger<CreateController> logger)
        {
            _logger = logger;
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public class UploadFileDto
        {
            public IFormFile? File { get; set; }
            public string Owner { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileDto uploadFileDto)
        {
            if (uploadFileDto.File == null || uploadFileDto.File.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (string.IsNullOrWhiteSpace(uploadFileDto.Owner))
            {
                return BadRequest("Owner name is required.");
            }

            try
            {
                var fileName = Path.GetFileName(uploadFileDto.File.FileName);
                var filePath = Path.Combine(_storagePath, fileName);
                var fileMetadataPath = Path.Combine(_storagePath, $"{fileName}.json");

                if (System.IO.File.Exists(filePath))
                {
                    return BadRequest("A file with the same name already exists.");
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadFileDto.File.CopyToAsync(stream);
                }

                var metadata = new
                {
                    Owner = uploadFileDto.Owner,
                    CreationTime = DateTime.UtcNow,
                    LastModificationTime = DateTime.UtcNow
                };

                await System.IO.File.WriteAllTextAsync(fileMetadataPath, System.Text.Json.JsonSerializer.Serialize(metadata));

                return Created(filePath, new { FilePath = filePath, Metadata = metadata });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading the file.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while uploading the file.");
            }
        }
    }
}
