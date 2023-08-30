using Lab2_ImageService.Models.ViewModel;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Lab2_ImageService.Services
{
    public interface IComputerVisionService
    {
        Task<ImageAnalysisViewModel> AnalyzeImageAsync(string imageUrl);
        Task GenerateThumbnailAsync(string imagePath, Stream thumbnailStream, int size);
        //Task<ImageAnalysisViewModel> AnalyzeImageUrlAsync(string imageUrl);
    }
}
