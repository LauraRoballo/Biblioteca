using BibliotecaLiceista.Web.Components;
using BibliotecaLiceista.Web.Data;
using BibliotecaLiceista.Web.Models;
using BibliotecaLiceista.Web.Repositories;
using BibliotecaLiceista.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MVC: controladores (equivalentes a los Servlets) y vistas Razor .cshtml (equivalentes a las JSP)
builder.Services.AddControllersWithViews();

// Autenticación por cookie y autorización por roles
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/login";
        opciones.AccessDeniedPath = "/acceso-denegado";
        opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Acceso a datos (ADO.NET)
builder.Services.AddSingleton<ConexionBd>();
builder.Services.AddScoped<LibroRepository>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<EditorialRepository>();
builder.Services.AddScoped<CategoriaRepository>();
builder.Services.AddScoped<PrestamoRepository>();

// Reglas de negocio de préstamos: valores leídos de appsettings.json (sección "PoliticasPrestamo")
builder.Services.Configure<PoliticasPrestamo>(builder.Configuration.GetSection("PoliticasPrestamo"));
builder.Services.AddScoped<PrestamoService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();     // ← antes de UseAntiforgery
app.UseAuthorization();
app.UseAntiforgery();

// Cerrar sesión: borra la cookie y vuelve al login
app.MapGet("/cerrar-sesion", async (HttpContext contexto) =>
{
    await contexto.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapStaticAssets();
app.MapControllers();     // rutas de los controladores MVC ([Route] en cada controlador)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();