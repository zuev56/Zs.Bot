

CREATE OR REPLACE FUNCTION zl.sf_get_most_popular_words(
    _chat_id integer,
    from_date timestamp with time zone,
    to_date timestamp with time zone DEFAULT now(),
    min_word_length integer DEFAULT 2)
    RETURNS TABLE(word character varying, count bigint) 
    LANGUAGE 'plpgsql'
    ROWS 1000
AS $BODY$
DECLARE
   msg_text text;
   words text[];
BEGIN
   msg_text = (select string_agg(m.raw_data ->> 'text', ' ')
                 from bot.messages m
                where m.insert_date > from_date and m.insert_date < to_date
                  and m.chat_id = _chat_id);
                              
   msg_text = REPLACE(msg_text, chr(10), ' ' );  
   msg_text = REPLACE(msg_text, '\\', ' ' );
   msg_text = REPLACE(msg_text, '\n', ' ' );
   msg_text = REPLACE(msg_text, '/', ' ' );
   msg_text = REPLACE(msg_text, ',', ' ' );
   msg_text = REPLACE(msg_text, '.', ' ' );
   msg_text = REPLACE(msg_text, '(', ' ' );
   msg_text = REPLACE(msg_text, ')', ' ' );
   msg_text = REPLACE(msg_text, '[', ' ' );
   msg_text = REPLACE(msg_text, ']', ' ' );
   msg_text = REPLACE(msg_text, '{', ' ' );
   msg_text = REPLACE(msg_text, '}', ' ' );
   msg_text = REPLACE(msg_text, '?', ' ' );
   msg_text = REPLACE(msg_text, '!', ' ' );
   msg_text = REPLACE(msg_text, '"', ' ' );
   msg_text = REPLACE(msg_text, '«', ' ' );
   msg_text = REPLACE(msg_text, '»', ' ' );
   msg_text = REPLACE(msg_text, '  ', ' ' );
   msg_text = REPLACE(msg_text, '  ', ' ' );
   msg_text = REPLACE(msg_text, '  ', ' ' ); 
   msg_text = REPLACE(msg_text, ' - ', ' ' );
   msg_text = REPLACE(msg_text, '"
"', ' ' );
   
   words = string_to_array(LOWER(msg_text), ' ');

    RETURN QUERY(
        SELECT REPLACE(REPLACE(w::varchar(100), '(', '' ), ')', '')::varchar(100) as word, count(*) as word_count
        FROM (select unnest(words)) as w
        where Length(w::varchar(100)) >= min_word_length + 2
          and w::varchar(100) not in (SELECT ('(' || the_word || ')') FROM zl.auxiliary_words)
        group by w
        having count(*) > 1
        order by word_count desc
    );
END;
$BODY$;
ALTER FUNCTION zl.sf_get_most_popular_words(integer, timestamp with time zone, timestamp with time zone, integer)
    OWNER TO postgres;

CREATE OR REPLACE FUNCTION zl.sf_get_most_popular_words(
    msg_text text,
    min_word_length integer DEFAULT 2)
    RETURNS TABLE(word text) 
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
DECLARE
   words text[];
BEGIN
   msg_text = REPLACE(msg_text, chr(10), ' ' );  
   msg_text = REPLACE(msg_text, '\\', ' ' );
   msg_text = REPLACE(msg_text, '\n', ' ' );
   msg_text = REPLACE(msg_text, '/', ' ' );
   msg_text = REPLACE(msg_text, ',', ' ' );
   msg_text = REPLACE(msg_text, '.', ' ' );
   msg_text = REPLACE(msg_text, '(', ' ' );
   msg_text = REPLACE(msg_text, ')', ' ' );
   msg_text = REPLACE(msg_text, '[', ' ' );
   msg_text = REPLACE(msg_text, ']', ' ' );
   msg_text = REPLACE(msg_text, '{', ' ' );
   msg_text = REPLACE(msg_text, '}', ' ' );
   msg_text = REPLACE(msg_text, '?', ' ' );
   msg_text = REPLACE(msg_text, '!', ' ' );
   msg_text = REPLACE(msg_text, '"', ' ' );
   msg_text = REPLACE(msg_text, '«', ' ' );
   msg_text = REPLACE(msg_text, '»', ' ' );
   msg_text = REPLACE(msg_text, '  ', ' ' );
   msg_text = REPLACE(msg_text, '  ', ' ' );
   msg_text = REPLACE(msg_text, '  ', ' ' ); 
   msg_text = REPLACE(msg_text, ' - ', ' ' );
   
   words = string_to_array(LOWER(msg_text), ' ');

    RETURN QUERY(
        SELECT REPLACE(REPLACE(w::varchar(100), '(', '' ), ')', '')::varchar(100) || ' (' || count(*) || ')' as word
        FROM (select unnest(words)) as w
        where Length(w::varchar(100)) >= min_word_length + 2
          and w::varchar(100) not in (SELECT ('(' || the_word || ')') FROM zl.auxiliary_words)
        group by w
        having count(*) > 1
        order by count(*) desc
    );
END;
$BODY$;
ALTER FUNCTION zl.sf_get_most_popular_words(text, integer)
    OWNER TO postgres;

