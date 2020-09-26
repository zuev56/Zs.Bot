-- It is need because after migrations seeding we add Chat and User 
-- with id = 1 and id's autoincrement not works (bug)
SELECT nextval(pg_get_serial_sequence('bot.chats', 'chat_id'));
SELECT nextval(pg_get_serial_sequence('bot.users', 'user_id'));