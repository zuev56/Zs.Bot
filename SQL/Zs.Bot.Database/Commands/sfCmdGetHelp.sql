-- FUNCTION: rmgr."sfCmdGetHelp"(varchar(50))

-- DROP FUNCTION rmgr."sfCmdGetHelp"(varchar(50))

-- ��������� ������� �� ��������, ��������� ��� ������ ����
CREATE OR REPLACE FUNCTION rmgr."sfCmdGetHelp"(
	role_name varchar(50)
	)
    RETURNS text
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
AS $BODY$
DECLARE
    result_text TEXT := ''; 
BEGIN
    -- ���� ���, ��-��������
    result_text := (select string_agg(("CommandName" || ' - ' || "CommandDesc"), E'\n') 
					from rmgr."Command"
				    where LOWER("RoleList") like '%' || LOWER(role_name) || '%');
					
  RETURN result_text;

END;
$BODY$;

ALTER FUNCTION rmgr."sfCmdGetHelp"(varchar(50))
    OWNER TO postgres;

COMMENT ON FUNCTION rmgr."sfCmdGetHelp"(varchar(50))
    IS '��������� ������� �� ��������, ��������� ��� ������ ����';