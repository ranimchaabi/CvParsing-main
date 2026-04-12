using Microsoft.AspNetCore.Mvc;
using CvParsing.Data;
using CvParsing.Services;

namespace CvParsing.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private readonly PasswordService _passwordService;

    public AccountController(AppDbContext context, PasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    public IActionResult Login(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            return RedirectToAction("Index", "Home");
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password, string? returnUrl = null)
    {
        var user = _context.Utilisateurs
            .FirstOrDefault(u =>
                u.nom_utilisateur == username || u.email == username);

        if (user != null && _passwordService.Verify(user.mot_passe, password))
        {
            user.date_derniere_connexion = DateTime.Now;
            _context.SaveChanges();

            HttpContext.Session.SetString("UserId", user.id.ToString());
            HttpContext.Session.SetString("UserName", user.nom_utilisateur ?? "");
            HttpContext.Session.SetString("UserEmail", user.email ?? "");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Nom d'utilisateur ou mot de passe incorrect.";
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    public IActionResult Register() => View();

    [HttpPost]
    public IActionResult Register(string username, string email, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Veuillez remplir tous les champs.";
            return View();
        }

        if (_context.Utilisateurs.Any(u => u.email == email))
        {
            ViewBag.Error = "Cet email est déjà utilisé.";
            return View();
        }

        var newUser = new CvParsing.Models.Utilisateur
        {
            nom_utilisateur = username,
            email = email,
            mot_passe = _passwordService.Hash(password),
            date_creation = DateTime.Now
        };
        _context.Utilisateurs.Add(newUser);
        _context.SaveChanges();

        var newCandidat = new CvParsing.Models.Candidat { id = newUser.id };
        _context.Candidats.Add(newCandidat);
        _context.SaveChanges();

        HttpContext.Session.SetString("UserId", newUser.id.ToString());
        HttpContext.Session.SetString("UserName", username);
        HttpContext.Session.SetString("UserEmail", email);

        return RedirectToAction("Index", "Home");
    }

    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpGet]
    [Route("/reset-password")]
    public IActionResult ResetPassword()
    {
        ViewData["Title"] = "Réinitialiser le mot de passe";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}