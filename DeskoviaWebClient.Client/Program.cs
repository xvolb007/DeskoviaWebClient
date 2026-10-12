using System.Globalization;
using DeskoviaWebClient.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-GB");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7095/") });
builder.Services.AddScoped<ApiClient>();
await builder.Build().RunAsync();
