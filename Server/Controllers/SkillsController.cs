using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Server.Data;
using SkillSnap.Shared.Models;

namespace SkillSnap.Api.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class SkillsController : ControllerBase
  {
    private readonly SkillSnapDbContext _context;

    public SkillsController(SkillSnapDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Skill>>> GetSkills()
    {
      List<Skill> skills = await _context.Skills.ToListAsync();
      return Ok(skills);
    }

    [HttpPost]
    public async Task<ActionResult<Skill>> CreateSkill(Skill skill)
    {
      if (skill == null)
        return BadRequest("Skill cannot be null.");

      _context.Skills.Add(skill);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetSkills), new { id = skill.Id }, skill);
    }
  }
}