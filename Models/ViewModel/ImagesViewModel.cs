namespace Lab2_ImageService.Models.ViewModel
{
    public class ImagesViewModel
    {
        public List<ImageModel> UploadedImages { get; set; }
        public List<ImageModel> ObjectImages { get; set; }
        public List<ImageModel> ThumbnailImages { get; set; }
    
    //public List<(ImageModel Image, string Category)> LatestAnalyzedImages { get; set; }

    //public int CurrentPage { get; set; }
    //public int PageSize { get; set; }
    //public int TotalObjectsPages { get; set; }
    //public int TotalThumbnailsPages { get; set; }
    //public int TotalUploadedPages { get; set; }


    //public string? FileName { get; set; }
    //public string? FilePath { get; set; }
    //public DateTime UploadedTimestamp { get; set; }
}
}

