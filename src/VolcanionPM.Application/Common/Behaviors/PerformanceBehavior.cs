using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace VolcanionPM.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that monitors performance and logs slow requests
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _timer;
    private const int SlowRequestThresholdMs = 500;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _timer = new Stopwatch();
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > SlowRequestThresholdMs)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogWarning(
                "Slow Request: {RequestName} took {ElapsedMilliseconds}ms. Request: {@Request}",
                requestName,
                elapsedMilliseconds,
                request);
        }

        return response;
    }
}
