var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseHttpsRedirection();

app.Map("{*anyurl}", async (HttpRequest httpRequest) =>
{
    var echoResponse = new EchoResponse
    {
        Method = httpRequest.Method,
        Headers = httpRequest.Headers,
        Path = httpRequest.Path,
        QueryString = httpRequest.QueryString,
        Query = httpRequest.Query
    };

    using (StreamReader streamReader = new(httpRequest.Body))
    {
        echoResponse.Body = await streamReader.ReadToEndAsync();
    };

    return echoResponse;
});

app.Run();

internal record EchoResponse
{
    public string Body { get; set; } = null!;
    public IHeaderDictionary? Headers { get; set; }
    public string Method { get; set; } = null!;
    public PathString Path { get; set; }
    public QueryString QueryString { get; set; }
    public IQueryCollection? Query { get; set; }
}
