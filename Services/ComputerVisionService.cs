using Lab2_ImageService.Models.ViewModel;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Diagnostics;
using System.Net;

using System.Drawing;
using Rectangle = System.Drawing.Rectangle;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;
using Microsoft.AspNetCore.Http;
using Lab2_ImageService.Models;

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
            //from appsettings.json
            string subscriptionKey = _configuration["CognitiveServiceKey"];
            string endpoint = _configuration["CognitiveServicesEndpoint"];

            //from env file
            //string subscriptionKey = Environment.GetEnvironmentVariable("COGNETIVE_SERVICE_KEY");
            //string endpoint = Environment.GetEnvironmentVariable("COGNETIVE_SERVICE_ENDPOINT");

            ComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
            {
                Endpoint = endpoint
            };
            return client;
        }

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

                //Draw boxes for objects
                //try
                //{
                //    ProcessImage(results, imageFileOrUrl);
                //}
                //catch (Exception ex)
                //{
                //    // Log the exception for further diagnosis
                //    _logger.LogError("Error generating object with box");
                //}
                // Create the view model and set the results
                ImageAnalysisViewModel imageAnalysis = new ImageAnalysisViewModel();
                imageAnalysis.ImageAnalysisResult = results;
                imageAnalysis.Landmarks = landmarks;
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

        public void ProcessImage(ImageAnalysis analysis, string imageFile)
        {
            // Get objects in the image
            if (analysis.Objects.Count > 0)
            {
                Console.WriteLine("Objects in image:");

                // Prepare image for drawing
                Image image = Image.FromFile(imageFile);
                Graphics graphics = Graphics.FromImage(image);
                Pen pen = new Pen(Color.Cyan, 3);
                Font font = new Font("Arial", 16);
                SolidBrush brush = new SolidBrush(Color.Black);

                foreach (var detectedObject in analysis.Objects)
                {
                    // Print object name
                    Console.WriteLine($" - {detectedObject.ObjectProperty} (confidence: {detectedObject.Confidence.ToString("P")})");

                    // Draw object bounding box
                    var r = detectedObject.Rectangle;
                    Rectangle rect = new Rectangle(r.X, r.Y, r.W, r.H);
                    graphics.DrawRectangle(pen, rect);
                    graphics.DrawString(detectedObject.ObjectProperty, font, brush, r.X, r.Y);
                }

                // Save annotated image with the same name as the input image
                string objectsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "Objects");
                string outputFileName = Path.GetFileNameWithoutExtension(imageFile) + "_object.jpg";
                string outputFilePath = Path.Combine(objectsFolderPath, outputFileName);
                image.Save(outputFilePath);
                Console.WriteLine("Results saved in " + outputFilePath);
            }
        }
    }
}
