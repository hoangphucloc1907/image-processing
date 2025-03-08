using ImageProcessing.Models;
using ImageProcessing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageProcessingService _processingService;
        private readonly ImageDbContext _dbContext;
        public ImageController(IImageProcessingService processingService, ImageDbContext dbContext)
        {
            _processingService = processingService;
            _dbContext = dbContext;
        }
        [HttpPost("upload")]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            using var stream = file.OpenReadStream();
            var variants = new List<ImageVariant>();

            foreach (VariantType type in Enum.GetValues(typeof(VariantType)))
            {
                stream.Position = 0;
                var variant = await _processingService.GenerateVariantAsync(stream, file.FileName, type);
                variants.Add(variant);
            }

            return Ok(variants.Select(v => new
            {
                v.Id,
                v.Type,
                v.Width,
                v.Height,
                v.FilePath,
                v.Format,
                v.FileSize
            }));
        }

        [HttpGet("variant/{originalFileName}/{type}")]
        public async Task<IActionResult> GetVariant(string originalFileName, VariantType type)
        {
            var variant = await _dbContext.ImageVariants
                .FirstOrDefaultAsync(v => v.OriginalFileName == originalFileName && v.Type == type);

            if (variant == null)
                return NotFound("No variant found for the original file and requested type.");

            var fileStream = System.IO.File.OpenRead(variant.FilePath);
            return File(fileStream, $"image/{variant.Format.TrimStart('.')}");
        }
    }
}
