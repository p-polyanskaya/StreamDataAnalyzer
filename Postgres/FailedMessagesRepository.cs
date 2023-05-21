using System.Text.Json;
using Dapper;
using Domain;
using Microsoft.Extensions.Options;
using Npgsql;
using Options;

namespace Postgres;

public class FailedMessagesRepository
{
    private readonly IOptions<PostgresConnection> _postgresOptions;

    private const string GetSqlScript = @"--FailedMessagesRepository.GetSqlScript
                                          select id, analysis_result as AnalysisResult, topic from failed_messages limit 100;";

    private const string DeleteSqlScript = @"--FailedMessagesRepository.DeleteSqlScript 
                                             delete from failed_messages where id = any(@Ids)";

    private const string InsertSqlScript = @"--DeleteSqlScript.InsertSqlScript
                                             insert into failed_messages (id, analysis_result, topic) 
                                             values (@Id, @AnalysisResult::jsonb, @Topic)";

    public FailedMessagesRepository(IOptions<PostgresConnection> postgresOptions)
    {
        _postgresOptions = postgresOptions;
    }

    public async Task<IReadOnlyCollection<FailedMessage>> Get()
    {
        using (var connection = new NpgsqlConnection(_postgresOptions.Value.Connection))
        {
            var failedMessagesDto = await connection.QueryAsync<FailedMessageDto>(GetSqlScript);

            var failedMessages = failedMessagesDto.Select(message =>
                    new FailedMessage
                    {
                        Id = message.Id, Topic = message.Topic,
                        AnalysisResult = JsonSerializer.Deserialize<AnalysisResult>(
                            message.AnalysisResult,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    })
                .ToList();

            return failedMessages;
        }
    }

    public async Task Delete(IReadOnlyCollection<FailedMessage> messages)
    {
        using (var connection = new NpgsqlConnection(_postgresOptions.Value.Connection))
        {
            var queryArgs = new { Ids = messages.Select(message => message.Id).ToList() };
            await connection.ExecuteAsync(DeleteSqlScript, queryArgs);
        }
    }

    public async Task Insert(FailedMessage message)
    {
        using (var connection = new NpgsqlConnection(_postgresOptions.Value.Connection))
        {
            await connection.ExecuteAsync(InsertSqlScript,
                new
                {
                    Id = message.Id,
                    AnalysisResult = JsonSerializer.Serialize(message.AnalysisResult),
                    Topic = message.Topic
                });
        }
    }
}