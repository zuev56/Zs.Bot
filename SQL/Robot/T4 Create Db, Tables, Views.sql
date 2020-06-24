
--DROP USER IF EXISTS zuev56;
--DROP USER IF EXISTS app;
--CREATE USER zuev56 WITH PASSWORD 'xxx';
--CREATE USER app WITH PASSWORD 'app';
\c postgres postgres;
set timezone = 'Europe/Moscow';
DROP DATABASE IF EXISTS "ZsBot";
CREATE DATABASE "ZsBot" WITH ENCODING = 'UTF8';
\connect "ZsBot" postgres;
DROP SCHEMA public;
CREATE SCHEMA bot;
CREATE SCHEMA zl;
CREATE SCHEMA helper;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA helper;


CREATE OR REPLACE FUNCTION helper.reset_update_date()
RETURNS TRIGGER AS $$
BEGIN
   NEW.update_date = now(); 
   RETURN NEW;
END;
$$ language 'plpgsql';

COMMENT ON FUNCTION helper.reset_update_date()
    IS '����� ���������� ������� ��������� ���� update_date';
    





--!!������� ���������� UPDATEDATE ��� ��������� ������� ��



CREATE TABLE bot.messengers (
    messenger_code varchar(2)  NOT NULL PRIMARY KEY,
    messenger_name varchar(20) NOT NULL,
    update_date    timestamptz NOT NULL DEFAULT now(),
    insert_date    timestamptz NOT NULL DEFAULT now()
);
CREATE TRIGGER messengers_reset_update_date BEFORE UPDATE
ON bot.messengers FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.messengers IS '������� ������ �����������';

INSERT INTO bot.messengers(messenger_code, messenger_name) VALUES('TG', 'Telegram');
INSERT INTO bot.messengers(messenger_code, messenger_name) VALUES('VK', '���������');
INSERT INTO bot.messengers(messenger_code, messenger_name) VALUES('SK', 'Skype');
INSERT INTO bot.messengers(messenger_code, messenger_name) VALUES('FB', 'Facebook');
INSERT INTO bot.messengers(messenger_code, messenger_name) VALUES('DC', 'Discord');



CREATE TABLE bot.bots (
    bot_id          serial       NOT NULL PRIMARY KEY,
    messenger_code  varchar(2)   NOT NULL REFERENCES bot.messengers(messenger_code),
    bot_name        varchar(20)  NOT NULL,
    bot_token       varchar(100) NOT NULL,
    bot_description varchar(300)     NULL,
    update_date    timestamptz   NOT NULL DEFAULT now(),
    insert_date    timestamptz   NOT NULL DEFAULT now()
);
CREATE TRIGGER bots_reset_update_date BEFORE UPDATE
ON bot.bots FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.bots IS 'Bots info';



CREATE TABLE bot.chat_types (
    chat_type_code varchar(10) NOT NULL PRIMARY KEY,
    chat_type_name varchar(10) NOT NULL,
    update_date    timestamptz NOT NULL DEFAULT now(),
    insert_date    timestamptz NOT NULL DEFAULT now()
);
CREATE TRIGGER chat_types_reset_update_date BEFORE UPDATE
ON bot.chat_types FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.chat_types IS 'Chat types (group, private, etc.)';

INSERT INTO bot.chat_types(chat_type_code, chat_type_name) VALUES('CHANNEL', 'Channel');
INSERT INTO bot.chat_types(chat_type_code, chat_type_name) VALUES('GROUP',   'Group');
INSERT INTO bot.chat_types(chat_type_code, chat_type_name) VALUES('PRIVATE', 'Private');
INSERT INTO bot.chat_types(chat_type_code, chat_type_name) VALUES('UNDEFINED', 'Undefined');



