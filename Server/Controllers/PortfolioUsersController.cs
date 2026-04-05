using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Server.Data;
using SkillSnap.Shared.Models;

namespace SkillSnap.Server.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class PortfolioUsersController : ControllerBase
  {
    private readonly SkillSnapDbContext _context;

    public PortfolioUsersController(SkillSnapDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PortfolioUser>>> GetPortfolioUsers()
    {
      List<PortfolioUser> users = await _context.PortfolioUsers
        .Include(user => user.Projects)
        .Include(user => user.Skills)
        .ToListAsync();

      return Ok(users);
    }
  }
}