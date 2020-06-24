--DROP USER IF EXISTS zuev56;
--DROP USER IF EXISTS app;
--CREATE USER zuev56 WITH PASSWORD 'xxx';
--CREATE USER app WITH PASSWORD 'app';
\c postgres postgres;
set timezone = 'Europe/Moscow';
DROP DATABASE IF EXISTS "ZsBot";
CREATE DATABASE "ZsBot" WITH ENCODING = 'UTF8';
\connect "ZsBot" postgres;
DROP SCHEMA public;
CREATE SCHEMA bot;
CREATE SCHEMA zl;
CREATE SCHEMA helper;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA helper;


CREATE OR REPLACE FUNCTION helper.reset_update_date()
RETURNS TRIGGER AS $$
BEGIN
   NEW.update_date = now(); 
   RETURN NEW;
END;
$$ language 'plpgsql';

COMMENT ON FUNCTION helper.reset_update_date()
    IS 'Общая триггерная функция обовления поля update_date';
    