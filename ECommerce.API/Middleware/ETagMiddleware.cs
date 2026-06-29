using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class ETagMiddleware
{
    private readonly RequestDelegate _next;

    public ETagMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Method != "GET")
        {
            await _next(context);
            return;
        }

        var original = context.Response.Body;
        using var stream = new MemoryStream();
        context.Response.Body = stream;

        await _next(context);

        if (context.Response.StatusCode == 200)
        {
            stream.Position = 0;
            var body = await new StreamReader(stream).ReadToEndAsync();

            var etag = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
            etag = $"\"{etag}\"";

            context.Response.Headers["ETag"] = etag;

            var requestTag = context.Request.Headers["If-None-Match"].ToString();

            if (requestTag == etag)
            {
                context.Response.Body = original;
                context.Response.StatusCode = 304;
                return;
            }

            stream.Position = 0;
        }

        await stream.CopyToAsync(original);
        context.Response.Body = original;
    }
}