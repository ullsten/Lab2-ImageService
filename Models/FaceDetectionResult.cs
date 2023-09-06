namespace Lab2_ImageService.Models
{
    internal class FaceDetectionResult
    {
        public string ImagePath { get; set; }
        public object DetectedFaces { get; set; }
    }
}