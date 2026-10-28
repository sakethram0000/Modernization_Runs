using ChecklistsModern.Options;
using Microsoft.Extensions.Options;

namespace ChecklistsModern.Services;

public interface IEmailSender
{
    Task SendAsync(string? destination, string subject, string body);
}

public interface ISmsSender
{
    Task SendAsync(string? destination, string body);
}

// The legacy EmailService sent nothing. Email settings are read but no provider is wired.
public class NoOpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<NoOpEmailSender> _logger;

    public NoOpEmailSender(IOptions<EmailOptions> options, ILogger<NoOpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task SendAsync(string? destination, string subject, string body)
    {
        _logger.LogInformation("Email to {Destination} not sent; no email provider is configured (SmtpHost set: {Configured}).",
            destination, !string.IsNullOrEmpty(_options.SmtpHost));
        return Task.CompletedTask;
    }
}

// The legacy SmsService sent nothing. SMS settings are read but no provider is wired.
public class NoOpSmsSender : ISmsSender
{
    private readonly SmsOptions _options;
    private readonly ILogger<NoOpSmsSender> _logger;

    public NoOpSmsSender(IOptions<SmsOptions> options, ILogger<NoOpSmsSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task SendAsync(string? destination, string body)
    {
        _logger.LogInformation("SMS to {Destination} not sent; no SMS provider is configured (AccountSid set: {Configured}).",
            destination, !string.IsNullOrEmpty(_options.AccountSid));
        return Task.CompletedTask;
    }
}
