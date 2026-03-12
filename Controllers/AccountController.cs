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

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _emailService = new EmailService();
        }

        // LOGIN GET
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN POST
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

            // Génération du code 2FA
            string code = new Random().Next(100000, 999999).ToString();
            user.Code2FA = code;
            user.Expiration2FA = DateTime.Now.AddMinutes(5);
            _context.SaveChanges();

            _emailService.EnvoyerCode(user.Email, code);

            HttpContext.Session.SetInt32("UserId2FA", user.Id);

            return RedirectToAction("Verify2FA");
        }

        // VERIFY 2FA GET
        public IActionResult Verify2FA()
        {
            return View();
        }

        // VERIFY 2FA POST
        [HttpPost]
        public IActionResult Verify2FA(string code)
        {
            int? userId = HttpContext.Session.GetInt32("UserId2FA");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Utilisateurs.Find(userId);

            if (user.Code2FA == code && user.Expiration2FA > DateTime.Now)
            {
                HttpContext.Session.Remove("UserId2FA");
                HttpContext.Session.SetString("user", user.Email);

                if(user.Role == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                if(user.Role == "Citoyen")
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Code invalide ou expiré";
            return View();
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // REGISTER GET
        public IActionResult Register()
        {
            return View();
        }

        // REGISTER POST
        [HttpPost]
        public IActionResult Register(string email, string password, string confirmPassword, string role)
        {
            // Vérification des champs vides
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                ViewBag.Error = "Tous les champs sont obligatoires.";
                return View();
            }

            // Vérification confirmation mot de passe
            if (password != confirmPassword)
            {
                ViewBag.Error = "Les mots de passe ne correspondent pas.";
                return View();
            }

            // Vérification email déjà existant
            if (_context.Utilisateurs.Any(u => u.Email == email))
            {
                ViewBag.Error = "Cet email est déjà utilisé.";
                return View();
            }

            // Vérification que le rôle est valide (sécurité)
            if (role != "Admin" && role != "Utilisateur")
            {
                ViewBag.Error = "Rôle invalide.";
                return View();
            }

            // Création de l'utilisateur
            var user = new Utilisateur
            {
                Email = email,
                Role = role
            };

            // Hashage du mot de passe
            var hasher = new PasswordHasher<Utilisateur>();
            user.MotDePasse = hasher.HashPassword(user, password);

            _context.Utilisateurs.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Inscription réussie ! Vous pouvez vous connecter.";
            return RedirectToAction("Login");
        }
    }
}