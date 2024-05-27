namespace MembershipSite.Logic.Mail;

public class MailgunProvider(EmailConfig emailConfig) : IEmailProvider
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

    private bool ConfigIsValid(EmailConfig emailConfig)
    {
        if (emailConfig is null ||
            string.IsNullOrWhiteSpace(emailConfig.Password) ||
            string.IsNullOrWhiteSpace(emailConfig.Server) ||
            string.IsNullOrWhiteSpace(emailConfig.Username) ||
            emailConfig.Port == 0)
        {
            return false;
        }

        return true;
    }
}
