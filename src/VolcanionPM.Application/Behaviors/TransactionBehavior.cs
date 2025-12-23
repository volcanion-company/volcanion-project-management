using MediatR;
using VolcanionPM.Application.Common.Interfaces;

namespace VolcanionPM.Application.Behaviors;

/// <summary>
/// Pipeline behavior for handling database transactions
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
        // Skip transaction for queries (by convention, queries don't modify data)
        var requestName = typeof(TRequest).Name;
        if (requestName.Contains("Query", StringComparison.OrdinalIgnoreCase))
        {
            return await next();
        }

        // Execute command within transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return response;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
