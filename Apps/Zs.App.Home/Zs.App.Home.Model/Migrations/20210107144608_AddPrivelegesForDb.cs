using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Zs.App.Home.Data.Migrations
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
                value: new DateTime(2021, 1, 7, 17, 46, 8, 22, DateTimeKind.Local).AddTicks(4606));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "GROUP",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 22, DateTimeKind.Local).AddTicks(5134));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "PRIVATE",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 22, DateTimeKind.Local).AddTicks(5139));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "UNDEFINED",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 22, DateTimeKind.Local).AddTicks(5141));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chats",
                keyColumn: "chat_id",
                keyValue: -1,
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 22, DateTimeKind.Local).AddTicks(8785));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chats",
                keyColumn: "chat_id",
                keyValue: 1,
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 22, DateTimeKind.Local).AddTicks(9283));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/help",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(5921));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/nulltest",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(5271));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/sqlquery",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(5939));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/test",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(4814));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "AUD",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(2370));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "CNT",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(2377));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "DOC",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(2374));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "LOC",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(2376));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "OTH",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(2380));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "PHT",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(2369));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "SRV",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(2379));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "STK",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(2375));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "TXT",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(2364));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "UKN",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(1892));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "VID",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(2371));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "VOI",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(2373));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "DC",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 21, DateTimeKind.Local).AddTicks(464));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "FB",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 21, DateTimeKind.Local).AddTicks(462));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "SK",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 21, DateTimeKind.Local).AddTicks(460));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "TG",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 19, DateTimeKind.Local).AddTicks(8442));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "VK",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 21, DateTimeKind.Local).AddTicks(446));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "ADMIN",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 23, DateTimeKind.Local).AddTicks(1907));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "MODERATOR",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 23, DateTimeKind.Local).AddTicks(1912));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "OWNER",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 23, DateTimeKind.Local).AddTicks(1432));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "USER",
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 23, DateTimeKind.Local).AddTicks(1914));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "users",
                keyColumn: "user_id",
                keyValue: -10,
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 23, DateTimeKind.Local).AddTicks(9752));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "users",
                keyColumn: "user_id",
                keyValue: -1,
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(327));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "users",
                keyColumn: "user_id",
                keyValue: 1,
                column: "insert_date",
                value: new DateTime(2021, 1, 7, 17, 46, 8, 24, DateTimeKind.Local).AddTicks(332));

            string priveleges = @"GRANT CONNECT        ON DATABASE ""HomeNew""       TO app;
                                  GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA vk TO app;
                                  GRANT ALL PRIVILEGES ON SCHEMA vk                  TO app;
                                  GRANT ALL PRIVILEGES ON ALL TABLES    IN SCHEMA vk TO app;

                                  GRANT CONNECT        ON DATABASE ""HomeNew""        TO app;
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
                value: new DateTime(2020, 12, 12, 16, 57, 9, 496, DateTimeKind.Local).AddTicks(9606));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "GROUP",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(148));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "PRIVATE",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(152));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chat_types",
                keyColumn: "chat_type_code",
                keyValue: "UNDEFINED",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(155));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chats",
                keyColumn: "chat_id",
                keyValue: -1,
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(4076));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "chats",
                keyColumn: "chat_id",
                keyValue: 1,
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(4646));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/help",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(7940));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/nulltest",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(7495));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/sqlquery",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(7949));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "commands",
                keyColumn: "command_name",
                keyValue: "/test",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(7015));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "AUD",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4543));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "CNT",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4552));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "DOC",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4548));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "LOC",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4551));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "OTH",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4554));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "PHT",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4542));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "SRV",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4553));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "STK",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4549));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "TXT",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4537));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "UKN",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4044));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "VID",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4545));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "message_types",
                keyColumn: "message_type_code",
                keyValue: "VOI",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4547));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "DC",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 495, DateTimeKind.Local).AddTicks(6044));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "FB",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 495, DateTimeKind.Local).AddTicks(6043));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "SK",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 495, DateTimeKind.Local).AddTicks(6040));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "TG",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 494, DateTimeKind.Local).AddTicks(5629));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "messengers",
                keyColumn: "messenger_code",
                keyValue: "VK",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 495, DateTimeKind.Local).AddTicks(6015));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "ADMIN",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(7553));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "MODERATOR",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(7557));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "OWNER",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(7059));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "user_roles",
                keyColumn: "user_role_code",
                keyValue: "USER",
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(7561));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "users",
                keyColumn: "user_id",
                keyValue: -10,
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(1374));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "users",
                keyColumn: "user_id",
                keyValue: -1,
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(1854));

            migrationBuilder.UpdateData(
                schema: "bot",
                table: "users",
                keyColumn: "user_id",
                keyValue: 1,
                column: "insert_date",
                value: new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(1860));
        }
    }
}
