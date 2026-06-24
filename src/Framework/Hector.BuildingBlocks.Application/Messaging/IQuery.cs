using Hector.BuildingBlocks.Application.Results;

namespace Hector.BuildingBlocks.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}