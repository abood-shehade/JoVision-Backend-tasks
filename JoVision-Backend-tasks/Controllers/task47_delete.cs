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
    public class DeleteController : ControllerBase
    {
        private readonly ILogger<DeleteController> _logger;
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

        public DeleteController(ILogger<DeleteController> logger)
        {
            _logger = logger;
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        [HttpGet]
        public IActionResult DeleteFile([FromQuery] string fileName, [FromQuery] string fileOwner)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(fileOwner))
            {
                return BadRequest("fileName and fileOwner required");
            }

            var filePath = Path.Combine(_storagePath, fileName);
            var fileMetadataPath = Path.Combine(_storagePath, $"{fileName}.json");

            if (!System.IO.File.Exists(filePath) || !System.IO.File.Exists(fileMetadataPath))
            {
                return BadRequest("File or metadata not found");
            }

            try
            {
                var metadataContent = System.IO.File.ReadAllText(fileMetadataPath);
                var metadata = System.Text.Json.JsonSerializer.Deserialize<FileMetadata>(metadataContent);

                if (metadata == null || !string.Equals(metadata.Owner, fileOwner, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid("File owner does not match");
                }

                System.IO.File.Delete(filePath);
                System.IO.File.Delete(fileMetadataPath);

                return Ok("File deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error deleting the file");
                return StatusCode(StatusCodes.Status500InternalServerError, "error deleting the file");
            }
        }

        private class FileMetadata
        {
            public string Owner { get; set; } = string.Empty;
            public DateTime CreationTime { get; set; }
            public DateTime LastModificationTime { get; set; }
        }
    }
}