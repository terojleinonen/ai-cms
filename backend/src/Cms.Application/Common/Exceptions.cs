namespace Cms.Application.Common;

public class NotFoundException(string message) : Exception(message);

public class ConflictException(string message) : Exception(message);

public class ValidationFailedException(IDictionary<string, string[]> errors) : Exception("Validation failed")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}

/// <summary>Raised when the upstream AI provider fails or returns unusable output.</summary>
public class AiProviderException(string message, Exception? inner = null) : Exception(message, inner);
