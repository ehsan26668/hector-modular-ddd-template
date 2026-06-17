using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.Messaging;

public interface IModuleIdentity
{
    string Name { get; }

    void Register(IServiceCollection services, IConfiguration configuration);
}
