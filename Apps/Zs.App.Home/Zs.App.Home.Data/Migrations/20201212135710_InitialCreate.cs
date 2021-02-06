using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Zs.App.Home.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vk");

            migrationBuilder.EnsureSchema(
                name: "bot");

            migrationBuilder.CreateTable(
                name: "activity_log",
                schema: "vk",
                columns: table => new
                {
                    activity_log_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    is_online = table.Column<bool>(type: "boolean", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    online_app = table.Column<int>(type: "integer", nullable: true),
                    is_online_mobile = table.Column<bool>(type: "boolean", nullable: false),
                    last_seen = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_log", x => x.activity_log_id);
                });

            migrationBuilder.CreateTable(
                name: "chat_types",
                schema: "bot",
                columns: table => new
                {
                    chat_type_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    chat_type_name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_types", x => x.chat_type_code);
                });

            migrationBuilder.CreateTable(
                name: "commands",
                schema: "bot",
                columns: table => new
                {
                    command_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    command_script = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    command_default_args = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    command_desc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    command_group = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commands", x => x.command_name);
                });

            migrationBuilder.CreateTable(
                name: "logs",
                schema: "bot",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    log_type = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    log_initiator = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    log_message = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    log_data = table.Column<string>(type: "json", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs", x => x.log_id);
                });

            migrationBuilder.CreateTable(
                name: "message_types",
                schema: "bot",
                columns: table => new
                {
                    message_type_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    message_type_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_types", x => x.message_type_code);
                });

            migrationBuilder.CreateTable(
                name: "messengers",
                schema: "bot",
                columns: table => new
                {
                    messenger_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    messenger_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messengers", x => x.messenger_code);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "bot",
                columns: table => new
                {
                    user_role_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    user_role_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_role_permissions = table.Column<string>(type: "json", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.user_role_code);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "vk",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    raw_data = table.Column<string>(type: "json", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "chats",
                schema: "bot",
                columns: table => new
                {
                    chat_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    chat_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    chat_description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    chat_type_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    raw_data = table.Column<string>(type: "json", nullable: false),
                    raw_data_hash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raw_data_history = table.Column<string>(type: "json", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chats", x => x.chat_id);
                    table.ForeignKey(
                        name: "FK_chats_chat_types_chat_type_code",
                        column: x => x.chat_type_code,
                        principalSchema: "bot",
                        principalTable: "chat_types",
                        principalColumn: "chat_type_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "bot",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_full_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_role_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    user_is_bot = table.Column<bool>(type: "bool", nullable: false),
                    raw_data = table.Column<string>(type: "json", nullable: false),
                    raw_data_hash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raw_data_history = table.Column<string>(type: "json", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_users_user_roles_user_role_code",
                        column: x => x.user_role_code,
                        principalSchema: "bot",
                        principalTable: "user_roles",
                        principalColumn: "user_role_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "bot",
                columns: table => new
                {
                    message_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    reply_to_message_id = table.Column<int>(type: "integer", nullable: true),
                    messenger_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    message_type_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    chat_id = table.Column<int>(type: "integer", nullable: false),
                    message_text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    raw_data = table.Column<string>(type: "json", nullable: false),
                    raw_data_hash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raw_data_history = table.Column<string>(type: "json", nullable: true),
                    is_succeed = table.Column<bool>(type: "bool", nullable: false),
                    fails_count = table.Column<int>(type: "integer", nullable: false),
                    fail_description = table.Column<string>(type: "json", nullable: true),
                    is_deleted = table.Column<bool>(type: "bool", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.message_id);
                    table.ForeignKey(
                        name: "FK_messages_chats_chat_id",
                        column: x => x.chat_id,
                        principalSchema: "bot",
                        principalTable: "chats",
                        principalColumn: "chat_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_message_types_message_type_code",
                        column: x => x.message_type_code,
                        principalSchema: "bot",
                        principalTable: "message_types",
                        principalColumn: "message_type_code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_messages_reply_to_message_id",
                        column: x => x.reply_to_message_id,
                        principalSchema: "bot",
                        principalTable: "messages",
                        principalColumn: "message_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_messengers_messenger_code",
                        column: x => x.messenger_code,
                        principalSchema: "bot",
                        principalTable: "messengers",
                        principalColumn: "messenger_code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "bot",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "chat_types",
                columns: new[] { "chat_type_code", "insert_date", "chat_type_name" },
                values: new object[,]
                {
                    { "CHANNEL", new DateTime(2020, 12, 12, 16, 57, 9, 496, DateTimeKind.Local).AddTicks(9606), "Channel" },
                    { "GROUP", new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(148), "Group" },
                    { "PRIVATE", new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(152), "Private" },
                    { "UNDEFINED", new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(155), "Undefined" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "commands",
                columns: new[] { "command_name", "command_default_args", "command_desc", "command_group", "insert_date", "command_script" },
                values: new object[,]
                {
                    { "/test", null, "Тестовый запрос к боту. Возвращает ''Test''", "moderatorCmdGroup", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(7015), "SELECT 'Test'" },
                    { "/nulltest", null, "Тестовый запрос к боту. Возвращает NULL", "moderatorCmdGroup", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(7495), "SELECT null" },
                    { "/help", "<UserRoleId>", "Получение справки по доступным функциям", "userCmdGroup", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(7940), "SELECT bot.sf_cmd_get_help({0})" },
                    { "/sqlquery", "select 'Pass your query as parameter in double quotes'", "SQL-запрос", "adminCmdGroup", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(7949), "select (with userQuery as ({0}) select json_agg(q) from userQuery q)" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "message_types",
                columns: new[] { "message_type_code", "insert_date", "message_type_name" },
                values: new object[,]
                {
                    { "OTH", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4554), "Other" },
                    { "SRV", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4553), "Service message" },
                    { "CNT", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4552), "Contact" },
                    { "LOC", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4551), "Location" },
                    { "STK", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4549), "Sticker" },
                    { "DOC", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4548), "Document" },
                    { "VID", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4545), "Video" },
                    { "AUD", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4543), "Audio" },
                    { "PHT", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4542), "Photo" },
                    { "TXT", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4537), "Text" },
                    { "UKN", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4044), "Unknown" },
                    { "VOI", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(4547), "Voice" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "messengers",
                columns: new[] { "messenger_code", "insert_date", "messenger_name" },
                values: new object[,]
                {
                    { "TG", new DateTime(2020, 12, 12, 16, 57, 9, 494, DateTimeKind.Local).AddTicks(5629), "Telegram" },
                    { "VK", new DateTime(2020, 12, 12, 16, 57, 9, 495, DateTimeKind.Local).AddTicks(6015), "Вконтакте" },
                    { "SK", new DateTime(2020, 12, 12, 16, 57, 9, 495, DateTimeKind.Local).AddTicks(6040), "Skype" },
                    { "FB", new DateTime(2020, 12, 12, 16, 57, 9, 495, DateTimeKind.Local).AddTicks(6043), "Facebook" },
                    { "DC", new DateTime(2020, 12, 12, 16, 57, 9, 495, DateTimeKind.Local).AddTicks(6044), "Discord" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "user_roles",
                columns: new[] { "user_role_code", "insert_date", "user_role_name", "user_role_permissions" },
                values: new object[,]
                {
                    { "MODERATOR", new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(7557), "Moderator", "[ \"moderatorCmdGroup\", \"userCmdGroup\" ]" },
                    { "OWNER", new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(7059), "Owner", "[ \"All\" ]" },
                    { "ADMIN", new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(7553), "Administrator", "[ \"adminCmdGroup\", \"moderatorCmdGroup\", \"userCmdGroup\" ]" },
                    { "USER", new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(7561), "User", "[ \"userCmdGroup\" ]" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "chats",
                columns: new[] { "chat_id", "chat_type_code", "chat_description", "insert_date", "chat_name", "raw_data", "raw_data_hash", "raw_data_history" },
                values: new object[,]
                {
                    { -1, "PRIVATE", "UnitTestChat", new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(4076), "UnitTestChat", "{ \"test\": \"test\" }", "-1063294487", null },
                    { 1, "PRIVATE", null, new DateTime(2020, 12, 12, 16, 57, 9, 497, DateTimeKind.Local).AddTicks(4646), "zuev56", "{ \"Id\": 210281448 }", "-1063294487", null }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "users",
                columns: new[] { "user_id", "user_full_name", "insert_date", "user_is_bot", "user_name", "raw_data", "raw_data_hash", "raw_data_history", "user_role_code" },
                values: new object[,]
                {
                    { 1, "Сергей Зуев", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(1860), false, "zuev56", "{ \"Id\": 210281448 }", "-1063294487", null, "ADMIN" },
                    { -10, "for exported message reading", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(1374), false, "Unknown", "{ \"test\": \"test\" }", "-1063294487", null, "USER" },
                    { -1, "UnitTest", new DateTime(2020, 12, 12, 16, 57, 9, 498, DateTimeKind.Local).AddTicks(1854), false, "UnitTestUser", "{ \"test\": \"test\" }", "-1063294487", null, "USER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_chats_chat_type_code",
                schema: "bot",
                table: "chats",
                column: "chat_type_code");

            migrationBuilder.CreateIndex(
                name: "IX_messages_chat_id",
                schema: "bot",
                table: "messages",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_message_type_code",
                schema: "bot",
                table: "messages",
                column: "message_type_code");

            migrationBuilder.CreateIndex(
                name: "IX_messages_messenger_code",
                schema: "bot",
                table: "messages",
                column: "messenger_code");

            migrationBuilder.CreateIndex(
                name: "IX_messages_reply_to_message_id",
                schema: "bot",
                table: "messages",
                column: "reply_to_message_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_user_id",
                schema: "bot",
                table: "messages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_user_role_code",
                schema: "bot",
                table: "users",
                column: "user_role_code");

            migrationBuilder.Sql(Zs.Bot.Data.BotContext.GetOtherSqlScripts());
            migrationBuilder.Sql(Data.HomeContext.GetOtherSqlScripts());
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_log",
                schema: "vk");

            migrationBuilder.DropTable(
                name: "commands",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "logs",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "users",
                schema: "vk");

            migrationBuilder.DropTable(
                name: "chats",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "message_types",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "messengers",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "users",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "chat_types",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "bot");
        }
    }
}
