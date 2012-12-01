//
// LX.EasyDb.Dialect.cs
//
// Authors:
//	Longshine He <longshinehe@users.sourceforge.net>
//
// Copyright (c) 2012 Longshine He
//
// This code is distributed in the hope that it will be useful,
// but WITHOUT WARRANTY OF ANY KIND.
//

using System;
using System.Collections.Generic;
using LX.EasyDb.Dialects.Function;

namespace LX.EasyDb
{
    /// <summary>
    /// Represents a dialect of SQL implemented by a particular RDBMS.
    /// </summary>
    public abstract class Dialect
    {
        /// <summary>
        /// Default length of columns.
        /// </summary>
        public const Int32 DefaultColumnLength = 255;
        /// <summary>
        /// Default precision of columns.
        /// </summary>
        public const Int32 DefaultColumnPrecision = 19;
        /// <summary>
        /// Default scale of columns.
        /// </summary>
        public const Int32 DefaultColumnScale = 2;

        static readonly String QUOTES = "`\"[";

        private TypeNames<DbType> _typeNames = new TypeNames<DbType>();
        private Dictionary<String, ISQLFunction> _sqlFunctions = new Dictionary<String, ISQLFunction>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if a string is quoted.
        /// </summary>
        public static Boolean IsQuoted(String s)
        {
            return !String.IsNullOrEmpty(s) && QUOTES.IndexOf(s[0]) > -1;
        }

        /// <summary>
        /// Unquotes the input string, and returns a value indicating whether the input string needs unquoting.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns>true if the input string is quoted, or false if the input string does not need unquoting</returns>
        public static Boolean UnQuote(String input, out String output)
        {
            Boolean quoted = IsQuoted(input);
            output = quoted ? (input.Substring(1, input.Length - 1)) : input;
            return quoted;
        }

        /// <summary>
        /// 
        /// </summary>
        public Dialect()
        {
            StandardAnsiSqlAggregationFunctions.RegisterFunctions(_sqlFunctions);

            // standard sql92 functions (can be overridden by subclasses)
            RegisterFunction("substring", new SQLFunctionTemplate(DbType.String, "substring(?1, ?2, ?3)"));
            RegisterFunction("locate", new SQLFunctionTemplate(DbType.Int32, "locate(?1, ?2, ?3)"));
            RegisterFunction("trim", new SQLFunctionTemplate(DbType.String, "trim(?1 ?2 ?3 ?4)"));
            RegisterFunction("length", new StandardSQLFunction("length", DbType.Int32));
            RegisterFunction("bit_length", new StandardSQLFunction("bit_length", DbType.Int32));
            RegisterFunction("coalesce", new StandardSQLFunction("coalesce"));
            RegisterFunction("nullif", new StandardSQLFunction("nullif"));
            RegisterFunction("abs", new StandardSQLFunction("abs"));
            RegisterFunction("mod", new StandardSQLFunction("mod", DbType.Int32));
            RegisterFunction("sqrt", new StandardSQLFunction("sqrt", DbType.Double));
            RegisterFunction("upper", new StandardSQLFunction("upper"));
            RegisterFunction("lower", new StandardSQLFunction("lower"));
            RegisterFunction("cast", new CastFunction());
            RegisterFunction("extract", new SQLFunctionTemplate(DbType.Int32, "extract(?1 ?2 ?3)"));

            //map second/minute/hour/day/month/year to ANSI extract(), override on subclasses
            RegisterFunction("second", new SQLFunctionTemplate(DbType.Int32, "extract(second from ?1)"));
            RegisterFunction("minute", new SQLFunctionTemplate(DbType.Int32, "extract(minute from ?1)"));
            RegisterFunction("hour", new SQLFunctionTemplate(DbType.Int32, "extract(hour from ?1)"));
            RegisterFunction("day", new SQLFunctionTemplate(DbType.Int32, "extract(day from ?1)"));
            RegisterFunction("month", new SQLFunctionTemplate(DbType.Int32, "extract(month from ?1)"));
            RegisterFunction("year", new SQLFunctionTemplate(DbType.Int32, "extract(year from ?1)"));

            RegisterFunction("str", new SQLFunctionTemplate(DbType.String, "cast(?1 as char)"));
        }

        /// <summary>
        /// Gets the prefix character of SQL parameters.
        /// </summary>
        public virtual String ParamPrefix { get { return "@"; } }

        /// <summary>
        /// Gets the open character for quoting SQL identifiers.
        /// </summary>
        public virtual Char OpenQuote { get { return '"'; } }

        /// <summary>
        /// Gets the close character for quoting SQL identifiers.
        /// </summary>
        public virtual Char CloseQuote { get { return '"'; } }

        /// <summary>
        /// Finds a function by its name.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public ISQLFunction FindFunction(String functionName)
        {
            return _sqlFunctions.ContainsKey(functionName) ? _sqlFunctions[functionName] : null;
        }

