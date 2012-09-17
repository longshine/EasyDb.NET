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

namespace LX.EasyDb
{
    public class Dialect
    {
        static readonly String QUOTES = "`\"[";

        private TypeNames<DbType> _typeNames = new TypeNames<DbType>();

        public static Boolean IsQuoted(String s)
        {
            return !String.IsNullOrEmpty(s) && QUOTES.IndexOf(s[0]) > -1;
        }

        public static Boolean UnQuote(String input, out String output)
        {
            Boolean quoted = IsQuoted(input);
            output = quoted ? (input.Substring(1, input.Length - 1)) : input;
            return quoted;
        }

        public String Quote(String name)
        {
            String unquote = null;
            UnQuote(name, out unquote);
            return OpenQuote + unquote + CloseQuote;
        }

        public String GetTypeName(DbType type)
        {
            return _typeNames.Get(type);
        }

        public String GetTypeName(DbType type, Int32 length, Int32 precision, Int32 scale)
        {
            return _typeNames.Get(type, length, precision, scale);
        }

        public virtual String ParamPrefix { get { return "@"; } }

        public virtual Boolean SupportsUnique
        {
            get { return false; }
        }

        public virtual Boolean SupportsNotNullUnique
        {
            get { return true; }
        }

        public virtual Boolean SupportsUniqueConstraintInCreateAlterTable
        {
            get { return true; }
        }

        public virtual Boolean QualifyIndexName
        {
            get { return true; }
        }

        public virtual Boolean SupportsColumnCheck
        {
            get { return true; }
        }

        public virtual Boolean SupportsTableCheck
        {
            get { return true; }
        }

        public virtual Boolean SupportsIfExistsBeforeTableName
        {
            get { return false; }
        }

        public virtual Boolean SupportsIfExistsAfterTableName
        {
            get { return false; }
        }

        public virtual String CascadeConstraintsString
        {
            get { return String.Empty; }
        }

        public virtual Char OpenQuote
        {
            get { return '"'; }
        }

        public virtual Char CloseQuote
        {
            get { return '"'; }
        }

        public virtual String CreateTableString
        {
            get { return "create table"; }
        }

        public virtual String CreateMultisetTableString
        {
            get { return CreateTableString; }
        }

        public virtual String NullColumnString
        {
            get { return String.Empty; }
        }

        public virtual String GetAddPrimaryKeyConstraintString(String constraintName)
        {
            return " add constraint " + Quote(constraintName) + " primary key ";
        }

        public virtual String GetAddUniqueKeyConstraintString(String constraintName)
        {
            return " add constraint " + Quote(constraintName) + " unique ";
        }

        public virtual String GetColumnComment(String comment)
        {
            return String.Empty;
        }

        public virtual String GetTableComment(String comment)
        {
            return String.Empty;
        }

        protected void RegisterColumnType(DbType type, String name)
        {
            _typeNames.Put(type, name);
        }

        protected void RegisterColumnType(DbType type, Int32 capacity, String name)
        {
            _typeNames.Put(type, capacity, name);
        }

        class TypeNames<T>
        {
            private Dictionary<T, String> _defaults = new Dictionary<T, String>();
            private Dictionary<T, IDictionary<Int32, String>> _weighted = new Dictionary<T, IDictionary<Int32, String>>();

            public String Get(T type)
            {
                if (_defaults.ContainsKey(type))
                    return _defaults[type];
                else
                    throw new Exception("No dialect mapping for type: " + type);
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
                return type.Replace("$s", scale.ToString()).Replace("$l", length.ToString()).Replace("$p", precision.ToString());
            }
        }
    }

