//
// LX.EasyDb.Dialects.MySQLDialect.cs
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
using System.Text;
using LX.EasyDb.Dialects.Function;

namespace LX.EasyDb.Dialects
{
    /// <summary>
    /// Dialect for MySQL.
    /// </summary>
    public class MySQLDialect : Dialect
    {
        /// <summary>
        /// </summary>
        public MySQLDialect()
        {
            RegisterColumnType(DbType.Identity, "bigint");
            RegisterColumnType(DbType.Boolean, "bit");
            RegisterColumnType(DbType.Int64, "bigint");
            RegisterColumnType(DbType.UInt64, "bigint");
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

            RegisterFunction("ascii", new StandardSQLFunction("ascii", DbType.Int32));
            RegisterFunction("bin", new StandardSQLFunction("bin", DbType.String));
            RegisterFunction("char_length", new StandardSQLFunction("char_length", DbType.Int64));
            RegisterFunction("character_length", new StandardSQLFunction("character_length", DbType.Int64));
            RegisterFunction("lcase", new StandardSQLFunction("lcase"));
            RegisterFunction("lower", new StandardSQLFunction("lower"));
            RegisterFunction("ltrim", new StandardSQLFunction("ltrim"));
            RegisterFunction("ord", new StandardSQLFunction("ord", DbType.Int32));
            RegisterFunction("quote", new StandardSQLFunction("quote"));
            RegisterFunction("reverse", new StandardSQLFunction("reverse"));
            RegisterFunction("rtrim", new StandardSQLFunction("rtrim"));
            RegisterFunction("soundex", new StandardSQLFunction("soundex"));
            RegisterFunction("space", new StandardSQLFunction("space", DbType.String));
            RegisterFunction("ucase", new StandardSQLFunction("ucase"));
            RegisterFunction("upper", new StandardSQLFunction("upper"));
            RegisterFunction("unhex", new StandardSQLFunction("unhex", DbType.String));

            RegisterFunction("abs", new StandardSQLFunction("abs"));
            RegisterFunction("sign", new StandardSQLFunction("sign", DbType.Int32));

            RegisterFunction("acos", new StandardSQLFunction("acos", DbType.Double));
            RegisterFunction("asin", new StandardSQLFunction("asin", DbType.Double));
            RegisterFunction("atan", new StandardSQLFunction("atan", DbType.Double));
            RegisterFunction("cos", new StandardSQLFunction("cos", DbType.Double));
            RegisterFunction("cot", new StandardSQLFunction("cot", DbType.Double));
            RegisterFunction("crc32", new StandardSQLFunction("crc32", DbType.Int64));
            RegisterFunction("exp", new StandardSQLFunction("exp", DbType.Double));
            RegisterFunction("ln", new StandardSQLFunction("ln", DbType.Double));
            RegisterFunction("log", new StandardSQLFunction("log", DbType.Double));
            RegisterFunction("log2", new StandardSQLFunction("log2", DbType.Double));
            RegisterFunction("log10", new StandardSQLFunction("log10", DbType.Double));
            RegisterFunction("pi", new NoArgSQLFunction("pi", DbType.Double));
            RegisterFunction("rand", new NoArgSQLFunction("rand", DbType.Double));
            RegisterFunction("sin", new StandardSQLFunction("sin", DbType.Double));
            RegisterFunction("sqrt", new StandardSQLFunction("sqrt", DbType.Double));
            RegisterFunction("tan", new StandardSQLFunction("tan", DbType.Double));

            RegisterFunction("radians", new StandardSQLFunction("radians", DbType.Double));
            RegisterFunction("degrees", new StandardSQLFunction("degrees", DbType.Double));

            RegisterFunction("ceiling", new StandardSQLFunction("ceiling", DbType.Int32));
            RegisterFunction("ceil", new StandardSQLFunction("ceil", DbType.Int32));
            RegisterFunction("floor", new StandardSQLFunction("floor", DbType.Int32));
            RegisterFunction("round", new StandardSQLFunction("round"));

            RegisterFunction("datediff", new StandardSQLFunction("datediff", DbType.Int32));
            RegisterFunction("timediff", new StandardSQLFunction("timediff", DbType.DateTime));
            RegisterFunction("date_format", new StandardSQLFunction("date_format", DbType.String));

            RegisterFunction("curdate", new NoArgSQLFunction("curdate", DbType.Date));
            RegisterFunction("curtime", new NoArgSQLFunction("curtime", DbType.DateTime));
            RegisterFunction("current_date", new NoArgSQLFunction("current_date", DbType.Date, false));
            RegisterFunction("current_time", new NoArgSQLFunction("current_time", DbType.DateTime, false));
            RegisterFunction("current_timestamp", new NoArgSQLFunction("current_timestamp", DbType.DateTime, false));
            RegisterFunction("date", new StandardSQLFunction("date", DbType.Date));
            RegisterFunction("day", new StandardSQLFunction("day", DbType.Int32));
            RegisterFunction("dayofmonth", new StandardSQLFunction("dayofmonth", DbType.Int32));
            RegisterFunction("dayname", new StandardSQLFunction("dayname", DbType.String));
            RegisterFunction("dayofweek", new StandardSQLFunction("dayofweek", DbType.Int32));
            RegisterFunction("dayofyear", new StandardSQLFunction("dayofyear", DbType.Int32));
            RegisterFunction("from_days", new StandardSQLFunction("from_days", DbType.Date));
            RegisterFunction("from_unixtime", new StandardSQLFunction("from_unixtime", DbType.DateTime));
            RegisterFunction("hour", new StandardSQLFunction("hour", DbType.Int32));
            RegisterFunction("last_day", new StandardSQLFunction("last_day", DbType.Date));
            RegisterFunction("localtime", new NoArgSQLFunction("localtime", DbType.DateTime));
            RegisterFunction("localtimestamp", new NoArgSQLFunction("localtimestamp", DbType.DateTime));
            RegisterFunction("microseconds", new StandardSQLFunction("microseconds", DbType.Int32));
            RegisterFunction("minute", new StandardSQLFunction("minute", DbType.Int32));
            RegisterFunction("month", new StandardSQLFunction("month", DbType.Int32));
            RegisterFunction("monthname", new StandardSQLFunction("monthname", DbType.String));
            RegisterFunction("now", new NoArgSQLFunction("now", DbType.DateTime));
            RegisterFunction("quarter", new StandardSQLFunction("quarter", DbType.Int32));
            RegisterFunction("second", new StandardSQLFunction("second", DbType.Int32));
            RegisterFunction("sec_to_time", new StandardSQLFunction("sec_to_time", DbType.DateTime));
            RegisterFunction("sysdate", new NoArgSQLFunction("sysdate", DbType.DateTime));
            RegisterFunction("time", new StandardSQLFunction("time", DbType.DateTime));
            RegisterFunction("timestamp", new StandardSQLFunction("timestamp", DbType.DateTime));
            RegisterFunction("time_to_sec", new StandardSQLFunction("time_to_sec", DbType.Int32));
            RegisterFunction("to_days", new StandardSQLFunction("to_days", DbType.Int64));
            RegisterFunction("unix_timestamp", new StandardSQLFunction("unix_timestamp", DbType.Int64));
            RegisterFunction("utc_date", new NoArgSQLFunction("utc_date", DbType.String));
            RegisterFunction("utc_time", new NoArgSQLFunction("utc_time", DbType.String));
            RegisterFunction("utc_timestamp", new NoArgSQLFunction("utc_timestamp", DbType.String));
            RegisterFunction("week", new StandardSQLFunction("week", DbType.Int32));
            RegisterFunction("weekday", new StandardSQLFunction("weekday", DbType.Int32));
            RegisterFunction("weekofyear", new StandardSQLFunction("weekofyear", DbType.Int32));
            RegisterFunction("year", new StandardSQLFunction("year", DbType.Int32));
            RegisterFunction("yearweek", new StandardSQLFunction("yearweek", DbType.Int32));

            RegisterFunction("hex", new StandardSQLFunction("hex", DbType.String));
            RegisterFunction("oct", new StandardSQLFunction("oct", DbType.String));

            RegisterFunction("octet_length", new StandardSQLFunction("octet_length", DbType.Int64));
            RegisterFunction("bit_length", new StandardSQLFunction("bit_length", DbType.Int64));

            RegisterFunction("bit_count", new StandardSQLFunction("bit_count", DbType.Int64));
            RegisterFunction("encrypt", new StandardSQLFunction("encrypt", DbType.String));
            RegisterFunction("md5", new StandardSQLFunction("md5", DbType.String));
            RegisterFunction("sha1", new StandardSQLFunction("sha1", DbType.String));
            RegisterFunction("sha", new StandardSQLFunction("sha", DbType.String));

            RegisterFunction("concat", new StandardSQLFunction("concat", DbType.String));
        }

