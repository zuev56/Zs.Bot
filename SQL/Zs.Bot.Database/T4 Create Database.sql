--DROP USER IF EXISTS zuev56;
--DROP USER IF EXISTS app;
--CREATE USER zuev56 WITH PASSWORD 'xxx';

DO $$ BEGIN
    IF NOT EXISTS (select 1 from pg_user where usename='app') THEN
        CREATE USER app WITH PASSWORD 'app';
    END IF;
END $$;
\c postgres postgres;
set timezone = 'Europe/Moscow';
DROP DATABASE IF EXISTS "ZsBot";
CREATE DATABASE "ZsBot" WITH ENCODING = 'UTF8';
\connect "ZsBot" postgres;
DROP SCHEMA public;
CREATE SCHEMA bot;
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
    update_date     timestamptz  NOT NULL DEFAULT now(),
    insert_date     timestamptz  NOT NULL DEFAULT now()
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

insert into bot.chats (chat_id, chat_name, chat_description, chat_type_code, raw_data, raw_data_hash, raw_data_history, update_date, insert_date)
values(0, 'UnitTestChat', 'UnitTestChat', 'PRIVATE', '{ "test": "test" }', '123', null, now(), now());



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
INSERT INTO bot.user_roles(user_role_code, user_role_name, user_role_permissions) VALUES('ADMIN',     'Administrator', '[ "adminCmdGroup", "moderatorCmdGroup", "userCmdGroup" ]');
INSERT INTO bot.user_roles(user_role_code, user_role_name, user_role_permissions) VALUES('MODERATOR', 'Moderator',     '[ "moderatorCmdGroup", "userCmdGroup" ]');
INSERT INTO bot.user_roles(user_role_code, user_role_name, user_role_permissions) VALUES('USER',      'User',          '[ "userCmdGroup" ]');



CREATE TABLE bot.users (
    user_id          serial        NOT NULL PRIMARY KEY,
    user_name        varchar(50)       NULL,
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
COMMENT ON TABLE bot.users IS 'Chat members';

INSERT INTO bot.users(user_id, user_name, user_full_name, user_role_code, user_is_bot, raw_data, raw_data_hash, update_date, insert_date) 
VALUES(-10, 'Unknown', 'for exported message reading', 'USER', false, '{"test":"test"}', -1063294487, now(), now());
INSERT INTO bot.users(user_id, user_name, user_full_name, user_role_code, user_is_bot, raw_data, raw_data_hash, update_date, insert_date) 
VALUES(0, 'UnitTestUser', 'UnitTest', 'USER', false, '{"test":"test"}', -1063294487, now(), now());



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
    message_id          serial       NOT NULL PRIMARY KEY,
    reply_to_message_id int              NULL REFERENCES bot.messages(message_id),
    messenger_code      varchar(2)   NOT NULL REFERENCES bot.messengers(messenger_code),
    message_type_code   varchar(3)   NOT NULL REFERENCES bot.message_types(message_type_code),
    user_id             int          NOT NULL REFERENCES bot.users(user_id),
    chat_id             int          NOT NULL REFERENCES bot.chats(chat_id),
    message_text        varchar(100)     NULL, -- ������ ���� �������� � raw_data
    raw_data            json         NOT NULL, -- ������ ����� ������
    raw_data_hash       varchar(50)  NOT NULL,
    raw_data_history    json             NULL,
    is_succeed          bool         NOT NULL, -- Successfuly sent/received/deleted
    fails_count         int          NOT NULL DEFAULT 0,
    fail_description    json             NULL,
    is_deleted          bool         NOT NULL DEFAULT(false),
    update_date         timestamptz  NOT NULL DEFAULT now(),
    insert_date         timestamptz  NOT NULL DEFAULT now()
);
CREATE TRIGGER messages_reset_update_date BEFORE UPDATE
ON bot.messages FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.messages IS '�������� � ������������ ���������';



