using FluentMigrator;

namespace Migration;

[Migration(20180430121801)]
public class CreatePostgresTable : FluentMigrator.Migration
{
    public override void Up()
    {
        Create.Table("failed_messages")
            .WithColumn("id").AsGuid()
            .WithColumn("analysis_result").AsCustom("jsonb")
            .WithColumn("topic").AsString();
    }

    public override void Down()
    {
        Delete.Table("failed_messages");
    }
}