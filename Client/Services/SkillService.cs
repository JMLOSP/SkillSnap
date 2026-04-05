using System.Net.Http.Headers;
using System.Net.Http.Json;
using SkillSnap.Shared.Models;

namespace SkillSnap.Client.Services
{
  public class SkillService
  {
    private readonly HttpClient _httpClient;
    private readonly LocalStorageService _localStorageService;

    public SkillService(HttpClient httpClient, LocalStorageService localStorageService)
    {
      _httpClient = httpClient;
      _localStorageService = localStorageService;
    }

    public async Task<List<Skill>> GetSkillsAsync()
    {
      string? token = await _localStorageService.GetItemAsync("authToken");

      if (!string.IsNullOrWhiteSpace(token))
      {
        _httpClient.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Bearer", token);
      }

      HttpResponseMessage response = await _httpClient.GetAsync("api/skills");

      if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
      {
        return new List<Skill>();
      }

      response.EnsureSuccessStatusCode();

      List<Skill>? skills = await response.Content.ReadFromJsonAsync<List<Skill>>();
      return skills ?? new List<Skill>();
    }
  }
}