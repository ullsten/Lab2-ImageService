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
    }
}

