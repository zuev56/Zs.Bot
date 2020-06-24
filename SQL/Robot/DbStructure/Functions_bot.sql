
-- Получение массива разрешений для заданной роли
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
    IS 'Получение массива разрешений для заданной роли';



-- Получение справки по функциям, доступным для данной роли
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
    IS 'Получение справки по функциям, доступным для данной роли';









CREATE OR REPLACE FUNCTION rmgr."TESTsfAdmGetTopUserMsgCount"(
    bigint chat_id,
    datetime timestamp with time zone,
    num integer)
    RETURNS bigint
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE 
AS $BODY$
BEGIN
-- Получение количества неудалённых сообщений из заданного чата
    IF (dateTime IS NOT NULL AND num IS NOT NULL) 
    THEN
        RETURN (SELECT count(m.*)
                FROM rmgr."ReceivedMsg" m
                JOIN rmgr."User" u ON u."UserId" = m."UserId" -- -- ??? Зачем ???
                WHERE (m."ReceivedMsgDate" + INTERVAL '3 hours')::Date = dateTime::Date -- AND dateTime::Date + INTERVAL '1 DAY' 
                  AND m."ChatId" = chat_id
                  AND m."IsDeleted" <> true
                GROUP BY m."UserId"--, count(m.*) -- ??? Зачем ???
                ORDER BY count(m.*) DESC -- ??? Зачем ???
                LIMIT 1
                OFFSET num-1); -- ??? Зачем ???
    ELSE
        RETURN -1;
    END IF;
END;
$BODY$;

ALTER FUNCTION rmgr."TESTsfAdmGetTopUserMsgCount"(timestamp with time zone, integer)
    OWNER TO postgres;

COMMENT ON FUNCTION rmgr."TESTsfAdmGetTopUserMsgCount"(timestamp with time zone, integer)
    IS 'Возвращает количество сообщений N-го пользователя из списка самых активных за заданный день';





CREATE OR REPLACE FUNCTION bot.sf_get_group_statistics(
    rows_limit integer,
    from_date timestamp with time zone,
    to_date timestamp with time zone DEFAULT now())
    RETURNS TABLE(chat_id bigint, user_id integer, message_count bigint) 
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
BEGIN
    RETURN QUERY(
        SELECT m.chat_id
             , u.user_id
             , count(m.*) 
          FROM bot.messages m
          JOIN bot.users u ON u.user_id = m.user_id
         WHERE m.insert_date >= from_date AND m.insert_date <= to_date
           AND m.is_deleted = false
      GROUP BY m.chat_id, u.user_id
      ORDER BY count(m.*) DESC
      LIMIT rows_limit
    );

END;
$BODY$;

ALTER FUNCTION bot.sf_get_group_statistics(integer, timestamp with time zone, timestamp with time zone)
    OWNER TO postgres;

COMMENT ON FUNCTION bot.sf_get_group_statistics(integer, timestamp with time zone, timestamp with time zone)
    IS 'Возвращает заданное количество строк в заданном временном диапазоне - ИмяПользователя, КоличествоСообщений';



