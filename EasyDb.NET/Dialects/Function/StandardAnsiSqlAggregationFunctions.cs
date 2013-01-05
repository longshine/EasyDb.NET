//
// LX.EasyDb.Dialects.Function.StandardAnsiSqlAggregationFunctions.cs
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
using System.Text;

namespace LX.EasyDb.Dialects.Function
{
    class StandardAnsiSqlAggregationFunctions
    {
        public static StandardSQLFunction Count = new CountFunction();
        public static StandardSQLFunction Avg = new AvgFunction();
        public static StandardSQLFunction Max = new MaxFunction();
        public static StandardSQLFunction Min = new MinFunction();
        public static StandardSQLFunction Sum = new SumFunction();

        public static void RegisterFunctions(IDictionary<String, ISQLFunction> functions)
        {
            functions.Add(Count.Name, Count);
            functions.Add(Avg.Name, Avg);
            functions.Add(Max.Name, Max);
            functions.Add(Min.Name, Min);
            functions.Add(Sum.Name, Sum);
        }

        class CountFunction : StandardSQLFunction
        {
            public CountFunction()
                : base("count", DbType.Int64)
            {
            }

            protected override String DoRender(IList<Object> args, IConnectionFactory factory)
            {
                if (args.Count > 1 &&
                    "distinct".Equals(args[0].ToString(), StringComparison.Ordinal))
                        return RenderCountDistinct(args);
                else
                    return base.DoRender(args, factory);
            }

            private static String RenderCountDistinct(IList<Object> args)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("count(distinct ");
                sb.Append(args[1]);
                for (Int32 i = 2; i < args.Count; i++)
                    sb.Append(", ").Append(args[i]);
                return sb.Append(")").ToString();
            }
        }

        class AvgFunction : StandardSQLFunction
        { 
            public AvgFunction()
                : base("avg", DbType.Double)
            {
            }

            protected override String DoRender(IList<Object> args, IConnectionFactory factory)
            {
                return base.DoRender(args, factory);
            }
        }

        class MaxFunction : StandardSQLFunction
        {
            public MaxFunction()
                : base("max")
            {
            }
        }

        class MinFunction : StandardSQLFunction
        {
            public MinFunction()
                : base("min")
            {
            }
        }

        class SumFunction : StandardSQLFunction
        {
            public SumFunction()
                : base("sum")
            {
            }
        }
    }
}
