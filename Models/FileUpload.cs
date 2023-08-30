using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Lab2_ImageService.Models
{
    public class FileUpload
    {
        [Required]
        [DisplayName("File")]
        public IFormFile FormFile { get; set; }

        [DisplayName("Width")]
        public int ThumbnailWidth { get; set; }

        [DisplayName("Height")]
        public int ThumbnailHeight { get; set; }

        public bool CreateThumbnail { get; set; }

        public string SuccessMessage { get; set; }
    }
}
