namespace GBC_Ticketing.Services.Email;

public class EmailSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 25;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "GBC Ticketing";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
