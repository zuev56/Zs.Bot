1 PM> Add-Migration InitialChatAdminContext -Context ChatAdminContext -OutputDir "Data\Migrations"

2. Add to MigrationBuilder.Up(...) method:
            migrationBuilder.Sql(Zs.Bot.Data.BotContext.GetOtherSqlScripts("ChatAdmin"));
            migrationBuilder.Sql(ChatAdminContext.GetOtherSqlScripts("ChatAdmin"));

3. PM> Update-Database -Migration InitialChatAdminContext