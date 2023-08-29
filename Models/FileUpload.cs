using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Lab2_ImageService.Models
{
    public class FileUpload
    {
        [Required]
        [DisplayName("File")]
        public IFormFile FormFile { get; set; }

        public string SuccessMessage { get; set; }
    }
}
