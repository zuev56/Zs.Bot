
CREATE TABLE vk.users (
    user_id      serial       NOT NULL PRIMARY KEY,
    first_name   varchar(50)      NULL,
    last_name    varchar(50)      NULL,
    raw_data     json         NOT NULL,
    update_date  timestamptz  NOT NULL DEFAULT now(),
    insert_date  timestamptz  NOT NULL DEFAULT now()
);
CREATE TRIGGER users_reset_update_date BEFORE UPDATE
ON vk.users FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE vk.users IS 'Vk users';

CREATE TABLE vk.status_log (
    status_log_id  serial       NOT NULL PRIMARY KEY,
    user_id        int          NOT NULL,
    is_online      bool             NULL, -- 0:false, 1:true, null:unknown
    insert_date    timestamptz  NOT NULL DEFAULT now()
);
CREATE TRIGGER users_reset_update_date BEFORE UPDATE
ON vk.status_log FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE vk.status_log IS 'Vk users status log';



