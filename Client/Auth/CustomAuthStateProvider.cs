using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SkillSnap.Client.Services;

namespace SkillSnap.Client.Auth
{
  public class CustomAuthStateProvider : AuthenticationStateProvider
  {
    private const string TokenKey = "authToken";

    private readonly LocalStorageService _localStorageService;
    private readonly HttpClient _httpClient;

    public CustomAuthStateProvider(LocalStorageService localStorageService, HttpClient httpClient)
    {
      _localStorageService = localStorageService;
      _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
      string? token = await _localStorageService.GetItemAsync(TokenKey);

      if (string.IsNullOrWhiteSpace(token))
      {
        ClaimsPrincipal anonymous = new(new ClaimsIdentity());
        return new AuthenticationState(anonymous);
      }

      _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

      List<Claim> claims = ParseClaimsFromJwt(token);
      ClaimsIdentity identity = new(claims, "jwt");
      ClaimsPrincipal user = new(identity);

      return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication(string token)
    {
      List<Claim> claims = ParseClaimsFromJwt(token);
      ClaimsIdentity identity = new(claims, "jwt");
      ClaimsPrincipal user = new(identity);

      _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

      NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
      _httpClient.DefaultRequestHeaders.Authorization = null;

      ClaimsPrincipal anonymous = new(new ClaimsIdentity());
      NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }

    private static List<Claim> ParseClaimsFromJwt(string jwt)
    {
      JwtSecurityTokenHandler handler = new();
      JwtSecurityToken token = handler.ReadJwtToken(jwt);

      return token.Claims.ToList();
    }
  }
}