using Microsoft.AspNetCore.Mvc;
using CvParsing.Data;
using CvParsing.Models;
using CvParsing.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

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

    [HttpGet]
    public IActionResult Search(string? q, string? departement, string? typeContrat)
    {
        var all = _context.OffresEmploi.AsNoTracking().ToList();

        var vm = new OffreSearchResultsViewModel
        {
            Query = q,
            Departement = departement,
            TypeContrat = typeContrat,
            Departements = all
                .Select(o => (o.departement ?? "").Trim())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(d => d)
                .Select(d => new SelectListItem { Value = d, Text = d, Selected = string.Equals(d, departement, StringComparison.OrdinalIgnoreCase) })
                .ToList(),
            TypesContrat = all
                .Select(o => (o.type ?? "").Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t)
                .Select(t => new SelectListItem { Value = t, Text = t, Selected = string.Equals(t, typeContrat, StringComparison.OrdinalIgnoreCase) })
                .ToList()
        };

        IEnumerable<OffreEmploi> filtered = all;

        if (!string.IsNullOrWhiteSpace(departement))
        {
            filtered = filtered.Where(o => string.Equals((o.departement ?? "").Trim(), departement.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(typeContrat))
        {
            filtered = filtered.Where(o => string.Equals((o.type ?? "").Trim(), typeContrat.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        var query = (q ?? "").Trim();
        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(o => IsFuzzyMatch(query, (o.titre ?? "") + " " + (o.description ?? "")));
        }

        vm.Results = filtered
            .OrderByDescending(o => o.date_creation ?? DateTime.MinValue)
            .ToList();

        return View("~/Views/Offre/search-results.cshtml", vm);
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

    private static bool IsFuzzyMatch(string needle, string haystack)
    {
        var n = NormalizeForSearch(needle);
        if (string.IsNullOrWhiteSpace(n)) return true;

        var h = NormalizeForSearch(haystack);
        if (string.IsNullOrWhiteSpace(h)) return false;

        if (h.Contains(n, StringComparison.Ordinal)) return true;

        var words = h.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var maxDist = n.Length <= 4 ? 1 : (n.Length <= 7 ? 2 : 3);

        foreach (var w in words)
        {
            if (w.StartsWith(n, StringComparison.Ordinal)) return true;

            // quick length guard for performance
            if (Math.Abs(w.Length - n.Length) > maxDist) continue;

            if (LevenshteinDistance(n, w, maxDist) <= maxDist) return true;
        }

        return false;
    }

    private static string NormalizeForSearch(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var s = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (var ch in s)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (cat == UnicodeCategory.NonSpacingMark) continue;

            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
            }
            else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_' || ch == '/')
            {
                sb.Append(' ');
            }
        }

        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    // Bounded Levenshtein: early-exits when distance exceeds max
    private static int LevenshteinDistance(string a, string b, int max)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        if (Math.Abs(a.Length - b.Length) > max) return max + 1;

        var prev = new int[b.Length + 1];
        var curr = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++) prev[j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            curr[0] = i;
            var bestInRow = curr[0];
            var ca = a[i - 1];

            for (var j = 1; j <= b.Length; j++)
            {
                var cost = (ca == b[j - 1]) ? 0 : 1;
                var val = Math.Min(
                    Math.Min(curr[j - 1] + 1, prev[j] + 1),
                    prev[j - 1] + cost
                );
                curr[j] = val;
                if (val < bestInRow) bestInRow = val;
            }

            if (bestInRow > max) return max + 1;

            (prev, curr) = (curr, prev);
        }

        return prev[b.Length];
    }
}