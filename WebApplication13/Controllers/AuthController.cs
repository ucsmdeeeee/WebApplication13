using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication13.Models;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto register)
    {
        _logger.LogInformation("Received registration request: Username = {Username}, Email = {Email}", register.Username, register.Email);

        try
        {
            if (_context.Users.Any(u => u.Email == register.Email))
            {
                _logger.LogWarning("Registration failed: Email {Email} is already taken.", register.Email);
                return BadRequest(new { message = "Email is already taken." });
            }

            var user = new User
            {
                Username = register.Username,
                Email = register.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.PasswordHash),
                Role = register.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: Username = {Username}, Email = {Email}", user.Username, user.Email);
            return Ok(new { message = "User registered successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during registration.");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        try
        {
            _logger.LogInformation("Received login request: Email = {Email}", login?.Email);

            if (string.IsNullOrEmpty(login?.Email) || string.IsNullOrEmpty(login?.PasswordHash))
            {
                _logger.LogWarning("Login failed: Email or PasswordHash is missing.");
                return BadRequest(new { message = "Email and PasswordHash are required." });
            }
            // Преобразовать email в нижний регистр и удалить пробелы
            login.Email = login.Email.Trim().ToLower();

            var user = _context.Users.SingleOrDefault(u => u.Email == login.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for email {Email}.", login.Email);
                return Unauthorized(new { message = $"Invalid маил credentials.{login.Email }" });
            }

            _logger.LogInformation("User found for email: {Email}, Username: {Username}", login.Email, user.Username);

            // Проверка пароля с хэшированным значением в базе данных
            if (!BCrypt.Net.BCrypt.Verify(login.PasswordHash, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Incorrect password for email {Email}.", login.Email);
                return Unauthorized(new { message = "Invalid пас credentials." });
            }


            // Генерация JWT токена
            var token = GenerateJwtToken(user);
            _logger.LogInformation("User logged in successfully: {Email}", login.Email);
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login.");
            return StatusCode(500, new { message = $"An unexpected error occurred. {login.PasswordHash}" });
        }
    }


    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
