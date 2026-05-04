using CompanyFileManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace CompanyFileManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly AppSettingsService _settings;

        public FileController(AppSettingsService settings)
        {
            _settings = settings;
        }

        [HttpGet("download")]
        public IActionResult Download([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest("Path is required");

            if (!IsPathAllowed(path))
                return Forbid();

            if (!System.IO.File.Exists(path))
                return NotFound();

            try
            {
                var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var contentType = GetContentType(path);
                var fileName = System.IO.Path.GetFileName(path);
                return File(stream, contentType, fileName);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, "Access denied");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("preview")]
        public async Task<IActionResult> Preview([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return BadRequest();

            if (!IsPathAllowed(path))
                return Forbid();

            if (!System.IO.File.Exists(path))
                return NotFound();

            try
            {
                var content = await System.IO.File.ReadAllTextAsync(path);
                return Content(content, "text/plain; charset=utf-8");
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, "Access denied");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromQuery] string directory, IFormFile file)
        {
            if (!_settings.Settings.AllowUpload)
                return StatusCode(403, "Upload is disabled by server settings");

            if (string.IsNullOrWhiteSpace(directory) || file == null || file.Length == 0)
                return BadRequest();

            if (!IsPathAllowed(directory))
                return Forbid();

            if (!Directory.Exists(directory))
                return NotFound("Directory not found");

            try
            {
                var fileName = System.IO.Path.GetFileName(file.FileName);
                var destPath = System.IO.Path.Combine(directory, fileName);

                await using var stream = new FileStream(destPath, FileMode.Create);
                await file.CopyToAsync(stream);

                return Ok(new { path = destPath, name = fileName });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, "Access denied");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private bool IsPathAllowed(string path)
        {
            var root = _settings.Settings.SharedDirectory;
            if (string.IsNullOrEmpty(root)) return false;
            try
            {
                var fullPath = System.IO.Path.GetFullPath(path);
                var fullRoot = System.IO.Path.GetFullPath(root);
                return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private static string GetContentType(string path)
        {
            var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".txt" or ".log" or ".ini" or ".conf" or ".csv" or ".md" => "text/plain; charset=utf-8",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".mp4" => "video/mp4",
                _ => "application/octet-stream"
            };
        }
    }
}
