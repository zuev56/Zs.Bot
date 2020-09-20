using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Zs.Bot.Model.Abstractions;

namespace Zs.Bot.Model
{
    /// <summary> Bots info </summary>
    [Table("bots", Schema = "bot")]
    public class Bot : IBot
    {
        [Key]
        [Required(ErrorMessage = "Property 'BotId' is required")]
        [Column("bot_id", TypeName = "integer")]
        public int Id { get; set; }

        [StringLength(2)]
        [Required(ErrorMessage = "Property 'MessengerCode' is required")]
        [Column("messenger_code", TypeName = "character varying(2)")]
        public string MessengerCode { get; set; }

        [StringLength(20)]
        [Required(ErrorMessage = "Property 'BotName' is required")]
        [Column("bot_name", TypeName = "character varying(20)")]
        public string Name { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Property 'BotToken' is required")]
        [Column("bot_token", TypeName = "character varying(100)")]
        public string Token { get; set; }

        [StringLength(300)]
        [Column("bot_description", TypeName = "character varying(300)")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public MessengerInfo Messenger { get; set; }
    }


    /// <summary> Chat types (group, private, etc.) </summary>
    [Table("chat_types", Schema = "bot")]
    public class ChatType : IChatType
    {
        [Key]
        [StringLength(10)]
        [Required(ErrorMessage = "Property 'ChatTypeCode' is required")]
        [Column("chat_type_code", TypeName = "character varying(10)")]
        public string Code { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Property 'ChatTypeName' is required")]
        [Column("chat_type_name", TypeName = "character varying(10)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public ICollection<Chat> Chats { get; set; }
    }


    /// <summary> Chats info </summary>
    [Table("chats", Schema = "bot")]
    public class Chat : IChat
    {
        [Key]
        [Required(ErrorMessage = "Property 'ChatId' is required")]
        [Column("chat_id", TypeName = "integer")]
        public int Id { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'ChatName' is required")]
        [Column("chat_name", TypeName = "character varying(50)")]
        public string Name { get; set; }

        [StringLength(100)]
        [Column("chat_description", TypeName = "character varying(100)")]
        public string Description { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Property 'ChatTypeCode' is required")]
        [Column("chat_type_code", TypeName = "character varying(10)")]
        public string ChatTypeCode { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public string RawData { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'RawDataHash' is required")]
        [Column("raw_data_hash", TypeName = "character varying(50)")]
        public string RawDataHash { get; set; }

        [Column("raw_data_history", TypeName = "json")]
        public string RawDataHistory { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public ChatType ChatType { get; set; }
        public ICollection<Message> Messages { get; set; }
    }


    /// <summary> Параметры приложения </summary>
    [Table("commands", Schema = "bot")]
    public class Command : ICommand
    {
        [Key]
        [StringLength(50)]
        [Required(ErrorMessage = "Property 'CommandName' is required")]
        [Column("command_name", TypeName = "character varying(50)")]
        public string Name { get; set; }

        [StringLength(5000)]
        [Required(ErrorMessage = "Property 'CommandScript' is required")]
        [Column("command_script", TypeName = "character varying(5000)")]
        public string Script { get; set; }

        [StringLength(100)]
        [Column("command_default_args", TypeName = "character varying(100)")]
        public string DefaultArgs { get; set; }

        [StringLength(100)]
        [Column("command_desc", TypeName = "character varying(100)")]
        public string Description { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'CommandGroup' is required")]
        [Column("command_group", TypeName = "character varying(50)")]
        public string Group { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    /// <summary> Журнал </summary>
    [Table("logs", Schema = "bot")]
    public class Log : ILog
    {
        [Key]
        [Required(ErrorMessage = "Property 'LogId' is required")]
        [Column("log_id", TypeName = "bigint")]
        public long Id { get; set; }

        [StringLength(7)]
        [Required(ErrorMessage = "Property 'LogType' is required")]
        [Column("log_type", TypeName = "character varying(7)")]
        public string Type { get; set; }

        [StringLength(50)]
        [Column("log_initiator", TypeName = "character varying(50)")]
        public string Initiator { get; set; }

        [StringLength(200)]
        [Required(ErrorMessage = "Property 'LogMessage' is required")]
        [Column("log_message", TypeName = "character varying(200)")]
        public string Message { get; set; }

        [Column("log_data", TypeName = "json")]
        public string Data { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

    }


    /// <summary> Типы сообщений </summary>
    [Table("message_types", Schema = "bot")]
    public class MessageType : IMessageType
    {
        [Key]
        [StringLength(3)]
        [Required(ErrorMessage = "Property 'MessageTypeCode' is required")]
        [Column("message_type_code", TypeName = "character varying(3)")]
        public string Code { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'MessageTypeName' is required")]
        [Column("message_type_name", TypeName = "character varying(50)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public ICollection<Message> Messages { get; set; }
    }


    /// <summary> Принятые и отрпавленные сообщения </summary>
    [Table("messages", Schema = "bot")]
    public class Message : IMessage
    {
        [Key]
        [Required(ErrorMessage = "Property 'MessageId' is required")]
        [Column("message_id", TypeName = "integer")]
        public int Id { get; set; }

        [Column("reply_to_message_id", TypeName = "integer")]
        public int? ReplyToMessageId { get; set; }

        [StringLength(2)]
        [Required(ErrorMessage = "Property 'MessengerCode' is required")]
        [Column("messenger_code", TypeName = "character varying(2)")]
        public string MessengerCode { get; set; }

        [StringLength(3)]
        [Required(ErrorMessage = "Property 'MessageTypeCode' is required")]
        [Column("message_type_code", TypeName = "character varying(3)")]
        public string MessageTypeCode { get; set; }

        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Property 'ChatId' is required")]
        [Column("chat_id", TypeName = "integer")]
        public int ChatId { get; set; }

        [StringLength(100)]
        [Column("message_text", TypeName = "character varying(100)")]
        public string Text { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public string RawData { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'RawDataHash' is required")]
        [Column("raw_data_hash", TypeName = "character varying(50)")]
        public string RawDataHash { get; set; }

        [Column("raw_data_history", TypeName = "json")]
        public string RawDataHistory { get; set; }

        [Required(ErrorMessage = "Property 'IsSucceed' is required")]
        [Column("is_succeed", TypeName = "bool")]
        public bool IsSucceed { get; set; }

        [Required(ErrorMessage = "Property 'FailsCount' is required")]
        [Column("fails_count", TypeName = "integer")]
        public int FailsCount { get; set; }

        [Column("fail_description", TypeName = "json")]
        public string FailDescription { get; set; }

        [Required(ErrorMessage = "Property 'IsDeleted' is required")]
        [Column("is_deleted", TypeName = "bool")]
        public bool IsDeleted { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }


        public MessageType MessageType { get; set; }
        public Message ReplyToMessage { get; set; }
        public MessengerInfo Messenger { get; set; }
        public User User { get; set; }
        public Chat Chat { get; set; }

    }


    /// <summary> Система обмена сообщениями </summary>
    [Table("messengers", Schema = "bot")]
    public class MessengerInfo : IMessengerInfo
    {
        [Key]
        [StringLength(2)]
        [Required(ErrorMessage = "Property 'MessengerCode' is required")]
        [Column("messenger_code", TypeName = "character varying(2)")]
        public string Code { get; set; }

        [StringLength(20)]
        [Required(ErrorMessage = "Property 'MessengerName' is required")]
        [Column("messenger_name", TypeName = "character varying(20)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public ICollection<Bot> Bots { get; set; }
        public ICollection<Message> Messages { get; set; }

    }


    /// <summary> Chat members </summary>
    [Table("user_roles", Schema = "bot")]
    public class UserRole : IUserRole
    {
        [Key]
        [StringLength(10)]
        [Required(ErrorMessage = "Property 'UserRoleCode' is required")]
        [Column("user_role_code", TypeName = "character varying(10)")]
        public string Code { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'UserRoleName' is required")]
        [Column("user_role_name", TypeName = "character varying(50)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Property 'UserRolePermissions' is required")]
        [Column("user_role_permissions", TypeName = "json")]
        public string Permissions { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public ICollection<User> Users { get; set; }
    }


    [Table("users", Schema = "bot")]
    public class User : IUser
    {
        [Key]
        [Required(ErrorMessage = "Property 'UserId' is required")]
        [Column("user_id", TypeName = "integer")]
        public int Id { get; set; }

        [StringLength(50)]
        [Column("user_name", TypeName = "character varying(50)")]
        public string Name { get; set; }

        [StringLength(50)]
        [Column("user_full_name", TypeName = "character varying(50)")]
        public string FullName { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Property 'UserRoleCode' is required")]
        [Column("user_role_code", TypeName = "character varying(10)")]
        public string UserRoleCode { get; set; }

        [Required(ErrorMessage = "Property 'UserIsBot' is required")]
        [Column("user_is_bot", TypeName = "bool")]
        public bool IsBot { get; set; }

        [Required(ErrorMessage = "Property 'RawData' is required")]
        [Column("raw_data", TypeName = "json")]
        public string RawData { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Property 'RawDataHash' is required")]
        [Column("raw_data_hash", TypeName = "character varying(50)")]
        public string RawDataHash { get; set; }

        [Column("raw_data_history", TypeName = "json")]
        public string RawDataHistory { get; set; }

        [Required(ErrorMessage = "Property 'UpdateDate' is required")]
        [Column("update_date", TypeName = "timestamp with time zone")]
        public DateTime UpdateDate { get; set; }

        [Required(ErrorMessage = "Property 'InsertDate' is required")]
        [Column("insert_date", TypeName = "timestamp with time zone")]
        public DateTime InsertDate { get; set; }

        public ICollection<Message> Messages { get; set; }
        public UserRole UserRoles { get; set; }
    }

    /// <summary> SQL-query result. Not a table </summary>
    [NotMapped]
    public partial class DbQuery
    {
        [Key]
        public string Result { get; set; }
    }
}
