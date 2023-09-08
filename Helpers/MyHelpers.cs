using Microsoft.AspNetCore.Http;

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

        public static bool IsMobile(HttpContext context)
        {
            // You can use user agent detection or screen size to determine if it's a mobile device
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            // Example: Check if the user agent contains "Mobile" or "Android" (you can adjust this based on your requirements)
            return userAgent.Contains("Mobile") || userAgent.Contains("Android");
        }

        public static bool IsUserSignedIn(HttpContext context)
        {
            // Implement your Firebase Auth state check here
            // For example, you might check if there's a Firebase Auth token in the session.
            // Return true if the user is signed in, or false otherwise.
            var firebaseAuthToken = context.Session.GetString("_UserToken");

            return !string.IsNullOrEmpty(firebaseAuthToken);
        }
    }
}
