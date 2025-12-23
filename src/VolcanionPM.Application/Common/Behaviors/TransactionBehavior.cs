using MediatR;
using VolcanionPM.Application.Common.Interfaces;

namespace VolcanionPM.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that wraps command handlers in a transaction
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only wrap commands (not queries) in transactions
        var requestName = typeof(TRequest).Name;
        var isCommand = requestName.EndsWith("Command");

        if (!isCommand)
        {
            return await next();
        }

        // Execute within transaction
        try
        {
            var response = await next();
            // Transaction is committed by UnitOfWork.SaveChangesAsync in handlers
            return response;
        }
        catch
        {
            // Transaction will be rolled back automatically
            throw;
        }
    }
}
