using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public enum FilterType
{
    ByModificationDate,
    ByCreationDateDescending,
    ByCreationDateAscending,
    ByOwner
}

namespace JoVision_Backend_tasks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilterController : ControllerBase
    {
        private readonly ILogger<FilterController> _logger;
        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
        public FilterController(ILogger<FilterController> logger)
        {
            _logger = logger;
        }
        public class FilterFileDto
        {
            public DateTime? CreationDate { get; set; }
            public DateTime? ModificationDate { get; set; }
            public string Owner { get; set; } = string.Empty;
            public FilterType FilterType { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> FilterFiles([FromForm] FilterFileDto filterFileDto)
        {
            try
            {
                var files = Directory.GetFiles(_storagePath, "*.json")
                                     .Select(file => new
                                     {
                                         FilePath = file,
                                         Metadata = System.Text.Json.JsonSerializer.Deserialize<FileMetadata>(System.IO.File.ReadAllText(file))
                                     })
                                     .Where(file => file.Metadata != null)
                                     .ToList();

                IEnumerable<object> filteredFiles = null;

                switch (filterFileDto.FilterType)
                {
                    case FilterType.ByModificationDate:
                        if (!filterFileDto.ModificationDate.HasValue)
                            return BadRequest("ModificationDate is required for ByModificationDate filter.");

                        filteredFiles = files.Where(f => f.Metadata.LastModificationTime < filterFileDto.ModificationDate)
                                             .Select(f => new { f.Metadata.Owner, FileName = Path.GetFileNameWithoutExtension(f.FilePath) });
                        break;

                    case FilterType.ByCreationDateDescending:
                        if (!filterFileDto.CreationDate.HasValue)
                            return BadRequest("CreationDate is required for ByCreationDateDescending filter.");

                        filteredFiles = files.Where(f => f.Metadata.CreationTime > filterFileDto.CreationDate)
                                             .OrderByDescending(f => f.Metadata.CreationTime)
                                             .Select(f => new { f.Metadata.Owner, FileName = Path.GetFileNameWithoutExtension(f.FilePath) });
                        break;

                    case FilterType.ByCreationDateAscending:
                        if (!filterFileDto.CreationDate.HasValue)
                            return BadRequest("CreationDate is required for ByCreationDateAscending filter.");

                        filteredFiles = files.Where(f => f.Metadata.CreationTime > filterFileDto.CreationDate)
                                             .OrderBy(f => f.Metadata.CreationTime)
                                             .Select(f => new { f.Metadata.Owner, FileName = Path.GetFileNameWithoutExtension(f.FilePath) });
                        break;

                    case FilterType.ByOwner:
                        if (string.IsNullOrWhiteSpace(filterFileDto.Owner))
                            return BadRequest("Owner is required for ByOwner filter.");

                        filteredFiles = files.Where(f => f.Metadata.Owner == filterFileDto.Owner)
                                             .Select(f => new { f.Metadata.Owner, FileName = Path.GetFileNameWithoutExtension(f.FilePath) });
                        break;

                    default:
                        return BadRequest("Invalid FilterType.");
                }
                return Ok(filteredFiles.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error while transferring filter");
                return StatusCode(StatusCodes.Status500InternalServerError, "error occured filtering files");
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
