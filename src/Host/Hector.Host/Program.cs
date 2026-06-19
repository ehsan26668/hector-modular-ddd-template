using Hector.BuildingBlocks.Application;
using Hector.BuildingBlocks.Persistence;
using Hector.BuildingBlocks.Application.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHectorApplicationBuildingBlocks();
builder.Services.AddHectorPersistenceBuildingBlocks();

builder.Services.AddModules(builder.Configuration);

var app = builder.Build();

app.Run();

public partial class Program;
