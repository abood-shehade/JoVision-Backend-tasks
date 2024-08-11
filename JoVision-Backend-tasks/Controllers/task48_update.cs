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
    public class UpdateController : ControllerBase
    {
        private readonly ILogger<UpdateController> _logger;
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
        public UpdateController(ILogger<UpdateController> logger)
        {
            _logger = logger;
        }
        public class UpdateFileDto
        {
            public IFormFile? File { get; set; }
            public string Owner { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFile([FromForm] UpdateFileDto updateFileDto)
        {
            if (updateFileDto.File == null || updateFileDto.File.Length == 0)
            {
                return BadRequest("No file uploaded");
            }
            if (string.IsNullOrWhiteSpace(updateFileDto.Owner))
            {
                return BadRequest("Owner name required");
            }

            var fileName = Path.GetFileName(updateFileDto.File.FileName);
            var filePath = Path.Combine(_storagePath, fileName);
            var fileMetadataPath = Path.Combine(_storagePath, $"{fileName}.json");

            if (!System.IO.File.Exists(filePath) || !System.IO.File.Exists(fileMetadataPath))
            {
                return BadRequest("File does not exist");
            }
            try
            {
                var metadataJson = await System.IO.File.ReadAllTextAsync(fileMetadataPath);
                var metadata = System.Text.Json.JsonSerializer.Deserialize<FileMetadata>(metadataJson);
                if (metadata?.Owner != updateFileDto.Owner)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "file owner doesnt match");
                }
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await updateFileDto.File.CopyToAsync(stream);
                }
                metadata.LastModificationTime = DateTime.UtcNow;

                await System.IO.File.WriteAllTextAsync(fileMetadataPath, System.Text.Json.JsonSerializer.Serialize(metadata));

                return Ok("file updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error updating file");
                return StatusCode(StatusCodes.Status500InternalServerError, "error updating file");
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
