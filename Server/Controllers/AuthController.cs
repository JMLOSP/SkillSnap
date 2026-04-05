using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SkillSnap.Server.Data;
using SkillSnap.Shared.Models;

namespace SkillSnap.Server.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
      ApplicationUser? existingUser = await _userManager.FindByEmailAsync(request.Email);
      if (existingUser != null)
      {
        return BadRequest(new AuthResponse
        {
          Success = false,
          Message = "User already exists."
        });
      }

      ApplicationUser user = new ApplicationUser
      {
        UserName = request.Email,
        Email = request.Email
      };

      IdentityResult result = await _userManager.CreateAsync(user, request.Password);

      if (!result.Succeeded)
      {
        return BadRequest(new AuthResponse
        {
          Success = false,
          Message = string.Join(" | ", result.Errors.Select(e => e.Description))
        });
      }

      await _userManager.AddToRoleAsync(user, "User");

      List<string> roles = new() { "User" };
      string token = await GenerateJwtToken(user, roles);

      return Ok(new AuthResponse
      {
        Success = true,
        Token = token,
        Email = user.Email!,
        Roles = roles,
        Message = "Registration successful."
      });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
      ApplicationUser? user = await _userManager.FindByEmailAsync(request.Email);
      if (user == null)
      {
        return Unauthorized(new AuthResponse
        {
          Success = false,
          Message = "Invalid credentials."
        });
      }

      Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

      if (!result.Succeeded)
      {
        return Unauthorized(new AuthResponse
        {
          Success = false,
          Message = "Invalid credentials."
        });
      }

      IList<string> roles = await _userManager.GetRolesAsync(user);
      string token = await GenerateJwtToken(user, roles.ToList());

      return Ok(new AuthResponse
      {
        Success = true,
        Token = token,
        Email = user.Email!,
        Roles = roles.ToList(),
        Message = "Login successful."
      });
    }

    private Task<string> GenerateJwtToken(ApplicationUser user, List<string> roles)
    {
      List<Claim> claims = new()
      {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
        new Claim(ClaimTypes.NameIdentifier, user.Id)
      };

      foreach (string role in roles)
      {
        claims.Add(new Claim(ClaimTypes.Role, role));
      }

      SymmetricSecurityKey key = new(
        Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
      );

      SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

      JwtSecurityToken token = new(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: creds
      );

      string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
      return Task.FromResult(tokenString);
    }
  }
}