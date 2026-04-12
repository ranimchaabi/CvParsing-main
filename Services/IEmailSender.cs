namespace CvParsing.Services;

public interface IEmailSender
{
    Task<bool> SendPasswordResetAsync(string toEmail, string resetUrl, CancellationToken cancellationToken = default);
}
