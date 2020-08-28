CREATE OR REPLACE VIEW vk.v_status_log
AS
     SELECT (u.first_name::text || ' '::text) || u.last_name::text as user_name,
            u.user_id,
            l.is_online::int as online,
            l.is_online,
            to_char(l.insert_date, 'DD.MM.YYYY HH24:MI:SS') as date,
            l.insert_date
       FROM vk.status_log l
  LEFT JOIN vk.users u ON l.user_id = u.user_id;

