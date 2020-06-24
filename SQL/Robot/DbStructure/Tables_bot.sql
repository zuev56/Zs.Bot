


--!!СДЕЛАТЬ ОБНОВЛЕНИЕ UPDATEDATE при изменении записей БД



CREATE TABLE bot.messengers (
    messenger_code varchar(2)  NOT NULL PRIMARY KEY,
    messenger_name varchar(20) NOT NULL,
    update_date    timestamptz NOT NULL DEFAULT now(),
    insert_date    timestamptz NOT NULL DEFAULT now()
);
CREATE TRIGGER messengers_reset_update_date BEFORE UPDATE
ON bot.messengers FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.messengers IS 'Система обмена сообщениями';

INSERT INTO bot.messengers(messenger_code, messenger_name) VALUES('TG', 'Telegram');
INSERT INTO bot.messengers(messenger_code, messenger_name) VALUES('VK', 'Вконтакте');
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
    raw_data         json          NOT NULL, -- Полный набор данных
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
    user_role_permissions json         NOT NULL, -- в т.ч. команды (command_group)
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
COMMENT ON TABLE bot.message_types IS 'Типы сообщений';

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
    reply_to_message_id   int              NULL REFERENCES bot.messages(message_id),
    messenger_code        varchar(2)   NOT NULL REFERENCES bot.messengers(messenger_code),
    message_type_code     varchar(3)   NOT NULL REFERENCES bot.message_types(message_type_code),
    user_id               int          NOT NULL REFERENCES bot.users(user_id),
    chat_id               int          NOT NULL REFERENCES bot.chats(chat_id),
    message_text          varchar(100)     NULL, -- Полный техт доступен в raw_data
    raw_data              json         NOT NULL, -- Полный набор данных
    is_succeed            bool         NOT NULL, -- Successfuly sent/received/deleted
    fails_count           int          NOT NULL DEFAULT 0,
    fail_description      json             NULL,
    is_deleted            bool         NOT NULL DEFAULT(false),
    update_date           timestamptz  NOT NULL DEFAULT now(),
    insert_date           timestamptz  NOT NULL DEFAULT now()
);
CREATE TRIGGER messages_reset_update_date BEFORE UPDATE
ON bot.messages FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.messages IS 'Принятые и отрпавленные сообщения';



CREATE TABLE bot.logs (
    log_id       bigserial     NOT NULL PRIMARY KEY,
    log_type     varchar(7)    NOT NULL,            -- Info, warning, error
    log_group    varchar(50)       NULL,            -- Джоб, обработка сообщения, инициализация и т.д.
    log_message  varchar(200)  NOT NULL,            -- Краткое описание
    log_data     json              NULL,            -- Вся инфа о записи
    insert_date  timestamptz   NOT NULL DEFAULT now()
);
COMMENT ON TABLE bot.logs IS 'Журнал';



CREATE TABLE bot.commands (
    command_name         varchar(50)   PRIMARY KEY, -- Так много символов для программных запросов
    command_script       varchar(5000) NOT NULL,    -- SQL-скрипт с параметрами    
    command_default_args varchar(100)      NULL,    -- Дефолтные аргументы, которые будут вызванны при вызове команды без аргументов. Записываются через точку с запятой
    command_desc         varchar(100)      NULL,    -- Описание для пользователя
    command_group        varchar(50)   NOT NULL,    -- Вместо "RoleList" для связи с ролями
    update_date          timestamptz   NOT NULL DEFAULT now(),
    insert_date          timestamptz   NOT NULL DEFAULT now()
);
CREATE TRIGGER commands_reset_update_date BEFORE UPDATE
ON bot.commands FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE bot.commands IS 'Команды пользователей';



-- Проверка и правка command_name
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
VALUES('/GetUserStatistics', 'SELECT bot."sfCmdGetStatistics"({0}, {1}, {2})', '15; now()::Date; now()', 'Получение статистики по активности участников чата за определённый период', 'user');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/Test', 'SELECT ''Test''', null, 'Тестовый запрос к боту. Возвращает ''Test''', 'moderator');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/NullTest', 'SELECT null', null, 'Тестовый запрос к боту. Возвращает NULL', 'moderator');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/Help', 'SELECT bot.sf_cmd_get_help({0})', '''User''', 'Получение справки по функциям, доступным для данной роли', 'user');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/SetMessageLimit', 'SELECT bot."sfCmdSetMessageLimit"({0}, {1})', '0; 0', 'Установка лимита сообщений для пользователей', 'moderator');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/SqlQuery', 'select (with userQuery as ({0}) select json_agg(q) from userQuery q)', 'select ''Write your query''', 'SQL-запрос', 'admin');



-- Пользовательские настройки
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
COMMENT ON TABLE bot.commands IS 'Параметры приложения';



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
COMMENT ON TABLE bot.sessions IS 'Сессии пользователей - обслуживают последовательное общение с ботом';



--CREATE TABLE bot.jobs (
--    job_id            serial       PRIMARY KEY,
--    job_name          varchar(100) NOT NULL,
--    job_description   varchar(100)     NULL,   
--    job_is_active     bool         NOT NULL DEFAULT FALSE,
--    job_method_name   varchar(100) NOT NULL,
--    job_month         int              NULL, -- месяц, если событие ежегодное; null, если событие ежемесячное
--    job_day           int          NOT NULL, -- день месяца
--    job_hour          int          NOT NULL, -- время оповещения
--    job_minute        int          NOT NULL, -- время оповещения
--    job_last_execDate timestamptz      NULL,
--    update_date       timestamptz  NOT NULL DEFAULT now(),
--    insert_date       timestamptz  NOT NULL DEFAULT now()
--);
--CREATE TRIGGER jobs_reset_update_date BEFORE UPDATE
--ON bot.jobs FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
