using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TextTemplating;
using Newtonsoft.Json.Linq;

namespace Zs.Common.T4
{
    public static class ModelGenerator
    {
        /// <summary> Генерация классов - модели таблиц и вьюх базы данных </summary>
        /// <param name="tt"></param>
        /// <param name="dataBase"></param>
        /// <param name="usings"></param>
        /// <param name="namespase"></param>
        /// <param name="modifier"></param>
        /// <param name="namePrefix"></param>
        /// <param name="useAutoInterface">Если TRUE, то класс будет реализовывать интерфейс I{ClassName}. Сам интерфейс с аналогичным набором полей тоже будет создан</param>
        /// <param name="pluralToSingularMap"></param>
        public static void GenerateClasses(
            TextTransformation tt,
            DbInfo dataBase,
            string modifier = null,
            string namePrefix = null,
            bool useAutoInterface = false,
            Dictionary<string, string> pluralToSingularMap = null
            )
        {
            if (tt == null)
                throw new ArgumentNullException(nameof(tt));
            
            if (useAutoInterface)
            {
                foreach (var schemaInfo in dataBase)
                {
                    if (!string.IsNullOrWhiteSpace(schemaInfo.Description))
                        tt.WriteLine($"// {schemaInfo.Description}");
                    tt.WriteLine($"#region Interfaces for '{schemaInfo.Name}' schema");

                    foreach (var tableInfo in schemaInfo)
                    {
                        string interfaceName = GetClassName("I", pluralToSingularMap, tableInfo);

                        GenerateInterface(tt, modifier, interfaceName, tableInfo);

                        tt.WriteLine("");
                    }
                    tt.WriteLine("#endregion");
                }
            }

            foreach (var schemaInfo in dataBase)
            {
                if (!string.IsNullOrWhiteSpace(schemaInfo.Description))
                    tt.WriteLine($"// {schemaInfo.Description}");
                tt.WriteLine($"#region Classes for '{schemaInfo.Name}' schema");

                foreach (var tableInfo in schemaInfo)
                {
                    var interfaceName = GetClassName("I", pluralToSingularMap, tableInfo);
                    var className = GetClassName(namePrefix, pluralToSingularMap, tableInfo);
                    
                    var inheritedInterfaces = useAutoInterface ? new[] { interfaceName } : null;

                    GenerateDbClass(tt, modifier, className, interfaceName,
                        tableInfo, schemaInfo.Name, inheritedInterfaces);

                    tt.WriteLine("");
                }             
                tt.WriteLine("#endregion");
            }
        }
        
        private static void GenerateDbClass(
            TextTransformation tt, 
            string modifier, 
            string className,
            string interfaceName,
            DbTable table, 
            string schemaName,
            string[] inheritedInterfaces)
        {
            try
            {
                string inherit = inheritedInterfaces?.Length > 0 
                               ? $" : {string.Join(", ", inheritedInterfaces)}"
                               : "";

                tt.WriteLine("");
                if (!string.IsNullOrWhiteSpace(table.Description))
                    tt.WriteLine($"/// <summary> {table.Description} </summary>");
                tt.WriteLine($"[Table(\"{table.Name}\", Schema = \"{schemaName}\")]");
                tt.WriteLine($"{modifier} partial class {className}{inherit}");
                tt.WriteLine("{");
                tt.PushIndent("    ");
                {
                    WriteProperties(tt, table);
                    tt.WriteLine("");
                    WriteDeepCopyMethod(tt, className, interfaceName, table);
                }
                tt.PopIndent();
                tt.WriteLine("}");
            }
            catch (Exception ex)
            {
                T4Logger.TraceException(ex);
            }
        }

