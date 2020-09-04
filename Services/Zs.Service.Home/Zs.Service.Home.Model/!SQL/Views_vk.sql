--DROP VIEW vk.v_activity_log;

CREATE OR REPLACE VIEW vk.v_activity_log
AS
     SELECT l.activity_log_id,
            (u.first_name::text || ' '::text) || u.last_name::text as user_name,
            u.user_id,
            l.is_online::int as online,
            l.is_online,
            l.is_online_mobile::int as mobile,
            l.is_online_mobile,
            to_char(l.insert_date, 'DD.MM.YYYY HH24:MI:SS') as date,
            to_timestamp(l.last_seen) as last_seen,
            l.insert_date
       FROM vk.activity_log l
  LEFT JOIN vk.users u ON l.user_id = u.user_id;

  GRANT ALL ON vk.v_activity_log TO app;


