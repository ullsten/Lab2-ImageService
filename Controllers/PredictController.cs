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
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(ImageUploadModel model)
        {
            try
            {
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Get Configuration Settings 
                    IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                    IConfigurationRoot configuration = builder.Build();
                    string prediction_endpoint = configuration["PredictionEndpoint"];
                    string prediction_key = configuration["PredictionKey"];
                    Guid project_id = Guid.Parse(configuration["ProjectID"]);
                   
                    string model_name = configuration["ModelName"];

                    // Authenticate a client for the prediction API
                    CustomVisionPredictionClient prediction_client = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(prediction_key))
                    {
                        Endpoint = prediction_endpoint
                    };

                    // Read and classify the uploaded image
                    using (var stream = model.ImageFile.OpenReadStream())
                    {
                        var result = prediction_client.ClassifyImage(project_id, model_name, stream);

                        // Process and display prediction results
                        // You can modify this part to display the results on the view.
                        foreach (var prediction in result.Predictions)
                        {
                            if (prediction.Probability > 0.5)
                            {
                                ViewBag.PredictionResult = $"{prediction.TagName} ({prediction.Probability:P1})";
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Please select a valid image file.";
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

