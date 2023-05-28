using Application;
using Consumers;
using CronJob;
using Endpoint;
using EndPoint;
using GrpcServices;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.ML;
using Migration;
using Options;
using Postgres;
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

builder.Services.AddHangfire(x => x.UseMemoryStorage(new MemoryStorageOptions()));

builder.Services.AddHangfireServer();
builder.Services.AddScoped<RetryRedisJob>();

builder.Services.AddScoped<FailedMessagesRepository>();

//настройка миграций постгреса
builder.Services.SetPostgres();

builder.Services
    .AddPredictionEnginePool<ModelInput, ModelOutput>()
    .FromFile(
        modelName: "NewsRecommendation",
        filePath: "model.zip",
        watchForChanges: false);

var app = builder.Build();

app.Migrate();

app.MapGrpcService<StreamGrpcService>();
app.MapGrpcReflectionService();

var options = new BackgroundJobServerOptions
{
    SchedulePollingInterval = TimeSpan.FromMilliseconds(2000)
};
app.UseHangfireServer(options);
app.UseHangfireDashboard("/mydashboard");

RecurringJob.AddOrUpdate<RetryRedisJob>(nameof(RetryRedisJob), x => x.Execute(),  "*/2 * * * * *");
RecurringJob.AddOrUpdate<RetryPostgresJob>(nameof(RetryPostgresJob), x => x.Execute(),  "*/2 * * * * *");

app.Run();