        private static void WriteProperties(TextTransformation tt, DbTable table)
        {
            foreach (var column in table)
            {
                if (!string.IsNullOrWhiteSpace(column.Description))
                    tt.WriteLine($"/// <summary> {column.Description} </summary>");

                if (column.ConstraintType == ConstraintType.PrimaryKey)
                    tt.WriteLine("[Key]");

                if (column.StringLength != null)
                    tt.WriteLine($"[StringLength({column.StringLength})]");

                if (!column.IsNullable)
                    tt.WriteLine($"[Required(ErrorMessage = \"Property '{UnderscoreToPascalCase(column.Name)}' is required\")]");

                var columnTypeName = column.SqlDataType.Equals("character varying", StringComparison.InvariantCultureIgnoreCase)
                                          && column.StringLength != null
                            ? $"{column.SqlDataType}({column.StringLength})"
                            : column.SqlDataType;

                tt.WriteLine($"[Column(\"{column.Name}\", TypeName = \"{columnTypeName}\")]");

                string nullableMarker = "";
                if (column.DataType.IsValueType && column.IsNullable)
                    nullableMarker = "?";

                tt.WriteLine($"public {column.DataType.Name}{nullableMarker} {UnderscoreToPascalCase(column.Name)} {{ get; set; }}");
                tt.WriteLine("");
            }
        }

        private static void WriteDeepCopyMethod(TextTransformation tt, string className, string interfaceName, DbTable table)
        {
            tt.WriteLine($"public {interfaceName} DeepCopy()");
            tt.WriteLine("{");
            tt.PushIndent("    ");
            {
                tt.WriteLine($"return new {className}");
                tt.WriteLine("{");
                tt.PushIndent("    ");
                {
                    foreach (var column in table)
                    {
                        tt.WriteLine($"{UnderscoreToPascalCase(column.Name)} "
                            + $"= this.{UnderscoreToPascalCase(column.Name)},");
                    }
                }
                tt.PopIndent();
                tt.WriteLine("};");
            }
            tt.PopIndent();
            tt.WriteLine("}");
        }

        private static void GenerateInterface(
            TextTransformation tt,
            string modifier,
            string interfaceName,
            DbTable table)
        {
            try
            {
                tt.WriteLine(""); 
                if (!string.IsNullOrWhiteSpace(table.Description))
                    tt.WriteLine($"/// <summary> {table.Description} </summary>");
                tt.WriteLine($"{modifier} interface {interfaceName}");
                tt.WriteLine("{");
                tt.PushIndent("    ");
                {
                    foreach (var column in table)
                    {
                        if (!string.IsNullOrWhiteSpace(column.Description))
                            tt.WriteLine($"/// <summary> {column.Description} </summary>");

                        string nullableMarker = "";
                        if (column.DataType.IsValueType && column.IsNullable)
                            nullableMarker = "?";

                        tt.WriteLine($"public {column.DataType.Name}{nullableMarker} {UnderscoreToPascalCase(column.Name)} {{ get; set; }}");
                        tt.WriteLine("");
                    }

                    tt.WriteLine($"{interfaceName} DeepCopy();");
                }
                tt.PopIndent();
                tt.WriteLine("}");
            }
            catch (Exception ex)
            {
                T4Logger.TraceException(ex);
            }
        }

        public static void GenerateDbContext(
            TextTransformation tt,
            DbInfo dataBase,
            string modifier,
            string entityNamePrefix,
            string dbContextClassName = default,
            Dictionary<string, string> pluralToSingularMap = null)
        {
            try
            {
                var contextClassName = dbContextClassName ?? $"{dataBase.Name}DbContext";
                tt.WriteLine("");
                tt.WriteLine($"{modifier} partial class {contextClassName} : DbContext");
                tt.WriteLine("{");
                tt.PushIndent("    ");
                {
                    foreach (var schema in dataBase)
                        foreach (var table in schema)
                        {
                            var entityName = GetClassName(entityNamePrefix, pluralToSingularMap, table);
                            var propertyName = UnderscoreToPascalCase(table.Name);
                            tt.WriteLine($"public DbSet<{entityName}> {propertyName} {{ get; set; }}");
                        }
                }
                tt.PopIndent();
                tt.WriteLine("}");
                tt.WriteLine("");
            }
            catch (Exception ex)
            {
                T4Logger.TraceException(ex);
            }
        }

