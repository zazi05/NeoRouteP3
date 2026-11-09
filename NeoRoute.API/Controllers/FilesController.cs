using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeoRoute.API.Services;

namespace NeoRoute.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly AzureBlobService _blobService;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public FilesController(AzureBlobService blobService)
        {
            _blobService = blobService;
        }

        /// <summary>
        /// Upload an image file (event banner or profile picture). Returns the public URL.
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(MaxFileSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file provided." });

            if (file.Length > MaxFileSize)
                return BadRequest(new { message = "File size exceeds 5 MB limit." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return BadRequest(new { message = "Only image files (jpg, jpeg, png, gif, webp) are allowed." });

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Invalid content type. Only images are allowed." });

            try
            {
                await using var stream = file.OpenReadStream();
                var url = await _blobService.UploadAsync(stream, file.FileName, file.ContentType);
                return Ok(new { url });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Upload failed.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Delete a previously uploaded blob by URL.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest(new { message = "url query parameter is required." });

            await _blobService.DeleteAsync(url);
            return NoContent();
        }
    }
}
