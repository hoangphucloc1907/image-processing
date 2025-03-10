﻿
namespace ImageProcessing.Models
{
    public class ImageVariant
    {
        public int Id { get; set; }
        public string OriginalFileName { get; set; }
        public string FilePath { get; set; }
        public int VariantTypeId { get; set; }
        public VariantType VariantType { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Quality { get; set; }
    }

    
}
