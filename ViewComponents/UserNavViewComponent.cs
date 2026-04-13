using CvParsing.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvParsing.ViewComponents;

public class UserNavViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public UserNavViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userIdStr = HttpContext?.Session?.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
            return View(new UserNavVm());
        }

        var userName = HttpContext?.Session?.GetString("UserName") ?? "";

        var candidat = await _context.Candidats
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.id == userId);

        return View(new UserNavVm
        {
            IsAuthenticated = true,
            UserId = userId,
            UserName = userName,
            Designation = candidat?.designation,
            PhotoUrl = string.IsNullOrWhiteSpace(candidat?.photo_chemin) ? null : candidat!.photo_chemin
        });
    }

    public sealed class UserNavVm
    {
        public bool IsAuthenticated { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string? Designation { get; set; }
        public string? PhotoUrl { get; set; }
    }
}

