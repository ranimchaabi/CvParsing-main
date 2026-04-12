using Microsoft.AspNetCore.Mvc;
using CvParsing.Data;

namespace CvParsing.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
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
                (u.nom_utilisateur == username || u.email == username)
                && u.mot_passe == password);

        if (user != null)
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
            mot_passe = password,
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

    // POST: Forgot Password
    [HttpPost]
    public IActionResult ForgotPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            ViewBag.Error = "Veuillez entrer un email.";
            return View();
        }

        var user = _context.Utilisateurs.FirstOrDefault(u => u.email == email);

        if (user != null)
        {
            // Simulation envoi email (tu peux remplacer par vrai email plus tard)
            ViewBag.Success = "Un lien de réinitialisation a été envoyé à votre email.";
        }
        else
        {
            // message générique (bonne pratique sécurité)
            ViewBag.Success = "Si cet email existe, un lien a été envoyé.";
        }

        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}