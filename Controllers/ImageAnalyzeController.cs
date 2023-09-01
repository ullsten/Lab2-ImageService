using Lab2_ImageService.Models;
using Lab2_ImageService.Models.ViewModel;
using Lab2_ImageService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Lab2_ImageService.Controllers
{
    public class ImageAnalyzeController : Controller
    {
        private readonly ILogger<ImageAnalyzeController> _logger;
        private readonly IComputerVisionService _computerVisionService;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ImageAnalyzeController(IComputerVisionService computerVisionService, ILogger <ImageAnalyzeController> logger, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _computerVisionService = computerVisionService;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            //show success message after uploading image
            ViewData["SuccessMessage"] = "";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(FileUpload fileUpload)
        {
            // Path to save uploaded images
            string fullPath = Path.Combine(_hostEnvironment.WebRootPath, "UploadImages");

            // Check if directory exists or not, create if needed
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            if (fileUpload.LocalImageFile != null && fileUpload.LocalImageFile.Length > 0)
            {
                // User uploaded a local image file
                var formFile = fileUpload.LocalImageFile;
                var filePath = Path.Combine(fullPath, formFile.FileName);
                ViewData["ImageUrl"] = formFile.FileName;

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }

                // Using service to analyze the image
                var imageAnalysis = await _computerVisionService.AnalyzeImageAsync(filePath);

                if (imageAnalysis.ImageAnalysisResult != null)
                {
                    ViewData["ImageAnalysisViewModel"] = imageAnalysis;
                }

                // Check if checkbox is checked(true) then create thumbnail
                try
                {
                    await _computerVisionService.GetThumbnail(filePath, fileUpload.ThumbnailWidth, fileUpload.ThumbnailHeight);
                    ViewData["SuccessMessage"] = fileUpload.LocalImageFile.FileName + " file uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewData["ErrorMessage"] = "Error generating thumbnail: " + ex.Message;
                    // Log the exception for further diagnosis
                    _logger.LogError(ex, "Error generating thumbnail");
                }
            }
            else if (!string.IsNullOrEmpty(fileUpload.ImageUrl))
            {
                // User provided an image URL
                string imageUrl = fileUpload.ImageUrl;

                // Using service to analyze the image from the URL
                try
                {
                    var imageAnalysis = await _computerVisionService.AnalyzeImageAsync(imageUrl);

                    if (imageAnalysis.ImageAnalysisResult != null)
                    {
                        ViewData["ImageAnalysisViewModel"] = imageAnalysis;
                        ViewData["SuccessMessage"] = "Image from URL analyzed successfully";
                        ViewData["ImageUrl"] = imageUrl; // Pass the URL to the view
                    }
                }
                catch (Exception ex)
                {
                    ViewData["ErrorMessage"] = "Error analyzing image from URL: " + ex.Message;
                    // Log the exception for further diagnosis
                    _logger.LogError(ex, "Error analyzing image from URL");
                }
            }

            return View("Index");
        }

    }
}