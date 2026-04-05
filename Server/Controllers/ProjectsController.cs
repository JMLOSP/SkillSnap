using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SkillSnap.Server.Data;
using SkillSnap.Shared.Models;
using System.Diagnostics;

namespace SkillSnap.Api.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ProjectsController : ControllerBase
  {
    private readonly SkillSnapDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProjectsController> _logger;
    private const string ProjectsCacheKey = "projects_all";

    public ProjectsController(SkillSnapDbContext context, IMemoryCache cache, ILogger<ProjectsController> logger)
    {
      _context = context;
      _cache = cache;
      _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Project>>> GetProjects()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();

      if (_cache.TryGetValue(ProjectsCacheKey, out List<Project>? cachedProjects) && cachedProjects != null)
      {
        stopwatch.Stop();
        _logger.LogInformation("Retrieved projects from cache in {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
        return Ok(cachedProjects);
      }

      List<Project> projects = await _context.Projects.AsNoTracking().ToListAsync();

      MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions
      {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
        SlidingExpiration = TimeSpan.FromMinutes(2)
      };

      _cache.Set(ProjectsCacheKey, projects, cacheOptions);

      stopwatch.Stop();
      _logger.LogInformation("Retrieved projects from database and cached in {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);

      return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProject(int id)
    {
      string cacheKey = $"project_{id}";

      if (_cache.TryGetValue(cacheKey, out Project? cachedProject) && cachedProject != null)
        return Ok(cachedProject);

      Project? project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

      if (project == null)
        return NotFound();

      MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions
      {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
      };

      _cache.Set(cacheKey, project, cacheOptions);

      return Ok(project);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Project>> CreateProject(Project project)
    {
      if (project == null)
        return BadRequest("Project cannot be null.");

      _context.Projects.Add(project);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetProjects), new { id = project.Id }, project);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateProject(int id, Project project)
    {
      if (id != project.Id)
        return BadRequest("Project ID mismatch.");

      Project? existingProject = await _context.Projects.FindAsync(id);

      if (existingProject == null)
        return NotFound();

      existingProject.Title = project.Title;
      existingProject.Description = project.Description;

      await _context.SaveChangesAsync();

      _cache.Remove(ProjectsCacheKey);
      _cache.Remove($"project_{id}");

      return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProject(int id)
    {
      Project? project = await _context.Projects.FindAsync(id);

      if (project == null)
        return NotFound();

      _context.Projects.Remove(project);
      await _context.SaveChangesAsync();

      _cache.Remove(ProjectsCacheKey);
      _cache.Remove($"project_{id}");

      return NoContent();
    }
  }
}