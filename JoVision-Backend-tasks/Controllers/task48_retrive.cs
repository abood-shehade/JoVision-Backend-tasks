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
    public class RetrieveController : ControllerBase
    {
        private readonly ILogger<RetrieveController> _logger;
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

        public RetrieveController(ILogger<RetrieveController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> RetrieveFile([FromQuery] string fileName, [FromQuery] string fileOwner)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(fileOwner))
            {
                return BadRequest("FileName and FileOwner are required.");
            }

            var filePath = Path.Combine(_storagePath, fileName);
            var fileMetadataPath = Path.Combine(_storagePath, $"{fileName}.json");

            if (!System.IO.File.Exists(filePath) || !System.IO.File.Exists(fileMetadataPath))
            {
                return NotFound("File not found.");
            }

            try
            {
                var metadataJson = await System.IO.File.ReadAllTextAsync(fileMetadataPath);
                var metadata = System.Text.Json.JsonSerializer.Deserialize<FileMetadata>(metadataJson);

                if (metadata?.Owner != fileOwner)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "File owner does not match.");
                }

                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var contentType = "application/octet-stream"; 

                return File(fileStream, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the file.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the file.");
            }
        }

        private class FileMetadata
        {
            public string Owner { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime LastModificationTime { get; set; }
        }
    }
}
