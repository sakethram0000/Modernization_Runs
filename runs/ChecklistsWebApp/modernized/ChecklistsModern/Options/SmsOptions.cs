namespace ChecklistsModern.Options;

public class SmsOptions
{
    public const string SectionName = "Sms";

    public string AccountSid { get; set; } = string.Empty;

    public string AuthToken { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;
}
