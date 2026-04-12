using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using examencsharp.Models;
using examencsharp.Services;

namespace examencsharp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AccountController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Utilisateurs.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Email ou mot de passe incorrect";
                return View();
            }

            var hasher = new PasswordHasher<Utilisateur>();
            var result = hasher.VerifyHashedPassword(user, user.MotDePasse, password);
            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Email ou mot de passe incorrect";
                return View();
            }

            string code = new Random().Next(100000, 999999).ToString();
            user.Code2FA = code;
            user.Expiration2FA = DateTime.Now.AddMinutes(5);
            _context.SaveChanges();

            _emailService.EnvoyerCode(user.Email, code);
            HttpContext.Session.SetInt32("UserId2FA", user.Id);
            return RedirectToAction("Verify2FA");
        }

        public IActionResult Verify2FA() => View();

        [HttpPost]
        public IActionResult Verify2FA(string code)
        {
            int? userId = HttpContext.Session.GetInt32("UserId2FA");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Utilisateurs.Find(userId);
            if (user == null) return RedirectToAction("Login");

            if (user.Code2FA == code && user.Expiration2FA > DateTime.Now)
            {
                HttpContext.Session.Remove("UserId2FA");
                HttpContext.Session.SetString("user", user.Email);
                HttpContext.Session.SetString("role", user.Role);
                HttpContext.Session.SetInt32("userId", user.Id);

                if (user.Role == "Admin")
                    return RedirectToAction("Index", "Dashboard");

                // "Utilisateur" na role hafa rehetra → mankany Vote mivantana
                return RedirectToAction("Index", "Vote");
            }

            ViewBag.Error = "Code invalide ou expiré";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string email, string password, string confirmPassword, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                ViewBag.Error = "Tous les champs sont obligatoires.";
                return View();
            }
            if (password != confirmPassword)
            {
                ViewBag.Error = "Les mots de passe ne correspondent pas.";
                return View();
            }
            if (_context.Utilisateurs.Any(u => u.Email == email))
            {
                ViewBag.Error = "Cet email est déjà utilisé.";
                return View();
            }
            if (role != "Admin" && role != "Utilisateur")
            {
                ViewBag.Error = "Rôle invalide.";
                return View();
            }

            var user = new Utilisateur { Email = email, Role = role };
            var hasher = new PasswordHasher<Utilisateur>();
            user.MotDePasse = hasher.HashPassword(user, password);

            _context.Utilisateurs.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Inscription réussie ! Vous pouvez vous connecter.";
            return RedirectToAction("Login");
        }
    }
}