CREATE TABLE bot.chats (
    chat_id          serial        NOT NULL PRIMARY KEY,
    chat_name        varchar(50)   NOT NULL,
    chat_description varchar(100)      NULL,
    chat_type_code   varchar(10)   NOT NULL REFERENCES bot.chat_types(chat_type_code) DEFAULT('UNDEFINED'),
    raw_data         json          NOT NULL, -- ������ ����� ������
    raw_data_hash    varchar(50)   NOT NULL,
    raw_data_history json              NULL,
    update_date      timestamptz   NOT NULL DEFAULT now(),
    insert_date      timestamptz   NOT NULL DEFAULT now()
);
CREATE TRIGGER chats_reset_update_date BEFORE UPDATE
ON bot.chats FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.chats IS 'Chats info';



CREATE TABLE bot.user_roles (
    user_role_code        varchar(10)  NOT NULL PRIMARY KEY,
    user_role_name        varchar(50)  NOT NULL,
    user_role_permissions json         NOT NULL, -- � �.�. ������� (command_group)
    update_date           timestamptz  NOT NULL DEFAULT now(),
    insert_date           timestamptz  NOT NULL DEFAULT now()
);
CREATE TRIGGER user_roles_reset_update_date BEFORE UPDATE
ON bot.user_roles FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.user_roles IS 'Chat members roles';

INSERT INTO bot.user_roles(user_role_code, user_role_name, user_role_permissions) VALUES('OWNER',     'Owner',         '[ "All" ]');
INSERT INTO bot.user_roles(user_role_code, user_role_name, user_role_permissions) VALUES('ADMIN',     'Administrator', '[ "All" ]');
INSERT INTO bot.user_roles(user_role_code, user_role_name, user_role_permissions) VALUES('MODERATOR', 'Moderator',     '[ "userCmdGroup", "Test1CmdGroup" ]');
INSERT INTO bot.user_roles(user_role_code, user_role_name, user_role_permissions) VALUES('USER',      'User',          '[ "userCmdGroup" ]');



CREATE TABLE bot.users (
    user_id          serial        NOT NULL PRIMARY KEY,
    user_name        varchar(50)   NOT NULL,
    user_full_name   varchar(50)       NULL,
    user_role_code   varchar(10)   NOT NULL REFERENCES bot.user_roles(user_role_code),
    user_is_bot      bool          NOT NULL DEFAULT false,
    raw_data         json          NOT NULL,    
    raw_data_hash    varchar(50)   NOT NULL,
    raw_data_history json              NULL,
    update_date      timestamptz   NOT NULL DEFAULT now(),
    insert_date      timestamptz   NOT NULL DEFAULT now()
);
CREATE TRIGGER users_reset_update_date BEFORE UPDATE
ON bot.users FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.user_roles IS 'Chat members';

INSERT INTO bot.users(user_id, user_name, user_full_name, user_role_code, user_is_bot, raw_data, raw_data_hash, update_date, insert_date) 
VALUES(-10, 'Unknown', 'for exported message reading', 'USER', false, '{"test":"test"}', -1063294487, now(), now());



CREATE TABLE bot.message_types (
    message_type_code varchar(3)  NOT NULL PRIMARY KEY,
    message_type_name varchar(50) NOT NULL,
    update_date       timestamptz NOT NULL DEFAULT now(),
    insert_date       timestamptz NOT NULL DEFAULT now()
);
CREATE TRIGGER message_types_reset_update_date BEFORE UPDATE
ON bot.message_types FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.message_types IS '���� ���������';

INSERT INTO bot.message_types(message_type_code, message_type_name) VALUES('UKN', 'Unknown');
INSERT INTO bot.message_types(message_type_code, message_type_name) VALUES('TXT', 'Text');
INSERT INTO bot.message_types(message_type_code, message_type_name) VALUES('PHT', 'Photo');
INSERT INTO bot.message_types(message_type_code, message_type_name) VALUES('AUD', 'Audio');
INSERT INTO bot.message_types(message_type_code, message_type_name) VALUES('VID', 'Video');
INSERT INTO bot.message_types(message_type_code, message_type_name) VALUES('VOI', 'Voice');
INSERT INTO bot.message_types(message_type_code, message_type_name) VALUES('DOC', 'Document');
INSERT INTO bot.message_types(message_type_code, message_type_name) VALUES('STK', 'Sticker');
INSERT INTO bot.message_types(message_type_code, message_type_name) VALUES('LOC', 'Location');
INSERT INTO bot.message_types(message_type_code, message_type_name) VALUES('CNT', 'Contact');
INSERT INTO bot.message_types(message_type_code, message_type_name) VALUES('SRV', 'Service message');
INSERT INTO bot.message_types(message_type_code, message_type_name) VALUES('OTH', 'Other type');



