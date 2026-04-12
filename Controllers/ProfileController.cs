using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CvParsing.Data;
using CvParsing.Models;
using CvParsing.Models.ViewModels;

namespace CvParsing.Controllers;

public class ProfileController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] AllowedStatuses = { "Accepted", "Rejected", "Pending" };

    public ProfileController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index(int page = 1, string? status = null, string? tab = null)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Profile") });

        var utilisateur = await _context.Utilisateurs.AsNoTracking().FirstOrDefaultAsync(u => u.id == userId);
        if (utilisateur == null)
            return RedirectToAction("Logout", "Account");

        var candidat = await _context.Candidats.FirstOrDefaultAsync(c => c.id == userId);
        if (candidat == null)
        {
            candidat = new Candidat { id = userId };
            _context.Candidats.Add(candidat);
            await _context.SaveChangesAsync();
        }

        var deptOptions = await _context.OffresEmploi
            .AsNoTracking()
            .Where(o => o.departement != null && o.departement != "")
            .Select(o => o.departement!)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        if (deptOptions.Count == 0)
            deptOptions = new List<string> { "Design", "Développement", "RH", "Marketing", "Commercial" };

        const int pageSize = 8;
        page = page < 1 ? 1 : page;

        var statusNorm = NormalizeStatusFilter(status);
        var appsQuery = _context.Cvs
            .AsNoTracking()
            .Include(c => c.OffreEmploi)
            .Where(c => c.id_candidat == userId);

        if (!string.IsNullOrEmpty(statusNorm))
            appsQuery = appsQuery.Where(c => (c.statut_candidature ?? "Pending") == statusNorm);

        var totalApps = await appsQuery.CountAsync();
        var cvs = await appsQuery
            .OrderByDescending(c => c.upload_date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var applications = cvs.Select(c => new ApplicationRowViewModel
        {
            CvId = c.id,
            OffreId = c.id_offre ?? 0,
            TitrePoste = c.OffreEmploi?.titre ?? "—",
            DepartementOuEntreprise = c.OffreEmploi?.departement ?? "—",
            DateCandidature = c.upload_date,
            Statut = string.IsNullOrEmpty(c.statut_candidature) ? "Pending" : c.statut_candidature!
        }).ToList();

        var vm = new ProfilePageViewModel
        {
            NomComplet = utilisateur.nom_utilisateur ?? "",
            Email = utilisateur.email ?? "",
            Telephone = candidat.telephone,
            Departement = candidat.departement,
            Designation = candidat.designation,
            Langues = candidat.langues,
            Bio = candidat.bio,
            PhotoUrl = string.IsNullOrEmpty(candidat.photo_chemin) ? null : candidat.photo_chemin,
            DepartementOptions = deptOptions,
            DesignationOptions = DefaultDesignations,
            Applications = applications,
            ApplicationsTotal = totalApps,
            ApplicationsPage = page,
            ApplicationsPageSize = pageSize,
            StatusFilter = statusNorm
        };

        ViewBag.ActiveTab = string.IsNullOrWhiteSpace(tab) ? "profile" : tab.Trim().ToLowerInvariant();
        ViewData["Title"] = "Mon profil";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        string nomComplet,
        string email,
        string? telephone,
        string? departement,
        string? designation,
        string? langues,
        string? bio,
        IFormFile? photo)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Profile") });

        var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.id == userId);
        var candidat = await _context.Candidats.FirstOrDefaultAsync(c => c.id == userId);
        if (utilisateur == null)
            return RedirectToAction("Logout", "Account");

        if (candidat == null)
        {
            candidat = new Candidat { id = userId };
            _context.Candidats.Add(candidat);
        }

        if (string.IsNullOrWhiteSpace(nomComplet) || string.IsNullOrWhiteSpace(email))
        {
            TempData["ProfileError"] = "Le nom complet et l'email sont obligatoires.";
            return RedirectToAction(nameof(Index), new { tab = "profile" });
        }

        var emailTaken = await _context.Utilisateurs.AnyAsync(u => u.email == email && u.id != userId);
        if (emailTaken)
        {
            TempData["ProfileError"] = "Cet email est déjà utilisé par un autre compte.";
            return RedirectToAction(nameof(Index), new { tab = "profile" });
        }

        utilisateur.nom_utilisateur = nomComplet.Trim();
        utilisateur.email = email.Trim();
        HttpContext.Session.SetString("UserName", utilisateur.nom_utilisateur);
        HttpContext.Session.SetString("UserEmail", utilisateur.email);

        candidat.telephone = telephone;
        candidat.departement = departement;
        candidat.designation = designation;
        candidat.langues = langues;
        candidat.bio = bio;

        if (photo is { Length: > 0 })
        {
            var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
            {
                TempData["ProfileError"] = "Photo : formats acceptés JPG, PNG, WEBP.";
                return RedirectToAction(nameof(Index), new { tab = "profile" });
            }

            if (photo.Length > 2 * 1024 * 1024)
            {
                TempData["ProfileError"] = "Photo : taille maximale 2 Mo.";
                return RedirectToAction(nameof(Index), new { tab = "profile" });
            }

            var folder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(folder);
            var fileName = $"user-{userId}{ext}";
            var physical = Path.Combine(folder, fileName);
            await using (var stream = new FileStream(physical, FileMode.Create))
                await photo.CopyToAsync(stream);

            candidat.photo_chemin = $"/uploads/profiles/{fileName}";
        }

        await _context.SaveChangesAsync();
        TempData["ProfileSuccess"] = "Profil enregistré.";
        return RedirectToAction(nameof(Index), new { tab = "profile" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string motDePasseActuel, string nouveauMotDePasse, string confirmation)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login", "Account");

        var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.id == userId);
        if (utilisateur == null)
            return RedirectToAction("Logout", "Account");

        if (string.IsNullOrEmpty(nouveauMotDePasse) || nouveauMotDePasse != confirmation)
        {
            TempData["PasswordError"] = "Le nouveau mot de passe et la confirmation ne correspondent pas.";
            return RedirectToAction(nameof(Index), new { tab = "password" });
        }

        if (nouveauMotDePasse.Length < 6)
        {
            TempData["PasswordError"] = "Le mot de passe doit contenir au moins 6 caractères.";
            return RedirectToAction(nameof(Index), new { tab = "password" });
        }

        if (utilisateur.mot_passe != motDePasseActuel)
        {
            TempData["PasswordError"] = "Mot de passe actuel incorrect.";
            return RedirectToAction(nameof(Index), new { tab = "password" });
        }

        utilisateur.mot_passe = nouveauMotDePasse;
        await _context.SaveChangesAsync();
        TempData["PasswordSuccess"] = "Mot de passe mis à jour.";
        return RedirectToAction(nameof(Index), new { tab = "password" });
    }

    private static string? NormalizeStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status) || status.Equals("all", StringComparison.OrdinalIgnoreCase))
            return null;
        return AllowedStatuses.Contains(status) ? status : null;
    }

    private static readonly string[] DefaultDesignations =
    {
        "UI UX Designer", "Développeur Web", "Développeur Full Stack", "Chef de projet", "Data Analyst", "RH", "Autre"
    };
}
