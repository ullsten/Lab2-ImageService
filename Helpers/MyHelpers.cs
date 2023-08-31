namespace Lab2_ImageService.Helpers
{
    // Helper class (MyHelpers.cs)
    public static class MyHelpers
    {
        public static bool IsHome(HttpContext context)
        {
            // Define your condition to determine if it's the home page
            // For example:
            return context.Request.Path == "/";
        }
    }

}
