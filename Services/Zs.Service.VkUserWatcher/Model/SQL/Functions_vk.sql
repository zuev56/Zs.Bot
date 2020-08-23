--0. ФУНКЦИЯ, ОПОВЕЩАЮЩАЯ, ЕСЛИ ПОЛЬЗОВАТЕЛЬ НЕ БЫЛ В СЕТИ 24 ЧАСА
--1. ФУНКЦИЯ, ПОЛУЧАЮЩАЯ ВРЕМЯ ОНЛАЙН ДЛЯ ОДНОГО ПОЛЬЗОВАТЕЛЯ ЗА ПЕРИОД
--2. ФУНКЦИЯ, ПОКАЗЫВАЮЩАЯ СТАТИСТИКУ ПО ВСЕМ ПОЛЬЗОВАТЕЛЯМ НА ОСНОВЕ ФУНКЦИИ 1


-- ФУНКЦИЯ, ОПОВЕЩАЮЩАЯ, ЕСЛИ ПОЛЬЗОВАТЕЛЬ НЕ БЫЛ В СЕТИ 24 ЧАСА
CREATE OR REPLACE FUNCTION vk.sf_cmd_get_not_active_users(
    _vkUserIdsStr text) 
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
    WHERE cast(raw_data ->> 'id' AS integer) = any(_vkUserIds);
    RAISE NOTICE '2. _dbUserIds: %', _dbUserIds;
   
    SELECT array_agg(DISTINCT user_id) INTO _activeDbUserIds
    FROM vk.status_log
    WHERE insert_date > now() - interval '24 hours'
      and user_id = any(_dbUserIds)
      and is_online = true;
    RAISE NOTICE '3. _activeDbUserIds: %', _activeDbUserIds;

    SELECT array(SELECT unnest(_dbUserIds) EXCEPT SELECT unnest(_activeDbUserIds)) into _notActiveDbUserIds;
    RAISE NOTICE '4. _notActiveUserIds: %', _notActiveDbUserIds;

    IF (cardinality(_notActiveDbUserIds) > 0)
    THEN
        select 'В течение 24 часов не было активности от следующих пользователей: ' 
            || (select array_to_string(array_agg(first_name || ' ' || last_name), ', ')
                from vk.users
                where user_id = any(_notActiveDbUserIds)) INTO _result;
    END IF;

    RETURN _result;
END;
$BODY$;
ALTER FUNCTION vk.sf_cmd_get_not_active_users(text)
    OWNER TO postgres;



    