using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Lab2_ImageService.Models
{
    public class FileUpload
    {
        [Required]
        [DisplayName("File")]
        public IFormFile FormFile { get; set; }

        public bool CreateThumbnail { get; set; }
        public int ThumbnailWidth { get; set; }

        public int ThumbnailHeight { get; set; }

        public string SuccessMessage { get; set; }
    }
}