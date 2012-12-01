//
// LX.EasyDb.Dialects.PostgreSQLDialect.cs
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
    /// Dialect for Postgre SQL.
    /// </summary>
    public class PostgreSQLDialect : Dialect
    {
        /// <summary>
        /// </summary>
        public PostgreSQLDialect()
        {
            RegisterColumnType(DbType.Boolean, "bool");
            RegisterColumnType(DbType.Int64, "int8");
            RegisterColumnType(DbType.Int16, "int2");
            RegisterColumnType(DbType.Int32, "int4");
            //RegisterColumnType(DbType.CHAR, "char(1)");
            //RegisterColumnType(DbType.VARCHAR, "varchar($l)");
            RegisterColumnType(DbType.Single, "float4");
            RegisterColumnType(DbType.Double, "float8");
            RegisterColumnType(DbType.Date, "date");
            RegisterColumnType(DbType.Time, "time");
            RegisterColumnType(DbType.DateTime, "timestamp");
            //RegisterColumnType(DbType.VARBINARY, "bytea");
            //RegisterColumnType(DbType.LONGVARCHAR, "text");
            //RegisterColumnType(DbType.LONGVARBINARY, "bytea");
            //RegisterColumnType(DbType.CLOB, "text");
            //RegisterColumnType(DbType.BLOB, "oid");
            RegisterColumnType(DbType.VarNumeric, "numeric($p, $s)");
            RegisterColumnType(DbType.Object, "uuid");

            RegisterFunction("abs", new StandardSQLFunction("abs"));
            RegisterFunction("sign", new StandardSQLFunction("sign", DbType.Int32));

            RegisterFunction("acos", new StandardSQLFunction("acos", DbType.Double));
            RegisterFunction("asin", new StandardSQLFunction("asin", DbType.Double));
            RegisterFunction("atan", new StandardSQLFunction("atan", DbType.Double));
            RegisterFunction("cos", new StandardSQLFunction("cos", DbType.Double));
            RegisterFunction("cot", new StandardSQLFunction("cot", DbType.Double));
            RegisterFunction("exp", new StandardSQLFunction("exp", DbType.Double));
            RegisterFunction("ln", new StandardSQLFunction("ln", DbType.Double));
            RegisterFunction("log", new StandardSQLFunction("log", DbType.Double));
            RegisterFunction("sin", new StandardSQLFunction("sin", DbType.Double));
            RegisterFunction("sqrt", new StandardSQLFunction("sqrt", DbType.Double));
            RegisterFunction("cbrt", new StandardSQLFunction("cbrt", DbType.Double));
            RegisterFunction("tan", new StandardSQLFunction("tan", DbType.Double));
            RegisterFunction("radians", new StandardSQLFunction("radians", DbType.Double));
            RegisterFunction("degrees", new StandardSQLFunction("degrees", DbType.Double));

            RegisterFunction("stddev", new StandardSQLFunction("stddev", DbType.Double));
            RegisterFunction("variance", new StandardSQLFunction("variance", DbType.Double));

            RegisterFunction("random", new NoArgSQLFunction("random", DbType.Double));

            RegisterFunction("round", new StandardSQLFunction("round"));
            RegisterFunction("trunc", new StandardSQLFunction("trunc"));
            RegisterFunction("ceil", new StandardSQLFunction("ceil"));
            RegisterFunction("floor", new StandardSQLFunction("floor"));

            RegisterFunction("chr", new StandardSQLFunction("chr", DbType.Byte));
            RegisterFunction("lower", new StandardSQLFunction("lower"));
            RegisterFunction("upper", new StandardSQLFunction("upper"));
            RegisterFunction("substr", new StandardSQLFunction("substr", DbType.String));
            RegisterFunction("initcap", new StandardSQLFunction("initcap"));
            RegisterFunction("to_ascii", new StandardSQLFunction("to_ascii"));
            RegisterFunction("quote_ident", new StandardSQLFunction("quote_ident", DbType.String));
            RegisterFunction("quote_literal", new StandardSQLFunction("quote_literal", DbType.String));
            RegisterFunction("md5", new StandardSQLFunction("md5"));
            RegisterFunction("ascii", new StandardSQLFunction("ascii", DbType.Int32));
            RegisterFunction("char_length", new StandardSQLFunction("char_length", DbType.Int64));
            RegisterFunction("bit_length", new StandardSQLFunction("bit_length", DbType.Int64));
            RegisterFunction("octet_length", new StandardSQLFunction("octet_length", DbType.Int64));

            RegisterFunction("age", new StandardSQLFunction("age"));
            RegisterFunction("current_date", new NoArgSQLFunction("current_date", DbType.Date, false));
            RegisterFunction("current_time", new NoArgSQLFunction("current_time", DbType.DateTime, false));
            RegisterFunction("current_timestamp", new NoArgSQLFunction("current_timestamp", DbType.DateTime, false));
            RegisterFunction("date_trunc", new StandardSQLFunction("date_trunc", DbType.DateTime));
            RegisterFunction("localtime", new NoArgSQLFunction("localtime", DbType.DateTime, false));
            RegisterFunction("localtimestamp", new NoArgSQLFunction("localtimestamp", DbType.DateTime, false));
            RegisterFunction("now", new NoArgSQLFunction("now", DbType.DateTime));
            RegisterFunction("timeofday", new NoArgSQLFunction("timeofday", DbType.String));

            RegisterFunction("current_user", new NoArgSQLFunction("current_user", DbType.String, false));
            RegisterFunction("session_user", new NoArgSQLFunction("session_user", DbType.String, false));
            RegisterFunction("user", new NoArgSQLFunction("user", DbType.String, false));
            RegisterFunction("current_database", new NoArgSQLFunction("current_database", DbType.String, true));
            RegisterFunction("current_schema", new NoArgSQLFunction("current_schema", DbType.String, true));

            RegisterFunction("to_char", new StandardSQLFunction("to_char", DbType.String));
            RegisterFunction("to_date", new StandardSQLFunction("to_date", DbType.Date));
            RegisterFunction("to_timestamp", new StandardSQLFunction("to_timestamp", DbType.DateTime));
            RegisterFunction("to_number", new StandardSQLFunction("to_number", DbType.Decimal));

            RegisterFunction("concat", new VarArgsSQLFunction(DbType.String, "(", "||", ")"));

            //RegisterFunction("locate", new PositionSubstringFunction());

            RegisterFunction("str", new SQLFunctionTemplate(DbType.String, "cast(?1 as varchar)"));
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
        public override string SelectIdentityString
        {
            get { throw new NotImplementedException(); }
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
    }
}
