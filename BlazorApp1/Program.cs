// =====================================================================
// Program.cs  —  Blazor WASM 진입점
// =====================================================================
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using KnitLog;
using KnitLog.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// StorageService를 싱글톤으로 등록
builder.Services.AddSingleton<StorageService>();

await builder.Build().RunAsync();
