using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MudBlazor.Translations;
using Smpsp.Wasm;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient(Smpsp.Wasm.Data.HttpClientNames.Api, c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddMudServices();
builder.Services.AddMudTranslations();

builder.Services.AddScoped<Smpsp.Wasm.Data.LocalPreferencesService>();
builder.Services.AddScoped<Smpsp.Wasm.Data.TranslationService>();
builder.Services.AddScoped<Smpsp.Wasm.Data.HttpClientService>();
builder.Services.AddScoped<Smpsp.Wasm.Data.PostService>();

await builder.Build().RunAsync();