CREATE TABLE bot.messages (
    message_id            serial       NOT NULL PRIMARY KEY,
    reply_to_message_id   bigint           NULL REFERENCES bot.messages(message_id),
    messenger_code        varchar(2)   NOT NULL REFERENCES bot.messengers(messenger_code),
    message_type_code     varchar(3)   NOT NULL REFERENCES bot.message_types(message_type_code),
    user_id               int          NOT NULL REFERENCES bot.users(user_id),
    chat_id               int          NOT NULL REFERENCES bot.chats(chat_id),
    message_text          varchar(100)     NULL, -- ������ ���� �������� � raw_data
    raw_data              json         NOT NULL, -- ������ ����� ������
    is_succeed            bool         NOT NULL, -- Successfuly sent/received/deleted
    fails_count           int          NOT NULL DEFAULT 0,
    fail_description      json             NULL,
    is_deleted            bool         NOT NULL DEFAULT(false),
    update_date           timestamptz  NOT NULL DEFAULT now(),
    insert_date           timestamptz  NOT NULL DEFAULT now()
);
CREATE TRIGGER messages_reset_update_date BEFORE UPDATE
ON bot.messages FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.messages IS '�������� � ������������ ���������';



CREATE TABLE bot.logs (
    log_id       bigserial     NOT NULL PRIMARY KEY,
    log_type     varchar(7)    NOT NULL,            -- Info, warning, error
    log_group    varchar(50)       NULL,            -- ����, ��������� ���������, ������������� � �.�.
    log_message  varchar(200)  NOT NULL,            -- ������� ��������
    log_data     json              NULL,            -- ��� ���� � ������
    insert_date  timestamptz   NOT NULL DEFAULT now()
);
COMMENT ON TABLE bot.logs IS '������';



CREATE TABLE bot.commands (
    command_name         varchar(50)   PRIMARY KEY, -- ��� ����� �������� ��� ����������� ��������
    command_script       varchar(5000) NOT NULL,    -- SQL-������ � �����������    
    command_default_args varchar(100)      NULL,    -- ��������� ���������, ������� ����� �������� ��� ������ ������� ��� ����������. ������������ ����� ����� � �������
    command_desc         varchar(100)      NULL,    -- �������� ��� ������������
    command_group        varchar(50)   NOT NULL,    -- ������ "RoleList" ��� ����� � ������
    update_date          timestamptz   NOT NULL DEFAULT now(),
    insert_date          timestamptz   NOT NULL DEFAULT now()
);
CREATE TRIGGER commands_reset_update_date BEFORE UPDATE
ON bot.commands FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.commands IS '������� �������������';



-- �������� � ������ command_name
CREATE OR REPLACE FUNCTION bot.commands_new_command_correct()
RETURNS TRIGGER AS $$
BEGIN

    NEW.command_name = lower(trim(NEW.command_name));
    NEW.command_name = replace(NEW.command_name, '/', '' );
    NEW.command_name = '/' || NEW.command_name;
    
    NEW.command_group = lower(trim(NEW.command_group));
    NEW.command_group = replace(NEW.command_group, 'cmdgroup', '' );
    NEW.command_group = NEW.command_group || 'CmdGroup';
    
    --raise notice 'Value: %', NEW.command_name;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER commands_new_command_correct BEFORE INSERT OR UPDATE
ON bot.commands FOR EACH ROW EXECUTE PROCEDURE bot.commands_new_command_correct();

INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/GetUserStatistics', 'SELECT bot."sfCmdGetStatistics"({0}, {1}, {2})', '15; now()::Date; now()', '��������� ���������� �� ���������� ���������� ���� �� ����������� ������', 'user');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/Test', 'SELECT ''Test''', null, '�������� ������ � ����. ���������� ''Test''', 'moderator');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/NullTest', 'SELECT null', null, '�������� ������ � ����. ���������� NULL', 'moderator');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/Help', 'SELECT bot.sf_cmd_get_help({0})', '''User''', '��������� ������� �� ��������, ��������� ��� ������ ����', 'user');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/SetMessageLimit', 'SELECT bot."sfCmdSetMessageLimit"({0}, {1})', '0; 0', '��������� ������ ��������� ��� �������������', 'moderator');



-- ���������������� ���������
CREATE TABLE bot.options (
    option_name        varchar(50)   NOT NULL PRIMARY KEY,
    option_value       varchar(5000)     NULL,
    option_group       varchar(50)   NOT NULL,
    option_description varchar(500)      NULL,
    update_date        timestamptz   NOT NULL DEFAULT now(),
    insert_date        timestamptz   NOT NULL DEFAULT now()
);
CREATE TRIGGER options_reset_update_date BEFORE UPDATE
ON bot.options FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.commands IS '��������� ����������';



CREATE TABLE bot.sessions (
    session_id            serial      PRIMARY KEY,
    chat_id               int         NOT NULL REFERENCES bot.chats(chat_id),
    session_is_logged_in  bool        NOT NULL,  
    session_current_state json        NOT NULL,
    update_date           timestamptz NOT NULL DEFAULT now(),
    insert_date           timestamptz NOT NULL DEFAULT now()
);
CREATE TRIGGER sessions_reset_update_date BEFORE UPDATE
ON bot.sessions FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.sessions IS '������ ������������� - ����������� ���������������� ������� � �����';



--CREATE TABLE bot.jobs (
--    job_id            serial       PRIMARY KEY,
--    job_name          varchar(100) NOT NULL,
--    job_description   varchar(100)     NULL,   
--    job_is_active     bool         NOT NULL DEFAULT FALSE,
--    job_method_name   varchar(100) NOT NULL,
--    job_month         int              NULL, -- �����, ���� ������� ���������; null, ���� ������� �����������
--    job_day           int          NOT NULL, -- ���� ������
--    job_hour          int          NOT NULL, -- ����� ����������
--    job_minute        int          NOT NULL, -- ����� ����������
--    job_last_execDate timestamptz      NULL,
--    update_date       timestamptz  NOT NULL DEFAULT now(),
--    insert_date       timestamptz  NOT NULL DEFAULT now()
--);
--CREATE TRIGGER jobs_reset_update_date BEFORE UPDATE
--ON bot.jobs FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();




INSERT INTO bot.options(option_group, option_name, option_value, option_description) VALUES('ChatAdmin', 'ChatUserMessageCountHiHi', '-1', '������� ��������� �������');
INSERT INTO bot.options(option_group, option_name, option_value, option_description) VALUES('ChatAdmin', 'ChatUserMessageCountHi',   '-1', '������� ����������������� �������');
--INSERT INTO bot.options(option_group, option_name, option_value, option_description) VALUES('ChatAdmin','DefaultChatId',            '',   '������������� ����, � ������� ��� �������� �� ���������');

--DROP TABLE zl.bans;

CREATE TABLE zl.bans (
    ban_id          serial      NOT NULL PRIMARY KEY,
    user_id         int         NOT NULL REFERENCES bot.users (user_id),
    chat_id         int         NOT NULL REFERENCES bot.chats (chat_id),
    ban_finish_date timestamptz     NULL,
    update_date     timestamptz NOT NULL DEFAULT now(),
    insert_date     timestamptz NOT NULL DEFAULT now()
);
CREATE TRIGGER bans_reset_update_date BEFORE UPDATE
ON zl.bans FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE zl.bans IS '���������� � �����';




