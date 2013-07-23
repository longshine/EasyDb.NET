//
// LX.EasyDb.Dialects.SQLServerDialect.cs
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
    /// Dialect for Microsoft SQL Server 2000 and 2005.
    /// </summary>
    public class SQLServerDialect : AbstractTransactSQLDialect
    {
        /// <summary>
        /// </summary>
        public SQLServerDialect()
        {
            RegisterColumnType(DbType.Binary, "image");
            RegisterColumnType(DbType.Binary, 8000, "varbinary($l)");
            //RegisterColumnType(Types.LONGVARBINARY, "image");
            //RegisterColumnType(Types.LONGVARCHAR, "text");

            RegisterFunction("second", new SQLFunctionTemplate(DbType.Int32, "datepart(second, ?1)"));
            RegisterFunction("minute", new SQLFunctionTemplate(DbType.Int32, "datepart(minute, ?1)"));
            RegisterFunction("hour", new SQLFunctionTemplate(DbType.Int32, "datepart(hour, ?1)"));
            RegisterFunction("locate", new StandardSQLFunction("charindex", DbType.Int32));

            RegisterFunction("extract", new SQLFunctionTemplate(DbType.Int32, "datepart(?1, ?3)"));
            RegisterFunction("mod", new SQLFunctionTemplate(DbType.Int32, "?1 % ?2"));
            RegisterFunction("bit_length", new SQLFunctionTemplate(DbType.Int32, "datalength(?1) * 8"));

            RegisterFunction("trim", new AnsiTrimEmulationFunction());
        }

        public override Char OpenQuote { get { return '['; } }

        public override Char CloseQuote { get { return ']'; } }

        public override String GetPaging(String sql, String order, Int32 total, Int32 offset)
        {
            StringBuilder sb = new StringBuilder();
            if (offset > 0)
            {
                String anotherOrderby = order.ToUpperInvariant();
                if (anotherOrderby.Contains(" DESC"))
                    anotherOrderby = anotherOrderby.Replace(" DESC", " ASC");
                else if (anotherOrderby.Contains(" ASC"))
                    anotherOrderby = anotherOrderby.Replace(" ASC", " DESC");
                else
                    anotherOrderby += " DESC";
                sb.Append("SELECT * FROM (SELECT top ");
                sb.Append(total);
                sb.Append(" * FROM (SELECT top ");
                sb.Append(total + offset);
                sb.Append(" * FROM (");
                sb.Append(sql);
                sb.Append(") t1 ");
                sb.Append(order);
                sb.Append(") t2 ");
                sb.Append(anotherOrderby);
                sb.Append(") t3 ");
                sb.Append(order);
                return sb.ToString();
            }
            else
            {
                // TODO better idea?
                return "SELECT TOP " + total + " * from (" + sql + ") t " + (order ?? String.Empty);
            }
        }
    }
}
