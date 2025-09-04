[ApiController]
[Route("api/[controller]")]
public class ParentsController : ControllerBase
{
    private readonly DbContext1 _context;

    public ParentsController(DbContext1 context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Parent>>> GetParents()
    {
        return await _context.Parents.Include(p => p.Children).ToListAsync();
    }
}
