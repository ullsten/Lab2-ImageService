using Firebase.Auth;
using Lab2_ImageService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Lab2_ImageService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //private readonly IConfiguration _configuration;
        //FirebaseAuthProvider auth;

        //public HomeController(IConfiguration configuration)
        //{
        //    _configuration = configuration;

        //    // Retrieve the Firebase configuration key from appsettings.json
        //    string firebaseConfigKey = _configuration["Firebase_key"];
        //    auth = new FirebaseAuthProvider(new FirebaseConfig(firebaseConfigKey));
        //}

        //public IActionResult Index()
        //{
        //    var token = HttpContext.Session.GetString("_UserToken");
        //    if (token != null)
        //    {
        //        return View();
        //    }
        //    else
        //    {
        //        return RedirectToAction("SignIn");
        //    }
        //}

        //public IActionResult Register()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> Register(UserModel userModel)
        //{
        //    //create the user
        //    await auth.CreateUserWithEmailAndPasswordAsync(userModel.Email, userModel.Password);
        //    //log in the new user
        //    var fbAuthLink = await auth
        //                    .SignInWithEmailAndPasswordAsync(userModel.Email, userModel.Password);
        //    string token = fbAuthLink.FirebaseToken;
        //    //saving the token in a session variable
        //    if (token != null)
        //    {
        //        HttpContext.Session.SetString("_UserToken", token);

        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        return View();
        //    }
        //}

        //public IActionResult SignIn()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> SignIn(UserModel userModel)
        //{
        //    //log in the user
        //    var fbAuthLink = await auth
        //                    .SignInWithEmailAndPasswordAsync(userModel.Email, userModel.Password);
        //    string token = fbAuthLink.FirebaseToken;
        //    //saving the token in a session variable
        //    if (token != null)
        //    {
        //        HttpContext.Session.SetString("_UserToken", token);

        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        return View();
        //    }
        //}

        //public IActionResult LogOut()
        //{
        //    HttpContext.Session.Remove("_UserToken");
        //    return RedirectToAction("SignIn");
        //}
    }
}