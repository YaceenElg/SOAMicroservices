using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using SoapCore;
using GameStoreService.Data;
using GameStoreService.Services;
using GameStoreService.SOAP;
using GameStoreService.Models;

var builder = WebApplication.CreateBuilder(args);

// Force HTTP on port 5002
builder.WebHost.UseUrls("http://localhost:5002");

// Add Entity Framework
builder.Services.AddDbContext<GameStoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GameStoreServiceDb")));

// Add services
builder.Services.AddScoped<IGameStoreService, GameStoreService.Services.GameStoreService>();
builder.Services.AddScoped<IGameStoreServiceSOAP, GameStoreServiceSOAP>(); // Registered interface
builder.Services.AddScoped<GameStoreServiceSOAP>(); // Registered implementation

// Build app
var app = builder.Build();

// Apply migrations automatically on startup
Console.WriteLine("==========================================");
Console.WriteLine("Applying database migrations...");
Console.WriteLine("==========================================");
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<GameStoreDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("✓ Migrations applied successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ ERROR during migration: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
    }
}

// Seed initial data automatically on startup
Console.WriteLine("==========================================");
Console.WriteLine("Initializing game data...");
Console.WriteLine("==========================================");
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<GameStoreDbContext>();
        DataSeeder.SeedGames(dbContext);
        Console.WriteLine("==========================================");
        Console.WriteLine("Data initialization completed!");
        Console.WriteLine("==========================================");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ ERROR during data initialization: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
    }
}

// Configure routing
app.UseRouting();

// Configure SOAP endpoint - Use DataContractSerializer for compatibility with WCF clients
((IApplicationBuilder)app).UseSoapEndpoint<IGameStoreServiceSOAP>("/GameStoreService.svc", new SoapEncoderOptions(), SoapSerializer.DataContractSerializer);

app.MapGet("/", () => "GameStoreService SOAP is running. WSDL available at http://localhost:5002/GameStoreService.svc?wsdl");

// Log startup information
Console.WriteLine("==========================================");
Console.WriteLine("GameStoreService is starting...");
Console.WriteLine("WSDL available at: http://localhost:5002/GameStoreService.svc?wsdl");
Console.WriteLine("Service endpoint: http://localhost:5002/GameStoreService.svc");
Console.WriteLine("==========================================");

app.Run();
