//
// LX.EasyDb.Dialects.SQLServer2012Dialect.cs
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

namespace LX.EasyDb.Dialects
{
    /// <summary>
    /// Dialect for Microsoft SQL Server 2012.
    /// </summary>
    public class SQLServer2012Dialect : SQLServer2008Dialect
    {
        public override String GetPaging(String sql, String order, Int32 limit, Int32 offset)
        {
            StringBuilder sb = new StringBuilder(sql);
            if (!String.IsNullOrEmpty(order))
                sb.Append(" ").Append(order);
            if (offset > 0)
            {
                sb.Append(" OFFSET ")
                    .Append(offset)
                    .Append(" ROWS");
            }
            sb.Append(" FETCH NEXT ")
                .Append(limit)
                .Append(" ROWS ONLY");
            return sb.ToString();
        }
    }
}
