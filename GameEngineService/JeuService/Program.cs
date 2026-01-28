using Microsoft.AspNetCore.Builder;
using SoapCore;
using JeuService.Services;
using JeuService.SOAP;

var builder = WebApplication.CreateBuilder(args);

// Force HTTP on port 5003
builder.WebHost.UseUrls("http://localhost:5003");

// Add services
builder.Services.AddScoped<IGameStoreClient, GameStoreClient>();
builder.Services.AddSingleton<IGameEngine, GameEngine>(); // Singleton to persist game states across requests
builder.Services.AddScoped<IJeuService, JeuService.Services.JeuService>();
builder.Services.AddScoped<IJeuServiceSOAP, JeuServiceSOAP>();
builder.Services.AddScoped<JeuServiceSOAP>();

// Build app
var app = builder.Build();

// Configure routing
app.UseRouting();

// Configure SOAP endpoint - Use DataContractSerializer for compatibility with WCF clients
((IApplicationBuilder)app).UseSoapEndpoint<IJeuServiceSOAP>("/JeuService.svc", new SoapEncoderOptions(), SoapSerializer.DataContractSerializer);

app.MapGet("/", () => "JeuService SOAP is running. WSDL available at http://localhost:5003/JeuService.svc?wsdl");

app.Run();
