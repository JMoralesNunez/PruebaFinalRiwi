using System.Net;
using System.Net.Mail;

namespace TalentoPlus.Api.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpServer = _config["EmailSettings:Server"];
        var port = int.Parse(_config["EmailSettings:Port"]);
        var senderEmail = _config["EmailSettings:SenderEmail"];
        var password = _config["EmailSettings:Password"];
        var enableSsl = bool.Parse(_config["EmailSettings:EnableSsl"]);

        var client = new SmtpClient(smtpServer)
        {
            Port = port,
            Credentials = new NetworkCredential(senderEmail, password),
            EnableSsl = enableSsl,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(to);

        await client.SendMailAsync(mailMessage);
    }
}