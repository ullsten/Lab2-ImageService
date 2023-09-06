using Lab2_ImageService.Models;
using Lab2_ImageService.Models.ViewModel;
using Lab2_ImageService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Globalization;

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

        public IActionResult Index(int page = 1, int pageSize = 5)
        {
            try
            {
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

                // Generate a unique filename with a timestamp
                string timestampedFileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "_" + formFile.FileName;

                var filePath = Path.Combine(fullPath, timestampedFileName);
                ViewData["ImageUrl"] = timestampedFileName;

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

                // Define an array of options (checkboxes) and their corresponding actions
                var options = new List<(bool, Action)>
                {
                    (fileUpload.CreateAll, () =>
                    {
                        _computerVisionService.GetThumbnail(filePath, fileUpload.ThumbnailWidth, fileUpload.ThumbnailHeight);
                        _computerVisionService.DrawBoundingBoxObject_Face(imageAnalysis.ImageAnalysisResult, filePath);
                    }),
                        (fileUpload.CreateThumbnail, () => _computerVisionService.GetThumbnail(filePath, fileUpload.ThumbnailWidth, fileUpload.ThumbnailHeight)),
                        (fileUpload.CreateObjectBox, () => _computerVisionService.DrawBoundingBoxObject(imageAnalysis.ImageAnalysisResult, filePath)),
                        (fileUpload.CreateFaceBox, () => _computerVisionService.DrawBoundingBoxFace(imageAnalysis.ImageAnalysisResult, filePath))
                };

                // Check each option and execute the corresponding action if it's true
                var selectedActions = options.Where(option => option.Item1).Select(option => option.Item2).ToList();

                // Check if any actions were selected and perform them
                if (selectedActions.Any())
                {
                    foreach (var action in selectedActions)
                    {
                        action.Invoke();
                    }

                    // Build a success message based on the selected actions
                    var successMessage = fileUpload.LocalImageFile.FileName + " file uploaded with";

                    if (fileUpload.CreateThumbnail)
                    {
                        successMessage += " thumbnail created";
                    }

                    if (fileUpload.CreateObjectBox || fileUpload.CreateFaceBox)
                    {
                        if (fileUpload.CreateThumbnail)
                        {
                            successMessage += " and";
                        }
                        successMessage += " bounding boxes drawn";
                    }

                    successMessage += " successfully.";

                    ViewData["SuccessMessage"] = successMessage;
                }
                else
                {
                    ViewData["SuccessMessage"] = fileUpload.LocalImageFile.FileName + " file image analyzed successfully (without thumbnail and bounding box).";
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

                    // Generate a unique filename with a timestamp
                    string timestampedFileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "_" + Path.GetFileName(new Uri(imageUrl).LocalPath);
                    var localImagePath = Path.Combine(fullPath, timestampedFileName);

                    using (var fileStream = new FileStream(localImagePath, FileMode.Create))
                    {
                        await fileStream.WriteAsync(imageBytes, 0, imageBytes.Length);
                    }

                    // Create an ImageModel to hold image information
                    //var imageModel = new ImageModel
                    //{
                    //    FileName = timestampedFileName,
                    //    FilePath = localImagePath,
                    //};

                    // Using service to analyze the locally saved image
                    var imageAnalysis = await _computerVisionService.AnalyzeImageAsync(localImagePath);

                    if (imageAnalysis.ImageAnalysisResult != null)
                    {
                        ViewData["ImageAnalysisViewModel"] = imageAnalysis;
                        ViewData["SuccessMessage"] = "Image from URL analyzed successfully";
                        ViewData["ImageUrl"] = timestampedFileName; // Pass the timestamped image URL to the view
                    }

                    // Define an array of options (checkboxes) and their corresponding actions
                    var options = new List<(bool, Action)>
                {
                    (fileUpload.CreateAll, () =>
                    {
                        _computerVisionService.GetThumbnail(localImagePath, fileUpload.ThumbnailWidth, fileUpload.ThumbnailHeight);
                        _computerVisionService.DrawBoundingBoxObject_Face(imageAnalysis.ImageAnalysisResult, localImagePath);
                    }),
                        (fileUpload.CreateThumbnail, () => _computerVisionService.GetThumbnail(localImagePath, fileUpload.ThumbnailWidth, fileUpload.ThumbnailHeight)),
                        (fileUpload.CreateObjectBox, () => _computerVisionService.DrawBoundingBoxObject(imageAnalysis.ImageAnalysisResult, localImagePath)),
                        (fileUpload.CreateFaceBox, () => _computerVisionService.DrawBoundingBoxFace(imageAnalysis.ImageAnalysisResult, localImagePath))
                };

                    // Check each option and execute the corresponding action if it's true
                    var selectedActions = options.Where(option => option.Item1).Select(option => option.Item2).ToList();

                    // Check if any actions were selected and perform them
                    if (selectedActions.Any())
                    {
                        foreach (var action in selectedActions)
                        {
                            action.Invoke();
                        }

                        // Build a success message based on the selected actions
                        var successMessage = " file uploaded with";

                        if (fileUpload.CreateThumbnail)
                        {
                            successMessage += " thumbnail created";
                        }

                        if (fileUpload.CreateObjectBox || fileUpload.CreateFaceBox)
                        {
                            if (fileUpload.CreateThumbnail)
                            {
                                successMessage += " and";
                            }
                            successMessage += " bounding boxes drawn";
                        }

                        successMessage += " successfully.";

                        ViewData["SuccessMessage"] = successMessage;
                    }
                    else
                    {
                        ViewData["SuccessMessage"] = " file image analyzed successfully (without thumbnail and bounding box).";
                    }
                }
            }

            return View("Index");
        }

        [HttpGet]
        public IActionResult AnalyzedImages()
        {
            // Get folders from webRootPath
            string objectsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "Objects");
            string thumbnailsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "Thumbnails");
            string uploadedImagesFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "UploadedImages");

            // Create the folders if they don't exist
            Directory.CreateDirectory(objectsFolderPath);
            Directory.CreateDirectory(thumbnailsFolderPath);
            Directory.CreateDirectory(uploadedImagesFolderPath);

            // Create a list to hold ImageModel objects
            List<ImageModel> objectsImages = Directory.GetFiles(objectsFolderPath)
                .Select(filePath => new ImageModel
                {
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath,
                    UploadedTimestamp = new FileInfo(filePath).CreationTime,
                })
                .OrderByDescending(image => ExtractDateFromFileName(image.FileName))
                .ToList();

            List<ImageModel> thumbnailsImages = Directory.GetFiles(thumbnailsFolderPath)
                .Select(filePath => new ImageModel
                {
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath,
                    UploadedTimestamp = new FileInfo(filePath).CreationTime,
                })
                .OrderByDescending(image => ExtractDateFromFileName(image.FileName))
                .ToList();

            // Sort the uploadedImages list by parsing the date from the FileName
            List<ImageModel> uploadedImages = Directory.GetFiles(uploadedImagesFolderPath)
                .Select(filePath => new ImageModel
                {
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath,
                    UploadedTimestamp = new FileInfo(filePath).CreationTime,
                })
                .OrderByDescending(image => ExtractDateFromFileName(image.FileName))
                .ToList();

            // Create a list to hold the latest analyzed images as tuples (image, category)
            var latestAnalyzedImages = new List<(ImageModel Image, string Category)>();

            // Add the latest uploaded image, if available
            if (uploadedImages.Count > 0)
            {
                latestAnalyzedImages.Add((uploadedImages.First(), "Uploaded"));
            }

            // Add the latest thumbnail image, if available
            if (thumbnailsImages.Count > 0)
            {
                latestAnalyzedImages.Add((thumbnailsImages.First(), "Thumbnail"));
            }

            // Add the latest object image, if available
            if (objectsImages.Count > 0)
            {
                latestAnalyzedImages.Add((objectsImages.First(), "Object"));
            }

            // Pass the latest analyzed images to ViewData
            ViewData["LatestAnalyzedImages"] = latestAnalyzedImages;

            // Pass ImageModel lists to ViewData to use in the view
            ViewData["ObjectsImages"] = objectsImages;
            ViewData["ThumbnailsImages"] = thumbnailsImages;
            ViewData["UploadedImages"] = uploadedImages;

            // Pass the latest analyzed images to ViewData
            ViewData["LatestAnalyzedImages"] = latestAnalyzedImages;

            return View();
        }

        // Custom function to extract and parse the date from the FileName
        DateTime ExtractDateFromFileName(string fileName)
        {
            var datePart = fileName.Split('_')[0]; // Assuming the date part is before the underscore
            if (DateTime.TryParseExact(datePart, "yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return parsedDate;
            }
            // Return a default date or handle the error as needed
            return DateTime.MinValue;
        }

    }
}