-- FUNCTION: cas."sfCmdGetStatistics"(integer, timestamp with time zone, timestamp with time zone)

-- DROP FUNCTION cas."sfCmdGetStatistics"(integer, timestamp with time zone, timestamp with time zone);

-- Получение статистики по всем чатам за указанный период времени
CREATE OR REPLACE FUNCTION rmgr."sfCmdGetStatistics"(
        rows_limit integer,
        from_date  timestamp with time zone,
        to_date    timestamp with time zone DEFAULT now(
    ))
    RETURNS text
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
AS $BODY$
DECLARE
    result_text TEXT := ''; 
    h_1         TEXT := 'Активных пользователей';   
    h_2         TEXT := 'Всего сообщений';
    chat_name   TEXT;
BEGIN

  -- Получаем статистику для всех чатов во временную таблицу
  CREATE TEMP TABLE statisticsTable AS  
  SELECT full_query."Chat"
       , full_query."Name"
       , full_query."Count"
       , full_query."AccountedMessageCount"
    FROM
    (    
        -- Статитика участников чата
        (SELECT COALESCE(c."ChatFirstName" || COALESCE(' ' || c."ChatLastName", ' '), c."ChatTitle") AS "Chat"
              , COALESCE(u."UserFirstName" || COALESCE(' ' || u."UserLastName", ' '), u."UserName")  AS "Name"
              , sf1."MessageCount" AS "Count"
              , sf2."MessageCount" AS "AccountedMessageCount"
           FROM cas."sfGetGroupStatistics"(rows_limit, from_date, to_date) sf1
      LEFT JOIN rmgr."User" u ON sf1."UserId" = u."UserId"
      LEFT JOIN rmgr."Chat" c ON c."ChatId" = sf1."ChatId"
      LEFT JOIN cas."sfGetGroupStatistics"(rows_limit, (SELECT "Accounting"."StartDate" FROM cas."Accounting" WHERE "Accounting"."StartDate" > from_date LIMIT 1)
                                           , to_date) sf2 ON sf1."UserId" = sf2."UserId" AND sf1."ChatId" = sf2."ChatId"
       ORDER BY sf1."ChatId", sf1."MessageCount" DESC, sf2."MessageCount" DESC 
          LIMIT rows_limit)
       
    UNION
        -- Активных пользователей
         SELECT s."Chat" AS "Chat"
              , h_1      AS "Name"
              , count(*) AS "Count"
              , NULL     AS "AccountedMessageCount"
           FROM  (SELECT COALESCE(c."ChatFirstName" || COALESCE(' ' || c."ChatLastName", ' '), c."ChatTitle")  AS "Chat"
                    FROM rmgr."ReceivedMsg" m
               LEFT JOIN rmgr."Chat" c ON c."ChatId" = m."ChatId"
                   WHERE m."InsertDate" > from_date AND m."InsertDate" < to_date
                     AND m."IsDeleted" = false
                     AND c."ChatTitle" IS NOT NULL -- Отсекаем личные переписки с ботом
                GROUP BY m."UserId", c."ChatId" ) s
       GROUP BY s."Chat"
       
    UNION      
        -- Всего сообщений
         SELECT COALESCE(c."ChatFirstName" || COALESCE(' ' || c."ChatLastName", ' '), c."ChatTitle")  AS "Chat"
              , h_2               AS "Name"
              , count(m."UserId") AS "Count"
              , NULL              AS "AccountedMessageCount"
           FROM rmgr."ReceivedMsg" m
      LEFT JOIN rmgr."Chat" c ON c."ChatId" = m."ChatId"
          WHERE m."InsertDate" > from_date AND m."InsertDate" < to_date
            AND m."IsDeleted" = false
       GROUP BY c."ChatId"
   ) full_query;
   
  -- Для каждого чата формируем сводку
  FOR chat_name IN (select "Chat" from statisticsTable group by "Chat")
  LOOP
      result_text := result_text || '**' || chat_name || E'**\n';
      result_text := result_text || (select "Name" || ' ' || "Count" from statisticsTable where "Chat" = chat_name and "Name" = h_2);
      
      -- Для групповых чатов больше информации
      IF EXISTS (select 1 from statisticsTable where "Chat" = chat_name and "Name" = h_1) 
      THEN
          result_text := result_text || E'\n' || (select "Name" || ' ' || "Count" from statisticsTable where "Chat" = chat_name and "Name" = h_1) || E'\n';
          result_text := result_text || E'---\nСамые активные:\n';
          result_text := result_text || coalesce((select string_agg("Name" || ' ' || "Count" || COALESCE(' (' || "AccountedMessageCount" || '*)', ''), E'\n'
                                                                    order by "Count" desc, coalesce("AccountedMessageCount", 0) desc) 
                                                  from statisticsTable
                                                  where "Chat" = chat_name and "Name" not in (h_1, h_2)), 'exception');
      END IF;
      result_text := result_text || E'\n\n================\n\n';
  END LOOP;
  
  -- Удаляем временную таблицу
  drop table statisticsTable;
  
  RETURN result_text;

END;
$BODY$;

ALTER FUNCTION rmgr."sfCmdGetStatistics"(integer, timestamp with time zone, timestamp with time zone)
    OWNER TO postgres;

COMMENT ON FUNCTION rmgr."sfCmdGetStatistics"(integer, timestamp with time zone, timestamp with time zone)
    IS 'Получение статистики по всем чатам за указанный период времени';