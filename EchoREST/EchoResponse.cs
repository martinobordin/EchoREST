﻿namespace EchoREST;

public record EchoResponse
{
    public Guid RequestIdentifier { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Url { get; set; } = null;
    public string? Body { get; set; } = null;
    public IEnumerable<KeyValuePair<string, string?>>? Headers { get; set; }
    public string Method { get; set; } = null!;
    public PathString Path { get; set; }
    public QueryString QueryString { get; set; }
    public IQueryCollection? Query { get; set; }
}