    public class MySQLDialect : Dialect
    {
        public MySQLDialect()
        {
            RegisterColumnType(DbType.Identity, "int auto_increment");
            RegisterColumnType(DbType.Boolean, "bit");
            RegisterColumnType(DbType.Int64, "bigint");
            RegisterColumnType(DbType.Int16, "smallint");
            //RegisterColumnType(Types.TINYINT, "tinyint");
            RegisterColumnType(DbType.Int32, "integer");
            RegisterColumnType(DbType.StringFixedLength, "char(1)");
            RegisterColumnType(DbType.Single, "float");
            RegisterColumnType(DbType.Double, "double precision");
            RegisterColumnType(DbType.Date, "date");
            RegisterColumnType(DbType.Time, "time");
            RegisterColumnType(DbType.DateTime, "datetime");
            //RegisterColumnType(Types.VARBINARY, "longblob");
            //RegisterColumnType(Types.VARBINARY, 16777215, "mediumblob");
            //RegisterColumnType(Types.VARBINARY, 65535, "blob");
            //RegisterColumnType(Types.VARBINARY, 255, "tinyblob");
            //RegisterColumnType(Types.LONGVARBINARY, "longblob");
            //RegisterColumnType(Types.LONGVARBINARY, 16777215, "mediumblob");
            RegisterColumnType(DbType.Decimal, "decimal($p,$s)");
            //RegisterColumnType(Types.BLOB, "longblob");
            //RegisterColumnType( Types.BLOB, 16777215, "mediumblob" );
            //RegisterColumnType( Types.BLOB, 65535, "blob" );
            //RegisterColumnType(Types.CLOB, "longtext");
            //RegisterColumnType( Types.CLOB, 16777215, "mediumtext" );
            //RegisterColumnType( Types.CLOB, 65535, "text" );
            RegisterVarcharTypes();

            //RegisterFunction("ascii", new StandardSQLFunction("ascii", Types.INTEGER));
            //RegisterFunction("bin", new StandardSQLFunction("bin", Types.VARCHAR));
            //RegisterFunction("char_length", new StandardSQLFunction("char_length", Types.BIGINT));
            //RegisterFunction("character_length", new StandardSQLFunction("character_length", Types.BIGINT));
            //RegisterFunction("lcase", new StandardSQLFunction("lcase"));
            //RegisterFunction("lower", new StandardSQLFunction("lower"));
            //RegisterFunction("ltrim", new StandardSQLFunction("ltrim"));
            //RegisterFunction("ord", new StandardSQLFunction("ord", Types.INTEGER));
            //RegisterFunction("quote", new StandardSQLFunction("quote"));
            //RegisterFunction("reverse", new StandardSQLFunction("reverse"));
            //RegisterFunction("rtrim", new StandardSQLFunction("rtrim"));
            //RegisterFunction("soundex", new StandardSQLFunction("soundex"));
            //RegisterFunction("space", new StandardSQLFunction("space", Types.VARCHAR));
            //RegisterFunction("ucase", new StandardSQLFunction("ucase"));
            //RegisterFunction("upper", new StandardSQLFunction("upper"));
            //RegisterFunction("unhex", new StandardSQLFunction("unhex", Types.VARCHAR));

            //RegisterFunction("abs", new StandardSQLFunction("abs"));
            //RegisterFunction("sign", new StandardSQLFunction("sign", Types.INTEGER));

            //RegisterFunction("acos", new StandardSQLFunction("acos", Types.DOUBLE));
            //RegisterFunction("asin", new StandardSQLFunction("asin", Types.DOUBLE));
            //RegisterFunction("atan", new StandardSQLFunction("atan", Types.DOUBLE));
            //RegisterFunction("cos", new StandardSQLFunction("cos", Types.DOUBLE));
            //RegisterFunction("cot", new StandardSQLFunction("cot", Types.DOUBLE));
            //RegisterFunction("crc32", new StandardSQLFunction("crc32", Types.BIGINT));
            //RegisterFunction("exp", new StandardSQLFunction("exp", Types.DOUBLE));
            //RegisterFunction("ln", new StandardSQLFunction("ln", Types.DOUBLE));
            //RegisterFunction("log", new StandardSQLFunction("log", Types.DOUBLE));
            //RegisterFunction("log2", new StandardSQLFunction("log2", Types.DOUBLE));
            //RegisterFunction("log10", new StandardSQLFunction("log10", Types.DOUBLE));
            //RegisterFunction("pi", new NoArgSQLFunction("pi", Types.DOUBLE));
            //RegisterFunction("rand", new NoArgSQLFunction("rand", Types.DOUBLE));
            //RegisterFunction("sin", new StandardSQLFunction("sin", Types.DOUBLE));
            //RegisterFunction("sqrt", new StandardSQLFunction("sqrt", Types.DOUBLE));
            //RegisterFunction("tan", new StandardSQLFunction("tan", Types.DOUBLE));

            //RegisterFunction("radians", new StandardSQLFunction("radians", Types.DOUBLE));
            //RegisterFunction("degrees", new StandardSQLFunction("degrees", Types.DOUBLE));

            //RegisterFunction("ceiling", new StandardSQLFunction("ceiling", Types.INTEGER));
            //RegisterFunction("ceil", new StandardSQLFunction("ceil", Types.INTEGER));
            //RegisterFunction("floor", new StandardSQLFunction("floor", Types.INTEGER));
            //RegisterFunction("round", new StandardSQLFunction("round"));

            //RegisterFunction("datediff", new StandardSQLFunction("datediff", Types.INTEGER));
            //RegisterFunction("timediff", new StandardSQLFunction("timediff", Types.TIME));
            //RegisterFunction("date_format", new StandardSQLFunction("date_format", Types.VARCHAR));

            //RegisterFunction("curdate", new NoArgSQLFunction("curdate", Types.DATE));
            //RegisterFunction("curtime", new NoArgSQLFunction("curtime", Types.TIME));
            //RegisterFunction("current_date", new NoArgSQLFunction("current_date", Types.DATE, false));
            //RegisterFunction("current_time", new NoArgSQLFunction("current_time", Types.TIME, false));
            //RegisterFunction("current_timestamp", new NoArgSQLFunction("current_timestamp", Types.TIMESTAMP, false));
            //RegisterFunction("date", new StandardSQLFunction("date", Types.DATE));
            //RegisterFunction("day", new StandardSQLFunction("day", Types.INTEGER));
            //RegisterFunction("dayofmonth", new StandardSQLFunction("dayofmonth", Types.INTEGER));
            //RegisterFunction("dayname", new StandardSQLFunction("dayname", Types.VARCHAR));
            //RegisterFunction("dayofweek", new StandardSQLFunction("dayofweek", Types.INTEGER));
            //RegisterFunction("dayofyear", new StandardSQLFunction("dayofyear", Types.INTEGER));
            //RegisterFunction("from_days", new StandardSQLFunction("from_days", Types.DATE));
            //RegisterFunction("from_unixtime", new StandardSQLFunction("from_unixtime", Types.TIMESTAMP));
            //RegisterFunction("hour", new StandardSQLFunction("hour", Types.INTEGER));
            //RegisterFunction("last_day", new StandardSQLFunction("last_day", Types.DATE));
            //RegisterFunction("localtime", new NoArgSQLFunction("localtime", Types.TIMESTAMP));
            //RegisterFunction("localtimestamp", new NoArgSQLFunction("localtimestamp", Types.TIMESTAMP));
            //RegisterFunction("microseconds", new StandardSQLFunction("microseconds", Types.INTEGER));
            //RegisterFunction("minute", new StandardSQLFunction("minute", Types.INTEGER));
            //RegisterFunction("month", new StandardSQLFunction("month", Types.INTEGER));
            //RegisterFunction("monthname", new StandardSQLFunction("monthname", Types.VARCHAR));
            //RegisterFunction("now", new NoArgSQLFunction("now", Types.TIMESTAMP));
            //RegisterFunction("quarter", new StandardSQLFunction("quarter", Types.INTEGER));
            //RegisterFunction("second", new StandardSQLFunction("second", Types.INTEGER));
            //RegisterFunction("sec_to_time", new StandardSQLFunction("sec_to_time", Types.TIME));
            //RegisterFunction("sysdate", new NoArgSQLFunction("sysdate", Types.TIMESTAMP));
            //RegisterFunction("time", new StandardSQLFunction("time", Types.TIME));
            //RegisterFunction("timestamp", new StandardSQLFunction("timestamp", Types.TIMESTAMP));
            //RegisterFunction("time_to_sec", new StandardSQLFunction("time_to_sec", Types.INTEGER));
            //RegisterFunction("to_days", new StandardSQLFunction("to_days", Types.BIGINT));
            //RegisterFunction("unix_timestamp", new StandardSQLFunction("unix_timestamp", Types.BIGINT));
            //RegisterFunction("utc_date", new NoArgSQLFunction("utc_date", Types.VARCHAR));
            //RegisterFunction("utc_time", new NoArgSQLFunction("utc_time", Types.VARCHAR));
            //RegisterFunction("utc_timestamp", new NoArgSQLFunction("utc_timestamp", Types.VARCHAR));
            //RegisterFunction("week", new StandardSQLFunction("week", Types.INTEGER));
            //RegisterFunction("weekday", new StandardSQLFunction("weekday", Types.INTEGER));
            //RegisterFunction("weekofyear", new StandardSQLFunction("weekofyear", Types.INTEGER));
            //RegisterFunction("year", new StandardSQLFunction("year", Types.INTEGER));
            //RegisterFunction("yearweek", new StandardSQLFunction("yearweek", Types.INTEGER));

            //RegisterFunction("hex", new StandardSQLFunction("hex", Types.VARCHAR));
            //RegisterFunction("oct", new StandardSQLFunction("oct", Types.VARCHAR));

            //RegisterFunction("octet_length", new StandardSQLFunction("octet_length", Types.BIGINT));
            //RegisterFunction("bit_length", new StandardSQLFunction("bit_length", Types.BIGINT));

            //RegisterFunction("bit_count", new StandardSQLFunction("bit_count", Types.BIGINT));
            //RegisterFunction("encrypt", new StandardSQLFunction("encrypt", Types.VARCHAR));
            //RegisterFunction("md5", new StandardSQLFunction("md5", Types.VARCHAR));
            //RegisterFunction("sha1", new StandardSQLFunction("sha1", Types.VARCHAR));
            //RegisterFunction("sha", new StandardSQLFunction("sha", Types.VARCHAR));

            //RegisterFunction("concat", new StandardSQLFunction("concat", Types.VARCHAR));
        }

        protected virtual void RegisterVarcharTypes()
        {
            RegisterColumnType(DbType.String, "longtext");
            //RegisterColumnType( Types.VARCHAR, 16777215, "mediumtext" );
            //RegisterColumnType( Types.VARCHAR, 65535, "text" );
            RegisterColumnType(DbType.String, 255, "varchar($l)");
            //RegisterColumnType(Types.LONGVARCHAR, "longtext");
        }

        public override Char OpenQuote
        {
            get { return '`'; }
        }

        public override Char CloseQuote
        {
            get { return '`'; }
        }
    }
}
