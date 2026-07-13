using SkillJobAI.Api.Services;

namespace SkillJobAI.Api.Tests.Integration;

public sealed class FakeEmailService : IEmailService
{
private readonly object _lock = new();


private readonly List<FakeEmailMessage>
    _messages = new();

public IReadOnlyList<FakeEmailMessage> Messages
{
    get
    {
        lock (_lock)
        {
            return _messages.ToList();
        }
    }
}

public FakeEmailMessage? LastMessage
{
    get
    {
        lock (_lock)
        {
            return _messages.LastOrDefault();
        }
    }
}

public Task SendEmailAsync(
    string toEmail,
    string subject,
    string body)
{
    lock (_lock)
    {
        _messages.Add(
            new FakeEmailMessage
            {
                ToEmail = toEmail,
                Subject = subject,
                Body = body
            });
    }

    return Task.CompletedTask;
}

public void Clear()
{
    lock (_lock)
    {
        _messages.Clear();
    }
}


}

public sealed class FakeEmailMessage
{
public string ToEmail { get; init; } =
string.Empty;

public string Subject { get; init; } =
    string.Empty;

public string Body { get; init; } =
    string.Empty;


}
