
-- Process incoming messages of specific chat
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
    
    select user_id into _user_id from bot.messages where message_id = _message_id;

    select ban_id into _ban_id from zl.bans 
         where user_id = _user_id and chat_id = _chat_id
           and insert_date > now()::date -- - interval '3 hours' -- Захватываем баны с предыдущего дня
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

-- !!! На случай, если было разорвано соединение с интернетом, и бан пользователя 
--     закончился к моменту его восстановления, то, несмотря на сброшенную дату начала учёта
--     ориентируемся на кол-во сообщений пользователя, оставленных после окончания бана
    if (select insert_date::date from zl.bans where ban_id = _ban_id) = now()::date -- Важно учитывать только баны текущего дня
    then
        if (select count(m.*)
              from bot.messages m
         left join zl.bans b on b.ban_id = _ban_id
             where m.insert_date > b.ban_finish_date
               and m.user_id = _user_id
               and m.is_deleted = false) >= _msg_limit_after_ban
        then
            update zl.bans set ban_finish_date = now()::date + interval '1 day' - interval '1 second'
            where ban_id = _ban_id;
            return '{
                        "Action": "SendMessageToGroup", 
                        "MessageText" : "<UserName>, вы израсходовали свой лимит сообщений до конца дня",
                        "BanId": "' || _ban_id::text || '"
                    }';
        end if;
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
    elsif (_accounting_start_date is null and _daily_chat_msg_count < _start_account_after)
    then
        return '{ 
                    "Action": "Continue",
                    "Info": "Учёт сообщений ещё не начался" 
                }';
    end if;

    --select user_id into _user_id from bot.messages where message_id = _message_id;
     
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
                        "Info" : "Error! The user has exceeded the limit but no condition has been met!" 
                    }';
        end if;
    end if;
    return '{
                "Action": "SendMessageToOwner",
                "Info" : "Error! End of function has been reached!" 
            }';
END;
$BODY$;
ALTER FUNCTION zl.sf_process_group_message(integer, integer, timestamp with time zone, integer, integer, integer, integer)
    OWNER TO postgres;



    
-- Get statistics of all chats in the specified time interval
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
              , coalesce(u.user_full_name, u.user_name) as row_header
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
         -- Number of active users in chat
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
         -- Total messages in chat
         SELECT c.chat_id
              , null
              , c.chat_name--coalesce(u.user_full_name, c.chat_name)
              , _header_2        AS row_header
              , count(m.user_id) AS all_message_count
              , NULL             AS accounted_message_count
           FROM bot.messages m
      LEFT JOIN bot.chats c ON c.chat_id = m.chat_id
      LEFT JOIN bot.users u ON u.user_id = m.user_id
          WHERE m.insert_date > _from_date AND m.insert_date < _to_date
            AND (c.chat_type_code = 'GROUP' OR (c.chat_type_code = 'PRIVATE' AND u.user_is_bot = false)) -- remove bot's answers
            AND m.is_deleted = false
       GROUP BY c.chat_id--, u.user_full_name
   ) full_query;

   RAISE NOTICE '0. _result_text: %', _result_text;
   
  -- Для каждого чата формируем сводку
  FOR _chat_id IN (SELECT chat_id FROM statisticsTable GROUP BY chat_id)
  LOOP
      _result_text = _result_text || '**' || (SELECT chat_name FROM bot.chats WHERE chat_id = _chat_id LIMIT 1) || E'**\n';
      _result_text = coalesce(_result_text, '') || (SELECT _header_2 || ' ' || all_message_count FROM statisticsTable WHERE chat_id = _chat_id and row_header = _header_2);
      
      RAISE NOTICE '1.0. _result_text: %', _result_text;
      RAISE NOTICE '1.1. _chat_id: %', _chat_id;
      RAISE NOTICE '1.2. _header_1: %', _header_1;
      RAISE NOTICE '1.3. _header_2: %', _header_2;
      RAISE NOTICE '1.3. _chat_name: %', _chat_name;
   
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

      RAISE NOTICE '2. _result_text: %', _result_text;

      --_result_text = _result_text || E'\n\n================\n\n';
	  _result_text = coalesce(_result_text || E'\n\n================\n\n', '');
  END LOOP;
  
   _result_text = (SELECT trim(trailing E'\n\n================\n\n' from _result_text));

  DROP TABLE statisticsTable;
  RAISE NOTICE '3. _result_text: %', _result_text;
  
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
--select zl.sf_cmd_get_full_statistics(15, '2020-10-31 00:00:00', '2020-10-31 23:59:59')
    
    
    
    