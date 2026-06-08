namespace Hector.BuildingBlocks.Persistence.Outbox;

public interface IOutboxEventTypeResolver
{
    Type? Resolve(string typeName);
}