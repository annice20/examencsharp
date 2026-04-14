using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using examencsharp.Models;

namespace examencsharp.Controllers.API
{
    [Route("api/account")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST api/account/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginApiDto dto)
        {
            var user = _context.Utilisateurs.FirstOrDefault(u => u.Email == dto.Email);
            if (user == null)
                return Unauthorized(new { message = "Email ou mot de passe incorrect" });

            var hasher = new PasswordHasher<Utilisateur>();
            var result = hasher.VerifyHashedPassword(user, user.MotDePasse, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Email ou mot de passe incorrect" });

            return Ok(new {
                message = "Connexion réussie",
                id = user.Id,
                email = user.Email,
                role = user.Role
            });
        }

        // POST api/account/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterApiDto dto)
        {
            if (_context.Utilisateurs.Any(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email déjà utilisé" });

            if (dto.Role != "Admin" && dto.Role != "Utilisateur")
                return BadRequest(new { message = "Rôle invalide" });

            var user = new Utilisateur { Email = dto.Email, Role = dto.Role };
            var hasher = new PasswordHasher<Utilisateur>();
            user.MotDePasse = hasher.HashPassword(user, dto.Password);

            _context.Utilisateurs.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "Inscription réussie", id = user.Id });
        }
    }

    public class LoginApiDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class RegisterApiDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "Utilisateur";
    }
}