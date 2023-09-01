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
        //public async Task<ImageAnalysisViewModel> AnalyzeImageAsync(string imageFile)
        //{
        //    try
        //    {
        //        ComputerVisionClient client = Authenticate();

        //        List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
        //        {
        //                VisualFeatureTypes.Categories,
        //                VisualFeatureTypes.Description,
        //                VisualFeatureTypes.Faces,
        //                VisualFeatureTypes.ImageType,
        //                VisualFeatureTypes.Tags,
        //                VisualFeatureTypes.Adult,
        //                VisualFeatureTypes.Color,
        //                VisualFeatureTypes.Brands,
        //                VisualFeatureTypes.Objects,
        //        };

        //        ImageAnalysis results;

        //        using (Stream imageStream = File.OpenRead(imageFile))
        //        {
        //            results = await client.AnalyzeImageInStreamAsync(imageStream, visualFeatures: features);
        //        }

        //        // Process categories and landmarks
        //        List<LandmarksModel> landmarks = new List<LandmarksModel>();
        //        foreach (var category in results.Categories)
        //        {
        //            if (category.Detail?.Landmarks != null)
        //            {
        //                landmarks.AddRange(category.Detail.Landmarks);
        //            }
        //        }
        //        // Create the view model and set the results
        //        ImageAnalysisViewModel imageAnalysis = new ImageAnalysisViewModel();
        //        imageAnalysis.ImageAnalysisResult = results;
        //        imageAnalysis.Landmarks = landmarks; // You might need to adjust this based on your model structure

        //        return imageAnalysis;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while analyzing the image.");
        //        throw; // Re-throw the exception after logging
        //    }
        //}

        public async Task<ImageAnalysisViewModel> AnalyzeImageAsync(string imageFileOrUrl)
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

                // Check if the input is a URL or a local file path
                if (Uri.IsWellFormedUriString(imageFileOrUrl, UriKind.Absolute))
                {
                    // Input is a URL, download the image from the URL
                    using (Stream imageStream = new WebClient().OpenRead(imageFileOrUrl))
                    {
                        results = await client.AnalyzeImageInStreamAsync(imageStream, visualFeatures: features);
                    }
                }
                else
                {
                    // Input is a local file path, open the image file
                    using (Stream imageStream = File.OpenRead(imageFileOrUrl))
                    {
                        results = await client.AnalyzeImageInStreamAsync(imageStream, visualFeatures: features);
                    }
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


        public async Task GetThumbnail(string imageFileOrUrl, int width, int height)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                Stream imageStream;

                if (Uri.IsWellFormedUriString(imageFileOrUrl, UriKind.Absolute))
                {
                    // ImageFileOrUrl is a valid URL, download the image
                    byte[] imageBytes = await httpClient.GetByteArrayAsync(imageFileOrUrl);
                    imageStream = new MemoryStream(imageBytes);
                }
                else
                {
                    // ImageFileOrUrl is a local file path, open it
                    imageStream = File.OpenRead(imageFileOrUrl);
                }

                // Generate a thumbnail from the image stream
                ComputerVisionClient client = Authenticate();
                Debug.WriteLine("Generating thumbnail");

                // Get thumbnail data
                var thumbnailStream = await client.GenerateThumbnailInStreamAsync(width, height, imageStream, true);

                // Determine the full path to save the thumbnail
                string originalFileName = Path.GetFileNameWithoutExtension(imageFileOrUrl);
                string thumbnailFileName = $"{originalFileName}_thumbnail.png";
                string thumbnailPath = Path.Combine(_hostEnvironment.WebRootPath, "Thumbnails", thumbnailFileName);

                // Create the directory if it doesn't exist
                string thumbnailDirectory = Path.GetDirectoryName(thumbnailPath);

                if (!Directory.Exists(thumbnailDirectory))
                {
                    Directory.CreateDirectory(thumbnailDirectory);
                }

                // Save the thumbnail image
                using (Stream thumbnailFile = File.Create(thumbnailPath))
                {
                    await thumbnailStream.CopyToAsync(thumbnailFile);
                }

                Debug.WriteLine($"Thumbnail saved in {thumbnailPath}");
            }
        }

    }
}
