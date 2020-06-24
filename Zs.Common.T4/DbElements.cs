using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Zs.Common.T4
{
    public enum ConstraintType
    {
        Unknown = -1,
        PrimaryKey,
        ForeignKey
    }

    public abstract class DbElement
    {
        public string Name { get; }
        public string Description { get; set; }

        protected DbElement(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {Name}";
        }
    }

    public abstract class DbElement<T> : DbElement, IEnumerable<T>
    {
        protected readonly List<T> _childs = new List<T>();

        public int Count => _childs.Count;

        public virtual T this[int index]
        {
            get => _childs[index];
            set => _childs[index] = value;
        }

        protected DbElement(string name)
            : base(name)
        {
        }


        public IEnumerator<T> GetEnumerator() => _childs.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _childs.GetEnumerator();

        public void Add(T child)
        {
            _childs.Add(child);
        }

        public override string ToString()
        {
            return $"{base.ToString()}[{_childs.Count}]";
        }
    }

    public sealed class DbInfo : DbElement<DbSchema>
    {
        public string ConnectionString { get; set; }

        public DbInfo(string name, string connectingString)
            : base(name)
        {
            ConnectionString = connectingString;
        }
    }

    public sealed class DbSchema : DbElement<DbTable>
    {
        public DbSchema(string name) 
            : base(name) 
        {
        }
    }

    public sealed class DbTable : DbElement<DbColumn>
    {
        public DbTable(string name)
            : base(name)
        { 
        }
    }

    public sealed class DbColumn : DbElement
    {
        public string SqlDataType { get; }
        public Type DataType { get; }
        public bool IsNullable { get; }
        public int Position { get; }
        public string ConstraintName { get; }
        public ConstraintType ConstraintType { get; }


        public DbColumn(string name, string sqlDataType, bool isNullable = true, int position = -1, string constraintName = null, string constraintType = null)
            : base(name)
        {   
            SqlDataType    = sqlDataType;
            DataType       = FromSqlDataType(sqlDataType);
            IsNullable     = isNullable;
            Position       = position;
            ConstraintName = constraintName;
            ConstraintType = FromSqlConstraintType(constraintType);
        }

        private Type FromSqlDataType(string sqlDataType)
        {            
            switch (sqlDataType.ToLowerInvariant())
            {
                case "integer":                  return typeof(int);
                case "boolean":                  return typeof(bool);
                case "bigint":                   return typeof(long);
                case "double precision":         return typeof(double);
                case "name":
                case "json":
                case "character varying":
                case "text":                     return typeof(string);
                case "date":
                case "timestamp with time zone": return typeof(DateTime);
                default: throw new InvalidCastException($"Не удалось преобразовать SQL тип '{sqlDataType}'");
            }
        }
        private ConstraintType FromSqlConstraintType(string sqlConstraintType)
        {
            switch (sqlConstraintType)
            {
                case "PRIMARY KEY": return ConstraintType.PrimaryKey;
                case "FOREIGN KEY": return ConstraintType.ForeignKey;
                default: return ConstraintType.Unknown;
            }
        }

    }

    internal sealed class Comment
    {
        public string DbElementType      { get; }
        public string DbElementName      { get; }
        public string DbShortElementName { get; }
        public string CommentText        { get; }


        public Comment(string dbElementType, string dbElementName, string commentText)
        {
            DbElementType = dbElementType;
            DbElementName = dbElementName;
            CommentText = commentText;

            DbShortElementName = dbElementName.Contains('.')
                ? dbElementName.Substring(dbElementName.LastIndexOf('.') + 1)
                : dbElementName;
        }

        internal static void SetComments(DbInfo dataBase, List<Comment> comments)
        {
            foreach (var schema in dataBase)
            {
                schema.Description = comments.Find(c => c.DbElementType == "SCHEMA" 
                                                     && c.DbElementName == schema.Name)
                                             ?.CommentText;
                foreach (var table in schema)
                {
                    table.Description = comments.Find(c => c.DbElementType == "TABLE" 
                                                        && c.DbShortElementName == table.Name)
                                                ?.CommentText;
                    foreach (var column in table)
                    {
                        var getTablePattern = @"(?<=\.).*?(?=\.)";
                        column.Description = comments.Find(c => c.DbElementType == "COLUMN"
                                                        && table.Name == Regex.Match(c.DbElementName, getTablePattern).Value
                                                        && c.DbShortElementName == column.Name)
                                                     ?.CommentText;
                    }
                }
            }
        }

#if DEBUG
        public override string ToString()
        {
            return $"{DbElementType} : {DbElementName} : {CommentText}";
        }
#endif
    }

    internal sealed class TableRow
    {
        private Dictionary<string, string> _fieldValues { get; set; }
        public string this[string columnHeader]
        {
            get => _fieldValues[columnHeader];
            set => _fieldValues[columnHeader] = value;
        }

        //public TableRow(string)
        //{
        //
        //}
    }
}
