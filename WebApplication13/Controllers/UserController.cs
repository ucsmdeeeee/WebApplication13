using Microsoft.AspNetCore.Mvc;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("getAll")]
    public IActionResult GetAllUsers()
    {
        var users = _context.Users.Select(u => new { u.Id, u.Username }).ToList();
        return Ok(users);
    }
}

