using Lab2_ImageService.Models;
using Lab2_ImageService.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;

namespace Lab2_ImageService.Controllers
{
    public class ImgController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        public ImgController(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

     
        public IActionResult Index()
        {
            ImageClass ic = new ImageClass();
            var displayImg = Path.Combine(_hostEnvironment.WebRootPath, "UploadedImages");
            DirectoryInfo di = new DirectoryInfo(displayImg);
            FileInfo[] uploadInfo = di.GetFiles();
            ic.FileImage = uploadInfo;

            ImageClass thumb = new ImageClass();
            var displayThumbs = Path.Combine(_hostEnvironment.WebRootPath, "Thumbnails");
            DirectoryInfo th = new DirectoryInfo(displayThumbs);
            FileInfo[] thumbInfo = th.GetFiles();
            thumb.FileImage = thumbInfo;

            ImageClass objects = new ImageClass();
            var displayObject = Path.Combine(_hostEnvironment.WebRootPath, "Objects");
            DirectoryInfo obj = new DirectoryInfo(displayObject);
            FileInfo[] fileInfos = obj.GetFiles();
            objects.FileImage = fileInfos;

            ViewData["Uploaded_Images"] = ic;
            ViewData["Thumbnail_Images"] = thumb;
            ViewData["Object_Images"] = objects;

            ViewData["DeleteSuccess"] = "";

            return View(ic);
        }

        [HttpPost] //Upload to wwwRoot folder
        public async Task<IActionResult> UploadTowwwRoot(IFormFile imgfile, string folderName)
        {
            if (imgfile != null && imgfile.Length > 0)
            {
                string ext = Path.GetExtension(imgfile.FileName).ToLower();
                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif")
                {
                    // Specify the folder where you want to save the image
                    var imgSavePath = Path.Combine(_hostEnvironment.WebRootPath, folderName);

                    // Ensure the directory exists, create it if needed
                    if (!Directory.Exists(imgSavePath))
                    {
                        Directory.CreateDirectory(imgSavePath);
                    }

                    // Combine the path with the original filename
                    var imgFilePath = Path.Combine(imgSavePath, imgfile.FileName);

                    // Save the uploaded image
                    using (var stream = new FileStream(imgFilePath, FileMode.Create))
                    {
                        await imgfile.CopyToAsync(stream);
                        TempData["SuccessMessages"] = "Image successfully uploaded";
                    }

                    // Redirect to the index action or another appropriate action

                    return RedirectToAction("Index", "Img");
                }
                else
                {
                    // Handle invalid file type (e.g., return an error view)
                    ViewData["ErrorMessage"] = "Invalid file type. Please upload a valid image.";
                    return View();
                }
            }

            // Handle the case where no file was uploaded
            ViewData["ErrorMessage"] = "No file was uploaded.";
            return View();
        }
        public IActionResult Delete(string folderName, string fileName)
        {
            // Combine the WebRootPath with the folder name and file name to create the full path
            string fullPath = Path.Combine(_hostEnvironment.WebRootPath, folderName, fileName);

            // Check if the file exists
            if (System.IO.File.Exists(fullPath))
            {
                // Delete the file
                System.IO.File.Delete(fullPath);
                TempData["SuccessMessages"] = "Image successfully deleted";
            }

            // Get the current URL path
            string currentPath = HttpContext.Request.Path.Value;

                if (currentPath.Contains("/ImageAnalyze/AnalyzedImages"))
                {
                    return RedirectToAction("Index", "ImageAnalyze");
                }
                else if (currentPath.Contains(currentPath))
                {
                    return RedirectToAction("Index", "Img");
                }
                else
                {
                    // Redirect to a default action or controller if needed
                    return RedirectToAction("DefaultAction", "DefaultController");
                }
        }
    }
}

