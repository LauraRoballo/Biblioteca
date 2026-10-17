using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Aquí se registrarán los servicios que necesiten correr en el navegador
// (WebAssembly) cuando se agreguen componentes con @rendermode InteractiveWebAssembly
// o InteractiveAuto.

await builder.Build().RunAsync();
