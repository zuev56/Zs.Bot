1. PM> Add-Migration InitialCreate -Context HomeContext -OutputDir "Migrations"

2. Add to MigrationBuilder.Up(...) method:
            migrationBuilder.Sql(Zs.Bot.Data.BotContext.GetOtherSqlScripts());
            migrationBuilder.Sql(Model.Data.HomeContext.GetOtherSqlScripts());

3. PM> Update-Database -Migration InitialCreate