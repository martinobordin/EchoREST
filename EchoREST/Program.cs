using EchoREST;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Encodings.Web;
using System.Text.Json;

var cacheKey = "mrucache";
var jsonSerializerOptions = new JsonSerializerOptions()
{
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    WriteIndented = true
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
var app = builder.Build();

app.Map("history", (IMemoryCache memoryCache) =>
{
    var mruCache = memoryCache.GetOrCreate(cacheKey, (cacheEntry) => new List<EchoResponse>())!;

    var result = mruCache.Select(x => $"Identifier: {x.RequestIdentifier} - Date: {x.Date:s} - /{x.Method} {x.Path}");
    return Results.Json(result, jsonSerializerOptions);
});

app.Map("history/{id}", (Guid id, IMemoryCache memoryCache) =>
{
    var mruCache = memoryCache.GetOrCreate(cacheKey, (cacheEntry) => new List<EchoResponse>())!;
    
    var result = mruCache.FirstOrDefault(x => x.RequestIdentifier == id);
    if (result is null)
    {
        return Results.NotFound(id);
    }

    return Results.Json(result, jsonSerializerOptions);
});

app.Map("{*anyurl}", async (HttpRequest httpRequest, IMemoryCache memoryCache) =>
{
    var echoResponse = new EchoResponse
    {
        Url = httpRequest.GetDisplayUrl(),
        Method = httpRequest.Method,
        Headers = httpRequest.Headers.Select(x => new KeyValuePair<string, string?>(x.Key, x.Value)).ToList(),
        Path = httpRequest.Path,
        QueryString = httpRequest.QueryString,
        Query = httpRequest.Query
    };

    using (StreamReader streamReader = new(httpRequest.Body))
    {
        echoResponse.Body = await streamReader.ReadToEndAsync();
    };

    var mruCache = memoryCache.GetOrCreate(cacheKey, (cacheEntry) => new List<EchoResponse>())!;
    mruCache.Insert(0, echoResponse);
    mruCache = mruCache.Take(10).ToList();
    memoryCache.Set(cacheKey, mruCache);

    var result = JsonSerializer.Serialize(echoResponse, jsonSerializerOptions);
    return Results.Json(echoResponse, jsonSerializerOptions);
});

app.Use((context, next) =>
{
    context.Response.Headers.Append("X-Copyright", "EchoREST v1.0 developed by Martino Bordin");
    return next(context);
});

app.Run();