CREATE TABLE bot.logs (
    log_id        bigserial     NOT NULL PRIMARY KEY,
    log_type      varchar(7)    NOT NULL,            -- Info, warning, error
    log_initiator varchar(50)       NULL,            -- ����, ��������� ���������, ������������� � �.�.
    log_message   varchar(200)  NOT NULL,            -- ������� ��������
    log_data      json              NULL,            -- ��� ���� � ������
    insert_date   timestamptz   NOT NULL DEFAULT now()
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
VALUES('/Test', 'SELECT ''Test''', null, '�������� ������ � ����. ���������� ''Test''', 'moderatorCmdGroup');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/NullTest', 'SELECT null', null, '�������� ������ � ����. ���������� NULL', 'moderatorCmdGroup');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/Help', 'SELECT bot.sf_cmd_get_help({0})', '<UserRoleCode>', '��������� ������� �� ��������� ��������', 'userCmdGroup');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/SetMessageLimit', 'SELECT bot."sfCmdSetMessageLimit"({0}, {1})', '0; 0', '��������� ������ ��������� ��� �������������', 'moderatorCmdGroup');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/SqlQuery', 'select (with userQuery as ({0}) select json_agg(q) from userQuery q)', 'select ''Pass your query as parameter in double quotes''', 'SQL-������', 'adminCmdGroup');



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




-- Get permissions for a specific user role
CREATE OR REPLACE FUNCTION bot.sf_get_permission_array(
    user_role_code_ varchar(10))
    RETURNS TEXT[]
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE       
AS $BODY$
BEGIN
    RETURN array(
        with json_permissions as (select ur.user_role_permissions as col
                                    from bot.user_roles ur 
                                   where upper(ur.user_role_code) = upper(user_role_code_)),
              row_Permissions as (select json_array_elements_text(col) as permissions
                                    from json_permissions)
        select permissions
          from row_Permissions
    );
END;
$BODY$;

ALTER FUNCTION bot.sf_get_permission_array(character varying)
    OWNER TO postgres;

COMMENT ON FUNCTION bot.sf_get_permission_array(character varying)
    IS 'Returns permission array for the role';




-- Get list of awailable functions for a specific user role
CREATE OR REPLACE FUNCTION bot.sf_cmd_get_help(
    user_role_code_ varchar(10))
    RETURNS TEXT
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE       
AS $BODY$
DECLARE
    role_permissions TEXT[];
BEGIN
    role_permissions := (select bot.sf_get_permission_array(user_role_code_));
    RAISE NOTICE 'role_permissions: %', role_permissions;
   
    IF 'ALL' = any(upper(role_permissions::text)::text[]) THEN
        RETURN (select string_agg((command_name || ' - ' || command_desc), E'\n') 
                  from bot.commands);
    ELSE
        RETURN (select string_agg((command_name || ' - ' || command_desc), E'\n') 
                  from bot.commands
                 where command_group = any(role_permissions));	
    END IF;	   
END;
$BODY$;

ALTER FUNCTION bot.sf_cmd_get_help(character varying)
    OWNER TO postgres;

COMMENT ON FUNCTION bot.sf_cmd_get_help(character varying)
    IS 'Returns help to features available for the role';




-- Get statistics of specific chat
CREATE OR REPLACE FUNCTION bot.sf_get_chat_statistics(
    _chat_id     integer,
    _users_limit integer,
    _from_date   timestamp with time zone,
    _to_date     timestamp with time zone DEFAULT now()
    )
    RETURNS table(chat_id integer, user_id integer, message_count bigint) 
    LANGUAGE 'plpgsql'
    ROWS 1000
AS $BODY$
BEGIN
    RETURN QUERY(
        SELECT _chat_id
             , u.user_id
             , count(m.*) 
          FROM bot.messages m
          JOIN bot.users u ON u.user_id = m.user_id
         WHERE m.chat_id = _chat_id
           AND m.insert_date >= _from_date AND m.insert_date <= _to_date
           AND m.is_deleted = false
      GROUP BY u.user_id
      ORDER BY count(m.*) DESC
      LIMIT _users_limit);
END;
$BODY$;
ALTER FUNCTION bot.sf_get_chat_statistics(integer, integer, timestamp with time zone, timestamp with time zone)
    OWNER TO postgres;
COMMENT ON FUNCTION bot.sf_get_chat_statistics(integer, integer, timestamp with time zone, timestamp with time zone)
    IS 'Returns a list of users and the number of their messages in the specified time range';
--select * from bot.sf_get_chat_statistics(1, 10, now()::date - interval '1 day', now())








GRANT ALL PRIVILEGES ON DATABASE "ZsBot"    TO zuev56;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA bot TO zuev56;
GRANT ALL PRIVILEGES ON SCHEMA bot                  TO zuev56;
GRANT ALL PRIVILEGES ON ALL TABLES    IN SCHEMA bot TO zuev56;

GRANT CONNECT        ON DATABASE "ZsBot"    TO app;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA bot TO app;
GRANT ALL PRIVILEGES ON SCHEMA bot                  TO app;
GRANT ALL PRIVILEGES ON ALL TABLES    IN SCHEMA bot TO app;
