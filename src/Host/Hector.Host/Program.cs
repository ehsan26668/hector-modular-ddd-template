using Hector.BuildingBlocks.Application;
using Hector.BuildingBlocks.Persistence;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHectorApplicationBuildingBlocks();
builder.Services.AddHectorPersistenceBuildingBlocks();
builder.Services.AddHectorWebBuildingBlocks();

builder.Services.AddModules(builder.Configuration);

var app = builder.Build();

app.UseHectorWebBuildingBlocks();

app.Run();

public partial class Program;
