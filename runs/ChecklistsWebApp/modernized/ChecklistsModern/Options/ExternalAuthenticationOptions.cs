namespace ChecklistsModern.Options;

public class ExternalAuthenticationOptions
{
    public const string SectionName = "Authentication";

    public ExternalProviderOptions Google { get; set; } = new();

    public ExternalProviderOptions Microsoft { get; set; } = new();

    public ExternalProviderOptions Facebook { get; set; } = new();

    public ExternalProviderOptions Twitter { get; set; } = new();
}

public class ExternalProviderOptions
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;
}
