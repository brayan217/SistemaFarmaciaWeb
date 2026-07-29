using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using SistemaFarmaciaWeb.Data;

// Configura la licencia de QuestPDF.
// Community es adecuada para este proyecto académico.
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Habilita controladores y vistas MVC.
builder.Services.AddControllersWithViews();

// Obtiene la conexión desde appsettings.json.
var connectionString = builder.Configuration
    .GetConnectionString("ConexionFarmacia")
    ?? throw new InvalidOperationException(
        "No se encontró la conexión ConexionFarmacia."
    );

// Conecta Entity Framework Core con SQL Server.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
);

// Configura el inicio de sesión mediante cookies.
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme
    )
    .AddCookie(options =>
    {
        // Página para los usuarios que no iniciaron sesión.
        options.LoginPath = "/Account/Login";

        // Página para usuarios sin permisos.
        options.AccessDeniedPath = "/Account/AccesoDenegado";

        // Duración de la sesión.
        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        // Renueva la sesión mientras se utiliza el sistema.
        options.SlidingExpiration = true;
    });

// Habilita el control de permisos.
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Primero identifica al usuario.
app.UseAuthentication();

// Después verifica sus permisos.
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
)
.WithStaticAssets();

app.Run();