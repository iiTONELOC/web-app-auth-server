using WebAppAuthenticationServer.Models;
using WebAppAuthenticationServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins(
                builder.Configuration.GetSection("AllowedHosts").ToString()!)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// add our database settings to the container
builder.Services.Configure<WebAppDatabaseSettings>(
    builder.Configuration.GetSection("WebAppDatabase"));

// add the user services to the container which allows
// us to access the database
builder.Services.AddSingleton<UserServices>();

// add api controllers
builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

// build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// Add cors before the routes
app.UseCors();

app.MapControllers();


app.Run();
