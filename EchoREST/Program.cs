using EchoREST;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Encodings.Web;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();


var app = builder.Build();

app.Map("history", (IMemoryCache memoryCache) =>
{
    var mruCache = memoryCache.GetOrCreate("mrucache", (cacheEntry) => new List<EchoResponse>())!;
    return Results.Json(mruCache.Select(x => $"Identifier: {x.Identifier} - Date: {x.Date:s} - /{x.Method} {x.Path}"),
            new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });
});

app.Map("history/{id}", (Guid id, IMemoryCache memoryCache) =>
{
    var mruCache = memoryCache.GetOrCreate("mrucache", (cacheEntry) => new List<EchoResponse>())!;
    var mruCacheItem = mruCache.First(x => x.Identifier == id);
    return Results.Json(mruCacheItem,
          new JsonSerializerOptions()
          {
              Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
              WriteIndented = true
          });
});

app.Map("{*anyurl}", async (HttpRequest httpRequest, IMemoryCache memoryCache) =>
{
    var echoResponse = new EchoResponse
    {
        Url = httpRequest.GetDisplayUrl(),
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

    var mruCache = memoryCache.GetOrCreate("mrucache", (cacheEntry) => new List<EchoResponse>())!;
    mruCache.Insert(0, echoResponse);
    mruCache = mruCache.Take(10).ToList();
    memoryCache.Set("mrucache", mruCache);

    return Results.Json(echoResponse,
        new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        });
});

app.Run();
