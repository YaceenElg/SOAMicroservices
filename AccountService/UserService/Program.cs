using Microsoft.EntityFrameworkCore;
using SoapCore;
using UserService.Data;
using UserService.Models;
using UserService.Services;
using UserService.SOAP;

var builder = WebApplication.CreateBuilder(args);

// Force HTTP on port 5001
builder.WebHost.UseUrls("http://localhost:5001");

// Add Entity Framework
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserServiceDb")));

// Add services
builder.Services.AddScoped<IUserService, UserService.Services.UserService>();
builder.Services.AddScoped<IUserServiceSOAP, UserServiceSOAP>();
builder.Services.AddScoped<UserServiceSOAP>();

// Note: IFaultExceptionTransformer is optional in SoapCore
// SoapCore will handle SOAP faults by default if not registered

// Build app
var app = builder.Build();

// Apply migrations (commented out - migrations applied manually)
// Uncomment if you want automatic migration on startup
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
//     dbContext.Database.Migrate();
// }

// Seed initial data if empty
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    
    if (!dbContext.Users.Any())
    {
        dbContext.Users.AddRange(
            new User
            {
                Username = "admin",
                Email = "admin@gamingestore.com",
                Password = "admin123",
                CreatedAt = DateTime.Now
            },
            new User
            {
                Username = "testuser",
                Email = "testuser@gamingestore.com",
                Password = "test123",
                CreatedAt = DateTime.Now
            },
            new User
            {
                Username = "player1",
                Email = "player1@gamingestore.com",
                Password = "player123",
                CreatedAt = DateTime.Now
            }
        );
        dbContext.SaveChanges();
        Console.WriteLine("✓ UserService: Initial data seeded successfully!");
    }
}

// Configure routing
app.UseRouting();

// Configure SOAP endpoint - Use DataContractSerializer for compatibility with WCF clients
((IApplicationBuilder)app).UseSoapEndpoint<IUserServiceSOAP>("/UserService.svc", new SoapEncoderOptions(), SoapSerializer.DataContractSerializer);

app.MapGet("/", () => "UserService SOAP is running. WSDL available at http://localhost:5001/UserService.svc?wsdl");

// Log startup information
Console.WriteLine("==========================================");
Console.WriteLine("UserService is starting...");
Console.WriteLine("WSDL available at: http://localhost:5001/UserService.svc?wsdl");
Console.WriteLine("Service endpoint: http://localhost:5001/UserService.svc");
Console.WriteLine("==========================================");

app.Run();
