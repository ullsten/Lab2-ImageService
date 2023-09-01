using Lab2_ImageService.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Image = SixLabors.ImageSharp.Image;

namespace Lab2_ImageService.Services
{
    public class ComputerVisionService : IComputerVisionService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ComputerVisionService> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ComputerVisionService(IConfiguration configuration, ILogger<ComputerVisionService> logger, IWebHostEnvironment hostEnvironment)
        {
            _configuration = configuration;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        //Sets subscriptionKey and endpoint
        public ComputerVisionClient Authenticate()
        {
            string subscriptionKey = _configuration["CognitiveServiceKey"];
            string endpoint = _configuration["CognitiveServicesEndpoint"];

            ComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
            {
                Endpoint = endpoint
            };
            return client;
        }
        public async Task<ImageAnalysisViewModel> AnalyzeImageAsync(string imageFile)
        {
            try
            {
                ComputerVisionClient client = Authenticate();

                List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
                {
                        VisualFeatureTypes.Categories,
                        VisualFeatureTypes.Description,
                        VisualFeatureTypes.Faces,
                        VisualFeatureTypes.ImageType,
                        VisualFeatureTypes.Tags,
                        VisualFeatureTypes.Adult,
                        VisualFeatureTypes.Color,
                        VisualFeatureTypes.Brands,
                        VisualFeatureTypes.Objects,
                };

                ImageAnalysis results;

                using (Stream imageStream = File.OpenRead(imageFile))
                {
                    results = await client.AnalyzeImageInStreamAsync(imageStream, visualFeatures: features);
                }

                // Process categories and landmarks
                List<LandmarksModel> landmarks = new List<LandmarksModel>();
                foreach (var category in results.Categories)
                {
                    if (category.Detail?.Landmarks != null)
                    {
                        landmarks.AddRange(category.Detail.Landmarks);
                    }
                }
                // Create the view model and set the results
                ImageAnalysisViewModel imageAnalysis = new ImageAnalysisViewModel();
                imageAnalysis.ImageAnalysisResult = results;
                imageAnalysis.Landmarks = landmarks; // You might need to adjust this based on your model structure

                return imageAnalysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while analyzing the image.");
                throw; // Re-throw the exception after logging
            }
        }

        public async Task GetThumbnail(string imageFile, int width, int height)
        {
            ComputerVisionClient client = Authenticate();

            Debug.WriteLine("Generating thumbnail");

            // Generate a thumbnail
            using (var imageData = File.OpenRead(imageFile))
            {
                // Get thumbnail data
                var thumbnailStream = await client.GenerateThumbnailInStreamAsync(width, height, imageData, true);

                // Determine the full path to save the thumbnail
                string originalFileName = Path.GetFileNameWithoutExtension(imageFile); // Get the original file name without extension
                string thumbnailFileName = $"{originalFileName}_thumbnail.png"; // Add "_thumbnail" to the original file name
                string thumbnailPath = Path.Combine(_hostEnvironment.WebRootPath, "Thumbnails", thumbnailFileName);

                // Create the directory if it doesn't exist
                string thumbnailDirectory = Path.GetDirectoryName(thumbnailPath);

                if (!Directory.Exists(thumbnailDirectory))
                {
                    Directory.CreateDirectory(thumbnailDirectory);
                }

                // Save thumbnail image
                using (Stream thumbnailFile = File.Create(thumbnailPath))
                {
                    await thumbnailStream.CopyToAsync(thumbnailFile);
                }

                Debug.WriteLine($"Thumbnail saved in {thumbnailPath}");
            }
        }
    }
}
