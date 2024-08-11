using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JoVision_Backend_tasks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferOwnershipController : ControllerBase
    {
        private readonly ILogger<TransferOwnershipController> _logger;
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
        public TransferOwnershipController(ILogger<TransferOwnershipController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> TransferOwnership(string oldOwner, string newOwner)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(oldOwner) || string.IsNullOrWhiteSpace(newOwner))
                {
                    return BadRequest("OldOwner and NewOwner are required.");
                }

                var jsonFiles = Directory.GetFiles(_storagePath, "*.json");

                var filesToTransfer = new List<string>();
                var newOwnerFiles = new List<string>();

                foreach (var jsonFile in jsonFiles)
                {
                    var metadata = System.Text.Json.JsonSerializer.Deserialize<FileMetadata>(System.IO.File.ReadAllText(jsonFile));
                    if (metadata == null)
                        continue;

                    if (metadata.Owner == oldOwner)
                    {
                        metadata.Owner = newOwner;
                        System.IO.File.WriteAllText(jsonFile, System.Text.Json.JsonSerializer.Serialize(metadata));
                        filesToTransfer.Add(Path.GetFileNameWithoutExtension(jsonFile));
                    }
                    if (metadata.Owner == newOwner)
                    {
                        newOwnerFiles.Add(Path.GetFileNameWithoutExtension(jsonFile));
                    }
                }
                newOwnerFiles.AddRange(filesToTransfer.Distinct());
                return Ok(newOwnerFiles.Distinct().ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error transferring ownership");
                return StatusCode(StatusCodes.Status500InternalServerError, "error transferring ownership");
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
