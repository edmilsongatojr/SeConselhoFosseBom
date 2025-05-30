using Microsoft.Extensions.Logging;
using SeConselhoFosseBom.Class.ApiClients;
using System.Net.Http.Headers;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;
using System.Diagnostics;

namespace SeConselhoFosseBom
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddHttpClient("adviceslip", client =>
            {
                client.BaseAddress = new Uri("https://api.adviceslip.com");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).AddPolicyHandler(GetRetryPolicy());

            builder.Services.AddHttpClient("MyMemory", client =>
            {
                client.BaseAddress = new Uri("https://api.mymemory.translated.net");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).AddPolicyHandler(GetRetryPolicy());

            builder.Services.AddScoped<HttpClientHelper>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<App>();

            ConfigureGlobalExceptionHandlers();

            return builder.Build();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2));
        }

        private static void ConfigureGlobalExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = (Exception)e.ExceptionObject;
                Debug.WriteLine($"[AppDomain] Erro: {ex.Message}");
            };


            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Debug.WriteLine($"[TaskScheduler] Erro: {e.Exception.Message}");
                e.SetObserved(); // Marca como tratado, evita crash
            };

        }
    }
}
