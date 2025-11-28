using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ColorMix.Data;
using ColorMix.Services;
using ColorMix.Views;
using ColorMix.ViewModel;

namespace ColorMix
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("fontello.ttf", "IconFont");
                });

            // Configure SQLite database
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "colormix.db");
            builder.Services.AddDbContext<ColorMixDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Register services
            builder.Services.AddScoped<IColorService, ColorService>();
            builder.Services.AddScoped<DatabaseInitializer>();

            // Register ViewModels
            builder.Services.AddTransient<ColorsViewModel>();
            builder.Services.AddTransient<CreateColorViewModel>();

            // Register views
            builder.Services.AddTransient<ColorsView>();
            builder.Services.AddTransient<CreateColorView>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Initialize database
            Task.Run(async () =>
            {
                using var scope = app.Services.CreateScope();
                var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
                await initializer.InitializeAsync();
            }).Wait();

            return app;
        }
    }
}
