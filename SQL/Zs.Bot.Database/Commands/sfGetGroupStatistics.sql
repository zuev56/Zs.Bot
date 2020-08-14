-- FUNCTION: cas."sfGetGroupStatistics"(integer, timestamp with time zone, timestamp with time zone)

-- DROP FUNCTION cas."sfGetGroupStatistics"(integer, timestamp with time zone, timestamp with time zone);

-- Видимо, получение статистики по активности пользователей в чатах
CREATE OR REPLACE FUNCTION rmgr."sfGetGroupStatistics"(
	rows_limit integer,
	from_date timestamp with time zone,
	to_date timestamp with time zone DEFAULT now(
	))
    RETURNS TABLE("ChatId" bigint, "UserId" integer, "MessageCount" bigint) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
BEGIN
	RETURN QUERY(
		SELECT m."ChatId"
		     , u."UserId"
             , count(m.*) 
          FROM rmgr."ReceivedMsg" m
          JOIN rmgr."User" u ON u."UserId" = m."UserId"
         WHERE --m."InsertDate" > now()::Date - INTERVAL '1 DAY' AND m."InsertDate" < now()::Date
               m."InsertDate" >= from_date AND m."InsertDate" <= to_date
           AND m."IsDeleted" = false
         --AND NOT u."RoleName" NOT IN ('Owner', 'Administrator')
      GROUP BY m."ChatId", u."UserId"
      ORDER BY count(m.*) DESC
      LIMIT rows_limit
	);

END;
$BODY$;

ALTER FUNCTION cas."sfGetGroupStatistics"(integer, timestamp with time zone, timestamp with time zone)
    OWNER TO postgres;

COMMENT ON FUNCTION cas."sfGetGroupStatistics"(integer, timestamp with time zone, timestamp with time zone)
    IS 'Возвращает заданное количество строк в заданном временном диапазоне - ИмяПользователя, КоличествоСообщений';
