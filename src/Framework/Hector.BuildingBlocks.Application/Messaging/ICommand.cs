using Hector.BuildingBlocks.Application.Results;

namespace Hector.BuildingBlocks.Application.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}