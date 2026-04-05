using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Server.Data;
using SkillSnap.Shared.Models;

namespace SkillSnap.Api.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ProjectsController : ControllerBase
  {
    private readonly SkillSnapDbContext _context;

    public ProjectsController(SkillSnapDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
      List<Project> projects = await _context.Projects.ToListAsync();
      return Ok(projects);
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

      return NoContent();
    }
  }
}