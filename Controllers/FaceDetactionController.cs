using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lab2_ImageService.Models;

namespace Lab2_ImageService.Controllers
{
    public class FaceDetectionController : Controller
    {
        private readonly IFaceClient _faceClient;
        private readonly string _imageFolderPath;
        

        public FaceDetectionController(IConfiguration configuration )
        {
            // Initialize the Face API client with your credentials from appsettings.json
            string endpoint = configuration["CognitiveServicesEndpoint"];
            string subscriptionKey = configuration["CognitiveServiceKey"];

            var credentials = new ApiKeyServiceClientCredentials(subscriptionKey);
            _faceClient = new FaceClient(credentials) { Endpoint = endpoint };

            // Get the folder path for storing uploaded images
            _imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages");
            Directory.CreateDirectory(_imageFolderPath);

        
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DetectFaces()
        {
            try
            {
                var imageFile = Request.Form.Files["imageFile"];

                if (imageFile != null && imageFile.Length > 0)
                {
                    // Save the uploaded image to a temporary location
                    var imagePath = Path.Combine(_imageFolderPath, imageFile.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Call the face detection method and process the results
                    var result = await DetectFacesInImage(imagePath);

                    // Return the results to the view
                    return View("DetectionResult", result);
                }

                // Handle the case where no file was uploaded
                return View("Error");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during processing
                return View("Error");
            }
        }

        private async Task<FaceDetectionResult> DetectFacesInImage(string imagePath)
        {
            // Detect faces in the uploaded image
            var detectedFaces = await _faceClient.Face.DetectWithStreamAsync(
                new FileStream(imagePath, FileMode.Open),
                returnFaceAttributes: new List<FaceAttributeType>
                {
                    FaceAttributeType.Age,
                    FaceAttributeType.Gender,
                    FaceAttributeType.Smile,
                    FaceAttributeType.Glasses
                });

            // Create a view model to pass the results to the view
            var viewModel = new FaceDetectionResult
            {
                ImagePath = imagePath,
                DetectedFaces = detectedFaces
            };

            return viewModel;
        }
    }
}
