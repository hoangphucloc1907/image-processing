using ImageProcessing.Models;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ImageSharpImage = SixLabors.ImageSharp.Image;


namespace ImageProcessing.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly IMemoryCache _cache;
        private readonly ImageDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public ImageProcessingService(IMemoryCache cache, ImageDbContext dbContext, IConfiguration configuration)
        {
            _cache = cache;
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task ProcessImageAsync(Models.Image image)
        {
            var variantTypes = await _dbContext.VariantTypes.ToListAsync();

            foreach (var variantType in variantTypes)
            {
                string variantPath = await GenerateVariantAsync(image, variantType.Name, variantType.MaxSize, 80);

                var variant = new ImageVariant
                {
                    ImageId = image.Id,
                    VariantTypeId = variantType.Id,
                    Path = variantPath,
                    Format = "webp",
                    Width = variantType.MaxSize,
                    Quality = 80
                };

                _dbContext.ImageVariants.Add(variant);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<string> GenerateVariantAsync(Models.Image image, string variantType, int maxSize, int quality)
        {
            string variantPath = Path.Combine(_configuration["ImageSettings:VariantPath"], $"{image.Id}_{variantType}.webp");

            using (var img = ImageSharpImage.Load(image.OriginalPath))
            {
                img.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(maxSize, maxSize)
                }));

                await img.SaveAsync(variantPath, new WebpEncoder { Quality = quality });
            }

            return variantPath;
        }

        public async Task<string?> GetVariantPathAsync(string imageName, string variantName)
        {
            string cacheKey = $"variant_{imageName}_{variantName}";

            if (_cache.TryGetValue(cacheKey, out string? cachedPath))
            {
                return cachedPath;
            }

            var image = await _dbContext.Images
                .Where(i => i.OriginalPath.Contains(imageName))
                .FirstOrDefaultAsync();
            if (image == null) return null;

            var variant = await _dbContext.ImageVariants
                .Include(v => v.VariantType)
                .Where(v => v.ImageId == image.Id && v.VariantType.Name == variantName)
                .FirstOrDefaultAsync();

            if (variant == null) return null;

            _cache.Set(cacheKey, variant.Path, TimeSpan.FromMinutes(10));

            return variant.Path;
        }
        public async Task ProcessAndStoreVariantsAsync(Models.Image image)
        {
            await ProcessImageAsync(image);
        }

    }
}
