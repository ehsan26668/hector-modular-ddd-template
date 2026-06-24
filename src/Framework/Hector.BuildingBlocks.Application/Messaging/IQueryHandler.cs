using Hector.BuildingBlocks.Application.Results;

namespace Hector.BuildingBlocks.Application.Messaging;

public interface IQueryHandler<in TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}