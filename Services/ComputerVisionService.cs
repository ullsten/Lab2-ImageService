using Lab2_ImageService.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
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

                //if(results.Objects.Count > 0)
                //{
                //    //Prepare image for drawing
                    
                //}


                return imageAnalysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while analyzing the image.");
                throw; // Re-throw the exception after logging
            }
        }


        //public async Task<ImageAnalysisViewModel> AnalyzeImageUrlAsync(string imageUrl)
        //{
        //    try
        //    {
        //        ComputerVisionClient client = Authenticate();

        //        List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
        //{
        //    VisualFeatureTypes.Categories,
        //    VisualFeatureTypes.Description,
        //    VisualFeatureTypes.Faces,
        //    VisualFeatureTypes.ImageType,
        //    VisualFeatureTypes.Tags,
        //    VisualFeatureTypes.Adult,
        //    VisualFeatureTypes.Color,
        //    VisualFeatureTypes.Brands,
        //    VisualFeatureTypes.Objects,
        //};

        //        ImageAnalysis results;

        //        using (var httpClient = new HttpClient())
        //        {
        //            // Download the image content from the URL
        //            var imageContent = await httpClient.GetByteArrayAsync(imageUrl);

        //            // Analyze the downloaded image content
        //            using (MemoryStream imageStream = new MemoryStream(imageContent))
        //            {
        //                results = await client.AnalyzeImageInStreamAsync(imageStream, visualFeatures: features);
        //            }
        //        }

        //        ImageAnalysisViewModel imageAnalysis = new ImageAnalysisViewModel();
        //        imageAnalysis.ImageAnalysisResult = results;
        //        return imageAnalysis;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while analyzing the image.");
        //        throw; // Re-throw the exception after logging
        //    }
        //}

        public async Task GetThumbnail(string imageFile)
        {
            ComputerVisionClient client = Authenticate();

            Debug.WriteLine("Generating thumbnail");

            // Generate a thumbnail
            using (var imageData = File.OpenRead(imageFile))
            {
                // Get thumbnail data
                var thumbnailStream = await client.GenerateThumbnailInStreamAsync(50, 50, imageData, true);

                // Save thumbnail image
                string thumbnailFileName = "thumbnail.png";
                using (Stream thumbnailFile = File.Create(thumbnailFileName))
                {
                    thumbnailStream.CopyTo(thumbnailFile);
                }

                Console.WriteLine($"Thumbnail saved in {thumbnailFileName}");
            }

        }

        public async Task GenerateThumbnailAsync(string imagePath, Stream thumbnailStream, int size)
        {
            using (var image = Image.Load(imagePath))
            {
                // Calculate new dimensions while maintaining aspect ratio
                var width = size;
                var height = (int)((float)image.Height / image.Width * size);

                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = ResizeMode.Max
                }));

                image.Save(thumbnailStream, new JpegEncoder()); // Save as JPEG
            }
        }
    }
}
