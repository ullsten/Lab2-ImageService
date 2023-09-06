using Lab2_ImageService.Models.ViewModel;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Lab2_ImageService.Services
{
    public interface IComputerVisionService
    {
        Task<ImageAnalysisViewModel> AnalyzeImageAsync(string imageUrl);
        Task GetThumbnail(string imageFile, int width, int height);
        void DrawBoundingBoxObject(ImageAnalysis analysis, string imageFile);
        void DrawBoundingBoxFace(ImageAnalysis analysis, string imageFile);
        void DrawBoundingBoxObject_Face(ImageAnalysis analysis, string imageFile);
    }
}