using FluentMigrator.Runner;
using Migration;

namespace EndPoint;

public static class DependencyInjection
{
    public static void SetPostgres(this IServiceCollection services, string connectionString)
    {
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(CreatePostgresTable).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());
    }
}