using Application;
using Consumers;
using GrpcServices;
using Microsoft.Spark.Hadoop.Conf;
using Options;
using Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RedisConnection>(builder.Configuration.GetSection(nameof(RedisConnection)));
builder.Services.Configure<PostgresConnection>(builder.Configuration.GetSection(nameof(PostgresConnection)));
builder.Services.Configure<ConsumersSettings>(builder.Configuration.GetSection(nameof(ConsumersSettings)));
builder.Services.Configure<ProducersSettings>(builder.Configuration.GetSection(nameof(ProducersSettings)));

builder.Services.AddHostedService<Consumer>();
builder.Services.AddMediatR(x =>
    x.RegisterServicesFromAssemblies(typeof(HandleStreamMessageCommand.Handler).Assembly));
builder.Services.AddScoped<RedisConnectorHelper>();
builder.Services.AddScoped<RedisOperations>();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();


var app = builder.Build();

app.MapGrpcService<StreamGrpcService>();
app.MapGrpcReflectionService();

app.Run();