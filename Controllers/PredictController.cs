using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using Microsoft.AspNetCore.Http;
using Lab2_ImageService.Models;

namespace Lab2_ImageService.Controllers
{
    public class PredictController : Controller
    {
        private readonly IConfiguration _configuration;

        public PredictController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(ImageUploadModel model)
        {
            try
            {
                string prediction_endpoint = _configuration["PredictionEndpoint"];
                string prediction_key = _configuration["PredictionKey"];
                Guid project_id = Guid.Parse(_configuration["ProjectID"]);
                string model_name = _configuration["ModelName"];

                // Authenticate a client for the prediction API
                CustomVisionPredictionClient prediction_client = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(prediction_key))
                {
                    Endpoint = prediction_endpoint
                };

                Stream imageStream = null;

                if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    // Download the image from the URL
                    using (var client = new HttpClient())
                    {
                        var response = client.GetAsync(model.ImageUrl).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            imageStream = response.Content.ReadAsStreamAsync().Result;
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Failed to download the image from the URL.";
                            return View();
                        }
                    }
                }
                else if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Use the locally uploaded image
                    imageStream = model.ImageFile.OpenReadStream();
                }
                else
                {
                    ViewBag.ErrorMessage = "Please provide a valid image URL or select a valid image file.";
                    return View();
                }

                // Predict from the image stream
                var result = prediction_client.ClassifyImage(project_id, model_name, imageStream);

                // Process and display prediction results
                foreach (var prediction in result.Predictions)
                {
                    if (prediction.Probability > 0.5)
                    {
                        ViewBag.PredictionResult = $"{prediction.TagName} {prediction.TagType} ({prediction.Probability:P1})";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error: " + ex.Message;
            }

            return View();
        }


    }
}

