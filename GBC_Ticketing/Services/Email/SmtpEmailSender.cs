using System.Net;
using System.Net.Mail;
using GBC_Ticketing.Services.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace GBC_Ticketing.Services.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            _logger.LogWarning("Email settings are incomplete. Skipping email send to {Email}", email);
            return;
        }

        using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = string.IsNullOrWhiteSpace(_settings.Username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_settings.Username, _settings.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(
                string.IsNullOrWhiteSpace(_settings.FromEmail)
                    ? "no-reply@gbc-ticketing.com"
                    : _settings.FromEmail,
                string.IsNullOrWhiteSpace(_settings.FromName)
                    ? "GBC Ticketing"
                    : _settings.FromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        message.To.Add(email);

        try
        {
            await smtpClient.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Email} with subject {Subject}", email, subject);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            throw;
        }
    }
}