CREATE TABLE zl.accountings (
    accounting_id         serial      NOT NULL PRIMARY KEY,
    accounting_start_date timestamptz NOT NULL DEFAULT now(),   
    update_date           timestamptz NOT NULL DEFAULT now()
);
CREATE TRIGGER accountings_reset_update_date BEFORE UPDATE
ON zl.accountings FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE zl.accountings IS '���������� � ������� ������ ����� ��������� ������� ���������� ������������';




CREATE TABLE zl.notifications (
    notification_id           serial        NOT NULL PRIMARY KEY,
    notification_is_active     bool         NOT NULL DEFAULT TRUE,
    notification_message      varchar(2000) NOT NULL,
    notification_month        int               NULL, -- ����, ���� ������� ���������; null, ���� ������� ����������
    notification_day          int           NOT NULL, -- ���� �����
    notification_hour         int           NOT NULL, -- ���� ���������
    notification_minute       int           NOT NULL, -- ���� ���������
    notification_exec_date    timestamptz       NULL, -- ����� ���������� ������������
    update_date               timestamptz   NOT NULL DEFAULT now(),
    insert_date               timestamptz   NOT NULL DEFAULT now()
);
CREATE TRIGGER notifications_reset_update_date BEFORE UPDATE
ON zl.notifications FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE zl.notifications IS '����������� � ��������';




CREATE TABLE zl.auxiliary_words (
    the_word    varchar(100)  NOT NULL PRIMARY KEY, -- word ����� ������������ � ���������
    insert_date timestamptz   NOT NULL DEFAULT now()
);
COMMENT ON TABLE zl.auxiliary_words IS '��������������� ����� - ��, ��� ������ ���� ������� �� ����������';





GRANT ALL PRIVILEGES ON DATABASE "ZsBot"            TO zuev56;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA zl  TO zuev56;
GRANT ALL PRIVILEGES ON DATABASE "ZsBot"            TO zuev56;
GRANT ALL PRIVILEGES ON SCHEMA zl                   TO zuev56;
GRANT ALL PRIVILEGES ON SCHEMA bot                  TO zuev56;
GRANT ALL PRIVILEGES ON ALL TABLES    IN SCHEMA zl  TO zuev56;
GRANT ALL PRIVILEGES ON ALL TABLES    IN SCHEMA bot TO zuev56;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA bot TO zuev56;

GRANT CONNECT        ON DATABASE "ZsBot"            TO app;
GRANT ALL PRIVILEGES ON SCHEMA zl                   TO app;
GRANT ALL PRIVILEGES ON SCHEMA bot                  TO app;
GRANT ALL PRIVILEGES ON ALL TABLES    IN SCHEMA bot TO app;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA bot TO app;
GRANT INSERT         ON ALL TABLES    IN SCHEMA bot TO app;
GRANT ALL PRIVILEGES ON ALL TABLES    IN SCHEMA zl  TO app;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA zl  TO app;




\connect "ZsBot" postgres;
CREATE SCHEMA rmgr;
CREATE EXTENSION postgres_fdw WITH SCHEMA rmgr;
CREATE SERVER ActiveRmgrDb FOREIGN DATA WRAPPER postgres_fdw OPTIONS (host '192.168.1.12', dbname 'RemoteManagerDb', port '5632');
CREATE USER MAPPING FOR postgres SERVER ActiveRmgrDb OPTIONS(user 'postgres', password 'postgres');


