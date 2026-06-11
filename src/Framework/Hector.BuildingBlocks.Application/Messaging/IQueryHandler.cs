namespace Hector.BuildingBlocks.Application.Messaging;

public interface IQueryHandler<TQuery, TResult>
    : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}