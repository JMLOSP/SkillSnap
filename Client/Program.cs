using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SkillSnap.Client;
using SkillSnap.Client.Auth;
using SkillSnap.Client.Services;

internal class Program
{
  private static async Task Main(string[] args)
  {
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

    builder.Services.AddScoped<LocalStorageService>();
    builder.Services.AddScoped<AuthService>();

    builder.Services.AddScoped<CustomAuthStateProvider>();
    builder.Services.AddScoped<AuthenticationStateProvider>(sp =>  sp.GetRequiredService<CustomAuthStateProvider>());

    builder.Services.AddScoped<ProjectService>();
    builder.Services.AddScoped<SkillService>();

    builder.Services.AddScoped<UserSessionService>();

    builder.Services.AddAuthorizationCore();

    await builder.Build().RunAsync();
  }
}