using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Zs.Bot.Model.Db
{
    #region Interfaces for schema 'bot'

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

        IBot DeepCopy();
    }


    /// <summary> Chat types (group, private, etc.) </summary>
    public interface IChatType
    {
        public String ChatTypeCode { get; set; }

        public String ChatTypeName { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

        IChatType DeepCopy();
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

        IChat DeepCopy();
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

        ICommand DeepCopy();
    }


    /// <summary> Журнал </summary>
    public interface ILog
    {
        public Int64 LogId { get; set; }

        public String LogType { get; set; }

        public String LogInitiator { get; set; }

        public String LogMessage { get; set; }

        public String LogData { get; set; }

        public DateTime InsertDate { get; set; }

        ILog DeepCopy();
    }


    /// <summary> Типы сообщений </summary>
    public interface IMessageType
    {
        public String MessageTypeCode { get; set; }

        public String MessageTypeName { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

        IMessageType DeepCopy();
    }


    /// <summary> Принятые и отрпавленные сообщения </summary>
    public interface IMessage
    {
        public Int32 MessageId { get; set; }

        public Int32? ReplyToMessageId { get; set; }

        public String MessengerCode { get; set; }

        public String MessageTypeCode { get; set; }

        public Int32 UserId { get; set; }

        public Int32 ChatId { get; set; }

        public String MessageText { get; set; }

        public String RawData { get; set; }

        public String RawDataHash { get; set; }

        public String RawDataHistory { get; set; }

        public Boolean IsSucceed { get; set; }

        public Int32 FailsCount { get; set; }

        public String FailDescription { get; set; }

        public Boolean IsDeleted { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

        IMessage DeepCopy();
    }


    /// <summary> Система обмена сообщениями </summary>
    public interface IMessengerInfo
    {
        public String MessengerCode { get; set; }

        public String MessengerName { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

        IMessengerInfo DeepCopy();
    }


    public interface IOption
    {
        public String OptionName { get; set; }

        public String OptionValue { get; set; }

        public String OptionGroup { get; set; }

        public String OptionDescription { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

        IOption DeepCopy();
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

        ISession DeepCopy();
    }


    /// <summary> Chat members </summary>
    public interface IUserRole
    {
        public String UserRoleCode { get; set; }

        public String UserRoleName { get; set; }

        public String UserRolePermissions { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime InsertDate { get; set; }

        IUserRole DeepCopy();
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

        IUser DeepCopy();
    }

    #endregion
    #region Classes for schema 'bot'

    /// <summary> Bots info </summary>
    [Table("bots", Schema = "bot")]
    public partial class DbBot : IBot
    {
        [Key]
        [Required(ErrorMessage = "Property 'BotId' is required")]
        [Column("bot_id", TypeName = "integer")]
        public Int32 BotId { get; set; }

        [StringLength(2)]
        [Required(ErrorMessage = "Property 'MessengerCode' is required")]
        [Column("messenger_code", TypeName = "character varying(2)")]
        public String MessengerCode { get; set; }

        [StringLength(20)]
        [Required(ErrorMessage = "Property 'BotName' is required")]
        [Column("bot_name", TypeName = "character varying(20)")]
        public String BotName { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Property 'BotToken' is required")]
        [Column("bot_token", TypeName = "character varying(100)")]
        public String BotToken { get; set; }

        [StringLength(300)]
        [Column("bot_description", TypeName = "character varying(300)")]
        public String BotDescription { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }


        public IBot DeepCopy()
        {
            return new DbBot
            {
                BotId = this.BotId,
                MessengerCode = this.MessengerCode,
                BotName = this.BotName,
                BotToken = this.BotToken,
                BotDescription = this.BotDescription,
                UpdateDate = this.UpdateDate,
                InsertDate = this.InsertDate,
            };
        }
    }


    /// <summary> Chat types (group, private, etc.) </summary>
    [Table("chat_types", Schema = "bot")]
    public partial class DbChatType : IChatType
    {
        [Key]
        [StringLength(10)]
        [Required(ErrorMessage = "Property 'ChatTypeCode' is required")]
        [Column("chat_type_code", TypeName = "character varying(10)")]
        public String ChatTypeCode { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Property 'ChatTypeName' is required")]
        [Column("chat_type_name", TypeName = "character varying(10)")]
        public String ChatTypeName { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }


        public IChatType DeepCopy()
        {
            return new DbChatType
            {
                ChatTypeCode = this.ChatTypeCode,
                ChatTypeName = this.ChatTypeName,
                UpdateDate = this.UpdateDate,
                InsertDate = this.InsertDate,
            };
        }
    }


    /// <summary> Chats info </summary>
    [Table("chats", Schema = "bot")]
    public partial class DbChat : IChat
    {
        [Key]
        [Required(ErrorMessage = "Property 'ChatId' is required")]
        [Column("chat_id", TypeName = "integer")]
        public Int32 ChatId { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'ChatName' is required")]
        [Column("chat_name", TypeName = "character varying(50)")]
        public String ChatName { get; set; }

        [StringLength(100)]
        [Column("chat_description", TypeName = "character varying(100)")]
        public String ChatDescription { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Property 'ChatTypeCode' is required")]
        [Column("chat_type_code", TypeName = "character varying(10)")]
        public String ChatTypeCode { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public String RawData { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'RawDataHash' is required")]
        [Column("raw_data_hash", TypeName = "character varying(50)")]
        public String RawDataHash { get; set; }

        [Column("raw_data_history", TypeName = "json")]
        public String RawDataHistory { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }


        public IChat DeepCopy()
        {
            return new DbChat
            {
                ChatId = this.ChatId,
                ChatName = this.ChatName,
                ChatDescription = this.ChatDescription,
                ChatTypeCode = this.ChatTypeCode,
                RawData = this.RawData,
                RawDataHash = this.RawDataHash,
                RawDataHistory = this.RawDataHistory,
                UpdateDate = this.UpdateDate,
                InsertDate = this.InsertDate,
            };
        }
    }


    /// <summary> Параметры приложения </summary>
    [Table("commands", Schema = "bot")]
    public partial class DbCommand : ICommand
    {
        [Key]
        [StringLength(50)]
        [Required(ErrorMessage = "Property 'CommandName' is required")]
        [Column("command_name", TypeName = "character varying(50)")]
        public String CommandName { get; set; }

        [StringLength(5000)]
        [Required(ErrorMessage = "Property 'CommandScript' is required")]
        [Column("command_script", TypeName = "character varying(5000)")]
        public String CommandScript { get; set; }

        [StringLength(100)]
        [Column("command_default_args", TypeName = "character varying(100)")]
        public String CommandDefaultArgs { get; set; }

        [StringLength(100)]
        [Column("command_desc", TypeName = "character varying(100)")]
        public String CommandDesc { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'CommandGroup' is required")]
        [Column("command_group", TypeName = "character varying(50)")]
        public String CommandGroup { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }


        public ICommand DeepCopy()
        {
            return new DbCommand
            {
                CommandName = this.CommandName,
                CommandScript = this.CommandScript,
                CommandDefaultArgs = this.CommandDefaultArgs,
                CommandDesc = this.CommandDesc,
                CommandGroup = this.CommandGroup,
                UpdateDate = this.UpdateDate,
                InsertDate = this.InsertDate,
            };
        }
    }


    /// <summary> Журнал </summary>
    [Table("logs", Schema = "bot")]
    public partial class DbLog : ILog
    {
        [Key]
        [Required(ErrorMessage = "Property 'LogId' is required")]
        [Column("log_id", TypeName = "bigint")]
        public Int64 LogId { get; set; }

        [StringLength(7)]
        [Required(ErrorMessage = "Property 'LogType' is required")]
        [Column("log_type", TypeName = "character varying(7)")]
        public String LogType { get; set; }

        [StringLength(50)]
        [Column("log_initiator", TypeName = "character varying(50)")]
        public String LogInitiator { get; set; }

        [StringLength(200)]
        [Required(ErrorMessage = "Property 'LogMessage' is required")]
        [Column("log_message", TypeName = "character varying(200)")]
        public String LogMessage { get; set; }

        [Column("log_data", TypeName = "json")]
        public String LogData { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }


        public ILog DeepCopy()
        {
            return new DbLog
            {
                LogId = this.LogId,
                LogType = this.LogType,
                LogInitiator = this.LogInitiator,
                LogMessage = this.LogMessage,
                LogData = this.LogData,
                InsertDate = this.InsertDate,
            };
        }
    }


    /// <summary> Типы сообщений </summary>
    [Table("message_types", Schema = "bot")]
    public partial class DbMessageType : IMessageType
    {
        [Key]
        [StringLength(3)]
        [Required(ErrorMessage = "Property 'MessageTypeCode' is required")]
        [Column("message_type_code", TypeName = "character varying(3)")]
        public String MessageTypeCode { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'MessageTypeName' is required")]
        [Column("message_type_name", TypeName = "character varying(50)")]
        public String MessageTypeName { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }


        public IMessageType DeepCopy()
        {
            return new DbMessageType
            {
                MessageTypeCode = this.MessageTypeCode,
                MessageTypeName = this.MessageTypeName,
                UpdateDate = this.UpdateDate,
                InsertDate = this.InsertDate,
            };
        }
    }


    /// <summary> Принятые и отрпавленные сообщения </summary>
    [Table("messages", Schema = "bot")]
    public partial class DbMessage : IMessage
    {
        [Key]
        [Required(ErrorMessage = "Property 'MessageId' is required")]
        [Column("message_id", TypeName = "integer")]
        public Int32 MessageId { get; set; }

        [Column("reply_to_message_id", TypeName = "integer")]
        public Int32? ReplyToMessageId { get; set; }

        [StringLength(2)]
        [Required(ErrorMessage = "Property 'MessengerCode' is required")]
        [Column("messenger_code", TypeName = "character varying(2)")]
        public String MessengerCode { get; set; }

        [StringLength(3)]
        [Required(ErrorMessage = "Property 'MessageTypeCode' is required")]
        [Column("message_type_code", TypeName = "character varying(3)")]
        public String MessageTypeCode { get; set; }

        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public Int32 UserId { get; set; }

        [Required(ErrorMessage = "Property 'ChatId' is required")]
        [Column("chat_id", TypeName = "integer")]
        public Int32 ChatId { get; set; }

        [StringLength(100)]
        [Column("message_text", TypeName = "character varying(100)")]
        public String MessageText { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public String RawData { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'RawDataHash' is required")]
        [Column("raw_data_hash", TypeName = "character varying(50)")]
        public String RawDataHash { get; set; }

        [Column("raw_data_history", TypeName = "json")]
        public String RawDataHistory { get; set; }

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


        public IMessage DeepCopy()
        {
            return new DbMessage
            {
                MessageId = this.MessageId,
                ReplyToMessageId = this.ReplyToMessageId,
                MessengerCode = this.MessengerCode,
                MessageTypeCode = this.MessageTypeCode,
                UserId = this.UserId,
                ChatId = this.ChatId,
                MessageText = this.MessageText,
                RawData = this.RawData,
                RawDataHash = this.RawDataHash,
                RawDataHistory = this.RawDataHistory,
                IsSucceed = this.IsSucceed,
                FailsCount = this.FailsCount,
                FailDescription = this.FailDescription,
                IsDeleted = this.IsDeleted,
                UpdateDate = this.UpdateDate,
                InsertDate = this.InsertDate,
            };
        }
    }


    /// <summary> Система обмена сообщениями </summary>
    [Table("messengers", Schema = "bot")]
    public partial class DbMessengerInfo : IMessengerInfo
    {
        [Key]
        [StringLength(2)]
        [Required(ErrorMessage = "Property 'MessengerCode' is required")]
        [Column("messenger_code", TypeName = "character varying(2)")]
        public String MessengerCode { get; set; }

        [StringLength(20)]
        [Required(ErrorMessage = "Property 'MessengerName' is required")]
        [Column("messenger_name", TypeName = "character varying(20)")]
        public String MessengerName { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }


        public IMessengerInfo DeepCopy()
        {
            return new DbMessengerInfo
            {
                MessengerCode = this.MessengerCode,
                MessengerName = this.MessengerName,
                UpdateDate = this.UpdateDate,
                InsertDate = this.InsertDate,
            };
        }
    }


    [Table("options", Schema = "bot")]
    public partial class DbOption : IOption
    {
        [Key]
        [StringLength(50)]
        [Required(ErrorMessage = "Property 'OptionName' is required")]
        [Column("option_name", TypeName = "character varying(50)")]
        public String OptionName { get; set; }

        [StringLength(5000)]
        [Column("option_value", TypeName = "character varying(5000)")]
        public String OptionValue { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'OptionGroup' is required")]
        [Column("option_group", TypeName = "character varying(50)")]
        public String OptionGroup { get; set; }

        [StringLength(500)]
        [Column("option_description", TypeName = "character varying(500)")]
        public String OptionDescription { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }


        public IOption DeepCopy()
        {
            return new DbOption
            {
                OptionName = this.OptionName,
                OptionValue = this.OptionValue,
                OptionGroup = this.OptionGroup,
                OptionDescription = this.OptionDescription,
                UpdateDate = this.UpdateDate,
                InsertDate = this.InsertDate,
            };
        }
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


        public ISession DeepCopy()
        {
            return new DbSession
            {
                SessionId = this.SessionId,
                ChatId = this.ChatId,
                SessionIsLoggedIn = this.SessionIsLoggedIn,
                SessionCurrentState = this.SessionCurrentState,
                UpdateDate = this.UpdateDate,
                InsertDate = this.InsertDate,
            };
        }
    }


    /// <summary> Chat members </summary>
    [Table("user_roles", Schema = "bot")]
    public partial class DbUserRole : IUserRole
    {
        [Key]
        [StringLength(10)]
        [Required(ErrorMessage = "Property 'UserRoleCode' is required")]
        [Column("user_role_code", TypeName = "character varying(10)")]
        public String UserRoleCode { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'UserRoleName' is required")]
        [Column("user_role_name", TypeName = "character varying(50)")]
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


        public IUserRole DeepCopy()
        {
            return new DbUserRole
            {
                UserRoleCode = this.UserRoleCode,
                UserRoleName = this.UserRoleName,
                UserRolePermissions = this.UserRolePermissions,
                UpdateDate = this.UpdateDate,
                InsertDate = this.InsertDate,
            };
        }
    }


    [Table("users", Schema = "bot")]
    public partial class DbUser : IUser
    {
        [Key]
        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public Int32 UserId { get; set; }

        [StringLength(50)]
        [Column("user_name", TypeName = "character varying(50)")]
        public String UserName { get; set; }

        [StringLength(50)]
        [Column("user_full_name", TypeName = "character varying(50)")]
        public String UserFullName { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Property 'UserRoleCode' is required")]
        [Column("user_role_code", TypeName = "character varying(10)")]
        public String UserRoleCode { get; set; }

        [Required(ErrorMessage = "Property 'UserIsBot' is required")]
        [Column("user_is_bot", TypeName = "boolean")]
        public Boolean UserIsBot { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public String RawData { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'RawDataHash' is required")]
        [Column("raw_data_hash", TypeName = "character varying(50)")]
        public String RawDataHash { get; set; }

        [Column("raw_data_history", TypeName = "json")]
        public String RawDataHistory { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }


        public IUser DeepCopy()
        {
            return new DbUser
            {
                UserId = this.UserId,
                UserName = this.UserName,
                UserFullName = this.UserFullName,
                UserRoleCode = this.UserRoleCode,
                UserIsBot = this.UserIsBot,
                RawData = this.RawData,
                RawDataHash = this.RawDataHash,
                RawDataHistory = this.RawDataHistory,
                UpdateDate = this.UpdateDate,
                InsertDate = this.InsertDate,
            };
        }
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