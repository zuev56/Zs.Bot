using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Zs.Service.ChatAdmin.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bot");

            migrationBuilder.EnsureSchema(
                name: "zl");

            migrationBuilder.CreateTable(
                name: "chat_types",
                schema: "bot",
                columns: table => new
                {
                    chat_type_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    chat_type_name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_types", x => x.chat_type_code);
                });

            migrationBuilder.CreateTable(
                name: "message_types",
                schema: "bot",
                columns: table => new
                {
                    message_type_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    message_type_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.user_role_code);
                });

            migrationBuilder.CreateTable(
                name: "accountings",
                schema: "zl",
                columns: table => new
                {
                    accounting_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    accounting_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountings", x => x.accounting_id);
                });

            migrationBuilder.CreateTable(
                name: "auxiliary_words",
                schema: "zl",
                columns: table => new
                {
                    the_word = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auxiliary_words", x => x.the_word);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "zl",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    notification_is_active = table.Column<bool>(type: "boolean", nullable: false),
                    notification_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    notification_month = table.Column<int>(type: "integer", nullable: true),
                    notification_day = table.Column<int>(type: "integer", nullable: false),
                    notification_hour = table.Column<int>(type: "integer", nullable: false),
                    notification_minute = table.Column<int>(type: "integer", nullable: false),
                    notification_exec_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.notification_id);
                });

            migrationBuilder.CreateTable(
                name: "chats",
                schema: "bot",
                columns: table => new
                {
                    chat_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    chat_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    chat_description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    chat_type_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    raw_data = table.Column<string>(type: "json", nullable: false),
                    raw_data_hash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raw_data_history = table.Column<string>(type: "json", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                name: "bots",
                schema: "bot",
                columns: table => new
                {
                    bot_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    messenger_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    bot_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    bot_token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bot_description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bots", x => x.bot_id);
                    table.ForeignKey(
                        name: "FK_bots_messengers_messenger_code",
                        column: x => x.messenger_code,
                        principalSchema: "bot",
                        principalTable: "messengers",
                        principalColumn: "messenger_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "bot",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_full_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_role_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    user_is_bot = table.Column<bool>(type: "bool", nullable: false),
                    raw_data = table.Column<string>(type: "json", nullable: false),
                    raw_data_hash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raw_data_history = table.Column<string>(type: "json", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    message_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                        name: "FK_messages_messengers_messenger_code",
                        column: x => x.messenger_code,
                        principalSchema: "bot",
                        principalTable: "messengers",
                        principalColumn: "messenger_code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_messages_reply_to_message_id",
                        column: x => x.reply_to_message_id,
                        principalSchema: "bot",
                        principalTable: "messages",
                        principalColumn: "message_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "bot",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bans",
                schema: "zl",
                columns: table => new
                {
                    ban_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    chat_id = table.Column<int>(type: "integer", nullable: false),
                    warning_message_id = table.Column<int>(type: "integer", nullable: true),
                    ban_finish_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    MessageId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bans", x => x.ban_id);
                    table.ForeignKey(
                        name: "FK_bans_chats_chat_id",
                        column: x => x.chat_id,
                        principalSchema: "bot",
                        principalTable: "chats",
                        principalColumn: "chat_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bans_messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "bot",
                        principalTable: "messages",
                        principalColumn: "message_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bans_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "bot",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bots_messenger_code",
                schema: "bot",
                table: "bots",
                column: "messenger_code");

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

            migrationBuilder.CreateIndex(
                name: "IX_bans_chat_id",
                schema: "zl",
                table: "bans",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "IX_bans_MessageId",
                schema: "zl",
                table: "bans",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_bans_user_id",
                schema: "zl",
                table: "bans",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bots",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "accountings",
                schema: "zl");

            migrationBuilder.DropTable(
                name: "auxiliary_words",
                schema: "zl");

            migrationBuilder.DropTable(
                name: "bans",
                schema: "zl");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "zl");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "bot");

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
