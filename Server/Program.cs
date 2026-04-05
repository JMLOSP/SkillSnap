using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SkillSnap.Server.Data;

internal class Program
{
  private static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    //Add services to the container.
    builder.Services.AddControllersWithViews().AddJsonOptions(options =>
    {
      options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

    //Cors
    builder.Services.AddCors(options =>
    {
      options.AddPolicy("AllowClient", policy =>
      {
        policy.WithOrigins("https://localhost:5001")
        .AllowAnyMethod()
        .AllowAnyHeader();
      });
    });

    // Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
      options.Password.RequireDigit = true;
      options.Password.RequireLowercase = true;
      options.Password.RequireUppercase = false;
      options.Password.RequireNonAlphanumeric = false;
      options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<SkillSnapDbContext>()
    .AddDefaultTokenProviders();

    // JWT
    string jwtKey = builder.Configuration["Jwt:Key"]!;
    string jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
    string jwtAudience = builder.Configuration["Jwt:Audience"]!;

    builder.Services
      .AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwtIssuer,
          ValidAudience = jwtAudience,
          IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
        };
      });

    builder.Services.AddAuthorization();
    builder.Services.AddRazorPages();


    //Configure Entity Framework with SQLite.
    builder.Services.AddDbContext<SkillSnapDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    var app = builder.Build();

    using (IServiceScope scope = app.Services.CreateScope())
    {
      IServiceProvider services = scope.ServiceProvider;

      SkillSnapDbContext db = services.GetRequiredService<SkillSnapDbContext>();
      db.Database.Migrate();

      RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
      UserManager<ApplicationUser> userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

      string[] roles = new[] { "Admin", "User" };

      foreach (string role in roles)
      {
        if (!roleManager.RoleExistsAsync(role).Result)
          roleManager.CreateAsync(new IdentityRole(role)).Wait();
      }

      string adminEmail = "admin@skillsnap.com";
      ApplicationUser? adminUser = userManager.FindByEmailAsync(adminEmail).Result;
      if (adminUser == null)
      {
        adminUser = new ApplicationUser
        {
          UserName = adminEmail,
          Email = adminEmail,
          EmailConfirmed = true
        };

        IdentityResult result = userManager.CreateAsync(adminUser, "Admin123!").Result;
        if (result.Succeeded)
          userManager.AddToRoleAsync(adminUser, "Admin").Wait();
      }
    }


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
    app.UseCors("AllowClient");

    app.UseRouting();
    app.UseAuthorization();
    app.MapRazorPages();
    app.MapControllers();
    app.MapFallbackToFile("index.html");

    app.Run();
  }
}