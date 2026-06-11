using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.BuildingBlocks.Application.UnitTests.TestDoubles;

internal sealed record TestCommand(string Name) : ICommand<string>;

internal sealed record TestQuery(int Value) : IQuery<int>;