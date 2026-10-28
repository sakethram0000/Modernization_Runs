namespace ChecklistsModern.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;
}
