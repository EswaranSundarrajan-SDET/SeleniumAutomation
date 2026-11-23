namespace ApiAutomation.Clients;

/// <summary>Controls whether a request may be sent again after a transient failure.</summary>
public enum ApiRetryMode
{
    /// <summary>Applies the default GET and PUT retry policy.</summary>
    IdempotentMethods,
    /// <summary>Sends the request once so tests can assert on the first result.</summary>
    Never,
    /// <summary>Allows retry when the caller has verified that the endpoint safely deduplicates or repeats the request.</summary>
    ReplaySafeRequest
}
