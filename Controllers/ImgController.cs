//using Lab2_ImageService.Models;
//using Lab2_ImageService.Models.ViewModel;
//using Microsoft.AspNetCore.Mvc;
//using System.Globalization;

//namespace Lab2_ImageService.Controllers
//{
    
//    public class ImgController : Controller
//    {
//        private readonly IWebHostEnvironment _hostEnvironment;
//        public ImgController(IWebHostEnvironment hostEnvironment)
//        {
//            _hostEnvironment = hostEnvironment;
//        }

//        public IActionResult Index()
//        {
//            ImageClass ic = new ImageClass();
//            var displayImg = Path.Combine(_hostEnvironment.WebRootPath, "UploadedImages");
//            DirectoryInfo di = new DirectoryInfo(displayImg);
//            FileInfo[] uploadInfo = di.GetFiles();
//            ic.FileImage = uploadInfo; 
            
//            ImageClass thumb = new ImageClass();
//            var displayThumbs = Path.Combine(_hostEnvironment.WebRootPath, "Thumbnails");
//            DirectoryInfo th = new DirectoryInfo(displayThumbs);
//            FileInfo[] thumbInfo = th.GetFiles();
//            thumb.FileImage = thumbInfo;

//            ImageClass objects = new ImageClass();
//            var displayObject = Path.Combine(_hostEnvironment.WebRootPath, "Objects");
//            DirectoryInfo obj = new DirectoryInfo(displayObject);
//            FileInfo[] fileInfos = obj.GetFiles();
//            objects.FileImage = fileInfos;

//            ViewData["Uploaded_Images"] = ic;
//            ViewData["Thumbnail_Images"] = thumb;
//            ViewData["Object_Images"] = objects;


//            return View(objects);
//        }

//        [HttpPost] //Upload to wwwRoot folder
//        public async Task<IActionResult> Index(IFormFile imgfile)
//        {
//            if (imgfile != null && imgfile.Length > 0)
//            {
//                string ext = Path.GetExtension(imgfile.FileName).ToLower();
//                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif")
//                {
//                    // Specify the folder where you want to save the image
//                    var imgSavePath = Path.Combine(_hostEnvironment.WebRootPath, "UploadedImages");

//                    // Ensure the directory exists, create it if needed
//                    if (!Directory.Exists(imgSavePath))
//                    {
//                        Directory.CreateDirectory(imgSavePath);
//                    }

//                    // Combine the path with the original filename
//                    var imgFilePath = Path.Combine(imgSavePath, imgfile.FileName);

//                    // Save the uploaded image
//                    using (var stream = new FileStream(imgFilePath, FileMode.Create))
//                    {
//                        await imgfile.CopyToAsync(stream);
//                    }

//                    // Redirect to the index action or another appropriate action
//                    return RedirectToAction("Index");
//                }
//                else
//                {
//                    // Handle invalid file type (e.g., return an error view)
//                    ViewData["ErrorMessage"] = "Invalid file type. Please upload a valid image.";
//                    return View();
//                }
//            }

//            // Handle the case where no file was uploaded
//            ViewData["ErrorMessage"] = "No file was uploaded.";
//            return View();
//        }

//        public IActionResult Delete(string imgdel)
//        {
//            imgdel = Path.Combine(_hostEnvironment.WebRootPath, "UploadedImages", imgdel);
//            FileInfo fi = new FileInfo(imgdel);
//            if (fi != null)
//            {
//                System.IO.File.Delete(imgdel);
//                fi.Delete();
//            }

//            return RedirectToAction("Index");
//        }


//        //public IActionResult Delete(string folderName, string imgdel)
//        //{
//        //    string folderPath = Path.Combine(_hostEnvironment.WebRootPath, folderName, imgdel);

//        //    // Check if the image file exists in the specified folder
//        //    if (System.IO.File.Exists(folderPath))
//        //    {
//        //        System.IO.File.Delete(folderPath);

//        //    }

//        //    // Redirect back to the Index action
//        //    return RedirectToAction("Index");
//        //}

//        //public IActionResult Index()
//        //{
//        //    // Define the folder paths for UploadedImages, Objects, and Thumbnails
//        //    string uploadedImagesFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "UploadedImages");
//        //    string objectsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "Objects");
//        //    string thumbnailsFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "Thumbnails");

//        //    // Get lists of image files from each folder
//        //    var uploadedImages = GetImageFiles(uploadedImagesFolderPath);
//        //    var objectImages = GetImageFiles(objectsFolderPath);
//        //    var thumbnailImages = GetImageFiles(thumbnailsFolderPath);

//        //    // Create a view model to hold the lists of images
//        //    var viewModel = new ImagesViewModel
//        //    {
//        //        UploadedImages = uploadedImages,
//        //        ObjectImages = objectImages,
//        //        ThumbnailImages = thumbnailImages
//        //    };

//        //    return View(viewModel);
//        //}
//        //private List<ImageModel> GetImageFiles(string folderPath)
//        //{
//        //    if (!Directory.Exists(folderPath))
//        //    {
//        //        return new List<ImageModel>();
//        //    }

//        //    return Directory.GetFiles(folderPath)
//        //        .Select(filePath => new ImageModel
//        //        {
//        //            FileName = Path.GetFileName(filePath),
//        //            FilePath = filePath,
//        //            UploadedTimestamp = new FileInfo(filePath).CreationTime,
//        //        })
//        //        .OrderByDescending(image => ExtractDateFromFileName(image.FileName))
//        //        .ToList();
//        //}

//        //DateTime ExtractDateFromFileName(string fileName)
//        //{
//        //    var datePart = fileName.Split('_')[0]; // Assuming the date part is before the underscore
//        //    if (DateTime.TryParseExact(datePart, "yyyy-MM-dd-HH-mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
//        //    {
//        //        return parsedDate;
//        //    }
//        //    // Return a default date or handle the error as needed
//        //    return DateTime.MinValue;
//        //}


//    }
//}
