using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Stöd för API-kontroller
builder.Services.AddControllers();

// Lägg till konfiguration för databasanslutning
builder.Services.AddSingleton<string>(builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();

// Aktiverar API-kontroller i appen
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // Gör så att API kontroller fungerar

app.Run();