        /// <summary>
        /// Quotes the given string. If it is quoted, it will be unquoted first.
        /// </summary>
        public String Quote(String name)
        {
            String unquote = null;
            UnQuote(name, out unquote);
            return OpenQuote + unquote + CloseQuote;
        }

        /// <summary>
        /// Get the name of the database type associated with the given <see cref="LX.EasyDb.DbType"/>.
        /// </summary>
        /// <param name="type">the <see cref="LX.EasyDb.DbType"/></param>
        /// <returns>the database type name</returns>
        public String GetTypeName(DbType type)
        {
            String result = _typeNames.Get(type);
            if (result == null)
                throw new Exception("No dialect mapping for type: " + type);
            return result;
        }

        /// <summary>
        /// Get the name of the database type associated with the given <see cref="LX.EasyDb.DbType"/> with the given storage specification parameters..
        /// </summary>
        /// <param name="type">the <see cref="LX.EasyDb.DbType"/></param>
        /// <param name="length">the datatype length</param>
        /// <param name="precision">the datatype precision</param>
        /// <param name="scale">the datatype scale</param>
        /// <returns>the database type name</returns>
        public String GetTypeName(DbType type, Int32 length, Int32 precision, Int32 scale)
        {
            String result = _typeNames.Get(type, length, precision, scale);
            if (result == null)
                throw new Exception("No dialect mapping for type: " + type + ", length: " + length);
            return result;
        }

        /// <summary>
        /// Get the name of the database type appropriate for casting operations (via the CAST() SQL function) for the given type.
        /// </summary>
        public String GetCastTypeName(String type)
        {
            DbType t = DbType.Empty;

            if (type == null)
                t = DbType.Empty;
            else if (String.Equals(type, "int", StringComparison.OrdinalIgnoreCase)
                || String.Equals(type, "integer", StringComparison.OrdinalIgnoreCase))
                t = DbType.Int32;
            else if (String.Equals(type, "uint", StringComparison.OrdinalIgnoreCase)
                || String.Equals(type, "unsigned", StringComparison.OrdinalIgnoreCase))
                t = DbType.UInt32;
            else if (String.Equals(type, "float", StringComparison.OrdinalIgnoreCase)
                || String.Equals(type, "double", StringComparison.OrdinalIgnoreCase))
                t = DbType.Double;
            else if (String.Equals(type, "binary", StringComparison.OrdinalIgnoreCase))
                t = DbType.Binary;
            else if (String.Equals(type, "string", StringComparison.OrdinalIgnoreCase))
                t = DbType.String;

            return GetCastTypeName(t);
        }

        /// <summary>
        /// Do we need to qualify index names with the schema name?
        /// </summary>
        public virtual Boolean QualifyIndexName
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the keyword used to specify a nullable column.
        /// </summary>
        public virtual String NullColumnString { get { return String.Empty; } }

        /// <summary>
        /// Does this dialect support the <code>UNIQUE</code> column syntax?
        /// </summary>
        public virtual Boolean SupportsUnique { get { return false; } }

        /// <summary>
        /// Does this dialect support adding Unique constraints via create and alter table?
        /// </summary>
        public virtual Boolean SupportsUniqueConstraintInCreateAlterTable { get { return true; } }

        /// <summary>
        /// Does this dialect support the <code>UNIQUE</code> column syntax in nullable columns?
        /// </summary>
        public virtual Boolean SupportsNullableUnique { get { return true; } }

        /// <summary>
        /// Whether this dialect have an primary key clause added to the data type or a completely separate identity data type.
        /// </summary>
        public virtual Boolean HasPrimaryKeyInIdentityColumn { get { return false; } }

        /// <summary>
        /// Whether this dialect have an Identity clause added to the data type or a completely separate identity data type.
        /// </summary>
        public virtual Boolean HasDataTypeInIdentityColumn { get { return true; } }

        /// <summary>
        /// Does this dialect support column-level check constraints?
        /// </summary>
        public virtual Boolean SupportsColumnCheck { get { return true; } }

        /// <summary>
        /// Does this dialect support table-level check constraints?
        /// </summary>
        public virtual Boolean SupportsTableCheck { get { return true; } }

        /// <summary>
        /// Does this dialect support "if exists" syntax before table name?
        /// </summary>
        public virtual Boolean SupportsIfExistsBeforeTableName { get { return false; } }

        /// <summary>
        /// Does this dialect support "if exists" syntax after table name?
        /// </summary>
        public virtual Boolean SupportsIfExistsAfterTableName { get { return false; } }

        /// <summary>
        /// Completely optional cascading drop clause
        /// </summary>
        public virtual String CascadeConstraintsString { get { return String.Empty; } }

        /// <summary>
        /// Command used to create a table.
        /// </summary>
        public virtual String CreateTableString { get { return "create table"; } }

