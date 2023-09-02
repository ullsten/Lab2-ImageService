using Lab2_ImageService.Models;
using Lab2_ImageService.Models.ViewModel;
using Lab2_ImageService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


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
            try
            {
                //Get folders from webRootPath
                string objectsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "Objects");
                string thumbnailsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "Thumbnails");
                string uploadedImagesFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "UploadedImages");

                // Create the folders if they don't exist
                Directory.CreateDirectory(objectsFolderPath);
                Directory.CreateDirectory(thumbnailsFolderPath);
                Directory.CreateDirectory(uploadedImagesFolderPath);

                // Create list to hold images
                List<string> objectsImages = Directory.GetFiles(objectsFolderPath).Select(Path.GetFileName).ToList();
                List<string> thumbnailsImages = Directory.GetFiles(thumbnailsFolderPath).Select(Path.GetFileName).ToList();
                List<string> uploadedImages = Directory.GetFiles(uploadedImagesFolderPath).Select(Path.GetFileName).ToList();

                // Pass images to ViewData to use in view
                ViewData["ObjectsImages"] = objectsImages;
                ViewData["ThumbnailsImages"] = thumbnailsImages;
                ViewData["UploadedImages"] = uploadedImages;

                // Show success message after uploading image
                ViewData["SuccessMessage"] = "";

                return View();
            }
            catch (Exception ex)
            {
                // Handle the exception here or log it for debugging purposes
                // You can also display an error message to the user if needed
                ViewData["ErrorMessage"] = "An error occurred while retrieving image data: " + ex.Message;

                return View();
            }
        }



        [HttpPost]
        public async Task<IActionResult> UploadImage(FileUpload fileUpload)
        {
            // Path to save uploaded images
            string fullPath = Path.Combine(_hostEnvironment.WebRootPath, "UploadedImages");

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

                // Check if checkbox is checked(true)
                if (fileUpload.CreateThumbnail)
                {
                    // Create thumbnail here if checkbox is checked
                    await _computerVisionService.GetThumbnail(filePath, fileUpload.ThumbnailWidth, fileUpload.ThumbnailHeight);
                    ViewData["SuccessMessage"] = fileUpload.LocalImageFile.FileName + " file uploaded successfully with a thumbnail";
                }
                else
                {
                    ViewData["SuccessMessage"] = fileUpload.LocalImageFile.FileName + " file uploaded successfully without a thumbnail";
                }
                Debug.WriteLine(fileUpload.CreateThumbnail + " Hello from checkbox Local_IMG");
                Debug.WriteLine(imageAnalysis.AddThumbnail + " Hello from checkbox URL");

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

                    // Check if checkbox is checked(true)
                    if (imageAnalysis.AddThumbnail)
                    {
                        // Create thumbnail here if checkbox is checked
                        await _computerVisionService.GetThumbnail(localImagePath, fileUpload.ThumbnailWidth, fileUpload.ThumbnailHeight);
                        ViewData["SuccessMessage"] = fileUpload.LocalImageFile.FileName + " file uploaded successfully with a thumbnail";
                    }
                    else
                    {
                        ViewData["SuccessMessage"] = fileUpload.LocalImageFile.FileName + " file uploaded successfully without a thumbnail";
                    }
                    Debug.WriteLine(fileUpload.CreateThumbnail + " Hello from checkbox URL");
                    Debug.WriteLine(imageAnalysis.AddThumbnail + " Hello from checkbox URL");

                }
            }

            //did not need this before, but now and I don´t know why
            // Repopulate the dropdown lists in ViewData
            string objectsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "Objects");
            string thumbnailsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "Thumbnails");
            string uploadedImagesFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "UploadedImages");

            List<string> objectsImages = Directory.GetFiles(objectsFolderPath).Select(Path.GetFileName).ToList();
            List<string> thumbnailsImages = Directory.GetFiles(thumbnailsFolderPath).Select(Path.GetFileName).ToList();
            List<string> uploadedImages = Directory.GetFiles(uploadedImagesFolderPath).Select(Path.GetFileName).ToList();

            ViewData["ObjectsImages"] = objectsImages;
            ViewData["ThumbnailsImages"] = thumbnailsImages;
            ViewData["UploadedImages"] = uploadedImages;

            return View("Index");
        }
    }
}