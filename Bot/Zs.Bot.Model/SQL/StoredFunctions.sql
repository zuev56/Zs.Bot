
-- Get permissions for a specific user role
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




-- Get list of awailable functions for a specific user role
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




-- Get statistics of specific chat
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




