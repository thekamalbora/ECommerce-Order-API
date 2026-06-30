using System.Text;
using ECommerce.API.Data;
using ECommerce.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ApplicationDbContext db)
    {
        // Only POST requests are checked for idempotency
        if (!HttpMethods.IsPost(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Read the Idempotency-Key header
        var key = context.Request.Headers["Idempotency-Key"].ToString();

        if (string.IsNullOrWhiteSpace(key))
        {
            await _next(context);
            return;
        }

        // Disable compression for this request to ensure raw JSON string capturing
        context.Request.Headers.Remove("Accept-Encoding");

        // Remove expired idempotency keys
        var expired = await db.IdempotencyKeys
            .Where(x => x.ExpiryDate < DateTime.UtcNow)
            .ToListAsync();

        if (expired.Any())
        {
            db.IdempotencyKeys.RemoveRange(expired);
            await db.SaveChangesAsync();
        }

        // Check for existing request with the same key
        var existing = await db.IdempotencyKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == key);

        if (existing != null)
        {
            context.Response.Headers.Append("X-Idempotent","true");
            context.Response.Headers.Remove("Content-Encoding");
            context.Response.StatusCode = existing.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(existing.Response);
            return;
        }

        // Intercept and capture the response stream
        var original = context.Response.Body;
        await using var capture = new MemoryStream();
        context.Response.Body = capture;

        try
        {
            await _next(context);

            capture.Position = 0;
            string responseBody;

            using (var reader = new StreamReader(capture, Encoding.UTF8, leaveOpen: true))
            {
                responseBody = await reader.ReadToEndAsync();
            }

            // Save to database only if the response indicates success (2xx)
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                db.IdempotencyKeys.Add(new IdempotencyKey
                {
                    Key = key,
                    Response = responseBody,
                    StatusCode = context.Response.StatusCode,
                    CreatedDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddHours(1)
                });

                await db.SaveChangesAsync();
            }

            capture.Position = 0;
            await capture.CopyToAsync(original);
        }
        finally
        {
            context.Response.Body = original;
        }
    }
}