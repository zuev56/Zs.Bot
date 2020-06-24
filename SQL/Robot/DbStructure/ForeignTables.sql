
\connect "ZsBot" postgres;
CREATE SCHEMA rmgr;
CREATE EXTENSION postgres_fdw WITH SCHEMA rmgr;
CREATE SERVER ActiveRmgrDb FOREIGN DATA WRAPPER postgres_fdw OPTIONS (host '192.168.1.12', dbname 'RemoteManagerDb', port '5632');
CREATE USER MAPPING FOR postgres SERVER ActiveRmgrDb OPTIONS(user 'postgres', password 'postgres');


CREATE FOREIGN TABLE rmgr."ReceivedMsg" (
    "ReceivedMsgId"                    bigint NOT NULL,
    "ReceivedMsgMessageId"             integer NOT NULL,
    "UserId"                           integer NOT NULL,
    "ChatId"                           bigint NOT NULL,
    "MessageTypeName"                  character varying(50) COLLATE pg_catalog."default" NOT NULL,
    "ReceivedMsgAuthorSignature"       character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgCaption"               character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgChannelChatCreated"    boolean,
    "ReceivedMsgConnectedWebsite"      character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgDate"                  timestamp with time zone,
    "ReceivedMsgDeleteChatPhoto"       boolean,
    "ReceivedMsgEditDate"              timestamp with time zone,
    "ReceivedMsgForwardDate"           timestamp with time zone,
    "ReceivedMsgForwardFromId"         integer,
    "ReceivedMsgForwardFromChatId"     bigint,
    "ReceivedMsgForwardFromMessageId"  integer,
    "ReceivedMsgForwardSignature"      character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgGroupChatCreated"      boolean,
    "ReceivedMsgIsForwarded"           boolean,
    "ReceivedMsgLeftChatMemberId"      integer,
    "ReceivedMsgLocation"              character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgMediaGroupId"          character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgMigrateFromChatId"     bigint,
    "ReceivedMsgMigrateToChatId"       bigint,
    "ReceivedMsgNewChatTitle"          character varying(500) COLLATE pg_catalog."default",
    "ReceivedMsgPinnedMessageId"       integer,
    "ReceivedMsgReplyToMessageId"      integer,
    "ReceivedMsgSupergroupChatCreated" boolean,
    "ReceivedMsgText"                  character varying(5000) COLLATE pg_catalog."default",
    "InsertDate"                       timestamp with time zone NOT NULL DEFAULT now(),
    "UpdateDate"                       timestamp with time zone NOT NULL DEFAULT now(),
    "IsDeleted"                        boolean DEFAULT false
)
SERVER ActiveRmgrDb OPTIONS(schema_name 'rmgr', table_name 'ReceivedMsg');


CREATE FOREIGN TABLE rmgr."Chat"
(
    "ChatId"                          bigint NOT NULL,
    "ChatTitle"                       character varying(50) COLLATE pg_catalog."default",
    "ChatFirstName"                   character varying(50) COLLATE pg_catalog."default",
    "ChatLastName"                    character varying(50) COLLATE pg_catalog."default",
    "ChatDescription"                 character varying(100) COLLATE pg_catalog."default",
    "ChatType"                        character varying(50) COLLATE pg_catalog."default",
    "ChatInviteLink"                  character varying(10) COLLATE pg_catalog."default",
    "ChatUserName"                    character varying(50) COLLATE pg_catalog."default",
    "ChatAllMembersAreAdministrators" boolean,
    "ChatCanSetStickerSet"            boolean,
    "ChatStickerSetName"              character varying(50) COLLATE pg_catalog."default",
    "PinnedMessageId"                 integer,
    "IsSubscribed"                    boolean NOT NULL,
    "InsertDate"                      timestamp with time zone NOT NULL DEFAULT now(),
    "UpdateDate"                      timestamp with time zone NOT NULL DEFAULT now()
)
SERVER ActiveRmgrDb OPTIONS(schema_name 'rmgr', table_name 'Chat');



CREATE FOREIGN TABLE rmgr."User"
(
    "UserId"         integer NOT NULL,
    "UserName"       character varying(50) COLLATE pg_catalog."default",
    "UserManualName" character varying(50) COLLATE pg_catalog."default",
    "UserFirstName"  character varying(50) COLLATE pg_catalog."default",
    "UserLastName"   character varying(50) COLLATE pg_catalog."default",
    "RoleName"       character varying(50) COLLATE pg_catalog."default" NOT NULL,
    "IsBot"          boolean,
    "UpdateDate"     timestamp with time zone NOT NULL DEFAULT now(),
    "InsertDate"     timestamp with time zone NOT NULL DEFAULT now()
)
SERVER ActiveRmgrDb OPTIONS(schema_name 'rmgr', table_name 'User');


CREATE FOREIGN TABLE rmgr."UnInterestedWords"
(
    wd character varying(100) COLLATE pg_catalog."default" NOT NULL,
    insert_date timestamp with time zone NOT NULL DEFAULT now()
)
SERVER ActiveRmgrDb OPTIONS(schema_name 'cas', table_name 'UnInterestedWords');

insert into zl.auxiliary_words (the_word, insert_date)
SELECT wd, insert_date from rmgr."UnInterestedWords";
