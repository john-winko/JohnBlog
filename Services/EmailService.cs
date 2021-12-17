
using JohnBlog.ViewModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace JohnBlog.Services;

public class EmailService : IEmailSender
{
    private readonly MailSettings _mailSettings;

    public EmailService(IOptions<MailSettings> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string message)
    {
        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        var builder = new BodyBuilder();
        builder.HtmlBody = message;
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}