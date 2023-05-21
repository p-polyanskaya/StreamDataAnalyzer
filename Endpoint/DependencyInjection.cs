using FluentMigrator.Runner;
using Migration;

namespace EndPoint;

public static class DependencyInjection
{
    public static void SetPostgres(this IServiceCollection services)
    {
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(
                    "Server=127.0.0.1;Port=5432;Userid=postgres;Password=postgres;Database=course_db")
                .ScanIn(typeof(CreatePostgresTable).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());
    }
}