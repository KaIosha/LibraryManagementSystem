using System.Net.Mail;
using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Application.Interfaces.IServices;
using LibraryManagementSystem.Infrastructure.Helper;
using Microsoft.Extensions.Options;
using MimeKit;


namespace LibraryManagementSystem.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }



    public async Task SendAsync(string to, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Bib_Alex_Project", _settings.EMAIL));

            message.To.Add(MailboxAddress.Parse(to));

            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();


            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(_settings.HOST, _settings.PORT, MailKit.Security.SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_settings.EMAIL,_settings.PASSWORD);

            await client.SendAsync(message);

            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to send email.", ex);
        }
    }


}
