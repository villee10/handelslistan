using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;


[ApiController]
[Route("api/shoppinglists")]
public class ShoppingListController : ControllerBase
{
    private readonly AppDbContext _context;

    public ShoppingListController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetLists(int userId)
    {
        var lists = await _context.ShoppingLists.Where(l => l.OwnerId == userId).ToListAsync();
        return Ok(lists);
    }
}