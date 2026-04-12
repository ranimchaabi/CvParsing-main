using Microsoft.AspNetCore.Mvc;

namespace CvParsing.Controllers;

public class ContactController : Controller
{
    /// <summary>Landing route for global search; forwards to home until a dedicated contact page exists.</summary>
    [HttpGet("/Contact")]
    public IActionResult Index() => RedirectToAction("Index", "Home");
}
