
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



CREATE OR REPLACE FUNCTION zl.sf_get_most_popular_words(
    _chat_id integer,
    from_date timestamp with time zone,
    to_date timestamp with time zone DEFAULT now(),
    min_word_length integer DEFAULT 2)
    RETURNS TABLE(word character varying, count bigint) 
    LANGUAGE 'plpgsql'
    ROWS 1000
AS $BODY$
DECLARE
   msg_text text;
   words text[];
BEGIN
   msg_text = (select string_agg(m.raw_data ->> 'text', ' ')
                from bot.messages m
                where m.insert_date > from_date and m.insert_date < to_date
                  and m.chat_id = _chat_id);
                              
   msg_text = REPLACE(msg_text, chr(10), ' ' );  
   msg_text = REPLACE(msg_text, '\\', ' ' );
   msg_text = REPLACE(msg_text, '\n', ' ' );
   msg_text = REPLACE(msg_text, '/', ' ' );
   msg_text = REPLACE(msg_text, ',', ' ' );
   msg_text = REPLACE(msg_text, '.', ' ' );
   msg_text = REPLACE(msg_text, '(', ' ' );
   msg_text = REPLACE(msg_text, ')', ' ' );
   msg_text = REPLACE(msg_text, '[', ' ' );
   msg_text = REPLACE(msg_text, ']', ' ' );
   msg_text = REPLACE(msg_text, '{', ' ' );
   msg_text = REPLACE(msg_text, '}', ' ' );
   msg_text = REPLACE(msg_text, '?', ' ' );
   msg_text = REPLACE(msg_text, '!', ' ' );
   msg_text = REPLACE(msg_text, '"', ' ' );
   msg_text = REPLACE(msg_text, '«', ' ' );
   msg_text = REPLACE(msg_text, '»', ' ' );
   msg_text = REPLACE(msg_text, '  ', ' ' );
   msg_text = REPLACE(msg_text, '  ', ' ' );
   msg_text = REPLACE(msg_text, '  ', ' ' ); 
   msg_text = REPLACE(msg_text, ' - ', ' ' );
   msg_text = REPLACE(msg_text, '"
"', ' ' );
   
   words = string_to_array(LOWER(msg_text), ' ');

    RETURN QUERY(
        SELECT REPLACE(REPLACE(w::varchar(100), '(', '' ), ')', '')::varchar(100) as word, count(*) as word_count
        FROM (select unnest(words)) as w
        where Length(w::varchar(100)) >= min_word_length + 2
          and w::varchar(100) not in (SELECT ('(' || the_word || ')') FROM zl.auxiliary_words)
        group by w
        having count(*) > 1
        order by word_count desc
    );
END;
$BODY$;
ALTER FUNCTION zl.sf_get_most_popular_words(integer, timestamp with time zone, timestamp with time zone, integer)
    OWNER TO postgres;

CREATE OR REPLACE FUNCTION zl.sf_get_most_popular_words(
    msg_text text,
    min_word_length integer DEFAULT 2)
    RETURNS TABLE(word text) 
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
DECLARE
   words text[];
BEGIN
   msg_text = REPLACE(msg_text, chr(10), ' ' );  
   msg_text = REPLACE(msg_text, '\\', ' ' );
   msg_text = REPLACE(msg_text, '\n', ' ' );
   msg_text = REPLACE(msg_text, '/', ' ' );
   msg_text = REPLACE(msg_text, ',', ' ' );
   msg_text = REPLACE(msg_text, '.', ' ' );
   msg_text = REPLACE(msg_text, '(', ' ' );
   msg_text = REPLACE(msg_text, ')', ' ' );
   msg_text = REPLACE(msg_text, '[', ' ' );
   msg_text = REPLACE(msg_text, ']', ' ' );
   msg_text = REPLACE(msg_text, '{', ' ' );
   msg_text = REPLACE(msg_text, '}', ' ' );
   msg_text = REPLACE(msg_text, '?', ' ' );
   msg_text = REPLACE(msg_text, '!', ' ' );
   msg_text = REPLACE(msg_text, '"', ' ' );
   msg_text = REPLACE(msg_text, '«', ' ' );
   msg_text = REPLACE(msg_text, '»', ' ' );
   msg_text = REPLACE(msg_text, '  ', ' ' );
   msg_text = REPLACE(msg_text, '  ', ' ' );
   msg_text = REPLACE(msg_text, '  ', ' ' ); 
   msg_text = REPLACE(msg_text, ' - ', ' ' );
   
   words = string_to_array(LOWER(msg_text), ' ');

    RETURN QUERY(
        SELECT REPLACE(REPLACE(w::varchar(100), '(', '' ), ')', '')::varchar(100) || ' (' || count(*) || ')' as word
        FROM (select unnest(words)) as w
        where Length(w::varchar(100)) >= min_word_length + 2
          and w::varchar(100) not in (SELECT ('(' || the_word || ')') FROM zl.auxiliary_words)
        group by w
        having count(*) > 1
        order by count(*) desc
    );
END;
$BODY$;
ALTER FUNCTION zl.sf_get_most_popular_words(text, integer)
    OWNER TO postgres;





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
        return '{ "Action": "DeleteMessage" }';
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
        return '{ "Action": "Continue" }';
    elsif (_accounted_user_msg_count >= _msg_limit_hi and _accounted_user_msg_count < _msg_limit_hihi) then

        -- Создаём неактивную запись в таблице банов (без даты окончания), 
        -- выдаём предупреждение и фиксируем это, чтоб не повторяться
        if (_ban_id is null) then        
            insert into zl.bans (user_id, chat_id)
            select _user_id, _chat_id;
            return '{
                        "Action": "SendMessageToGroup", 
                        "MessageText": "<UserName>, количеcтво сообщений, отправленных Вами с начала учёта: ' || _accounted_user_msg_count::text || '.\nОсталось сообщений до бана: ' || (_msg_limit_hihi - _accounted_user_msg_count)::text || '" 
                    }';
        else
            return '{ "Action": "Continue" }';
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
                        "MessageText": "<UserName>, Вы превысили лимит сообщений (' || _msg_limit_hihi::text || '). Все последующие сообщения в течение 3-х часов будут удаляться.\nПотом до конца дня у Вас будет ' || _msg_limit_after_ban::text || ' сообщений."
                    }';

        -- Иначе, если бан активен и функция всё ещё выполняется, значит бан отработал 
        -- и у пользователя осталось _msg_limit_after_ban сообщений.
        elsif (_accounted_user_msg_count >= _msg_limit_hihi and _accounted_user_msg_count < _msg_limit_hihi + _msg_limit_after_ban) then
            return '{ "Action": "Continue" }';

        -- При достижении второго предела отодвигаем время бана на конец дня и шлём предупреждающее сообщение
        elsif (_accounted_user_msg_count >= _msg_limit_hihi + _msg_limit_after_ban) then
            update zl.bans set ban_finish_date = now()::date + interval '1 day' - interval '1 second'
            where ban_id = _ban_id;
            return '{
                        "Action": "SendMessageToGroup", 
                        "MessageText" : "<UserName>, вы израсходовали свой лимит сообщений до конца дня" 
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
              , u.user_name      as row_header
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

      _result_text = _result_text || E'\n\n================\n\n';
  END LOOP;
  
   _result_text = (SELECT trim(trailing E'\n\n================\n\n' from _result_text));

  DROP TABLE statisticsTable;
  
  RETURN _result_text;
END;
$BODY$;
ALTER FUNCTION zl.sf_cmd_get_full_statistics(integer, timestamp with time zone, timestamp with time zone)
    OWNER TO postgres;
COMMENT ON FUNCTION zl.sf_cmd_get_full_statistics(integer, timestamp with time zone, timestamp with time zone)
    IS 'Returns all chats statistics in the specified time range';
    
    
    
    
    
    



    
