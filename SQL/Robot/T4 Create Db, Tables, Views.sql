
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
    IS 'Общая триггерная функция обовления поля update_date';
    





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
    raw_data         json          NOT NULL, -- Полный набор данных
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
    user_role_permissions json         NOT NULL, -- в т.ч. команды (command_group)
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
COMMENT ON TABLE bot.user_roles IS 'Chat members';

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
    message_id          serial       NOT NULL PRIMARY KEY,
    reply_to_message_id int              NULL REFERENCES bot.messages(message_id),
    messenger_code      varchar(2)   NOT NULL REFERENCES bot.messengers(messenger_code),
    message_type_code   varchar(3)   NOT NULL REFERENCES bot.message_types(message_type_code),
    user_id             int          NOT NULL REFERENCES bot.users(user_id),
    chat_id             int          NOT NULL REFERENCES bot.chats(chat_id),
    message_text        varchar(100)     NULL, -- Полный техт доступен в raw_data
    raw_data            json         NOT NULL, -- Полный набор данных
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
COMMENT ON TABLE bot.messages IS 'Принятые и отрпавленные сообщения';



CREATE TABLE bot.logs (
    log_id        bigserial     NOT NULL PRIMARY KEY,
    log_type      varchar(7)    NOT NULL,            -- Info, warning, error
    log_initiator varchar(50)       NULL,            -- Джоб, обработка сообщения, инициализация и т.д.
    log_message   varchar(200)  NOT NULL,            -- Краткое описание
    log_data      json              NULL,            -- Вся инфа о записи
    insert_date   timestamptz   NOT NULL DEFAULT now()
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
VALUES('/GetUserStatistics', 'SELECT zl.sf_cmd_get_full_statistics({0}, {1}, {2})', '15; now()::Date; now()', 'Получение статистики по активности участников всех чатов за определённый период', 'adminCmdGroup');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/Test', 'SELECT ''Test''', null, 'Тестовый запрос к боту. Возвращает ''Test''', 'moderatorCmdGroup');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/NullTest', 'SELECT null', null, 'Тестовый запрос к боту. Возвращает NULL', 'moderatorCmdGroup');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/Help', 'SELECT bot.sf_cmd_get_help({0})', '<UserRoleCode>', 'Получение справки по доступным функциям', 'userCmdGroup');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/SetMessageLimit', 'SELECT bot."sfCmdSetMessageLimit"({0}, {1})', '0; 0', 'Установка лимита сообщений для пользователей', 'moderatorCmdGroup');
INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/SqlQuery', 'select (with userQuery as ({0}) select json_agg(q) from userQuery q)', 'select ''Pass your query as parameter in double quotes''', 'SQL-запрос', 'adminCmdGroup');



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




INSERT INTO bot.options(option_group, option_name, option_value, option_description) VALUES('ChatAdmin', 'ChatUserMessageCountHiHi', '-1', 'Верхняя аварийная уставка');
INSERT INTO bot.options(option_group, option_name, option_value, option_description) VALUES('ChatAdmin', 'ChatUserMessageCountHi',   '-1', 'Верхняя предупредительная уставка');
--INSERT INTO bot.options(option_group, option_name, option_value, option_description) VALUES('ChatAdmin','DefaultChatId',            '',   '»дентификатор чата, с которым бот работает по умолчанию');

--DROP TABLE zl.bans;

CREATE TABLE zl.bans (
    ban_id             serial      NOT NULL PRIMARY KEY,
    user_id            int         NOT NULL REFERENCES bot.users (user_id),
    chat_id            int         NOT NULL REFERENCES bot.chats (chat_id),
    warning_message_id int             NULL REFERENCES bot.messages (message_id),
    ban_finish_date    timestamptz     NULL,
    update_date        timestamptz NOT NULL DEFAULT now(),
    insert_date        timestamptz NOT NULL DEFAULT now()
);
CREATE TRIGGER bans_reset_update_date BEFORE UPDATE
ON zl.bans FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE zl.bans IS 'Информация о банах';




CREATE TABLE zl.accountings (
    accounting_id         serial      NOT NULL PRIMARY KEY,
    accounting_start_date timestamptz NOT NULL DEFAULT now(),   
    update_date           timestamptz NOT NULL DEFAULT now()
);
CREATE TRIGGER accountings_reset_update_date BEFORE UPDATE
ON zl.accountings FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE zl.accountings IS 'Информация о времени начала учёта сообщений каждого отдельного пользователя';




CREATE TABLE zl.notifications (
    notification_id           serial        NOT NULL PRIMARY KEY,
    notification_is_active     bool         NOT NULL DEFAULT TRUE,
    notification_message      varchar(2000) NOT NULL,
    notification_month        int               NULL, -- мес¤ц, если событие ежегодное; null, если событие ежемес¤чное
    notification_day          int           NOT NULL, -- день мес¤ца
    notification_hour         int           NOT NULL, -- врем¤ оповещени¤
    notification_minute       int           NOT NULL, -- врем¤ оповещени¤
    notification_exec_date    timestamptz       NULL, -- время последнего срабатывания
    update_date               timestamptz   NOT NULL DEFAULT now(),
    insert_date               timestamptz   NOT NULL DEFAULT now()
);
CREATE TRIGGER notifications_reset_update_date BEFORE UPDATE
ON zl.notifications FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE zl.notifications IS 'Напоминание о событиях';




CREATE TABLE zl.auxiliary_words (
    the_word    varchar(100)  NOT NULL PRIMARY KEY, -- word нельз¤ использовать в постгресе
    insert_date timestamptz   NOT NULL DEFAULT now()
);
COMMENT ON TABLE zl.auxiliary_words IS 'Вспомогательные слова - то, что должно быть отсеяно из статистики';





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




-- READY
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




-- READY
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



    
-- READY
CREATE OR REPLACE FUNCTION zl.sf_process_group_message(
    _chat_id integer,
    _message_id integer,
    _accounting_start_date timestamp with time zone, -- важно переопределять во время выполнения
    _msg_limit_hi integer,                           -- важно переопределять во время выполнения
    _msg_limit_hihi integer,                         -- важно переопределять во время выполнения
    _msg_limit_after_ban integer = 5,
    _start_account_after integer default 100)        -- важно переопределять во время выполнения
    RETURNS json 
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
   _user_id integer;
   _accounted_user_msg_count integer;
   _daily_chat_msg_count integer;
   _ban_id integer;
BEGIN
 -- При достижении лимита пользователь банится на 3 часа. 
 --     Если лимит достигнут ближе к концу дня, бан продолжает своё действие 
 --     до окончания трёхчасового периода. Если 3 часа бана прошло, 
 --     а день не закончился, позволяем пользователю отправку 5-ти сообщений 
 --     до начала следующего дня
 -- 
 -- После восстановления интернета через 1 минуту происходит 
 --     переопределение лимитов для того, чтобы не перетереть 
 --     только что полученные сообщения
    
    select user_id into _user_id from bot.messages where message_id = _message_id;

    select ban_id into _ban_id from zl.bans 
         where user_id = _user_id and chat_id = _chat_id
           and insert_date > now()::date 
      order by insert_date desc limit 1;
      
    -- Если для пользователя есть активный бан, то удаляем сообщение. Учитываем бан с предыдущего дня
    if exists (select 1 from zl.bans 
                where ban_id = _ban_id
                  and ban_finish_date > now() 
             order by insert_date desc)
    then
        return '{ 
                     "Action": "DeleteMessage",
                     "Info": "Для пользователя имеется активный бан"
                }';
    end if;
 
    -- Начало индивидуального учёта после _start_account_after сообщений в чате 
    -- от любых пользователей с 00:00 текущего дня
    _daily_chat_msg_count = (select count(*) from bot.messages where chat_id = _chat_id and insert_date > now()::date);
    if (_accounting_start_date is not null and _daily_chat_msg_count < _start_account_after)
    then
        return '{ 
                    "Action": "SetAccountingStartDate", 
                    "AccountingStartDate": null 
                }';
    end if;
 
    -- Дата начала учёта хранится в пямяти программы и передаётся в этот метод
    -- Переопределяется после перезагрузки или восстановления соединения с сетью
    if (_accounting_start_date is null and _daily_chat_msg_count >= _start_account_after)
    then
        return '{ 
                    "Action": "SetAccountingStartDate",
                    "AccountingStartDate": "' || now()::text || E'"\n' ||',
                    "MessageText" : "В чате уже ' || _daily_chat_msg_count::text || ' сообщений. Начинаю персональный учёт." 
               }';
    end if;

    select user_id into _user_id from bot.messages where message_id = _message_id;
     
    select count(*) into _accounted_user_msg_count from bot.messages 
    where insert_date > _accounting_start_date 
      and user_id = _user_id
      and is_deleted = false;
  

    -- С начала учёта каждому доступно максимум _msg_limit_hihi сообщений.
    -- После _msg_limit_hi сообщения с начала учёта надо выдать пользователю 
    --     предупреждение о приближении к лимиту. При этом создаётся запись
    --     в таблице zl.bans и ставится пометка о том, что пользователь предупреждён
    if (_accounted_user_msg_count < _msg_limit_hi) then
        return '{ 
                     "Action": "Continue",
                     "Info": "Количество учтённых сообщений пользователя меньше предупредитетельной уставки: ' || _accounted_user_msg_count::text || ' < ' || _msg_limit_hi::text || '"
                }';
    elsif (_accounted_user_msg_count >= _msg_limit_hi and _accounted_user_msg_count < _msg_limit_hihi) then

        -- Создаём неактивную запись в таблице банов (без даты окончания), 
        -- выдаём предупреждение и фиксируем это, чтоб не повторяться
        if (_ban_id is null) then        
            insert into zl.bans (user_id, chat_id)
            select _user_id, _chat_id;
            
            select ban_id into _ban_id from zl.bans 
             where user_id = _user_id and chat_id = _chat_id
               and insert_date > now()::date 
          order by insert_date desc limit 1;

            return '{
                        "Action": "SendMessageToGroup", 
                        "MessageText": "<UserName>, количеcтво сообщений, отправленных Вами с начала учёта: ' || _accounted_user_msg_count::text || '.\nОсталось сообщений до бана: ' || (_msg_limit_hihi - _accounted_user_msg_count)::text || '",
                        "BanId": "' || _ban_id::text || '"
                    }';
        else
            return '{ 
                         "Action": "Continue",
                         "Info": "Предупреждение о приближении к лимиту было выслано ранее"
                    }';
        end if;
    elsif (_accounted_user_msg_count >= _msg_limit_hihi) then
        if (_ban_id is null) then
            return '{ 
                        "Action": "SendMessageToOwner", 
                        "MessageText": "Error! The user has exceeded the limit and I don''t know what to do!" 
                    }';
 
        -- Если бан не активен, активируем его (задаём дату окончания)
        -- Отправляем сообщение пользователю
        elsif (select ban_finish_date from zl.bans where ban_id = _ban_id) is null then
            update zl.bans set ban_finish_date = now() + interval '3 hours'
            where ban_id = _ban_id;
            return '{
                        "Action": "SendMessageToGroup", 
                        "MessageText": "<UserName>, Вы превысили лимит сообщений (' || _msg_limit_hihi::text || '). Все последующие сообщения в течение 3-х часов будут удаляться.\nПотом до конца дня у Вас будет ' || _msg_limit_after_ban::text || ' сообщений.",
                        "BanId": "' || _ban_id::text || '"
                    }';

        -- Иначе, если бан активен и функция всё ещё выполняется, значит бан отработал 
        -- и у пользователя осталось _msg_limit_after_ban сообщений.
        elsif (_accounted_user_msg_count >= _msg_limit_hihi and _accounted_user_msg_count < _msg_limit_hihi + _msg_limit_after_ban) then
            return '{ 
                         "Action": "Continue",
                         "Info": "Бан закончился, пользователь расходует последние ' || _msg_limit_after_ban::text || ' сообщений за день"
                    }';

        -- При достижении второго предела отодвигаем время бана на конец дня и шлём предупреждающее сообщение
        elsif (_accounted_user_msg_count >= _msg_limit_hihi + _msg_limit_after_ban) then
            update zl.bans set ban_finish_date = now()::date + interval '1 day' - interval '1 second'
            where ban_id = _ban_id;
            return '{
                        "Action": "SendMessageToGroup", 
                        "MessageText" : "<UserName>, вы израсходовали свой лимит сообщений до конца дня",
                        "BanId": "' || _ban_id::text || '"
                    }';
        ---- иначе баним до конца дня
        --elsif (_accounted_user_msg_count > _msg_limit_hihi + _msg_limit_after_ban) then
        --    return '{ "Answer" : "Banned" }';
        else
            return '{ 
                        "Action": "SendMessageToOwner",
                        "MessageText" : "Error! The user has exceeded the limit but no condition has been met!" 
                    }';
        end if;
    end if;
    return '{
                "Action": "SendMessageToOwner",
                "MessageText" : "Error! End of function has been reached!" 
            }';
END;
$BODY$;
ALTER FUNCTION zl.sf_process_group_message(integer, integer, timestamp with time zone, integer, integer, integer, integer)
    OWNER TO postgres;




-- READY
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




-- READY
CREATE OR REPLACE FUNCTION zl.sf_cmd_get_full_statistics(
    _users_limit integer,
    _from_date   timestamp with time zone,
    _to_date     timestamp with time zone DEFAULT now()
    )
    RETURNS text
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    _result_text text = ''; 
    _header_1    text = 'Active users:';   
    _header_2    text = 'Total messages:';
    _chat_name   text;
    _chat_id     integer;
    _notice_item record;
BEGIN

  CREATE TEMP TABLE statisticsTable AS  
  SELECT full_query.chat_id
       , full_query.user_id
       , full_query.chat_name
       , full_query.row_header
       , full_query.all_message_count
       , full_query.accounted_message_count
    FROM
    (    
         -- All chats, all users, theirs all message count and message count from accounting date
        (SELECT s1.chat_id
              , s1.user_id
              , c.chat_name
              , coalesce(u.user_name, u.user_full_name) as row_header
      		  , s1.message_count as all_message_count
      		  , s2.message_count as accounted_message_count
           FROM bot.chats c
      LEFT JOIN bot.sf_get_chat_statistics(c.chat_id, _users_limit, _from_date, _to_date) s1 ON s1.chat_id = c.chat_id
      LEFT JOIN bot.sf_get_chat_statistics(c.chat_id, _users_limit, (SELECT accounting_start_date FROM zl.accountings WHERE accounting_start_date > _from_date LIMIT 1)
                 , _to_date) s2 ON s1.user_id = s2.user_id AND s1.chat_id = s2.chat_id
      LEFT JOIN bot.users u on u.user_id = s1.user_id
          WHERE s1.chat_id IS NOT NULL
       ORDER BY s1.chat_id, s1.message_count DESC, s2.message_count DESC)
              
    UNION
         -- Number of active users
         SELECT s.chat_id
              , null
              , s.chat_name AS chat_name
              , _header_1   AS row_header
              , count(*)    AS all_message_count
              , NULL        AS accounted_message_count
           FROM (SELECT c.chat_name, c.chat_id
                   FROM bot.messages m
              LEFT JOIN bot.chats c ON c.chat_id = m.chat_id
                  WHERE m.insert_date > _from_date AND m.insert_date < _to_date
                    AND m.is_deleted = false
                    AND c.chat_type_code = 'GROUP'
               GROUP BY m.user_id, c.chat_id) s
       GROUP BY s.chat_id, s.chat_name
       
    UNION      
        -- Всего сообщений
         SELECT c.chat_id
              , null
              , c.chat_name
              , _header_2        AS row_header
              , count(m.user_id) AS all_message_count
              , NULL             AS accounted_message_count
           FROM bot.messages m
      LEFT JOIN bot.chats c ON c.chat_id = m.chat_id
          WHERE m.insert_date > _from_date AND m.insert_date < _to_date
            AND m.is_deleted = false
       GROUP BY c.chat_id
   ) full_query;
   
  -- Для каждого чата формируем сводку
  FOR _chat_id IN (SELECT chat_id FROM statisticsTable GROUP BY chat_id)
  LOOP
      _result_text = _result_text || '**' || (SELECT chat_name FROM bot.chats WHERE chat_id = _chat_id LIMIT 1) || E'**\n';
      _result_text = _result_text || (SELECT _header_2 || ' ' || all_message_count FROM statisticsTable WHERE chat_id = _chat_id and row_header = _header_2);
      
      --RAISE NOTICE '1. _result_text: %', _result_text;
   
      -- Для групповых чатов больше информации
      IF EXISTS (SELECT 1 FROM statisticsTable WHERE chat_id = _chat_id and row_header = _header_1) 
      THEN
          _result_text = _result_text || E'\n' || (SELECT row_header || ' ' || all_message_count FROM statisticsTable WHERE chat_id = _chat_id and row_header = _header_1) || E'\n';
          _result_text = _result_text || E'---\nMost active users:\n';
          _result_text = _result_text || coalesce((SELECT string_agg(row_header || ' ' || all_message_count || COALESCE(' (' || accounted_message_count || '*)', ''), E'\n'
                                                                   ORDER BY all_message_count DESC, coalesce(accounted_message_count, 0) DESC) 
                                                   FROM statisticsTable
                                                  WHERE chat_id = _chat_id and row_header not in (_header_1, _header_2)), '[exception]');
      END IF;

      --RAISE NOTICE '2. _result_text: %', _result_text;

      _result_text = _result_text || E'\n\n================\n\n';
  END LOOP;
  
   _result_text = (SELECT trim(trailing E'\n\n================\n\n' from _result_text));

  DROP TABLE statisticsTable;
  --RAISE NOTICE '3. _result_text: %', _result_text;
  
  IF (_result_text is null or length(trim(_result_text)) = 0) THEN
      RETURN 'There are no messages in the specified time range';
  END IF;
  
  RETURN _result_text;
END;
$BODY$;
ALTER FUNCTION zl.sf_cmd_get_full_statistics(integer, timestamp with time zone, timestamp with time zone)
    OWNER TO postgres;
COMMENT ON FUNCTION zl.sf_cmd_get_full_statistics(integer, timestamp with time zone, timestamp with time zone)
    IS 'Returns all chats statistics in the specified time range';
    
    
    
    
    



    




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
