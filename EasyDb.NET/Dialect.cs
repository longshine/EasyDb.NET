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

        public virtual String LowercaseFunction { get { return "lower"; } }

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
}
