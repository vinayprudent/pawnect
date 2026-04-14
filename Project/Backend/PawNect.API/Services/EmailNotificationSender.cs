using System.Net;
using System.Net.Mail;
using PawNect.Application.Interfaces;
using PawNect.Domain.Enums;

namespace PawNect.API.Services;

public class EmailNotificationSender : INotificationSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationSender> _logger;

    public EmailNotificationSender(IConfiguration configuration, ILogger<EmailNotificationSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOtpAsync(OtpChannel channel, string destination, string code, OtpPurpose purpose, CancellationToken cancellationToken = default)
    {
        if (channel != OtpChannel.Email)
            throw new NotSupportedException("Only email OTP is supported currently.");

        var host = _configuration["Email:Smtp:Host"];
        var portText = _configuration["Email:Smtp:Port"];
        var username = _configuration["Email:Smtp:Username"];
        var password = _configuration["Email:Smtp:Password"];
        var fromAddress = _configuration["Email:Smtp:FromAddress"];
        var fromName = _configuration["Email:Smtp:FromName"] ?? "PawNect";
        var enableSsl = bool.TryParse(_configuration["Email:Smtp:EnableSsl"], out var parsedEnableSsl) ? parsedEnableSsl : true;
        var subject = purpose == OtpPurpose.Register
            ? "PawNect registration OTP"
            : "PawNect login OTP";
        var body = $"Your OTP for {purpose.ToString().ToLowerInvariant()} is {code}. It expires in {_configuration["Otp:ExpiryMinutes"] ?? "5"} minutes.";

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portText) || string.IsNullOrWhiteSpace(fromAddress))
        {
            await SendToPickupDirectoryAsync(destination, subject, body, fromName, purpose, "smtp_missing_config");
            return;
        }
        if (!int.TryParse(portText, out var port))
            throw new InvalidOperationException("Invalid SMTP port configuration.");

        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = subject,
            Body = body
        };
        message.To.Add(destination);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(username))
            client.Credentials = new NetworkCredential(username, password);

        _logger.LogInformation("Sending OTP email for purpose {Purpose} to {Destination}", purpose, destination);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception)
        {
            await SendToPickupDirectoryAsync(destination, subject, body, fromName, purpose, "smtp_send_exception_fallback");
        }
    }

    private async Task SendToPickupDirectoryAsync(string destination, string subject, string body, string fromName, OtpPurpose purpose, string reason)
    {
        var pickupDir = _configuration["Email:Smtp:PickupDirectory"] ?? "d:/Projects/PawNect/.maildrop";
        System.IO.Directory.CreateDirectory(pickupDir);
        using var devMessage = new MailMessage
        {
            From = new MailAddress("no-reply@pawnect.local", fromName),
            Subject = subject,
            Body = body
        };
        devMessage.To.Add(destination);

        using var pickupClient = new SmtpClient
        {
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
            PickupDirectoryLocation = pickupDir
        };

        _logger.LogWarning("SMTP fallback triggered ({Reason}). OTP email written to pickup directory {PickupDirectory}.", reason, pickupDir);
        await pickupClient.SendMailAsync(devMessage);
    }
}
