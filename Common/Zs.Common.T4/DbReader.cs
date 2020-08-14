﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Npgsql;

namespace Zs.Common.T4
{
    public static class DbReader
    {
        public static string GetVersion(string connectionString)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    var query = "SELECT version()";

                    using (var command = new NpgsqlCommand(query, connection))
                        return command.ExecuteScalar().ToString();
                }
            }
            catch (Exception ex)
            {
                T4Logger.TraceException(ex);
                throw;
            }
        }

        public static DbInfo GetDbInfo(string connectionString, params string[] schemas)
        {
            try
            {
                DbInfo dataBase;

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    dataBase = new DbInfo(connection.Database, connectionString);

                    ReadDbStructure(connection, dataBase, schemas);
                    ReadDbComments(connection, dataBase, schemas);
                }

                return dataBase;
            }
            catch (Exception ex)
            {
                T4Logger.TraceException(ex);
                throw;
            }
        }

        internal static IEnumerable<DataRow> GetTableRows(string connectionString, string tableName, int limit = 1000)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var query = $"select * from {tableName}";

                using (var command = new NpgsqlCommand(query, connection))
                using (var dataAdapter = new NpgsqlDataAdapter(command))
                {
                    var table = new DataTable();
                    dataAdapter.Fill(0, limit, table);
                    return table.Rows.Cast<DataRow>();
                }
            }
        }

        private static void ReadDbStructure(NpgsqlConnection connection, DbInfo dataBase, params string[] schemas)
        {
            string schemasStr = "";
            if (schemas?.Length > 0)
            {
                schemas.ToList().ForEach(s => schemasStr += $"'{s}',");
                schemasStr = schemasStr.TrimEnd(',');
            }

            var structureQuery = "select c.table_schema, c.table_name, c.column_name, c.ordinal_position, c.is_nullable, c.data_type\n"
                               + "	  , kc.constraint_name, tc.constraint_type, c.character_maximum_length\n"
                               + "from information_schema.columns c\n"
                               + "left outer join information_schema.key_column_usage kc on kc.table_schema = c.table_schema\n"
                               + "                                                      and kc.table_name   = c.table_name\n"
                               + "                                                      and kc.column_name  = c.column_name\n"
                               + "left outer join information_schema.table_constraints tc on tc.constraint_schema = kc.constraint_schema\n"
                               + "                                                       and tc.table_name        = kc.table_name\n"
                               + "												        and tc.constraint_name   = kc.constraint_name\n"
                               + "where c.table_schema not in ('information_schema', 'pg_catalog')\n"
                               + (schemasStr.Length > 0
                                  ? $"  and c.table_schema in ({schemasStr})\n" 
                                  : "")
                               + "order by c.table_schema, c.table_name, c.ordinal_position";

            using (var command = new NpgsqlCommand(structureQuery, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var schema         = reader.GetString(0);
                    var table          = reader.GetString(1);
                    var column         = reader.GetString(2).Replace('-', '_').Replace('.', '_').Replace(' ', '_');
                    var position       = reader.GetInt32(3);
                    var isNullable     = reader.GetString(4).ToUpperInvariant() == "YES" ? true : false;
                    var dataType       = reader.GetString(5);
                    var constraintName = !reader.IsDBNull(6) ? reader.GetString(6) : null;
                    var constraintType = !reader.IsDBNull(7) ? reader.GetString(7) : null;
                    var stringLength   = !reader.IsDBNull(8) ? (int?)reader.GetInt32(8) : null;

                    DbSchema dbSchema;
                    if (dataBase.Any(s => s.Name == schema))
                        dbSchema = dataBase.First(s => s.Name == schema);
                    else
                    {
                        dbSchema = new DbSchema(schema);
                        dataBase.Add(dbSchema);
                    }

                    DbTable dbTable;
                    if (dbSchema.Any(t => t.Name == table))
                        dbTable = dbSchema.First(t => t.Name == table);
                    else
                    {
                        dbTable = new DbTable(table);
                        dbSchema.Add(dbTable);
                    }

                    var dbColumn = new DbColumn(
                        column,
                        dataType,
                        isNullable,
                        position,
                        constraintName,
                        constraintType,
                        stringLength);
      
                    dbTable.Add(dbColumn);
                }
            }
        }

        private static void ReadDbComments(NpgsqlConnection connection, DbInfo dataBase, params string[] schemas)
        {
            string schemasStr = "";
            if (schemas?.Length > 0)
            {
                schemas.ToList().ForEach(s => schemasStr += $"'{s}',");
                schemasStr = schemasStr.TrimEnd(',');
            }

            var commentsQuery = "select 'SCHEMA' as type, nspname as name, obj_description(oid) as description\n"
                              + "from pg_catalog.pg_namespace\n"
                              + "where nspname not in ('pg_toast', 'pg_temp_1', 'pg_toast_temp_1', 'pg_catalog', 'information_schema')\n"
                              + "union all\n"
                              + "select (case t.table_type when 'BASE TABLE' then 'TABLE' else 'VIEW' end)  as type\n"
                              + "     , (t.table_schema || '.' || t.table_name) as name\n"
                              + "     , obj_description((t.table_schema || '.' || t.table_name)::regclass::oid) as description\n"
                              + "from information_schema.tables t\n"
                              + "where t.table_type in ('BASE TABLE', 'VIEW')\n"
                              + "and t.table_schema not in ('pg_catalog', 'information_schema')\n"
                              + "union all\n"
                              + "select 'COLUMN' as type\n"
                              + "     , (table_schema || '.' || table_name || '.' || column_name) as name\n"
                              + "     , col_description((table_schema || '.' || table_name)::regclass, ordinal_position) as description\n"
                              + "from information_schema.columns\n"
                              + "where table_schema not in ('pg_catalog', 'information_schema')"
                              + (schemasStr.Length > 0
                                  ? $"  and table_schema in ({schemasStr});"
                                  : ";");

            using (var command = new NpgsqlCommand(commentsQuery, connection))
            using (var reader = command.ExecuteReader())
            {
                var comments = new List<Comment>(100);
                while (reader.Read())
                {
                    var type    = reader.GetString(0);
                    var name    = reader.GetString(1);
                    var comment = !reader.IsDBNull(2) ? reader.GetString(2) : null;

                    comments.Add(new Comment(type, name, comment));
                }

                Comment.SetComments(dataBase, comments);
            }
        }


    }
}