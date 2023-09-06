using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Lab2_ImageService.Models
{
    public class FileUpload
    {
        // Local image file upload (optional)
        [DisplayName("Local Image File")]
        public IFormFile LocalImageFile { get; set; }

        // Image URL input (optional)
        [DisplayName("Image URL")]
        public string? ImageUrl { get; set; }
        public string? ThumbnailFileName { get; set; }

        public bool CreateThumbnail { get; set; }
        public bool CreateObjectBox { get; set; }
        public bool CreateFaceBox { get; set; }
        public bool CreateAll { get; set; }
        public int ThumbnailWidth { get; set; } = 50;
        public int ThumbnailHeight { get; set; } = 50;

        public string? SuccessMessage { get; set; }
    }
}
