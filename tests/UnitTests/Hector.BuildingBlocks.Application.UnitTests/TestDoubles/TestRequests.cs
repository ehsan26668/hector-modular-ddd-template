using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Results;

namespace Hector.BuildingBlocks.Application.UnitTests.TestDoubles;

internal sealed record TestCommand(string Name) : ICommand<string>;

internal sealed record TestQuery(int Value) : IQuery<int>;

internal sealed record TestResultCommand(string Name) : IRequest<Result>;