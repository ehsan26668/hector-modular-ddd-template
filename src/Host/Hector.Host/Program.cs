using Hector.BuildingBlocks.Application.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddModules(builder.Configuration);

var app = builder.Build();

app.Run();
