namespace Hector.BuildingBlocks.Domain.Primitives;

public sealed record OutboxEventMetadata(string Name, int Version, Type ClrType);