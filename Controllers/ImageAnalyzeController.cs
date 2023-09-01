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
            //path to save uploaded images
            string fullPath = Path.Combine(_hostEnvironment.WebRootPath, "UploadImages");

            //CHeck if directory exists or not, create if needed
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            var formFile = fileUpload.FormFile;

            if (formFile.Length > 0)
            {
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

                //not working inside if statement ??
               // await _computerVisionService.GetThumbnail(filePath, fileUpload.ThumbnailWidth, fileUpload.ThumbnailHeight);
                // Check if checkbox is checked(true) then create thumbnail
                try
                {
                    await _computerVisionService.GetThumbnail(filePath, fileUpload.ThumbnailWidth, fileUpload.ThumbnailHeight);
                    ViewData["SuccessMessage"] = fileUpload.FormFile.FileName.ToString() + " file uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewData["ErrorMessage"] = "Error generating thumbnail: " + ex.Message;
                    // Log the exception for further diagnosis
                    _logger.LogError(ex, "Error generating thumbnail");
                }



                ViewData["SuccessMessage"] = fileUpload.FormFile.FileName.ToString() + " file uploaded successfully";
            }

            return View("Index");
        }
    }
}