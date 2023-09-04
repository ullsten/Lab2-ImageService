using Microsoft.AspNetCore.Http;

namespace Lab2_ImageService.Models
{
    public class ImageUploadModel
    {
        public IFormFile ImageFile { get; set; }
        public string ImageUrl { get; set; }
    }
}
