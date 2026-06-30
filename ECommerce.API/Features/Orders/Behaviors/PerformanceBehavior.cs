using System.Diagnostics;
using MediatR;

namespace ECommerce.API.Behaviors;

public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
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

        var response = await next();

        watch.Stop();
        var elapsed = watch.ElapsedMilliseconds;

        if (elapsed > 1000)
        {
            _logger.LogWarning(
                "SLOW REQUEST → {Request} took {Time} ms",
                requestName,
                elapsed);
        }
        else
        {
            _logger.LogInformation(
                "PERFORMANCE → {Request} took {Time} ms",
                requestName,
                elapsed);
        }

        return response;
    }
}