CREATE OR REPLACE FUNCTION zl.sf_get_most_popular_words(
    _chat_id integer,
    from_date timestamp with time zone,
    to_date timestamp with time zone DEFAULT now(),
    min_word_length integer DEFAULT 2)
    RETURNS TABLE(word character varying, count bigint) 
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE 
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
    _start_account_after integer default 100)
    RETURNS json 
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE
AS $BODY$
DECLARE
   _result text = '{ "Answer" : "Not initialized" }';
   _user_id integer;
   _user_msg_count integer;
   _msg_limit_after_ban integer = 5;
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
    
    select ban_id into _ban_id from zl.bans 
         where user_id = _user_id and chat_id = _chat_id
           and insert_date > now()::date 
      order by insert_date desc limit 1;

    -- Если для пользователя есть активный бан, то удаляем сообщение. Учитываем бан с предыдущего дня
    if exists (select 1 from zl.bans 
                where ban_id = _ban_id
                  and ban_finish_date + interval '3 hours' < now() 
             order by insert_date desc)
    then
        return '{ "Answer" : "Banned" }';
    end if;
 
    -- Начало индивидуального учёта после 100 сообщений в чате 
    -- от любых пользователей с 00:00 текущего дня
    if ((select count(*) from bot.messages where insert_date > now()::date) < _start_account_after)
    then
        return '{ "Answer" : "Ok", "AccountingStartDate" : null }';
    end if;
 
    -- Дата начала учёта хранится в пямяти программы и передаётся в этот метод
    -- Переопределяется после перезагрузки или восстановления соединения с сетью
    if (_accounting_start_date is null)
    then
        return '{ "Answer" : "Ok", "AccountingStartDate" : ' || now() || ' }';
    end if;
 
    select user_id into _user_id from bot.messages where message_id = _message_id;
     
    select count(*) into _user_msg_count from bot.messages where insert_date > now()::date and user_id = _user_id;
  
 
    -- С начала учёта каждому доступно максимум 30 сообщений.
    -- После 25-го сообщения с начала учёта выдать пользователю 
    --     предупреждение о приближении к лимиту. При этом создаётся запись
    --     в таблице Ban и ставится пометка о том, что пользователь предупреждён
    if (_user_msg_count < _msg_limit_hi) then
        return '{ "Status" : "Ok" }';
    elsif (_user_msg_count > _msg_limit_hi and _user_msg_count < _msg_limit_hihi) then
        -- Создаём неактивную запись в таблице банов (без даты окончания), 
        -- выдаём предупреждение и фиксируем это, чтоб не повторяться
        if (_ban_id is null) then        
            insert into zl.bans (user_id, chat_id)
            select _user_id, _chat_id;
            return '{
                        "Status" : "BanWarning", 
                        "MessageText" : "количеcтво сообщений, отправленных Вами с начала учёта: _user_msg_count\\n
                                         Осталось сообщений до трёхчасового бана: _msg_limit_hihi - _user_msg_count" 
                    }';
        end if;
    elsif (_user_msg_count >= _msg_limit_hihi) then
        if (_ban_id is null) then
            return '{ "Status" : "Error", "MessageText" : "Пользователь превысил лимит, но запись о бане ещё не создана!" }';
 
        -- Если бан не активен, активируем его (задаём дату окончания)
        -- Отправляем сообщение пользователю
        elsif (select ban_finish_date from zl.ban where ban_id = _ban_id) is null then
            update zl.bans set ban_finish_date = now() + interval '3 hours'
            where ban_id = _ban_id;
            
        -- Иначе, если бан активен, этот код выполняется, значит бан отработал 
        -- и у пользователя осталось _msg_limit_after_ban сообщений.
        -- после выхода за второй предел, шлём сообщение и блочим всё остальное
        elsif (_user_msg_count > _msg_limit_hihi 
              and ban_finish_date < now()::date + interval '1 day' - interval '1 second') then
            update zl.bans set ban_finish_date = now()::date + interval '1 day' - interval '1 second'
            where ban_id = _ban_id;
            return '{
                        "Status" : "BanWarning", 
                        "MessageText" : "вы израсходовали свой лимит сообщений до конца дня" 
                    }';
        -- иначе баним до конца дня
        elsif (_user_msg_count > _msg_limit_hihi + _msg_limit_after_ban) then
            return '{ "Answer" : "Banned" }';
        else
            return '{ "Status" : "Error", "MessageText" : "Пользователь превысил лимит, но ни одно условие не выполнилось!" }';
        end if;
    end if;
    return _result;
END;
$BODY$;
ALTER FUNCTION zl.sf_process_group_message(integer, integer, timestamp with time zone, integer, integer, integer)
    OWNER TO postgres;





CREATE OR REPLACE FUNCTION zl.sf_cmd_get_statistics(
    rows_limit integer,
    from_date timestamp with time zone,
    to_date timestamp with time zone DEFAULT now()
    )
    RETURNS text
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE
AS $BODY$
DECLARE
    result_text TEXT := ''; 
    h_1 TEXT := 'Активных пользователей';   
    h_2 TEXT := 'Всего сообщений';
    chat_name TEXT;
