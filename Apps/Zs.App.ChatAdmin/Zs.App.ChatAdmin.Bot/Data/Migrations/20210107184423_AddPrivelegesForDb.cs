using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Zs.App.ChatAdmin.Data.Migrations
{
    public partial class AddPrivelegesForDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "CHANNEL",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 769, DateTimeKind.Local).AddTicks(6574));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "GROUP",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 769, DateTimeKind.Local).AddTicks(7085));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "PRIVATE",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 769, DateTimeKind.Local).AddTicks(7092));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "UNDEFINED",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 769, DateTimeKind.Local).AddTicks(7094));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chats",
                keyColumn: "chat_id",
                keyValue: -1,
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 770, DateTimeKind.Local).AddTicks(305));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chats",
                keyColumn: "chat_id",
                keyValue: 1,
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 770, DateTimeKind.Local).AddTicks(838));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/getuserstatistics",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 772, DateTimeKind.Local).AddTicks(9337));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/help",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(4139));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/nulltest",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(3656));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/sqlquery",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(4149));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/test",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(3086));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "AUD",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(43));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "CNT",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(50));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "DOC",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(46));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "LOC",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(49));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "OTH",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(52));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "PHT",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(41));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "SRV",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(51));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "STK",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(47));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "TXT",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(36));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "UKN",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 770, DateTimeKind.Local).AddTicks(9581));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "VID",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(44));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "VOI",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 771, DateTimeKind.Local).AddTicks(45));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "DC",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 768, DateTimeKind.Local).AddTicks(3505));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "FB",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 768, DateTimeKind.Local).AddTicks(3504));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "SK",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 768, DateTimeKind.Local).AddTicks(3502));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "TG",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 767, DateTimeKind.Local).AddTicks(2862));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "VK",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 768, DateTimeKind.Local).AddTicks(3486));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "ADMIN",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 770, DateTimeKind.Local).AddTicks(4042));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "MODERATOR",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 770, DateTimeKind.Local).AddTicks(4047));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "OWNER",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 770, DateTimeKind.Local).AddTicks(3561));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "USER",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 770, DateTimeKind.Local).AddTicks(4049));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "users",
                keyColumn: "user_id",
                keyValue: -10,
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 770, DateTimeKind.Local).AddTicks(7665));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "users",
                keyColumn: "user_id",
                keyValue: -1,
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 770, DateTimeKind.Local).AddTicks(8131));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "users",
                keyColumn: "user_id",
                keyValue: 1,
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 21, 44, 22, 770, DateTimeKind.Local).AddTicks(8136));

            string priveleges = @"GRANT CONNECT        ON DATABASE ""ChatAdmin""     TO app;
                                  GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA zl TO app;
                                  GRANT ALL PRIVILEGES ON SCHEMA zl                  TO app;
                                  GRANT ALL PRIVILEGES ON ALL TABLES    IN SCHEMA zl TO app;

                                  GRANT CONNECT        ON DATABASE ""ChatAdmin""      TO app;
                                  GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA bot TO app;
                                  GRANT ALL PRIVILEGES ON SCHEMA bot                  TO app;
                                  GRANT ALL PRIVILEGES ON ALL TABLES    IN SCHEMA bot TO app;";

            migrationBuilder.Sql(priveleges);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "CHANNEL",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 514, DateTimeKind.Local).AddTicks(8697));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "GROUP",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 514, DateTimeKind.Local).AddTicks(9591));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "PRIVATE",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 514, DateTimeKind.Local).AddTicks(9602));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "UNDEFINED",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 514, DateTimeKind.Local).AddTicks(9605));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chats",
                keyColumn: "chat_id",
                keyValue: -1,
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 515, DateTimeKind.Local).AddTicks(6119));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chats",
                keyColumn: "chat_id",
                keyValue: 1,
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 515, DateTimeKind.Local).AddTicks(6932));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/getuserstatistics",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 521, DateTimeKind.Local).AddTicks(1348));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/help",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 518, DateTimeKind.Local).AddTicks(923));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/nulltest",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 518, DateTimeKind.Local).AddTicks(86));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/sqlquery",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 518, DateTimeKind.Local).AddTicks(940));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/test",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 517, DateTimeKind.Local).AddTicks(8443));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "AUD",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 517, DateTimeKind.Local).AddTicks(4302));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "CNT",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 517, DateTimeKind.Local).AddTicks(4314));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "DOC",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 517, DateTimeKind.Local).AddTicks(4308));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "LOC",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 517, DateTimeKind.Local).AddTicks(4312));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "OTH",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 517, DateTimeKind.Local).AddTicks(4319));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "PHT",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 517, DateTimeKind.Local).AddTicks(4300));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "SRV",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 517, DateTimeKind.Local).AddTicks(4316));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "STK",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 517, DateTimeKind.Local).AddTicks(4310));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "TXT",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 517, DateTimeKind.Local).AddTicks(4293));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "UKN",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 517, DateTimeKind.Local).AddTicks(3426));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "VID",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 517, DateTimeKind.Local).AddTicks(4304));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "VOI",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 517, DateTimeKind.Local).AddTicks(4306));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "DC",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 512, DateTimeKind.Local).AddTicks(3792));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "FB",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 512, DateTimeKind.Local).AddTicks(3790));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "SK",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 512, DateTimeKind.Local).AddTicks(3787));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "TG",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 510, DateTimeKind.Local).AddTicks(7635));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "VK",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 512, DateTimeKind.Local).AddTicks(3757));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "ADMIN",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 516, DateTimeKind.Local).AddTicks(2415));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "MODERATOR",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 516, DateTimeKind.Local).AddTicks(2423));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "OWNER",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 516, DateTimeKind.Local).AddTicks(1613));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "USER",
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 516, DateTimeKind.Local).AddTicks(2427));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "users",
                keyColumn: "user_id",
                keyValue: -10,
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 516, DateTimeKind.Local).AddTicks(9030));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "users",
                keyColumn: "user_id",
                keyValue: -1,
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 516, DateTimeKind.Local).AddTicks(9884));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "users",
                keyColumn: "user_id",
                keyValue: 1,
                column: "insert_date",
                value: new DateTime(2020, 12, 11, 21, 9, 0, 516, DateTimeKind.Local).AddTicks(9896));
        }
    }
}
