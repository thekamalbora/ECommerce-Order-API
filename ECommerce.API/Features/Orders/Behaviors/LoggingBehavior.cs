using System.Diagnostics;
using MediatR;

namespace ECommerce.API.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var watch = Stopwatch.StartNew();

        _logger.LogInformation(
            "REQUEST → {Request} {@Data}",
            requestName,
            request);

        var response = await next();

        watch.Stop();

        _logger.LogInformation(
            "RESPONSE → {Request} completed in {Time} ms Result {@Response}",
            requestName,
            watch.ElapsedMilliseconds,
            response);

        return response;
    }
}