BEGIN

  -- Получаем статистику для всех чатов во временную таблицу
  CREATE TEMP TABLE statisticsTable AS  
  SELECT full_query.chat_name
       , full_query.user_name
       , full_query.all_message_count
       , full_query.accounted_message_count
    FROM
    (    
        -- Статитика участников чата
        (SELECT c.chat_title AS chat_name
              , COALESCE(u.user_full_name, u.user_name)  AS user_name
              , sf1.message_count AS all_message_count
              , sf2.message_count AS accounted_message_count
           FROM bot.sf_get_group_statistics(rows_limit, from_date, to_date) sf1
      LEFT JOIN bot.users u ON sf1.user_id = u.user_id
      LEFT JOIN bot.chats c ON c.chat_id = sf1.chat_id
      LEFT JOIN bot.sf_get_group_statistics(rows_limit, (SELECT accounting_start_date FROM zl.accountings WHERE accounting_start_date > from_date LIMIT 1)
                                           , to_date) sf2 ON sf1.user_id = sf2.user_id AND sf1.chat_id = sf2.chat_id
       ORDER BY sf1.chat_id, sf1.message_count DESC, sf2.message_count DESC 
          LIMIT rows_limit)
       
    UNION
        -- Активных пользователей
         SELECT s.chat_name AS chat_name
              , h_1         AS user_name
              , count(*)    AS all_message_count
              , NULL        AS accounted_message_count
           FROM (SELECT c.chat_title  AS chat_name
                    FROM bot.messages m
               LEFT JOIN bot.chats c ON c.chat_id = m.chat_id
                   WHERE m.insert_date > from_date AND m.insert_date < to_date
                     AND m.is_deleted = false
                     AND c.chat_title IS NOT NULL -- Отсекаем личные переписки с ботом
                GROUP BY m.user_id, c.chat_id ) s
       GROUP BY s.chat_name
       
    UNION      
        -- Всего сообщений
         SELECT chat_title       AS chat_name
              , h_2              AS user_name
              , count(m.user_id) AS all_message_count
              , NULL             AS accounted_message_count
           FROM bot.messages m
      LEFT JOIN bot.chats c ON c.chat_id = m.chat_id
          WHERE m.insert_date > from_date AND m.insert_date < to_date
            AND m.is_deleted = false
       GROUP BY c.chat_id
   ) full_query;
   
  -- Для каждого чата формируем сводку
  FOR c_name IN (select chat_name from statisticsTable group by chat_name)
  LOOP
      result_text := result_text || '**' || c_name || E'**\n';
      result_text := result_text || (select chat_name || ' ' || all_message_count from statisticsTable where chat_name = c_name and user_name = h_2);
      
      -- Для групповых чатов больше информации
      IF EXISTS (select 1 from statisticsTable where chat_name = c_name and user_name = h_1) 
      THEN
          result_text := result_text || E'\n' || (select user_name || ' ' || all_message_count from statisticsTable where chat_name = c_name and user_name = h_1) || E'\n';
          result_text := result_text || E'---\nСамые активные:\n';
          result_text := result_text || coalesce((select string_agg(user_name || ' ' || all_message_count || COALESCE(' (' || accounted_message_count || '*)', ''), E'\n'
                                                                    order by all_message_count desc, coalesce(accounted_message_count, 0) desc) 
                                                    from statisticsTable
                                                   where chat_name = c_name and user_name not in (h_1, h_2)), 'exception');
      END IF;
      result_text := result_text || E'\n\n================\n\n';
  END LOOP;
  
  -- Удаляем временную таблицу
  drop table statisticsTable;
  
  RETURN result_text;

END;
$BODY$;

ALTER FUNCTION rmgr.sf_cmd_get_statistics(integer, timestamp with time zone, timestamp with time zone)
    OWNER TO postgres;

COMMENT ON FUNCTION bot.sf_cmd_get_statistics(integer, timestamp with time zone, timestamp with time zone)
    IS 'Получение статистики по всем чатам за указанный период времени';
    
    
    
    
    
    



    
