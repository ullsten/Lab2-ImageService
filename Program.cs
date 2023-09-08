using DotNetEnv;
using Lab2_ImageService.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Extensions.Logging;

namespace Lab2_ImageService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Env.Load();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            //builder.Services.AddControllersWithViews(options =>
            //{
            //    var policy = new AuthorizationPolicyBuilder()
            //    .RequireAuthenticatedUser()
            //    .Build();
            //    options.Filters.Add(new AuthorizeFilter(policy));
            //});

            //builder.Services.AddMvc().AddSessionStateTempDataProvider();
            //builder.Services.AddSession();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    // Specify the login path here
                    options.LoginPath = "/Login"; // Customize this path as needed
                });

            builder.Services.AddScoped<IComputerVisionService, ComputerVisionService>();
            builder.Services.AddSingleton<ICustomVisionPredictionClient>(sp =>
            {
                IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
                string prediction_endpont = configuration["PredictionEndpoint"];
                string prediction_key = configuration["PredictionKey"];

                var predictionClient = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(prediction_key))
                {
                    Endpoint = prediction_endpont,
                };
                return predictionClient;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}