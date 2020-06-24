﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Zs.Bot.Model.Db
{
    #region Interfaces bot

    /// <summary> Bots info </summary>
    public interface IBot
    {
        public Int32 BotId { get; set; }

        public String MessengerCode { get; set; }

        public String BotName { get; set; }

        public String BotToken { get; set; }

        public String BotDescription { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

    }


    /// <summary> Chat types (group, private, etc.) </summary>
    public interface IChatType
    {
        public String ChatTypeCode { get; set; }

        public String ChatTypeName { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

    }


    /// <summary> Chats info </summary>
    public interface IChat
    {
        public Int32 ChatId { get; set; }

        public String ChatName { get; set; }

        public String ChatDescription { get; set; }

        public String ChatTypeCode { get; set; }

        public String RawData { get; set; }

        public String RawDataHash { get; set; }

        public String RawDataHistory { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

    }


    /// <summary> Параметры приложения </summary>
    public interface ICommand
    {
        public String CommandName { get; set; }

        public String CommandScript { get; set; }

        public String CommandDefaultArgs { get; set; }

        public String CommandDesc { get; set; }

        public String CommandGroup { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

    }


    /// <summary> Журнал </summary>
    public interface ILog
    {
        public Int64 LogId { get; set; }

        public String LogType { get; set; }

        public String LogGroup { get; set; }

        public String LogMessage { get; set; }

        public String LogData { get; set; }

        public DateTime InsertDate { get; set; }

    }


    /// <summary> Типы сообщений </summary>
    public interface IMessageType
    {
        public String MessageTypeCode { get; set; }

        public String MessageTypeName { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

    }


    /// <summary> Принятые и отрпавленные сообщения </summary>
    public interface IMessage
    {
        public Int32 MessageId { get; set; }

        public Int64? ReplyToMessageId { get; set; }

        public String MessengerCode { get; set; }

        public String MessageTypeCode { get; set; }

        public Int32 UserId { get; set; }

        public Int32 ChatId { get; set; }

        public String MessageText { get; set; }

        public String RawData { get; set; }

        public Boolean IsSucceed { get; set; }

        public Int32 FailsCount { get; set; }

        public String FailDescription { get; set; }

        public Boolean IsDeleted { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

    }


    /// <summary> Система обмена сообщениями </summary>
    public interface IMessengerInfo
    {
        public String MessengerCode { get; set; }

        public String MessengerName { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

    }


    public interface IOption
    {
        public String OptionName { get; set; }

        public String OptionValue { get; set; }

        public String OptionGroup { get; set; }

        public String OptionDescription { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

    }


    /// <summary> Сессии пользователей - обслуживают последовательное общение с ботом </summary>
    public interface ISession
    {
        public Int32 SessionId { get; set; }

        public Int32 ChatId { get; set; }

        public Boolean SessionIsLoggedIn { get; set; }

        public String SessionCurrentState { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

    }


    /// <summary> Chat members </summary>
    public interface IUserRole
    {
        public String UserRoleCode { get; set; }

        public String UserRoleName { get; set; }

        public String UserRolePermissions { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

    }


    public interface IUser
    {
        public Int32 UserId { get; set; }

        public String UserName { get; set; }

        public String UserFullName { get; set; }

        public String UserRoleCode { get; set; }

        public Boolean UserIsBot { get; set; }

        public String RawData { get; set; }

        public String RawDataHash { get; set; }

        public String RawDataHistory { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

    }

    #endregion
    #region Classes bot

    /// <summary> Bots info </summary>
    [Table("bots", Schema = "bot")]
    public partial class DbBot : IBot
    {
        [Key]
        [Required(ErrorMessage = "Property 'BotId' is required")]
        [Column("bot_id", TypeName = "integer")]
        public Int32 BotId { get; set; }

        [Required(ErrorMessage = "Property 'MessengerCode' is required")]
        [Column("messenger_code", TypeName = "character varying")]
        public String MessengerCode { get; set; }

        [Required(ErrorMessage = "Property 'BotName' is required")]
        [Column("bot_name", TypeName = "character varying")]
        public String BotName { get; set; }

        [Required(ErrorMessage = "Property 'BotToken' is required")]
        [Column("bot_token", TypeName = "character varying")]
        public String BotToken { get; set; }

        [Column("bot_description", TypeName = "character varying")]
        public String BotDescription { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    /// <summary> Chat types (group, private, etc.) </summary>
    [Table("chat_types", Schema = "bot")]
    public partial class DbChatType : IChatType
    {
        [Key]
        [Required(ErrorMessage = "Property 'ChatTypeCode' is required")]
        [Column("chat_type_code", TypeName = "character varying")]
        public String ChatTypeCode { get; set; }

        [Required(ErrorMessage = "Property 'ChatTypeName' is required")]
        [Column("chat_type_name", TypeName = "character varying")]
        public String ChatTypeName { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    /// <summary> Chats info </summary>
    [Table("chats", Schema = "bot")]
    public partial class DbChat : IChat
    {
        [Key]
        [Required(ErrorMessage = "Property 'ChatId' is required")]
        [Column("chat_id", TypeName = "integer")]
        public Int32 ChatId { get; set; }

        [Required(ErrorMessage = "Property 'ChatName' is required")]
        [Column("chat_name", TypeName = "character varying")]
        public String ChatName { get; set; }

        [Column("chat_description", TypeName = "character varying")]
        public String ChatDescription { get; set; }

        [Required(ErrorMessage = "Property 'ChatTypeCode' is required")]
        [Column("chat_type_code", TypeName = "character varying")]
        public String ChatTypeCode { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public String RawData { get; set; }

        [Required(ErrorMessage = "Property 'RawDataHash' is required")]
        [Column("raw_data_hash", TypeName = "character varying")]
        public String RawDataHash { get; set; }

        [Column("raw_data_history", TypeName = "json")]
        public String RawDataHistory { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    /// <summary> Параметры приложения </summary>
    [Table("commands", Schema = "bot")]
    public partial class DbCommand : ICommand
    {
        [Key]
        [Required(ErrorMessage = "Property 'CommandName' is required")]
        [Column("command_name", TypeName = "character varying")]
        public String CommandName { get; set; }

        [Required(ErrorMessage = "Property 'CommandScript' is required")]
        [Column("command_script", TypeName = "character varying")]
        public String CommandScript { get; set; }

        [Column("command_default_args", TypeName = "character varying")]
        public String CommandDefaultArgs { get; set; }

        [Column("command_desc", TypeName = "character varying")]
        public String CommandDesc { get; set; }

        [Required(ErrorMessage = "Property 'CommandGroup' is required")]
        [Column("command_group", TypeName = "character varying")]
        public String CommandGroup { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    /// <summary> Журнал </summary>
    [Table("logs", Schema = "bot")]
    public partial class DbLog : ILog
    {
        [Key]
        [Required(ErrorMessage = "Property 'LogId' is required")]
        [Column("log_id", TypeName = "bigint")]
        public Int64 LogId { get; set; }

        [Required(ErrorMessage = "Property 'LogType' is required")]
        [Column("log_type", TypeName = "character varying")]
        public String LogType { get; set; }

        [Column("log_group", TypeName = "character varying")]
        public String LogGroup { get; set; }

        [Required(ErrorMessage = "Property 'LogMessage' is required")]
        [Column("log_message", TypeName = "character varying")]
        public String LogMessage { get; set; }

        [Column("log_data", TypeName = "json")]
        public String LogData { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    /// <summary> Типы сообщений </summary>
    [Table("message_types", Schema = "bot")]
    public partial class DbMessageType : IMessageType
    {
        [Key]
        [Required(ErrorMessage = "Property 'MessageTypeCode' is required")]
        [Column("message_type_code", TypeName = "character varying")]
        public String MessageTypeCode { get; set; }

        [Required(ErrorMessage = "Property 'MessageTypeName' is required")]
        [Column("message_type_name", TypeName = "character varying")]
        public String MessageTypeName { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    /// <summary> Принятые и отрпавленные сообщения </summary>
    [Table("messages", Schema = "bot")]
    public partial class DbMessage : IMessage
    {
        [Key]
        [Required(ErrorMessage = "Property 'MessageId' is required")]
        [Column("message_id", TypeName = "integer")]
        public Int32 MessageId { get; set; }

        [Column("reply_to_message_id", TypeName = "bigint")]
        public Int64? ReplyToMessageId { get; set; }

        [Required(ErrorMessage = "Property 'MessengerCode' is required")]
        [Column("messenger_code", TypeName = "character varying")]
        public String MessengerCode { get; set; }

        [Required(ErrorMessage = "Property 'MessageTypeCode' is required")]
        [Column("message_type_code", TypeName = "character varying")]
        public String MessageTypeCode { get; set; }

        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public Int32 UserId { get; set; }

        [Required(ErrorMessage = "Property 'ChatId' is required")]
        [Column("chat_id", TypeName = "integer")]
        public Int32 ChatId { get; set; }

        [Column("message_text", TypeName = "character varying")]
        public String MessageText { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public String RawData { get; set; }

        [Required(ErrorMessage = "Property 'IsSucceed' is required")]
        [Column("is_succeed", TypeName = "boolean")]
        public Boolean IsSucceed { get; set; }

        [Required(ErrorMessage = "Property 'FailsCount' is required")]
        [Column("fails_count", TypeName = "integer")]
        public Int32 FailsCount { get; set; }

        [Column("fail_description", TypeName = "json")]
        public String FailDescription { get; set; }

        [Required(ErrorMessage = "Property 'IsDeleted' is required")]
        [Column("is_deleted", TypeName = "boolean")]
        public Boolean IsDeleted { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    /// <summary> Система обмена сообщениями </summary>
    [Table("messengers", Schema = "bot")]
    public partial class DbMessengerInfo : IMessengerInfo
    {
        [Key]
        [Required(ErrorMessage = "Property 'MessengerCode' is required")]
        [Column("messenger_code", TypeName = "character varying")]
        public String MessengerCode { get; set; }

        [Required(ErrorMessage = "Property 'MessengerName' is required")]
        [Column("messenger_name", TypeName = "character varying")]
        public String MessengerName { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    [Table("options", Schema = "bot")]
    public partial class DbOption : IOption
    {
        [Key]
        [Required(ErrorMessage = "Property 'OptionName' is required")]
        [Column("option_name", TypeName = "character varying")]
        public String OptionName { get; set; }

        [Column("option_value", TypeName = "character varying")]
        public String OptionValue { get; set; }

        [Required(ErrorMessage = "Property 'OptionGroup' is required")]
        [Column("option_group", TypeName = "character varying")]
        public String OptionGroup { get; set; }

        [Column("option_description", TypeName = "character varying")]
        public String OptionDescription { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    /// <summary> Сессии пользователей - обслуживают последовательное общение с ботом </summary>
    [Table("sessions", Schema = "bot")]
    public partial class DbSession : ISession
    {
        [Key]
        [Required(ErrorMessage = "Property 'SessionId' is required")]
        [Column("session_id", TypeName = "integer")]
        public Int32 SessionId { get; set; }

        [Required(ErrorMessage = "Property 'ChatId' is required")]
        [Column("chat_id", TypeName = "integer")]
        public Int32 ChatId { get; set; }

        [Required(ErrorMessage = "Property 'SessionIsLoggedIn' is required")]
        [Column("session_is_logged_in", TypeName = "boolean")]
        public Boolean SessionIsLoggedIn { get; set; }

        [Required(ErrorMessage = "Property 'SessionCurrentState' is required")]
        [Column("session_current_state", TypeName = "json")]
        public String SessionCurrentState { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    /// <summary> Chat members </summary>
    [Table("user_roles", Schema = "bot")]
    public partial class DbUserRole : IUserRole
    {
        [Key]
        [Required(ErrorMessage = "Property 'UserRoleCode' is required")]
        [Column("user_role_code", TypeName = "character varying")]
        public String UserRoleCode { get; set; }

        [Required(ErrorMessage = "Property 'UserRoleName' is required")]
        [Column("user_role_name", TypeName = "character varying")]
        public String UserRoleName { get; set; }

        [Required(ErrorMessage = "Property 'UserRolePermissions' is required")]
        [Column("user_role_permissions", TypeName = "json")]
        public String UserRolePermissions { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    [Table("users", Schema = "bot")]
    public partial class DbUser : IUser
    {
        [Key]
        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public Int32 UserId { get; set; }

        [Required(ErrorMessage = "Property 'UserName' is required")]
        [Column("user_name", TypeName = "character varying")]
        public String UserName { get; set; }

        [Column("user_full_name", TypeName = "character varying")]
        public String UserFullName { get; set; }

        [Required(ErrorMessage = "Property 'UserRoleCode' is required")]
        [Column("user_role_code", TypeName = "character varying")]
        public String UserRoleCode { get; set; }

        [Required(ErrorMessage = "Property 'UserIsBot' is required")]
        [Column("user_is_bot", TypeName = "boolean")]
        public Boolean UserIsBot { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public String RawData { get; set; }

        [Required(ErrorMessage = "Property 'RawDataHash' is required")]
        [Column("raw_data_hash", TypeName = "character varying")]
        public String RawDataHash { get; set; }

        [Column("raw_data_history", TypeName = "json")]
        public String RawDataHistory { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }

    #endregion

    public partial class ZsBotDbContext : DbContext
    {
        public DbSet<DbBot> Bots { get; set; }
        public DbSet<DbChatType> ChatTypes { get; set; }
        public DbSet<DbChat> Chats { get; set; }
        public DbSet<DbCommand> Commands { get; set; }
        public DbSet<DbLog> Logs { get; set; }
        public DbSet<DbMessageType> MessageTypes { get; set; }
        public DbSet<DbMessage> Messages { get; set; }
        public DbSet<DbMessengerInfo> Messengers { get; set; }
        public DbSet<DbOption> Options { get; set; }
        public DbSet<DbSession> Sessions { get; set; }
        public DbSet<DbUserRole> UserRoles { get; set; }
        public DbSet<DbUser> Users { get; set; }
    }

}