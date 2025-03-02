using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🟢 Lägg till stöd för API-kontroller
builder.Services.AddControllers();

// 🟢 Lägg till databaskopplingen från `appsettings.json`
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 🟢 Aktivera API-kontroller i appen
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // Gör så att API-kontroller fungerar

app.Run();