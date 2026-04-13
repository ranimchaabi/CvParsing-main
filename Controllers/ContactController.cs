using CvParsing.Services;
using Microsoft.AspNetCore.Mvc;

namespace CvParsing.Controllers;

public class ContactController : Controller
{
    private readonly IEmailSender _emailSender;

    public ContactController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    [HttpGet("/Contact")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Contact";
        return View();
    }

    [HttpPost("/Contact")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string name, string email, string subject, string message, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Contact";

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
        {
            TempData["ContactError"] = "Veuillez remplir tous les champs.";
            return View();
        }

        var ok = await _emailSender.SendContactAsync(
            toEmail: "ranimchaabi8@gmail.com",
            fromName: name,
            fromEmail: email,
            subject: subject,
            message: message,
            cancellationToken: cancellationToken);

        if (!ok)
        {
            TempData["ContactError"] = "Impossible d'envoyer votre message pour le moment. Veuillez réessayer plus tard.";
            return View();
        }

        TempData["ContactSuccess"] = "Votre message a été envoyé avec succès.";
        return RedirectToAction(nameof(Index));
    }
}
