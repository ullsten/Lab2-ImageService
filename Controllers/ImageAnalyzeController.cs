using Lab2_ImageService.Models;
using Lab2_ImageService.Models.ViewModel;
using Lab2_ImageService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Web.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using Color = System.Drawing.Color;
using FileUpload = Lab2_ImageService.Models.FileUpload;
using Image = System.Drawing.Image;
using Rectangle = System.Drawing.Rectangle;

namespace Lab2_ImageService.Controllers
{
    public class ImageAnalyzeController : Controller
    {
        private readonly ILogger<ImageAnalyzeController> _logger;
        private readonly IComputerVisionService _computerVisionService;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ImageAnalyzeController(IComputerVisionService computerVisionService, ILogger<ImageAnalyzeController> logger, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _computerVisionService = computerVisionService;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            //Get folders from webRootPath
            string objectsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "Objects");
            string thumbnailsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "Thumbnails");
            string uploadedImagesFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "UploadImages");

            //Create list to hold images
            List<string> objectsImages = Directory.GetFiles(objectsFolderPath).Select(Path.GetFileName).ToList();
            List<string> thumbnailsImages = Directory.GetFiles(thumbnailsFolderPath).Select(Path.GetFileName).ToList();
            List<string> uploadedImages = Directory.GetFiles(uploadedImagesFolderPath).Select(Path.GetFileName).ToList();

            //Pass images to ViewData to use in view
            ViewData["ObjectsImages"] = objectsImages;
            ViewData["ThumbnailsImages"] = thumbnailsImages;
            ViewData["UploadedImages"] = uploadedImages;

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
                // User upload a local image file
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

                // Download the image from the URL and save it locally
                using (HttpClient httpClient = new HttpClient())
                {
                    var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                    var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
                    var localImagePath = Path.Combine(fullPath, fileName);

                    using (var fileStream = new FileStream(localImagePath, FileMode.Create))
                    {
                        await fileStream.WriteAsync(imageBytes, 0, imageBytes.Length);
                    }

                    // Using service to analyze the locally saved image
                    var imageAnalysis = await _computerVisionService.AnalyzeImageAsync(localImagePath);

                    if (imageAnalysis.ImageAnalysisResult != null)
                    {
                        ViewData["ImageAnalysisViewModel"] = imageAnalysis;
                        ViewData["SuccessMessage"] = "Image from URL analyzed successfully";
                        ViewData["ImageUrl"] = imageUrl; // Pass the local image path to the view
                    }

                    // Check if checkbox is checked(true) then create thumbnail
                    try
                    {
                        await _computerVisionService.GetThumbnail(localImagePath, fileUpload.ThumbnailWidth, fileUpload.ThumbnailHeight);
                        ViewData["SuccessMessage"] = "Thumbnail generated successfully from URL: " + imageUrl;
                    }
                    catch (Exception ex)
                    {
                        ViewData["ErrorMessage"] = "Error generating thumbnail: " + ex.Message;
                        // Log the exception for further diagnosis
                        _logger.LogError(ex, "Error generating thumbnail");
                    }
                }
            }

            return View("Index");
        }
    }
}