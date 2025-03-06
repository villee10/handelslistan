using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Hämta anslutningssträng från appsettings.json
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Lägg till PostgreSQL-anslutning i Dependency Injection
builder.Services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));

// Stöd för API-kontroller
builder.Services.AddControllers();

var app = builder.Build();

// Aktiverar API-kontroller i appen
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();