using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Lab2_ImageService.Models.ViewModel
{
    public class ObjectDetectionViewModel
    {
        [Url(ErrorMessage = "Invalid URL")]
        public string? ImagePath { get; set; }

        [DisplayName("Upload Image")]
        [DataType(DataType.Upload)]
        public IFormFile ImageFile { get; set; }
        public List<ObjectInfoViewModel>? DetectedObject { get; set; }
    }
}

