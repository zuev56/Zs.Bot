1. PM> Add-Migration InitialHomeContext -Context HomeContext -OutputDir "Migrations"

2.1 Add to MigrationBuilder.Up(...) method:
            migrationBuilder.Sql(Zs.Bot.Data.BotContext.GetOtherSqlScripts("HomeNew"));
            migrationBuilder.Sql(Data.HomeContext.GetOtherSqlScripts("HomeNew"));

2.2 Add to MigrationBuilder.Down(...) method:
            TODO

3. PM> Update-Database -Migration InitialHomeContext