        /// <summary>
        /// </summary>
        public override Char OpenQuote
        {
            get { return '`'; }
        }

        /// <summary>
        /// </summary>
        public override Char CloseQuote
        {
            get { return '`'; }
        }

        /// <summary>
        /// </summary>
        public override String IdentityColumnString
        {
            get { return "not null auto_increment"; }
        }

        /// <summary>
        /// </summary>
        public override String SelectIdentityString
        {
            get { return "select @@IDENTITY id"; }
        }

        /// <summary>
        /// </summary>
        public override String GetCastTypeName(DbType type)
        {
            if (type == DbType.Int32 || type == DbType.Int16 || type == DbType.Int64)
                return "signed";
            else if (type == DbType.UInt32 || type == DbType.UInt16 || type == DbType.UInt64)
                return "unsigned";
            else if (type == DbType.Binary)
                return "binary";
            else
                return base.GetCastTypeName(type);
        }

        /// <summary>
        /// </summary>
        public override String GetPaging(String sql, String order, Int32 total, Int32 offset)
        {
            StringBuilder sb = StringHelper.CreateBuilder()
                .Append(sql)
                .Append(" LIMIT ")
                .Append(total);
            if (offset > 0)
            {
                sb.Append(" OFFSET ");
                sb.Append(offset);
            }
            return sb.ToString();
        }

        private void RegisterVarcharTypes()
        {
            RegisterColumnType(DbType.String, "longtext");
            //RegisterColumnType( DbType.String, 16777215, "mediumtext" );
            //RegisterColumnType( DbType.String, 65535, "text" );
            RegisterColumnType(DbType.String, 255, "varchar($l)");
            //RegisterColumnType(Types.LONGVARCHAR, "longtext");
        }
    }
}
