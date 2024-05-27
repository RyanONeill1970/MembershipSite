namespace MembershipSite.ViewModels;

public class EmailConfig
{
    public string Password { get; set; } = "";

    public int Port { get; set; }

    public string Server { get; set; } = "";

    public string Username { get; set; } = "";

    public bool UseSsl { get; set; }
}
