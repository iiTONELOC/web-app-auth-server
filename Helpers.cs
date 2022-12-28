using WebAppAuthenticationServer.Services;
using WebAppAuthenticationServer.Models;
using WebAppAuthenticationServer.Utils;

namespace WebAppAuthenticationServer.Helpers;

public static class ProgramConfig
{
    private static void LoadEnvironmentVariables()
    {
        // load our environment variables
        var root = Directory.GetCurrentDirectory();
        DotEnv.Load(Path.Combine(root, ".env").ToString());
    }

    private static void SetAllowedOrigins(WebApplicationBuilder? builder)
    {
        var allowed = GetEnv("AllowedHosts") ??
         builder?.Configuration.GetSection("AllowedHosts").ToString()! ?? "";

        // Add services to the container.
        builder?.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    policy.WithOrigins(allowed.ToString())
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });
    }

    private static void ConfigureDatabaseSettings(WebApplicationBuilder? builder)
    {
        // look for environment variables
        var connectionString = GetEnv("ConnectionString");
        var databaseName = GetEnv("DatabaseName");
        var userCollectionName = GetEnv("UserCollectionName");
        var webApplicationsCollectionName = GetEnv("WebAppCollectionName");

        // Configure the database settings
        if (connectionString != null && databaseName != null
            && userCollectionName != null && webApplicationsCollectionName != null)
        {
            builder?.Services.Configure<WebAppDatabaseSettings>(options =>
            {
                options.ConnectionString = connectionString.ToString();
                options.DatabaseName = databaseName.ToString();
                options.UserCollectionName = userCollectionName.ToString();
                options.WebApplicationsCollectionName = webApplicationsCollectionName.ToString();
            });
        }
        // If the environment variables do no exist, we fallback to appsettings.json
        else
        {
            builder?.Services.Configure<WebAppDatabaseSettings>(
                builder.Configuration.GetSection("WebAppDatabase"));
        }
    }


    /// <summary>
    /// Runs the pre-build configuration sequence for the app
    /// </summary>
    /// <remarks>
    /// This method does the following:
    /// 
    /// 1. Loads the environment variables
    /// 2. Sets the allowed origins
    /// 3. Configures the database settings
    /// 4. Adds the user services to the DI container
    /// 5. Adds the controllers to the DI container
    /// </remarks>
    public static void InitApp(WebApplicationBuilder? builder)
    {
        LoadEnvironmentVariables();
        SetAllowedOrigins(builder);
        ConfigureDatabaseSettings(builder);
        builder?.Services.AddSingleton<UserServices>();
        builder?.Services.AddControllers()
        .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
    }

    private static Func<string, string?> GetEnv = (string key) =>
        Environment.GetEnvironmentVariable(key) ?? null;

}


public static class ProgramBuilder
{
    /// <summary>
    /// Runs the pre-build configuration sequence for the app and then builds it.
    /// </summary>
    public static WebApplication BuildApp(string[] args)
    {
        // init the builder
        var builder = WebApplication.CreateBuilder(args);

        // run the pre-build configuration sequence from above
        ProgramConfig.InitApp(builder);

        // Build the app using the builder's configuration
        var app = builder.Build();

        // Attach the middleware to the app

        // upgrade to https
        app.UseHttpsRedirection();

        // enable cors
        app.UseCors();

        // authenticate the user
        app.Use((context, next) => UserAuthService.Authenticate(
            context, next, app.Services.GetRequiredService<UserServices>()));

        // enable routing via controllers
        app.MapControllers();


        return app;
    }
}