--0. ФУНКЦИЯ, ОПОВЕЩАЮЩАЯ, ЕСЛИ ПОЛЬЗОВАТЕЛЬ НЕ БЫЛ В СЕТИ 24 ЧАСА
--1. ФУНКЦИЯ, ПОЛУЧАЮЩАЯ ВРЕМЯ ОНЛАЙН ДЛЯ ОДНОГО ПОЛЬЗОВАТЕЛЯ ЗА ПЕРИОД
--2. ФУНКЦИЯ, ПОКАЗЫВАЮЩАЯ СТАТИСТИКУ ПО ВСЕМ ПОЛЬЗОВАТЕЛЯМ НА ОСНОВЕ ФУНКЦИИ 1



-- ФУНКЦИЯ, ОПОВЕЩАЮЩАЯ, ЕСЛИ ПОЛЬЗОВАТЕЛЬ НЕ БЫЛ В СЕТИ ЗАДАННОЕ КОЛИЧЕСТВО ЧАСОВ
CREATE OR REPLACE FUNCTION vk.sf_cmd_get_not_active_users(
    _vkUserIdsStr text,
    _offlineHours int)
    RETURNS text
    LANGUAGE 'plpgsql'
AS $BODY$ 
DECLARE
    _vkUserIds int[];
    _dbUserIds int[];
    _activeDbUserIds int[];
    _notActiveDbUserIds int[];
    _result text = null;
BEGIN
    _vkUserIds = string_to_array(_vkUserIdsStr, ',');
    RAISE NOTICE '1. _vkUserIds: %', _vkUserIds;

    SELECT array_agg(user_id) INTO _dbUserIds
    FROM vk.users
    WHERE cast(raw_data ->> 'id' AS integer) = ANY(_vkUserIds);
    RAISE NOTICE '2. _dbUserIds: %', _dbUserIds;
   
    SELECT INTO _activeDbUserIds array_agg(DISTINCT user_id)
    FROM vk.activity_log
    WHERE insert_date > now() - (_offlineHours || ' hours')::interval
      AND user_id = ANY(_dbUserIds)
      AND is_online = true;
    RAISE NOTICE '3. _activeDbUserIds: %', _activeDbUserIds;

    SELECT array(SELECT unnest(_dbUserIds) EXCEPT SELECT unnest(_activeDbUserIds)) into _notActiveDbUserIds;
    RAISE NOTICE '4. _notActiveUserIds: %', _notActiveDbUserIds;

    -- Формирование сообщения "<UserName> is not active for <_exactOfflineHours> hours"
    IF (cardinality(_notActiveDbUserIds) > 0)
    THEN
        SELECT string_agg((u.first_name || ' ' || u.last_name || ' is not active for ' ||
            EXTRACT(epoch FROM (now() - (SELECT insert_date FROM vk.activity_log WHERE user_id = u.user_id ORDER BY insert_date DESC LIMIT 1)))::int / 3600
            || ' hours'), chr(10)) INTO _result
        FROM vk.users u
        WHERE u.user_id = ANY(_notActiveDbUserIds);
    END IF;

    RETURN _result;
END;
$BODY$;
ALTER FUNCTION vk.sf_cmd_get_not_active_users(text, integer)
    OWNER TO postgres;

--select vk.sf_cmd_get_not_active_users('51823577, 8790237, 24344351', 24);


-- Будет реализовано в WebAPI проекте
--CREATE OR REPLACE FUNCTION vk.sf_cmd_get_user_activity_minutes(
--    _vkUserId int,
--    _fromDate timestamptz,
--    _toDate timestamptz) 
--    RETURNS int
--    LANGUAGE 'plpgsql'
--AS $BODY$ 
--DECLARE
--    --_vkUserIds int[];
--    _dbUserId int;
--    --_activeDbUserIds int[];
--    --_notActiveDbUserIds int[];
--    --_result text = null;
--BEGIN
--    RAISE NOTICE '1. _vkUserId: %', _vkUserId;
--    
--    SELECT user_id INTO _dbUserId
--    FROM vk.users
--    WHERE cast(raw_data ->> 'id' AS integer) = _vkUserId;
--    RAISE NOTICE '2. _dbUserId: %', _dbUserId;
--
--    SELECT Count(*) 
--    FROM vk.activity_log
--    WHERE user_id = _dbUserId 
--    AND insert_date >= _fromDate 
--    AND insert_date <= _toDate
--    
--    --RETURN -1;
--END;
--$BODY$;
--ALTER FUNCTION vk.sf_cmd_get_user_activity_minutes(int, timestamptz, timestamptz)
--    OWNER TO postgres;
--    
----select vk.sf_cmd_get_user_activity_minutes(8790237, '2020-08-21 21:30:00', '2020-08-25 21:30:00')