using Hector.BuildingBlocks.Application.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence.Transactions;

public sealed class EfCoreTransactionPipelineBehavior<TRequest, TResponse>(
    DbContext dbContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly DbContext _dbContext = dbContext
        ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            return await next().ConfigureAwait(false);
        }

        await using var transaction = await _dbContext
            .Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var response = await next().ConfigureAwait(false);

        await _dbContext
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        await transaction
            .CommitAsync(cancellationToken)
            .ConfigureAwait(false);

        return response;
    }
}
