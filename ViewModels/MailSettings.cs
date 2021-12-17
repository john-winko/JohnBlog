namespace JohnBlog.ViewModels;

public class MailSettings
{
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once InconsistentNaming
    public const string JSONName = "MailSettings";
    public string Mail { get; set; }= String.Empty;
    public string DisplayName { get; set; }= String.Empty;
    public string Password { get; set; }= String.Empty;
    public string Host { get; set; }= String.Empty;
    public int Port { get; set; }
}