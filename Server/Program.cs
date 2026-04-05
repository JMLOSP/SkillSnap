using Microsoft.EntityFrameworkCore;
using SkillSnap.Server.Data;

internal class Program
{
  private static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    //Add services to the container.
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    //Configure Entity Framework with SQLite.
    builder.Services.AddDbContext<SkillSnapDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    var app = builder.Build();

    //Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
      app.UseWebAssemblyDebugging();
    else
    {
      app.UseExceptionHandler("/Error");
      //The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
      app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.UseRouting();

    app.MapRazorPages();
    app.MapControllers();
    app.MapFallbackToFile("index.html");

    app.Run();
  }
}