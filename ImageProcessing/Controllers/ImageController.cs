using ImageProcessing.Models;
using ImageProcessing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ImageSharpImage = SixLabors.ImageSharp.Image;


namespace ImageProcessing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageProcessingService _processingService;
        private readonly ImageDbContext _dbContext;
        private readonly ProducerService _producer;
        private readonly ILogger<ImageController> _logger;

        public ImageController(
            IImageProcessingService processingService,
            ImageDbContext dbContext,
            ProducerService producer,
            ILogger<ImageController> logger)
        {
            _processingService = processingService;
            _dbContext = dbContext;
            _producer = producer;
            _logger = logger;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            try
            {
                var directory = Path.Combine("wwwroot", "uploads");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var filePath = Path.Combine(directory, file.FileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var image = new Models.Image
                {
                    FileName = file.FileName,
                    OriginalPath = filePath,
                    UploadedAt = DateTime.UtcNow
                };

                _dbContext.Images.Add(image);
                await _dbContext.SaveChangesAsync();

                // Trigger image processing
                await _processingService.ProcessImageAsync(image);

                // Send message to Kafka
                await _producer.SendMessageAsync(image.Id.ToString(), image);

                return Ok(new { image.Id, filePath });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during image upload: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{imageName}/{variantName}")]
        public async Task<IActionResult> GetImageVariant(string imageName, string variantName)
        {
            try
            {
                var variantPath = await _processingService.GetVariantPathAsync(imageName, variantName);

                if (variantPath == null)
                {
                    _logger.LogWarning($"Variant '{variantName}' not found for Image '{imageName}'.");
                    return NotFound("Variant not found.");
                }

                return PhysicalFile(variantPath, "image/webp");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving variant '{variantName}' for image '{imageName}': {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
