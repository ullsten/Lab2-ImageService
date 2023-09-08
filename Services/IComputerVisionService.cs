using Lab2_ImageService.Models.ViewModel;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Lab2_ImageService.Services
{
    public interface IComputerVisionService
    {
        Task<ImageAnalysisViewModel> AnalyzeImageAsync(string imageUrl);
        Task GetThumbnail(string imageFile, int width, int height);
        void DrawBoundingBoxes(ImageAnalysis analysis, string imageFile);
    }
}