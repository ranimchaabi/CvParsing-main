using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using CvParsing.Data;
using CvParsing.Models;
using CvParsing.Models.Dtos;
using CvParsing.Options;
using CvParsing.Services;
using Microsoft.Extensions.Options;

namespace CvParsing.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private const string ForgotPasswordPublicMessage =
        "If an account exists with this email, a reset link has been sent.";

    private const string ResetPasswordInvalidMessage =
        "Invalid or expired reset link. Please request a new password reset.";

    private static readonly EmailAddressAttribute EmailValidator = new();

    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly PasswordService _passwordService;
    private readonly AppOptions _app;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AppDbContext context,
        IEmailSender emailSender,
        PasswordService passwordService,
        IOptions<AppOptions> app,
        ILogger<AuthController> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _passwordService = passwordService;
        _app = app.Value;
        _logger = logger;
    }

    [HttpPost("auth/forgot-password")]
    [EnableRateLimiting("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest? request, CancellationToken cancellationToken)
    {
        if (request?.Email is not { Length: > 0 } emailRaw || !EmailValidator.IsValid(emailRaw))
            return Ok(new { message = ForgotPasswordPublicMessage });

        var email = emailRaw.Trim();

        var emailNorm = email.ToLowerInvariant();
        var user = await _context.Utilisateurs
            .FirstOrDefaultAsync(
                u => u.email != null && u.email.ToLower() == emailNorm,
                cancellationToken);

        if (user != null)
        {
            var pending = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.id && !t.Used)
                .ToListAsync(cancellationToken);
            foreach (var p in pending)
                p.Used = true;

            var plainToken = PasswordResetTokenCrypto.CreateUrlToken();
            var hashHex = PasswordResetTokenCrypto.HashTokenToHex(plainToken);
            var now = DateTime.UtcNow;

            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.id,
                TokenHashHex = hashHex,
                ExpiresAtUtc = now.AddMinutes(20),
                Used = false,
                CreatedAtUtc = now
            });

            await _context.SaveChangesAsync(cancellationToken);

            var baseUrl = ResolvePublicBaseUrl();
            var resetUrl = $"{baseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(plainToken)}";

            var sent = await _emailSender.SendPasswordResetAsync(user.email!, resetUrl, cancellationToken);
            if (!sent)
                _logger.LogWarning("Password reset token created for user {UserId} but email delivery failed.", user.id);
        }

        return Ok(new { message = ForgotPasswordPublicMessage });
    }

    [HttpPost("auth/reset-password")]
    [EnableRateLimiting("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest? request, CancellationToken cancellationToken)
    {
        if (request?.Token is not { Length: > 0 } token ||
            string.IsNullOrEmpty(request.NewPassword) ||
            request.NewPassword != request.ConfirmPassword ||
            request.NewPassword.Length < 8)
        {
            return BadRequest(new { message = ResetPasswordInvalidMessage });
        }

        var hashHex = PasswordResetTokenCrypto.HashTokenToHex(token);

        var row = await _context.PasswordResetTokens
            .Where(t => t.TokenHashHex == hashHex && !t.Used && t.ExpiresAtUtc > DateTime.UtcNow)
            .Select(t => new { t.Id, t.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (row == null)
            return BadRequest(new { message = ResetPasswordInvalidMessage });

        var marked = await _context.PasswordResetTokens
            .Where(t => t.Id == row.Id && !t.Used)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Used, true), cancellationToken);

        if (marked != 1)
            return BadRequest(new { message = ResetPasswordInvalidMessage });

        var user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.id == row.UserId, cancellationToken);
        if (user == null)
            return BadRequest(new { message = ResetPasswordInvalidMessage });

        user.mot_passe = _passwordService.Hash(request.NewPassword);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = "Your password has been reset. You can sign in with your new password." });
    }

    private string ResolvePublicBaseUrl()
    {
        if (!string.IsNullOrWhiteSpace(_app.PublicBaseUrl))
            return _app.PublicBaseUrl.TrimEnd('/') + "/";

        return $"{Request.Scheme}://{Request.Host.Value}/";
    }
}