        /// <summary>
        /// Slight variation on CreateTableString, used to create a table when there is no primary key and duplicate rows are expected.
        /// </summary>
        public virtual String CreateMultisetTableString { get { return CreateTableString; } }

        /// <summary>
        /// Does this dialect support temporary tables?
        /// </summary>
        public virtual Boolean SupportsTemporaryTables { get { return false; } }

        /// <summary>
        /// Command used to create a temporary table.
        /// </summary>
        public virtual String CreateTemporaryTableString { get { return "create table"; } }

        /// <summary>
        /// Command used to drop a temporary table.
        /// </summary>
        public virtual String DropTemporaryTableString { get { return "drop table"; } }

        /// <summary>
        /// Gets the name of the SQL function that transforms a string to lowercase.
        /// </summary>
        public virtual String LowercaseFunction { get { return "lower"; } }

        /// <summary>
        /// Gets the syntax used during DDL to define a column as being an IDENTITY of a particular type.
        /// </summary>
        public virtual String GetIdentityColumnString()
        {
            throw new MappingException("Dialect does not support identity key generation");
        }

        /// <summary>
        /// Get the name of the database type appropriate for casting operations (via the CAST() SQL function) for the given <see cref="LX.EasyDb.DbType"/>.
        /// </summary>
        public virtual String GetCastTypeName(DbType type)
        {
            return GetTypeName(type, DefaultColumnLength, DefaultColumnPrecision, DefaultColumnScale);
        }

        /// <summary>
        /// Generate a temporary table name given the base table.
        /// </summary>
        public virtual String GenerateTemporaryTableName(String baseTableName)
        {
            return "EasyDb_" + baseTableName;
        }

        /// <summary>
        /// Gets the syntax used to add a primary key constraint to a table.
        /// </summary>
        public virtual String GetAddPrimaryKeyConstraintString(String constraintName)
        {
            return " add constraint " + Quote(constraintName) + " primary key ";
        }

        /// <summary>
        /// Gets the syntax used to add a unique key constraint to a table.
        /// </summary>
        public virtual String GetAddUniqueKeyConstraintString(String constraintName)
        {
            return " add constraint " + Quote(constraintName) + " unique ";
        }

        /// <summary>
        /// Gets the syntax used to add comment on a column.
        /// </summary>
        public virtual String GetColumnComment(String comment)
        {
            return String.Empty;
        }

        /// <summary>
        /// Gets the syntax used to add comment on a table.
        /// </summary>
        public virtual String GetTableComment(String comment)
        {
            return String.Empty;
        }

        /// <summary>
        /// Registers a type name for the given type.
        /// </summary>
        /// <param name="type">the <see cref="LX.EasyDb.DbType"/></param>
        /// <param name="name">the database type name</param>
        protected void RegisterColumnType(DbType type, String name)
        {
            _typeNames.Put(type, name);
        }

        /// <summary>
        /// Registers a type name for the given type code and maximum column length.
        /// <code>$l</code> in the type name with be replaced by the column length (if appropriate).
        /// </summary>
        /// <param name="type">the <see cref="LX.EasyDb.DbType"/></param>
        /// <param name="capacity">the maximum length of database type</param>
        /// <param name="name">the database type name</param>
        protected void RegisterColumnType(DbType type, Int32 capacity, String name)
        {
            _typeNames.Put(type, capacity, name);
        }

        /// <summary>
        /// Registers a function.
        /// </summary>
        /// <param name="name">the name of the function</param>
        /// <param name="function"></param>
        protected void RegisterFunction(String name, ISQLFunction function)
        {
            _sqlFunctions[name] = function;
        }

        class TypeNames<T>
        {
            private Dictionary<T, String> _defaults = new Dictionary<T, String>();
            private Dictionary<T, IDictionary<Int32, String>> _weighted = new Dictionary<T, IDictionary<Int32, String>>();

            public String Get(T type)
            {
                return _defaults.ContainsKey(type) ? _defaults[type] : null;
            }

            public String Get(T type, Int32 length, Int32 precision, Int32 scale)
            {
                if (_weighted.ContainsKey(type))
                {
                    // iterate entries ordered by capacity to find first fit
                    foreach (KeyValuePair<Int32, String> pair in _weighted[type])
                    {
                        if (length <= pair.Key)
                            return Replace(pair.Value, length, precision, scale);
                    }
                }
                return Replace(Get(type), length, precision, scale);
            }

            public void Put(T type, Int32 capacity, String name)
            {
                if (!_weighted.ContainsKey(type))
                    _weighted.Add(type, new SortedDictionary<Int32, String>());
                _weighted[type][capacity] = name;
            }

            public void Put(T type, String name)
            {
                _defaults[type] = name;
            }

            public String Replace(String type, Int32 length, Int32 precision, Int32 scale)
            {
                if (type == null)
                    return null;
                else
                    return type.Replace("$s", scale.ToString()).Replace("$l", length.ToString()).Replace("$p", precision.ToString());
            }
        }
    }
}
