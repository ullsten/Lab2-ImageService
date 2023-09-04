using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Lab2_ImageService.Models.ViewModel
{
    public class ImageAnalysisViewModel
    {
        public IFormFile FormFile { get; set; }
        public ImageAnalysis ImageAnalysisResult { get; set; }
        public List<LandmarksModel> Landmarks { get; set; }
        public bool CreateObjectBox { get; set; }

    }
}
