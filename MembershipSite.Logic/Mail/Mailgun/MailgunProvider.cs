namespace MembershipSite.Logic.Mail.Mailgun;
using MembershipSite.Logic.Mail;

public class MailgunProvider(EmailConfig emailConfig, ILogger<MailgunProvider> logger) : IEmailProvider
{
    public async Task SendAsync(string toName, string toEmail, string fromName, string fromEmail, string subject, string body, List<MimePart> files, bool isBodyHtml, string bcc, string replyName = "", string replyEmail = "")
    {
        if (!ConfigIsValid(emailConfig))
        {
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder();

        if (isBodyHtml)
        {
            builder.HtmlBody = body;
        }
        else
        {
            builder.TextBody = body;
        }

        if (files != null)
        {
            foreach (var file in files)
            {
                builder.Attachments.Add(file);
            }
        }

        if (!string.IsNullOrWhiteSpace(bcc))
        {
            message.Bcc.Add(MailboxAddress.Parse(bcc));
        }

        if (!string.IsNullOrWhiteSpace(replyName) && !string.IsNullOrWhiteSpace(replyEmail))
        {
            message.ReplyTo.Add(new MailboxAddress(replyName, replyEmail));
        }

        message.Body = builder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(emailConfig.Server, emailConfig.Port, emailConfig.UseSsl);
            await client.AuthenticateAsync(emailConfig.Username, emailConfig.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

    public async Task SendMultipleAsync(List<EmailAndNamePair> recipients, string fromName, string fromEmail, string subject, string body, List<MimePart> files, bool isBodyHtml, string bcc, string replyName = "", string replyEmail = "")
    {
        if (recipients is null)
        {
            return;
        }

        foreach (var recipient in recipients)
        {
            await SendAsync(recipient.Name, recipient.Email, fromName, fromEmail, subject, body, files, isBodyHtml, bcc, replyName, replyEmail);
        }
    }

    private bool ConfigIsValid(EmailConfig emailConfig)
    {
        if (emailConfig is null)
        {
            logger.LogError("Email configuration is null, unable to send emails.");
        }
        else if (string.IsNullOrWhiteSpace(emailConfig.Password))
        {
            logger.LogError("Email configuration password is null or empty, unable to send emails.");
        }
        else if (string.IsNullOrWhiteSpace(emailConfig.Server))
        {
            logger.LogError("Email configuration server is null or empty, unable to send emails.");
        }
        else if (string.IsNullOrWhiteSpace(emailConfig.Username))
        {
            logger.LogError("Email configuration username is null or empty, unable to send emails.");
        }
        else if (emailConfig.Port == 0)
        {
            logger.LogError("Email configuration port is 0, unable to send emails.");
        }
        else
        {
            return true;
        }

        return false;
    }
}
