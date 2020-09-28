1. PM> Add-Migration InitialCreate -Context HomeContext -OutputDir "Data\Migrations"

2. Add to MigrationBuilder.Up(...) method:
            migrationBuilder.Sql(Zs.Bot.Model.Data.BotContext.GetOtherSqlScripts());
            migrationBuilder.Sql(HomeContext.GetOtherSqlScripts());

3. PM> Update-Database -Migration InitialCreate