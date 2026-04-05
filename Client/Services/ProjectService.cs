using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SkillSnap.Shared.Models;

namespace SkillSnap.Client.Services
{
  public class ProjectService
  {
    private readonly HttpClient _httpClient;
    private readonly LocalStorageService _localStorage;

    public ProjectService(HttpClient httpClient, LocalStorageService localStorage)
    {
      _httpClient = httpClient;
      _localStorage = localStorage;
    }

    private async Task AddBearerTokenAsync()
    {
      string? token = await _localStorage.GetItemAsync("authToken");

      if (!string.IsNullOrWhiteSpace(token))
      {
        _httpClient.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Bearer", token);
      }
      else
      {
        _httpClient.DefaultRequestHeaders.Authorization = null;
      }
    }

    public async Task<List<Project>> GetProjectsAsync()
    {
      await AddBearerTokenAsync();

      HttpResponseMessage response = await _httpClient.GetAsync("api/projects");

      if (response.StatusCode == HttpStatusCode.Unauthorized)
      {
        return new List<Project>();
      }

      response.EnsureSuccessStatusCode();

      List<Project>? projects = await response.Content.ReadFromJsonAsync<List<Project>>();
      return projects ?? new List<Project>();
    }

    public async Task<Project?> AddProjectAsync(Project newProject)
    {
      await AddBearerTokenAsync();

      HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/projects", newProject);

      if (response.StatusCode == HttpStatusCode.Unauthorized)
      {
        return null;
      }

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadFromJsonAsync<Project>();
      }

      return null;
    }
  }
}