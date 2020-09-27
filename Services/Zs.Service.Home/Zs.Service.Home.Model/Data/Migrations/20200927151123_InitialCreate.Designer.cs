﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Zs.Service.Home.Model.Data;

namespace Zs.Service.Home.Model.Data.Migrations
{
    [DbContext(typeof(HomeContext))]
    [Migration("20200927151123_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Zs.Bot.Model.Chat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("chat_id")
                        .HasColumnType("int")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<string>("ChatTypeCode")
                        .IsRequired()
                        .HasColumnName("chat_type_code")
                        .HasColumnType("character varying(10)")
                        .HasMaxLength(10);

                    b.Property<string>("Description")
                        .HasColumnName("chat_description")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<DateTime>("InsertDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("insert_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("chat_name")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<string>("RawData")
                        .IsRequired()
                        .HasColumnName("raw_data")
                        .HasColumnType("json");

                    b.Property<string>("RawDataHash")
                        .IsRequired()
                        .HasColumnName("raw_data_hash")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<string>("RawDataHistory")
                        .HasColumnName("raw_data_history")
                        .HasColumnType("json");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("update_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Id");

                    b.HasIndex("ChatTypeCode");

                    b.ToTable("chats","bot");

                    b.HasData(
                        new
                        {
                            Id = -1,
                            ChatTypeCode = "PRIVATE",
                            Description = "UnitTestChat",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 915, DateTimeKind.Local).AddTicks(356),
                            Name = "UnitTestChat",
                            RawData = "{ \"test\": \"test\" }",
                            RawDataHash = "-1063294487",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Id = 1,
                            ChatTypeCode = "PRIVATE",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 915, DateTimeKind.Local).AddTicks(865),
                            Name = "zuev56",
                            RawData = "{ \"Id\": 210281448 }",
                            RawDataHash = "-1063294487",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        });
                });

            modelBuilder.Entity("Zs.Bot.Model.ChatType", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnName("chat_type_code")
                        .HasColumnType("character varying(10)")
                        .HasMaxLength(10);

                    b.Property<DateTime>("InsertDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("insert_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("chat_type_name")
                        .HasColumnType("character varying(10)")
                        .HasMaxLength(10);

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("update_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Code");

                    b.ToTable("chat_types","bot");

                    b.HasData(
                        new
                        {
                            Code = "CHANNEL",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 914, DateTimeKind.Local).AddTicks(5319),
                            Name = "Channel",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "GROUP",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 914, DateTimeKind.Local).AddTicks(6774),
                            Name = "Group",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "PRIVATE",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 914, DateTimeKind.Local).AddTicks(6783),
                            Name = "Private",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "UNDEFINED",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 914, DateTimeKind.Local).AddTicks(6785),
                            Name = "Undefined",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        });
                });

            modelBuilder.Entity("Zs.Bot.Model.Command", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnName("command_name")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<string>("DefaultArgs")
                        .HasColumnName("command_default_args")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Description")
                        .HasColumnName("command_desc")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Group")
                        .IsRequired()
                        .HasColumnName("command_group")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<DateTime>("InsertDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("insert_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Script")
                        .IsRequired()
                        .HasColumnName("command_script")
                        .HasColumnType("character varying(5000)")
                        .HasMaxLength(5000);

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("update_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Name");

                    b.ToTable("commands","bot");

                    b.HasData(
                        new
                        {
                            Name = "/test",
                            Description = "Тестовый запрос к боту. Возвращает ''Test''",
                            Group = "moderatorCmdGroup",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(3394),
                            Script = "SELECT 'Test'",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Name = "/nulltest",
                            Description = "Тестовый запрос к боту. Возвращает NULL",
                            Group = "moderatorCmdGroup",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(3895),
                            Script = "SELECT null",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Name = "/help",
                            DefaultArgs = "<UserRoleCode>",
                            Description = "Получение справки по доступным функциям",
                            Group = "userCmdGroup",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(4353),
                            Script = "SELECT bot.sf_cmd_get_help({0})",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Name = "/sqlquery",
                            DefaultArgs = "select 'Pass your query as parameter in double quotes'",
                            Description = "SQL-запрос",
                            Group = "adminCmdGroup",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(4364),
                            Script = "select (with userQuery as ({0}) select json_agg(q) from userQuery q)",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        });
                });

            modelBuilder.Entity("Zs.Bot.Model.Log", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("log_id")
                        .HasColumnType("int")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<string>("Data")
                        .HasColumnName("log_data")
                        .HasColumnType("json");

                    b.Property<string>("Initiator")
                        .HasColumnName("log_initiator")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<DateTime>("InsertDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("insert_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnName("log_message")
                        .HasColumnType("character varying(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnName("log_type")
                        .HasColumnType("character varying(7)")
                        .HasMaxLength(7);

                    b.HasKey("Id");

                    b.ToTable("logs","bot");
                });

            modelBuilder.Entity("Zs.Bot.Model.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("message_id")
                        .HasColumnType("int")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<int>("ChatId")
                        .HasColumnName("chat_id")
                        .HasColumnType("integer");

                    b.Property<string>("FailDescription")
                        .HasColumnName("fail_description")
                        .HasColumnType("json");

                    b.Property<int>("FailsCount")
                        .HasColumnName("fails_count")
                        .HasColumnType("integer");

                    b.Property<DateTime>("InsertDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("insert_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<bool>("IsDeleted")
                        .HasColumnName("is_deleted")
                        .HasColumnType("bool");

                    b.Property<bool>("IsSucceed")
                        .HasColumnName("is_succeed")
                        .HasColumnType("bool");

                    b.Property<string>("MessageTypeCode")
                        .IsRequired()
                        .HasColumnName("message_type_code")
                        .HasColumnType("character varying(3)")
                        .HasMaxLength(3);

                    b.Property<string>("MessengerCode")
                        .IsRequired()
                        .HasColumnName("messenger_code")
                        .HasColumnType("character varying(2)")
                        .HasMaxLength(2);

                    b.Property<string>("RawData")
                        .IsRequired()
                        .HasColumnName("raw_data")
                        .HasColumnType("json");

                    b.Property<string>("RawDataHash")
                        .IsRequired()
                        .HasColumnName("raw_data_hash")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<string>("RawDataHistory")
                        .HasColumnName("raw_data_history")
                        .HasColumnType("json");

                    b.Property<int?>("ReplyToMessageId")
                        .HasColumnName("reply_to_message_id")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .HasColumnName("message_text")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("update_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<int>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ChatId");

                    b.HasIndex("MessageTypeCode");

                    b.HasIndex("MessengerCode");

                    b.HasIndex("ReplyToMessageId");

                    b.HasIndex("UserId");

                    b.ToTable("messages","bot");
                });

            modelBuilder.Entity("Zs.Bot.Model.MessageType", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnName("message_type_code")
                        .HasColumnType("character varying(3)")
                        .HasMaxLength(3);

                    b.Property<DateTime>("InsertDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("insert_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("message_type_name")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("update_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Code");

                    b.ToTable("message_types","bot");

                    b.HasData(
                        new
                        {
                            Code = "UKN",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(247),
                            Name = "Unknown",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "TXT",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(753),
                            Name = "Text",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "PHT",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(756),
                            Name = "Photo",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "AUD",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(758),
                            Name = "Audio",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "VID",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(759),
                            Name = "Video",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "VOI",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(761),
                            Name = "Voice",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "DOC",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(762),
                            Name = "Document",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "STK",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(765),
                            Name = "Sticker",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "LOC",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(766),
                            Name = "Location",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "CNT",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(768),
                            Name = "Contact",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "SRV",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(769),
                            Name = "Service message",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "OTH",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 916, DateTimeKind.Local).AddTicks(770),
                            Name = "Other",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        });
                });

            modelBuilder.Entity("Zs.Bot.Model.MessengerInfo", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnName("messenger_code")
                        .HasColumnType("character varying(2)")
                        .HasMaxLength(2);

                    b.Property<DateTime>("InsertDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("insert_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("messenger_name")
                        .HasColumnType("character varying(20)")
                        .HasMaxLength(20);

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("update_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Code");

                    b.ToTable("messengers","bot");

                    b.HasData(
                        new
                        {
                            Code = "TG",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 911, DateTimeKind.Local).AddTicks(4729),
                            Name = "Telegram",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "VK",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 913, DateTimeKind.Local).AddTicks(86),
                            Name = "Вконтакте",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "SK",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 913, DateTimeKind.Local).AddTicks(109),
                            Name = "Skype",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "FB",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 913, DateTimeKind.Local).AddTicks(110),
                            Name = "Facebook",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "DC",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 913, DateTimeKind.Local).AddTicks(112),
                            Name = "Discord",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        });
                });

            modelBuilder.Entity("Zs.Bot.Model.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("user_id")
                        .HasColumnType("int")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<string>("FullName")
                        .HasColumnName("user_full_name")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<DateTime>("InsertDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("insert_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<bool>("IsBot")
                        .HasColumnName("user_is_bot")
                        .HasColumnType("bool");

                    b.Property<string>("Name")
                        .HasColumnName("user_name")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<string>("RawData")
                        .IsRequired()
                        .HasColumnName("raw_data")
                        .HasColumnType("json");

                    b.Property<string>("RawDataHash")
                        .IsRequired()
                        .HasColumnName("raw_data_hash")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<string>("RawDataHistory")
                        .HasColumnName("raw_data_history")
                        .HasColumnType("json");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("update_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("UserRoleCode")
                        .IsRequired()
                        .HasColumnName("user_role_code")
                        .HasColumnType("character varying(10)")
                        .HasMaxLength(10);

                    b.HasKey("Id");

                    b.HasIndex("UserRoleCode");

                    b.ToTable("users","bot");

                    b.HasData(
                        new
                        {
                            Id = -10,
                            FullName = "for exported message reading",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 915, DateTimeKind.Local).AddTicks(7533),
                            IsBot = false,
                            Name = "Unknown",
                            RawData = "{ \"test\": \"test\" }",
                            RawDataHash = "-1063294487",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            UserRoleCode = "USER"
                        },
                        new
                        {
                            Id = -1,
                            FullName = "UnitTest",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 915, DateTimeKind.Local).AddTicks(8024),
                            IsBot = false,
                            Name = "UnitTestUser",
                            RawData = "{ \"test\": \"test\" }",
                            RawDataHash = "-1063294487",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            UserRoleCode = "USER"
                        },
                        new
                        {
                            Id = 1,
                            FullName = "Сергей Зуев",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 915, DateTimeKind.Local).AddTicks(8028),
                            IsBot = false,
                            Name = "zuev56",
                            RawData = "{ \"Id\": 210281448 }",
                            RawDataHash = "-1063294487",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            UserRoleCode = "ADMIN"
                        });
                });

            modelBuilder.Entity("Zs.Bot.Model.UserRole", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnName("user_role_code")
                        .HasColumnType("character varying(10)")
                        .HasMaxLength(10);

                    b.Property<DateTime>("InsertDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("insert_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("user_role_name")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<string>("Permissions")
                        .IsRequired()
                        .HasColumnName("user_role_permissions")
                        .HasColumnType("json");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("update_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Code");

                    b.ToTable("user_roles","bot");

                    b.HasData(
                        new
                        {
                            Code = "OWNER",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 915, DateTimeKind.Local).AddTicks(3118),
                            Name = "Owner",
                            Permissions = "[ \"All\" ]",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "ADMIN",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 915, DateTimeKind.Local).AddTicks(3625),
                            Name = "Administrator",
                            Permissions = "[ \"adminCmdGroup\", \"moderatorCmdGroup\", \"userCmdGroup\" ]",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "MODERATOR",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 915, DateTimeKind.Local).AddTicks(3629),
                            Name = "Moderator",
                            Permissions = "[ \"moderatorCmdGroup\", \"userCmdGroup\" ]",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Code = "USER",
                            InsertDate = new DateTime(2020, 9, 27, 18, 11, 22, 915, DateTimeKind.Local).AddTicks(3631),
                            Name = "User",
                            Permissions = "[ \"userCmdGroup\" ]",
                            UpdateDate = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        });
                });

            modelBuilder.Entity("Zs.Service.Home.Model.VkActivityLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("activity_log_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<DateTime>("InsertDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("insert_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<bool?>("IsOnline")
                        .HasColumnName("is_online")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsOnlineMobile")
                        .HasColumnName("is_online_mobile")
                        .HasColumnType("boolean");

                    b.Property<int>("LastSeen")
                        .HasColumnName("last_seen")
                        .HasColumnType("integer");

                    b.Property<int?>("OnlineApp")
                        .HasColumnName("online_app")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("integer");

                    b.Property<int?>("VkUserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("VkUserId");

                    b.ToTable("activity_log","vk");
                });

            modelBuilder.Entity("Zs.Service.Home.Model.VkUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("user_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<string>("FirstName")
                        .HasColumnName("first_name")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<DateTime>("InsertDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("insert_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("LastName")
                        .HasColumnName("last_name")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<string>("RawData")
                        .IsRequired()
                        .HasColumnName("raw_data")
                        .HasColumnType("json");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("update_date")
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.HasKey("Id");

                    b.ToTable("users","vk");
                });

            modelBuilder.Entity("Zs.Bot.Model.Chat", b =>
                {
                    b.HasOne("Zs.Bot.Model.ChatType", "ChatType")
                        .WithMany("Chats")
                        .HasForeignKey("ChatTypeCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Zs.Bot.Model.Message", b =>
                {
                    b.HasOne("Zs.Bot.Model.Chat", "Chat")
                        .WithMany("Messages")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Zs.Bot.Model.MessageType", "MessageType")
                        .WithMany("Messages")
                        .HasForeignKey("MessageTypeCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Zs.Bot.Model.MessengerInfo", "Messenger")
                        .WithMany("Messages")
                        .HasForeignKey("MessengerCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Zs.Bot.Model.Message", "ReplyToMessage")
                        .WithMany()
                        .HasForeignKey("ReplyToMessageId");

                    b.HasOne("Zs.Bot.Model.User", "User")
                        .WithMany("Messages")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Zs.Bot.Model.User", b =>
                {
                    b.HasOne("Zs.Bot.Model.UserRole", "UserRoles")
                        .WithMany("Users")
                        .HasForeignKey("UserRoleCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Zs.Service.Home.Model.VkActivityLog", b =>
                {
                    b.HasOne("Zs.Service.Home.Model.VkUser", "VkUser")
                        .WithMany()
                        .HasForeignKey("VkUserId");
                });
#pragma warning restore 612, 618
        }
    }
}
