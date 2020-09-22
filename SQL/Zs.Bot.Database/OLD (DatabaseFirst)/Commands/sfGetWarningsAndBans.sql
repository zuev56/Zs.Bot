
-- Статистика по предупреждениям и банам

CREATE OR REPLACE FUNCTION cas."sfGetWarningsAndBans"(
    chat_id bigint,
    from_date timestamp with time zone,
    to_date timestamp with time zone = now()
    )
    RETURNS TABLE("UserName" text, "Warings" bigint, "Bans" bigint) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $$
BEGIN
    RETURN QUERY(
        WITH warns AS (
        SELECT b."ChatId", u."UserId", COALESCE((u."UserFirstName" || COALESCE((' ' || u."UserLastName"), ' ')), u."UserName") "User", Count(*) "WarnCount"
        FROM cas."Ban" b
        LEFT JOIN rmgr."User" u on u."UserId" = b."UserId"
        WHERE b."ChatId" = chat_id
          AND b."InsertDate" >= from_date
          AND b."InsertDate" <= to_date
        GROUP BY b."ChatId", "User", u."UserId"
        )
        SELECT w."User" 
              ,w."WarnCount" as "Warnings"
              ,(select count(*) from cas."Ban" where "UserId"= w."UserId" and  "ChatId" = chat_id and "BanFinishDate" is not null) as "Bans"
        FROM warns w
        ORDER BY "Warnings" DESC
    );

END;
$$;

ALTER FUNCTION cas."sfGetWarningsAndBans"(bigint, timestamp with time zone, timestamp with time zone)
    OWNER TO postgres;

COMMENT ON FUNCTION cas."sfGetWarningsAndBans"(bigint, timestamp with time zone, timestamp with time zone)
    IS 'Возвращает статистику по предупреждениям и банам пользователей за указанный период времени';

