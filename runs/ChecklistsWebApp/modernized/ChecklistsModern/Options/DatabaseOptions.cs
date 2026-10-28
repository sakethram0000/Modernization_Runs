namespace ChecklistsModern.Options;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Provider { get; set; } = "Sqlite";

    public bool EnsureCreated { get; set; }

    public bool SeedPreviewData { get; set; }
}