CREATE FOREIGN TABLE rmgr."ReceivedMsg" (
    "ReceivedMsgId"                    bigint NOT NULL,
    "ReceivedMsgMessageId"             integer NOT NULL,
    "UserId"                           integer NOT NULL,
    "ChatId"                           bigint NOT NULL,
    "MessageTypeName"                  character varying(50) COLLATE pg_catalog."default" NOT NULL,
    "ReceivedMsgAuthorSignature"       character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgCaption"               character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgChannelChatCreated"    boolean,
    "ReceivedMsgConnectedWebsite"      character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgDate"                  timestamp with time zone,
    "ReceivedMsgDeleteChatPhoto"       boolean,
    "ReceivedMsgEditDate"              timestamp with time zone,
    "ReceivedMsgForwardDate"           timestamp with time zone,
    "ReceivedMsgForwardFromId"         integer,
    "ReceivedMsgForwardFromChatId"     bigint,
    "ReceivedMsgForwardFromMessageId"  integer,
    "ReceivedMsgForwardSignature"      character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgGroupChatCreated"      boolean,
    "ReceivedMsgIsForwarded"           boolean,
    "ReceivedMsgLeftChatMemberId"      integer,
    "ReceivedMsgLocation"              character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgMediaGroupId"          character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgMigrateFromChatId"     bigint,
    "ReceivedMsgMigrateToChatId"       bigint,
    "ReceivedMsgNewChatTitle"          character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgPinnedMessageId"       integer,
    "ReceivedMsgReplyToMessageId"      integer,
    "ReceivedMsgSupergroupChatCreated" boolean,
    "ReceivedMsgText"                  character varying(5000) COLLATE pg_catalog."default",
    "InsertDate"                       timestamp with time zone NOT NULL DEFAULT now(),
    "UpdateDate"                       timestamp with time zone NOT NULL DEFAULT now(),
    "IsDeleted"                        boolean DEFAULT false
)
SERVER ActiveRmgrDb OPTIONS(schema_name 'rmgr', table_name 'ReceivedMsg');


CREATE FOREIGN TABLE rmgr."Chat"
(
    "ChatId"                          bigint NOT NULL,
    "ChatTitle"                       character varying(50) COLLATE pg_catalog."default",
    "ChatFirstName"                   character varying(50) COLLATE pg_catalog."default",
    "ChatLastName"                    character varying(50) COLLATE pg_catalog."default",
    "ChatDescription"                 character varying(100) COLLATE pg_catalog."default",
    "ChatType"                        character varying(50) COLLATE pg_catalog."default",
    "ChatInviteLink"                  character varying(10) COLLATE pg_catalog."default",
    "ChatUserName"                    character varying(50) COLLATE pg_catalog."default",
    "ChatAllMembersAreAdministrators" boolean,
    "ChatCanSetStickerSet"            boolean,
    "ChatStickerSetName"              character varying(50) COLLATE pg_catalog."default",
    "PinnedMessageId"                 integer,
    "IsSubscribed"                    boolean NOT NULL,
    "InsertDate"                      timestamp with time zone NOT NULL DEFAULT now(),
    "UpdateDate"                      timestamp with time zone NOT NULL DEFAULT now()
)
SERVER ActiveRmgrDb OPTIONS(schema_name 'rmgr', table_name 'Chat');



CREATE FOREIGN TABLE rmgr."User"
(
    "UserId"         integer NOT NULL,
    "UserName"       character varying(50) COLLATE pg_catalog."default",
    "UserManualName" character varying(50) COLLATE pg_catalog."default",
    "UserFirstName"  character varying(50) COLLATE pg_catalog."default",
    "UserLastName"   character varying(50) COLLATE pg_catalog."default",
    "RoleName"       character varying(50) COLLATE pg_catalog."default" NOT NULL,
    "IsBot"          boolean,
    "UpdateDate"     timestamp with time zone NOT NULL DEFAULT now(),
    "InsertDate"     timestamp with time zone NOT NULL DEFAULT now()
)
SERVER ActiveRmgrDb OPTIONS(schema_name 'rmgr', table_name 'User');


CREATE FOREIGN TABLE rmgr."UnInterestedWords"
(
    wd character varying(100) COLLATE pg_catalog."default" NOT NULL,
    insert_date timestamp with time zone NOT NULL DEFAULT now()
)
SERVER ActiveRmgrDb OPTIONS(schema_name 'cas', table_name 'UnInterestedWords');

insert into zl.auxiliary_words (the_word, insert_date)
SELECT wd, insert_date from rmgr."UnInterestedWords";
