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

            // 2FA désactivé temporairement - connexion directe
            HttpContext.Session.SetString("user", user.Email);
            HttpContext.Session.SetInt32("userId", user.Id);
            return RedirectToAction("Index", "Home");
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

            if (user != null && user.Code2FA == code && user.Expiration2FA > DateTime.Now)
            {
                HttpContext.Session.Remove("UserId2FA");
                HttpContext.Session.SetString("user", user.Email);
                HttpContext.Session.SetInt32("userId", user.Id);
                return RedirectToAction("Index", "Home");
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

<<<<<<< HEAD
            var hasher = new PasswordHasher<Utilisateur>();
            var user = new Utilisateur
            {
                Email = email,
                Role = "Citoyen"
            };
=======
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
>>>>>>> a228513f9f32357ac2e45ef74c82a2d6c0c185de
            user.MotDePasse = hasher.HashPassword(user, password);

            _context.Utilisateurs.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Inscription réussie ! Vous pouvez vous connecter.";
<<<<<<< HEAD
            return RedirectToAction("Login");
        }

        // MOT DE PASSE OUBLIE - GET
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // MOT DE PASSE OUBLIE - POST
        [HttpPost]
        public IActionResult ForgotPassword(string email, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Les mots de passe ne correspondent pas";
                return View();
            }

            var user = _context.Utilisateurs.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Email introuvable";
                return View();
            }

            var hasher = new PasswordHasher<Utilisateur>();
            user.MotDePasse = hasher.HashPassword(user, newPassword);
            _context.SaveChanges();

            TempData["Success"] = "Mot de passe modifié avec succès !";
=======
>>>>>>> a228513f9f32357ac2e45ef74c82a2d6c0c185de
            return RedirectToAction("Login");
        }
    }
}