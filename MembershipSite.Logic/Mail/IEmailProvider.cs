namespace MembershipSite.Logic.Mail;

public interface IEmailProvider
{
    Task SendAsync(string toName, string toEmail, string fromName, string fromEmail, string subject, string body, List<MimePart> files, bool isBodyHtml, string bcc, string replyName = "", string replyEmail = "");
}
