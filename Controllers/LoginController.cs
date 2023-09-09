using Firebase.Auth;
using Lab2_ImageService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Lab2_ImageService.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        FirebaseAuthProvider auth;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;

            // Retrieve the Firebase configuration key from appsettings.json
            string firebaseConfigKey = _configuration["Firebase_key"];
            auth = new FirebaseAuthProvider(new FirebaseConfig(firebaseConfigKey));
        }

        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("_UserToken");
            if (token != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("SignIn");
            }
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserModel userModel)
        {
            //create the user
            await auth.CreateUserWithEmailAndPasswordAsync(userModel.Email, userModel.Password);
            //log in the new user
            var fbAuthLink = await auth
                            .SignInWithEmailAndPasswordAsync(userModel.Email, userModel.Password);
            string token = fbAuthLink.FirebaseToken;
            //saving the token in a session variable
            if (token != null)
            {
                HttpContext.Session.SetString("_UserToken", token);

                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(UserModel userModel)
        {
            //log in the user
            var fbAuthLink = await auth
                            .SignInWithEmailAndPasswordAsync(userModel.Email, userModel.Password);
            string token = fbAuthLink.FirebaseToken;
            //saving the token in a session variable
            if (token != null)
            {
                HttpContext.Session.SetString("_UserToken", token);

                TempData["LoginMessage"] = "You have been successfully logged in";

                return RedirectToAction("AnalyzedImages", "ImageAnalyze");
            }
            else
            {
                return View();
            }
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("_UserToken");
            TempData["LogoutMessage"] = "You have been logged out. See ya!";
            return RedirectToAction("Index", "Home");
        }
    }
}
