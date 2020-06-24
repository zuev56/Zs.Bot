
INSERT INTO bot.options(option_group, option_name, option_value, option_description) VALUES('ChatAdmin', 'ChatUserMessageCountHiHi', '-1', 'Верхняя аварийная уставка');
INSERT INTO bot.options(option_group, option_name, option_value, option_description) VALUES('ChatAdmin', 'ChatUserMessageCountHi',   '-1', 'Верхняя предупредительная уставка');
--INSERT INTO bot.options(option_group, option_name, option_value, option_description) VALUES('ChatAdmin','DefaultChatId',            '',   '»дентификатор чата, с которым бот работает по умолчанию');

--DROP TABLE zl.bans;

CREATE TABLE zl.bans (
    ban_id          serial      NOT NULL PRIMARY KEY,
    user_id         int         NOT NULL REFERENCES bot.users (user_id),
    chat_id         int         NOT NULL REFERENCES bot.chats (chat_id),
    ban_finish_date timestamptz     NULL,
    update_date     timestamptz NOT NULL DEFAULT now(),
    insert_date     timestamptz NOT NULL DEFAULT now()
);
CREATE TRIGGER bans_reset_update_date BEFORE UPDATE
ON zl.bans FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE zl.bans IS 'Информация о банах';




CREATE TABLE zl.accountings (
    accounting_id         serial      NOT NULL PRIMARY KEY,
    accounting_start_date timestamptz NOT NULL DEFAULT now(),   
    update_date           timestamptz NOT NULL DEFAULT now()
);
CREATE TRIGGER accountings_reset_update_date BEFORE UPDATE
ON zl.accountings FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE zl.accountings IS 'Информация о времени начала учёта сообщений каждого отдельного пользователя';




CREATE TABLE zl.notifications (
    notification_id           serial        NOT NULL PRIMARY KEY,
    notification_is_active     bool         NOT NULL DEFAULT TRUE,
    notification_message      varchar(2000) NOT NULL,
    notification_month        int               NULL, -- мес¤ц, если событие ежегодное; null, если событие ежемес¤чное
    notification_day          int           NOT NULL, -- день мес¤ца
    notification_hour         int           NOT NULL, -- врем¤ оповещени¤
    notification_minute       int           NOT NULL, -- врем¤ оповещени¤
    notification_exec_date    timestamptz       NULL, -- время последнего срабатывания
    update_date               timestamptz   NOT NULL DEFAULT now(),
    insert_date               timestamptz   NOT NULL DEFAULT now()
);
CREATE TRIGGER notifications_reset_update_date BEFORE UPDATE
ON zl.notifications FOR EACH ROW EXECUTE PROCEDURE helper.reset_update_date();
COMMENT ON TABLE zl.notifications IS 'Напоминание о событиях';




CREATE TABLE zl.auxiliary_words (
    the_word    varchar(100)  NOT NULL PRIMARY KEY, -- word нельз¤ использовать в постгресе
    insert_date timestamptz   NOT NULL DEFAULT now()
);
COMMENT ON TABLE zl.auxiliary_words IS 'Вспомогательные слова - то, что должно быть отсеяно из статистики';

