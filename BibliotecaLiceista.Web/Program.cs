using BibliotecaLiceista.Web.Components;
using BibliotecaLiceista.Web.Data;
using BibliotecaLiceista.Web.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Componentes Razor con interactividad en el servidor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Acceso a datos (ADO.NET)
builder.Services.AddSingleton<ConexionBd>();
builder.Services.AddScoped<LibroRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();