using DotNetEnv;
using Lab2_ImageService.Services;
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
            builder.Services.AddScoped<IComputerVisionService, ComputerVisionService>();

            //builder.Services.AddScoped<ICustomVisionPredictionClient, CustomVisionPredictionClient>();
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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}