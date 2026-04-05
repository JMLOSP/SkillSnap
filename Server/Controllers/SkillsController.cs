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
  public class SkillsController : ControllerBase
  {
    private readonly SkillSnapDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SkillsController> _logger;

    private const string SkillsCacheKey = "skills_all";

    public SkillsController(SkillSnapDbContext context, IMemoryCache cache, ILogger<SkillsController> logger)
    {
      _context = context;
      _cache = cache;
      _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<Skill>>> GetSkills()
    {
      Stopwatch stopwatch = Stopwatch.StartNew();

      if (_cache.TryGetValue(SkillsCacheKey, out List<Skill>? cachedSkills) && cachedSkills != null)
      {
        stopwatch.Stop();
        _logger.LogInformation("Retrieved skills from cache in {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
        return Ok(cachedSkills);
      }

      List<Skill> skills = await _context.Skills
        .AsNoTracking()
        .ToListAsync();

      MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions
      {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        SlidingExpiration = TimeSpan.FromMinutes(3)
      };

      _cache.Set(SkillsCacheKey, skills, cacheOptions);

      stopwatch.Stop();
      _logger.LogInformation("GET /api/skills - DB LOAD - {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);

      return Ok(skills);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Skill>> CreateSkill(Skill skill)
    {
      if (skill == null)
      {
        return BadRequest("Skill cannot be null.");
      }

      _context.Skills.Add(skill);
      await _context.SaveChangesAsync();

      _cache.Remove(SkillsCacheKey);

      return CreatedAtAction(nameof(GetSkills), new { id = skill.Id }, skill);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateSkill(int id, Skill skillTag)
    {
      if (id != skillTag.Id)
      {
        return BadRequest("Skill ID mismatch.");
      }

      Skill? existingSkill = await _context.Skills.FindAsync(id);

      if (existingSkill == null)
      {
        return NotFound();
      }

      existingSkill.Name = skillTag.Name;

      await _context.SaveChangesAsync();

      _cache.Remove(SkillsCacheKey);

      return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
      Skill? skill = await _context.Skills.FindAsync(id);

      if (skill == null)
      {
        return NotFound();
      }

      _context.Skills.Remove(skill);
      await _context.SaveChangesAsync();

      _cache.Remove(SkillsCacheKey);

      return NoContent();
    }
  }
}