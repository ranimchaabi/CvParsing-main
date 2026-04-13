using Microsoft.AspNetCore.Mvc;

namespace CvParsing.Controllers;

public class NotificationsController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Notifications";
        return View();
    }
}

