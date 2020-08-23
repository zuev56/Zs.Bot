CREATE OR REPLACE VIEW vk.v_status_log
 AS
 SELECT (u.first_name::text || ' '::text) || u.last_name::text,
        l.is_online,
        l.insert_date
   FROM vk.status_log l
   LEFT JOIN vk.users u ON l.user_id = u.user_id;

ALTER TABLE vk.v_status_log
    OWNER TO postgres;