        public static void GenerateEnums(
            TextTransformation tt,
            DbInfo dataBase,
            string referenceTablesInfoPath
            )
        {
            // Для каждой таблицы из referenceTables
            // считываем все значения и создаём Enum с ними.
            // Значения пишутся в Enum в PascalCase без пробелов
            if (!File.Exists(referenceTablesInfoPath))
                throw new FileNotFoundException("Не удалось найти файл", referenceTablesInfoPath);
            var fileContent = File.ReadAllText(referenceTablesInfoPath);

            var refetenceTables = new List<ReferenceTableInfo>();
            foreach (var jItem in JArray.Parse(fileContent))
                refetenceTables.Add(jItem.ToObject<ReferenceTableInfo>());

            foreach (var table in refetenceTables)
            {
                var rows = DbReader.GetTableRows(dataBase.ConnectionString, $"{table.Schema}.{table.Table}");

                tt.WriteLine("");
                tt.WriteLine($"public enum {UnderscoreToPascalCase(table.Table)}");
                tt.WriteLine("{");
                tt.PushIndent("    ");
                {
                    foreach (var row in rows)
                    {
                        var comment = row[table.ValueColumn].ToString();
                        var value = row[table.KeyColumn].ToString();
                        
                        tt.WriteLine($"/// <summary> {comment} </summary>");
                        tt.WriteLine($"{value},");
                    }
                }
                tt.PopIndent();
                tt.WriteLine("}");

                //tt.WriteLine($"{table.Schema} - {table.Table} - {table.KeyColumn} - {table.ValueColumn}");
            }
        }

        internal static string UnderscoreToPascalCase(string value)
        {
            var parts = value.ToLowerInvariant().Split(' ', '_').ToList();
            parts.RemoveAll(p => p.Length == 0);


            for (int i = 0; i < parts.Count; i++)
                parts[i] = char.ToUpperInvariant(parts[i][0]) + (parts[i].Length > 1 ? parts[i].Substring(1) : "");

            return string.Join("", parts);
        }
        
        public static Dictionary<string, string> GetTableNameToClassNameMap(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Не удалось найти файл", filePath);

            var fileLines = File.ReadAllLines(filePath);
            var map = new Dictionary<string, string>(fileLines.Length);

            foreach (var rawLine in fileLines)
            {
                var line = rawLine.Trim();

                if (line == "" || line.StartsWith("#"))
                    continue;

                var words = line.Split(':');

                if (words?.Length < 2)
                    throw new FormatException($"Неверный формат строки файла \"{line}\". Необходимо привести строку к следующему виду: \"ИмяТаблицы:ИмяКласса\"");

                for (int i = 0; i < words.Length; i++)
                {
                    words[i] = words[i].Trim();

                    if (words[i].Length == 0)
                        throw new FormatException($"Неверный формат строки файла \"{line}\". Слева и справа от знака \":\" должно быть непустое значение");
                }

                map.Add(words[0], words[1]);
            }

            return map;
        }
    
        public static string GetConfigurationValue(string configFilePath, string parameterName)
        {
            var configuration =  new ConfigurationBuilder()
                      .AddJsonFile(configFilePath, true, false)
                      .Build();
            return configuration[parameterName];
        }

        private static string GetClassName(string namePrefix, Dictionary<string, string> pluralToSingularMap, DbTable tableInfo)
        {
            return pluralToSingularMap != null
                    && pluralToSingularMap.ContainsKey(tableInfo.Name)
                ? namePrefix + pluralToSingularMap[tableInfo.Name]
                : namePrefix + UnderscoreToPascalCase(tableInfo.Name).TrimEnd('s');
        }

    }
}
