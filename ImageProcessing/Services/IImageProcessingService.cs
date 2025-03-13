using ImageProcessing.Models;

namespace ImageProcessing.Services
{
    public interface IImageProcessingService
    {
        Task ProcessImageAsync(Models.Image image);
        Task<string> GenerateVariantAsync(Models.Image image, string variantType, int maxSize, int quality);
        Task<string?> GetVariantPathAsync(string imageName, string variantName);
        Task ProcessAndStoreVariantsAsync(Models.Image image);
    }
}
