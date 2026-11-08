namespace lab3_kpz.Core.WebApi.Exceptions;

/// <summary>
/// No requested resource found (404 Not Found).
/// </summary>
public sealed class ResourceNotFoundException(string message) : Exception(message)
{
    public static ResourceNotFoundException ForEntity(string entityName, object id) =>
        new($"{entityName} with id '{id}' not found.");
}

/// <summary>
/// Resource conflict  (409 Conflict).
/// </summary>
public sealed class ConflictException(string message) : Exception(message);

/// <summary>
/// Business logic violation (422 Unprocessable Entity).
/// </summary>
public sealed class BusinessRuleException(string message) : Exception(message);

