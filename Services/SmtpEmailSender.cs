using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using CvParsing.Options;

namespace CvParsing.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _smtp;
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly IWebHostEnvironment _env;

    public SmtpEmailSender(IOptions<SmtpOptions> smtp, ILogger<SmtpEmailSender> logger, IWebHostEnvironment env)
    {
        _smtp = smtp.Value;
        _logger = logger;
        _env = env;
    }

    public async Task<bool> SendPasswordResetAsync(string toEmail, string resetUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_smtp.Host))
        {
            _logger.LogWarning("SMTP host is not configured. Password reset email was not sent to {Email}.", toEmail);
            if (_env.IsDevelopment())
                _logger.LogInformation("DEV reset link for {Email}: {ResetUrl}", toEmail, resetUrl);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_smtp.FromEmail))
        {
            _logger.LogError("SMTP FromEmail is not configured.");
            return false;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Réinitialisation de votre mot de passe";

        var body = new BodyBuilder
        {
            TextBody =
                $"Bonjour,\n\n" +
                $"Vous avez demandé la réinitialisation de votre mot de passe.\n\n" +
                $"Ouvrez ce lien (valide 20 minutes) :\n{resetUrl}\n\n" +
                $"Si vous n'êtes pas à l'origine de cette demande, ignorez cet e-mail.\n\n" +
                $"— L'équipe Astree",
            HtmlBody =
                $@"<p>Bonjour,</p>
<p>Vous avez demandé la réinitialisation de votre mot de passe.</p>
<p><a href=""{System.Net.WebUtility.HtmlEncode(resetUrl)}"">Réinitialiser mon mot de passe</a></p>
<p>Ce lien est valable <strong>20 minutes</strong>.</p>
<p>Si vous n'êtes pas à l'origine de cette demande, vous pouvez ignorer ce message.</p>
<p>— L'équipe Astree</p>"
        };
        message.Body = body.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            var secure = _smtp.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            await client.ConnectAsync(_smtp.Host, _smtp.Port, secure, cancellationToken);

            if (!string.IsNullOrEmpty(_smtp.Username))
                await client.AuthenticateAsync(_smtp.Username, _smtp.Password, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            return false;
        }
    }
}
