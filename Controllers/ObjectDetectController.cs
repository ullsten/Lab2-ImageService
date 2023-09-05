using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using System;
using System.Drawing;
using System.IO;
using Microsoft.Extensions.Hosting;
using Rectangle = System.Drawing.Rectangle;
using Image = System.Drawing.Image;
using Color = System.Drawing.Color;
using Lab2_ImageService.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using Lab2_ImageService.Services;
using Newtonsoft.Json.Linq;

namespace Lab2_ImageService.Controllers
{
    public class ObjectDetectController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IComputerVisionService _computerVisionService;
        public ObjectDetectController(IConfiguration configuration, IWebHostEnvironment hostEnvironment, IComputerVisionService computerVisionService)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
            _computerVisionService = computerVisionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        

        [HttpGet]
        public ActionResult GetObject()
        {
            // Pass the imageUrl to the view
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GetObject(string imageUrl)
        {

            string prediction_endpoint = _configuration["PredictionEndpoint"];
            string prediction_key = _configuration["PredictionKey"];
            Guid project_id = Guid.Parse(_configuration["ProjectID_Object"]);
            string model_name = _configuration["ModelName_Object"];

            // Create the HttpClient
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Prediction-Key", prediction_key);

            // Create the request body
            var requestBody = new { Url = imageUrl };
            var jsonBody = JsonConvert.SerializeObject(requestBody);

            // Set the content type header
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Send the POST request to the Custom Vision endpoint
            var response = await client.PostAsync($"{prediction_endpoint}/customvision/v3.0/Prediction/{project_id}/detect/iterations/{model_name}/url", content);

            // Read the response content
            var responseContent = await response.Content.ReadAsStringAsync();

            // Parse the response JSON
            var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

            //Store inpute image url to use in view
            ViewData["Image"] = imageUrl;



            // Pass the result to the view
            return View(result);
        }



        //_computerVisionService.DrawBoundingBox(result, imageUrl);























        [HttpPost]
        public async Task<IActionResult> DetectObjects(ImageUploadModel model)
        {
            try
            {
                string prediction_endpoint = _configuration["PredictionEndpoint"];
                string prediction_key = _configuration["PredictionKey"];
                Guid project_id = Guid.Parse(_configuration["ProjectID_Object"]);
                string model_name = _configuration["ModelName_Object"];

                // Authenticate a client for the prediction API
                CustomVisionPredictionClient prediction_client = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(prediction_key))
                {
                    Endpoint = prediction_endpoint
                };

                if (model.ImageFile != null)
                {
                    // Check if a file is selected
                    if (model.ImageFile.Length == 0)
                    {
                        ViewBag.ErrorMessage = "Please select a valid image file.";
                        return View("Index");
                    }

                    // Validate the file type and size (optional)
                    if (!model.ImageFile.ContentType.StartsWith("image/") || model.ImageFile.Length > (1024 * 1024)) // 1 MB
                    {
                        ViewBag.ErrorMessage = "Please select a valid image file (max size: 1MB).";
                        return View("Index");
                    }

                    // Handle uploaded image
                    using (Stream stream = model.ImageFile.OpenReadStream())
                    {
                        // Reset the stream position to ensure it starts from the beginning
                        stream.Position = 0;

                        // Make a prediction against the Custom Vision model
                        var result = prediction_client.DetectImage(project_id, model_name, stream);

                        // Prepare image for drawing
                        using (var imageStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(imageStream);
                            using (Image image = Image.FromStream(imageStream))
                            {
                                Graphics graphics = Graphics.FromImage(image);
                                Pen pen = new Pen(Color.Cyan, 3);
                                Font font = new Font("Arial", 16);
                                SolidBrush brush = new SolidBrush(Color.Black);

                                foreach (var prediction in result.Predictions)
                                {
                                    // Draw bounding box around the detected object
                                    int left = Convert.ToInt32(prediction.BoundingBox.Left * image.Width);
                                    int top = Convert.ToInt32(prediction.BoundingBox.Top * image.Height);
                                    int width = Convert.ToInt32(prediction.BoundingBox.Width * image.Width);
                                    int height = Convert.ToInt32(prediction.BoundingBox.Height * image.Height);

                                    Rectangle rect = new Rectangle(left, top, width, height);
                                    graphics.DrawRectangle(pen, rect);
                                    graphics.DrawString(prediction.TagName, font, brush, left, top);
                                }

                                // Save the annotated image
                                string objectsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "ObjectsDetection");
                                string outputFileName = "detected_object.jpg"; // Change to your desired output file name
                                string outputFilePath = Path.Combine(objectsFolderPath, outputFileName);

                                if (!Directory.Exists(objectsFolderPath))
                                {
                                    Directory.CreateDirectory(objectsFolderPath);
                                }

                                image.Save(outputFilePath);
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    // Check if a URL is provided
                    if (!Uri.TryCreate(model.ImageUrl, UriKind.Absolute, out Uri uriResult) ||
                        (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                    {
                        ViewBag.ErrorMessage = "Please provide a valid HTTP/HTTPS image URL.";
                        return View("Index");
                    }

                    // Handle image URL
                    using (HttpClient client = new HttpClient())
                    {
                        // Download the image from the URL
                        var imageBytes = await client.GetByteArrayAsync(model.ImageUrl);

                        using (var stream = new MemoryStream(imageBytes))
                        {
                            // Make a prediction against the Custom Vision model
                            var result = prediction_client.DetectImage(project_id, model_name, stream);

                            // Prepare image for drawing
                            using (var image = Image.FromStream(stream))
                            {
                                Graphics graphics = Graphics.FromImage(image);
                                Pen pen = new Pen(Color.Cyan, 3);
                                Font font = new Font("Arial", 16);
                                SolidBrush brush = new SolidBrush(Color.Black);

                                foreach (var prediction in result.Predictions)
                                {
                                    // Draw bounding box around the detected object
                                    int left = Convert.ToInt32(prediction.BoundingBox.Left * image.Width);
                                    int top = Convert.ToInt32(prediction.BoundingBox.Top * image.Height);
                                    int width = Convert.ToInt32(prediction.BoundingBox.Width * image.Width);
                                    int height = Convert.ToInt32(prediction.BoundingBox.Height * image.Height);

                                    Rectangle rect = new Rectangle(left, top, width, height);
                                    graphics.DrawRectangle(pen, rect);
                                    graphics.DrawString(prediction.TagName, font, brush, left, top);
                                }

                                // Save the annotated image
                                string objectsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "ObjectsDetection");
                                string outputFileName = "detected_object.jpg"; // Change to your desired output file name
                                string outputFilePath = Path.Combine(objectsFolderPath, outputFileName);

                                if (!Directory.Exists(objectsFolderPath))
                                {
                                    Directory.CreateDirectory(objectsFolderPath);
                                }

                                image.Save(outputFilePath);
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Please select a valid image file or provide an image URL.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error: " + ex.Message;
            }

            return View("Index");
        }
    }
}

