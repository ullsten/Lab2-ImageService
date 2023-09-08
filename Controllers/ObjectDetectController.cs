using Lab2_ImageService.Models;
using Lab2_ImageService.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using System.Diagnostics;
using System.Drawing;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;
using Rectangle = System.Drawing.Rectangle;

namespace Lab2_ImageService.Controllers
{
    public class ObjectDetectController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ICustomVisionPredictionClient _predictionClient;
        private readonly ILogger<ObjectDetectController> _logger;
        public ObjectDetectController(
            IConfiguration configuration, 
            IWebHostEnvironment hostEnvironment, 
            ICustomVisionPredictionClient predictionClient, 
            ILogger<ObjectDetectController> logger)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
            _predictionClient = predictionClient;
            _logger = logger;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

  
        public IActionResult FindObject()
        {
            var viewModel = new ObjectDetectionViewModel();

            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> FindObject(string imagePath, IFormFile imageFile)
        {
            try
            {
                Guid project_id = Guid.Parse(_configuration["ProjectID_Object"]);
                string model_name = _configuration["ModelName_Object"];

                byte[] imageData;

                if (!string.IsNullOrEmpty(imagePath))
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        imageData = await httpClient.GetByteArrayAsync(imagePath);
                    }
                }
                else if (imageFile != null && imageFile.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(stream);
                        imageData = stream.ToArray();
                    }
                }
                else
                {
                    // Handle the case where neither imagePath nor imageFile is provided.
                    // You can return a custom view with an error message.
                    var noObjectsViewModel = new ObjectDetectionViewModel
                    {
                        DetectedObject = new List<ObjectInfoViewModel>(),
                        NoObjectsDetected = true // Add a flag to indicate no objects were detected
                    };

                    // Return the "FindObject" view with the noObjectsViewModel
                    return View("FindObject", noObjectsViewModel);
                }

                // Create a new MemoryStream for object detection
                using (MemoryStream imageStream = new MemoryStream(imageData))
                {
                    var result = _predictionClient.DetectImage(project_id, model_name, imageStream);

                    // Create a new MemoryStream for creating the Image object
                    using (MemoryStream imageStreamForDrawing = new MemoryStream(imageData))
                    {

                        // Check if no objects were detected
                        if (result.Predictions == null || !result.Predictions.Any())
                        {
                            var noObjectsViewModel = new ObjectDetectionViewModel
                            {
                                DetectedObject = new List<ObjectInfoViewModel>(),
                                NoObjectsDetected = true // Add a flag to indicate no objects were detected
                            };

                            // Return a custom view for the case of no detected objects
                            return View("NoObjectsDetected", noObjectsViewModel);
                        }
                        // Load the image for drawing
                        Image image = Image.FromStream(imageStreamForDrawing);
                        int h = image.Height;
                        int w = image.Width;
                        Graphics graphics = Graphics.FromImage(image);
                        Pen pen = new Pen(Color.Magenta, 3);
                        Font font = new Font("Arial", 16);
                        SolidBrush brush = new SolidBrush(Color.Black);

                        var viewModel = new ObjectDetectionViewModel
                        {
                            DetectedObject = new List<ObjectInfoViewModel>(),
                        };

                        foreach (var prediction in result.Predictions)
                        {
                            if (prediction.Probability > 0.5)
                            {
                                var objectInfo = new ObjectInfoViewModel
                                {
                                    Tag = prediction.TagName,
                                    TagType = prediction.TagType,
                                    Probability = prediction.Probability,
                                };

                                viewModel.DetectedObject.Add(objectInfo);

                                int left = Convert.ToInt32(prediction.BoundingBox.Left * w);
                                int top = Convert.ToInt32(prediction.BoundingBox.Top * h);
                                int height = Convert.ToInt32(prediction.BoundingBox.Height * h);
                                int width = Convert.ToInt32(prediction.BoundingBox.Width * w);

                                Rectangle rect = new Rectangle(left, top, width, height);
                                graphics.DrawRectangle(pen, rect);

                                graphics.DrawString(prediction.TagName, font, brush, left, top);
                            }
                        }

                        string objectsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "Object_detection18");
                        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // Generate a timestamp

                        string outputFileName;
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            // For images from URL, use a timestamp as the filename
                            outputFileName = $"{timestamp}_object.jpg";

                            ViewData["InputImage"] = outputFileName;
                        }
                        else
                        {
                            // For local image files, use the original filename with a timestamp
                            string originalFileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                            outputFileName = $"{timestamp}_{originalFileName}_object.jpg";

                            ViewData["InputImage"] = outputFileName;
                        }

                        string outputFilePath = Path.Combine(objectsFolderPath, outputFileName);

                        if (!Directory.Exists(objectsFolderPath))
                        {
                            Directory.CreateDirectory(objectsFolderPath);
                        }
                        image.Save(outputFilePath);

                        // Store the input image path or URL in ViewData
                        ViewData["InputImagePath"] = imagePath;

                        return View("FindObject", viewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                return View("NoObjectsDetected");
            }
        }



    }
}

