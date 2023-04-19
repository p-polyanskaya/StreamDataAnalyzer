using Application;
using Consumers;
using GrpcServices;
using Redis;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddHostedService<Consumer>();

builder.Services.AddMediatR(x =>
    x.RegisterServicesFromAssemblies(typeof(HandleStreamMessageCommand.Handler).Assembly));
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddScoped<RedisOperations>();

var app = builder.Build();

app.MapGrpcService<StreamGrpcService>();
app.MapGrpcReflectionService();

app.Run();