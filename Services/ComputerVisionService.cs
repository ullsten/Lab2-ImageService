using Lab2_ImageService.Models.ViewModel;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lab2_ImageService.Services
{
    public class ComputerVisionService : IComputerVisionService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ComputerVisionService> _logger;

        public ComputerVisionService(IConfiguration configuration, ILogger<ComputerVisionService> logger)
        {
            _configuration = configuration;
            _logger = logger;
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


        public async Task<ImageAnalysisViewModel> AnalyzeImageUrlAsync(string imageUrl)
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

                using (var httpClient = new HttpClient())
                {
                    // Download the image content from the URL
                    var imageContent = await httpClient.GetByteArrayAsync(imageUrl);

                    // Analyze the downloaded image content
                    using (MemoryStream imageStream = new MemoryStream(imageContent))
                    {
                        results = await client.AnalyzeImageInStreamAsync(imageStream, visualFeatures: features);
                    }
                }

                ImageAnalysisViewModel imageAnalysis = new ImageAnalysisViewModel();
                imageAnalysis.ImageAnalysisResult = results;
                return imageAnalysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while analyzing the image.");
                throw; // Re-throw the exception after logging
            }
        }

    }
}
