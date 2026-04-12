using Microsoft.AspNetCore.Mvc;
using CvParsing.Data;
using CvParsing.Models;

namespace CvParsing.Controllers;

public class OffreController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public OffreController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public IActionResult Index()
    {
        var offres = _context.OffresEmploi.ToList();
        return View("~/Views/Offre/offer.cshtml", offres);
    }

    public IActionResult Details(int id)
    {
        var offre = _context.OffresEmploi.FirstOrDefault(o => o.id == id);

        if (offre == null)
            return NotFound();

        ViewData["OffreId"] = id;

        var userId = HttpContext.Session.GetString("UserId");
        ViewBag.IsLoggedIn = !string.IsNullOrEmpty(userId);
        ViewBag.NomUtilisateur = HttpContext.Session.GetString("UserName");
        ViewBag.EmailUtilisateur = HttpContext.Session.GetString("UserEmail");

        return View("~/Views/Offre/offer-details.cshtml", offre);
    }

    [HttpPost]
    public async Task<IActionResult> UploadCv(int offreId, string nomComplet,
        string email, string? telephone, IFormFile cvFile, string? lettreMotivation)
    {
        ViewData["OffreId"] = offreId;
        var userId = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account",
                new { returnUrl = $"/Offre/Details/{offreId}" });

        var offre = _context.OffresEmploi.FirstOrDefault(o => o.id == offreId);

        if (cvFile == null || cvFile.Length == 0)
        {
            ViewBag.UploadError = "Veuillez sélectionner un fichier CV.";
            ViewBag.IsLoggedIn = true;
            ViewBag.NomUtilisateur = nomComplet;
            ViewBag.EmailUtilisateur = email;
            return View("~/Views/Offre/offer-details.cshtml", offre);
        }

        var ext = Path.GetExtension(cvFile.FileName).ToLower();
        if (ext != ".pdf" && ext != ".doc" && ext != ".docx")
        {
            ViewBag.UploadError = "Format non accepté. Utilisez PDF, DOC ou DOCX.";
            ViewBag.IsLoggedIn = true;
            ViewBag.NomUtilisateur = nomComplet;
            ViewBag.EmailUtilisateur = email;
            return View("~/Views/Offre/offer-details.cshtml", offre);
        }

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "cvs");
        Directory.CreateDirectory(uploadsFolder);
        var uniqueFileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await cvFile.CopyToAsync(stream);
        }

        var newCv = new Cv
        {
            id_offre = offreId,
            id_candidat = int.Parse(userId),
            chemin_fichier = $"/uploads/cvs/{uniqueFileName}",
            upload_date = DateTime.Now,
            statut_candidature = "Pending"
        };
        _context.Cvs.Add(newCv);
        await _context.SaveChangesAsync();

        ViewBag.UploadSuccess = true;
        ViewBag.IsLoggedIn = true;
        ViewBag.NomUtilisateur = nomComplet;
        ViewBag.EmailUtilisateur = email;
        return View("~/Views/Offre/offer-details.cshtml", offre);
    }
}