using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI_to_MySQL.Entities;

[ApiController]
[Route("api/[controller]")]
public class EEProjectController : ControllerBase
{
    private readonly NeurotechnexusContext _context;

    public EEProjectController(NeurotechnexusContext context)
    {
        _context = context;
    }

    [HttpGet("categories/{categoryId}/projects")]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjectsByCategory(int categoryId)
    {
        return await _context.Projects
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    [HttpGet("projects/{projectId}/criteria")]
    public async Task<ActionResult<IEnumerable<Criterion>>> GetCriteriaByProject(int projectId)
    {
        return await _context.Criteria
            .Where(c => c.ProjectId == projectId)
            .ToListAsync();
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        return await _context.Categories.ToListAsync();
    }

    [HttpGet("projects")]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        return await _context.Projects.ToListAsync();
    }

    [HttpGet("criteria")]
    public async Task<ActionResult<IEnumerable<Criterion>>> GetCriteria()
    {
        return await _context.Criteria.ToListAsync();
    }
}
