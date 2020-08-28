INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
VALUES('/GetUserStatistics', 'SELECT zl.sf_cmd_get_full_statistics({0}, {1}, {2})', '15; now()::Date; now()', 'Получение статистики по активности участников всех чатов за определённый период', 'adminCmdGroup');
--INSERT INTO bot.commands(command_name, command_script, command_default_args, command_desc, command_group) 
--VALUES('/SetMessageLimit', 'SELECT bot."sfCmdSetMessageLimit"({0}, {1})', '0; 0', 'Установка лимита сообщений для пользователей', 'moderatorCmdGroup');

CREATE TABLE zl.bans (
    ban_id             serial      NOT NULL PRIMARY KEY,
    user_id            int         NOT NULL REFERENCES bot.users (user_id),
    chat_id            int         NOT NULL REFERENCES bot.chats (chat_id),
    warning_message_id int             NULL REFERENCES bot.messages (message_id),
    ban_finish_date    timestamptz     NULL,
    update_date        timestamptz NOT NULL DEFAULT now(),
    insert_date        timestamptz NOT NULL DEFAULT now()
);
CREATE TRIGGER bans_reset_update_date BEFORE UPDATE
ON zl.bans FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE zl.bans IS 'Информация о банах';



CREATE TABLE zl.auxiliary_words (
    the_word    varchar(100)  NOT NULL PRIMARY KEY, -- word нельз¤ использовать в постгресе
    insert_date timestamptz   NOT NULL DEFAULT now()
);
COMMENT ON TABLE zl.auxiliary_words IS 'Вспомогательные слова - то, что должно быть отсеяно из статистики';


--CREATE TABLE zl.accountings (
--    accounting_id         serial      NOT NULL PRIMARY KEY,
--    accounting_start_date timestamptz NOT NULL DEFAULT now(),   
--    update_date           timestamptz NOT NULL DEFAULT now()
--);
--CREATE TRIGGER accountings_reset_update_date BEFORE UPDATE
--ON zl.accountings FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
--COMMENT ON TABLE zl.accountings IS 'Информация о времени начала учёта сообщений каждого отдельного пользователя';




--CREATE TABLE zl.notifications (
--    notification_id           serial        NOT NULL PRIMARY KEY,
--    notification_is_active     bool         NOT NULL DEFAULT TRUE,
--    notification_message      varchar(2000) NOT NULL,
--    notification_month        int               NULL, -- мес¤ц, если событие ежегодное; null, если событие ежемес¤чное
--    notification_day          int           NOT NULL, -- день мес¤ца
--    notification_hour         int           NOT NULL, -- врем¤ оповещени¤
--    notification_minute       int           NOT NULL, -- врем¤ оповещени¤
--    notification_exec_date    timestamptz       NULL, -- время последнего срабатывания
--    update_date               timestamptz   NOT NULL DEFAULT now(),
--    insert_date               timestamptz   NOT NULL DEFAULT now()
--);
--CREATE TRIGGER notifications_reset_update_date BEFORE UPDATE
--ON zl.notifications FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
--COMMENT ON TABLE zl.notifications IS 'Напоминание о событиях';



