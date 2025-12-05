/// <summary>
/// This file configures the MAUI application and sets up dependency injection.
/// Dependency Injection (DI) is a design pattern where objects don't create their own dependencies,
/// instead they receive them from an external source (the DI container). This makes testing easier
/// and reduces coupling between classes.
/// </summary>
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ColorMix.Data;
using ColorMix.Services;
using ColorMix.Views;
using ColorMix.ViewModel;

namespace ColorMix
{
    /// <summary>
    /// The MauiProgram class is responsible for configuring and building the MAUI application.
    /// This is where we register all our services, configure the database, and set up fonts.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Creates and configures the MAUI application.
        /// This method is called automatically when the app starts, before the App constructor runs.
        /// </summary>
        /// <returns>A fully configured MauiApp instance ready to run</returns>
        public static MauiApp CreateMauiApp()
        {
            // Create a builder to configure our app
            var builder = MauiApp.CreateBuilder();
            
            // Configure the basic MAUI app settings
            builder
                .UseMauiApp<App>()  // Tell MAUI to use our App class as the main application
                .UseMauiCommunityToolkit()  // Add CommunityToolkit features (popups, behaviors, etc.)
                .ConfigureFonts(fonts =>
                {
                    // Register fonts that can be used throughout the app
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");  // Default font
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");  // Bold font
                    fonts.AddFont("fontello.ttf", "IconFont");  // Custom icon font
                });

            // Configure the SQLite database
            // AppDataDirectory is a platform-specific folder where we can safely store data
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "colormix.db");
            
            // Register the database context with dependency injection
            // AddDbContext means each HTTP request or scope gets its own database context instance
            builder.Services.AddDbContext<ColorMixDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Register services with dependency injection
            // Services are classes that provide functionality to other parts of the app
            // AddScoped means one instance per scope (typically per page or operation)
            builder.Services.AddScoped<IColorService, ColorService>();  // Color data operations
            builder.Services.AddScoped<DatabaseInitializer>();  // Database setup and migrations

            // Register ViewModels
            // ViewModels contain the logic and data for Views (the MVVM pattern)
            // AddTransient means a new instance is created each time it's requested
            builder.Services.AddTransient<ColorsViewModel>();
            builder.Services.AddTransient<CreateColorViewModel>();

            // Register Views (Pages)
            // These are the actual UI pages that users interact with
            builder.Services.AddTransient<ColorsView>();
            builder.Services.AddTransient<CreateColorView>();

#if DEBUG
            // Only in debug mode: enable debug logging to help with troubleshooting
    		builder.Logging.AddDebug();
#endif

            // Build the app from our configuration
            var app = builder.Build();

            // Initialize the database before the app starts
            // This ensures tables are created and any migrations are applied
            Task.Run(async () =>
            {
                // Create a scope to get services from the DI container
                using var scope = app.Services.CreateScope();
                
                // Get the DatabaseInitializer service and run initialization
                var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
                await initializer.InitializeAsync();
            }).Wait();  // Wait for initialization to complete before continuing

            return app;
        }
    }
}
