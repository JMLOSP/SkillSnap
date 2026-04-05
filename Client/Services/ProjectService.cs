using System.Net.Http.Json;
using SkillSnap.Shared.Models;

namespace SkillSnap.Client.Services
{
  public class ProjectService
  {
    private readonly HttpClient _httpClient;

    public ProjectService(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    public async Task<List<Project>> GetProjectsAsync()
    {
      List<Project>? projects = await _httpClient.GetFromJsonAsync<List<Project>>("api/projects");
      return projects ?? new List<Project>();
    }

    public async Task<Project?> AddProjectAsync(Project newProject)
    {
      HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/projects", newProject);

      if (response.IsSuccessStatusCode)
        return await response.Content.ReadFromJsonAsync<Project>();

      return null;
    }
  }
}