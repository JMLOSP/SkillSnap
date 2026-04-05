using System.Net.Http.Headers;
using System.Net.Http.Json;
using SkillSnap.Shared.Models;

namespace SkillSnap.Client.Services
{
  public class AuthService
  {
    private const string TokenKey = "authToken";

    private readonly HttpClient _httpClient;
    private readonly LocalStorageService _localStorageService;

    public AuthService(HttpClient httpClient, LocalStorageService localStorageService)
    {
      _httpClient = httpClient;
      _localStorageService = localStorageService;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
      HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

      AuthResponse? result = await response.Content.ReadFromJsonAsync<AuthResponse>();

      if (result != null && result.Success && !string.IsNullOrWhiteSpace(result.Token))
      {
        await _localStorageService.SetItemAsync(TokenKey, result.Token);
        SetAuthorizationHeader(result.Token);
      }

      return result;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
      HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/register", request);

      AuthResponse? result = await response.Content.ReadFromJsonAsync<AuthResponse>();

      if (result != null && result.Success && !string.IsNullOrWhiteSpace(result.Token))
      {
        await _localStorageService.SetItemAsync(TokenKey, result.Token);
        SetAuthorizationHeader(result.Token);
      }

      return result;
    }

    public async Task LogoutAsync()
    {
      await _localStorageService.RemoveItemAsync(TokenKey);
      ClearAuthorizationHeader();
    }

    public async Task<string?> GetTokenAsync()
    {
      return await _localStorageService.GetItemAsync(TokenKey);
    }

    public void SetAuthorizationHeader(string token)
    {
      _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthorizationHeader()
    {
      _httpClient.DefaultRequestHeaders.Authorization = null;
    }
  }
}