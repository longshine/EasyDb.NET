//
// LX.EasyDb.StringHelper.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LX.EasyDb
{
    delegate Boolean AppendItemConfirmHandler<T>(T item);
    delegate void AppendItemHandler<T>(T item);

    class StringHelper
    {
        public static String Qualify(String prefix, String name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (prefix == null)
                throw new ArgumentNullException("prefix");
            return new StringBuilder(prefix.Length + name.Length + 1)
                    .Append(prefix)
                    .Append('.')
                    .Append(name)
                    .ToString();
        }

        public static String Unqualify(String qualifiedName)
        {
            int loc = qualifiedName.LastIndexOf(".");
            return (loc < 0) ? qualifiedName : qualifiedName.Substring(qualifiedName.LastIndexOf(".") + 1);
        }

        public static void AppendItemsWithComma<T>(IEnumerable<T> it, AppendItemHandler<T> appendItem, params StringBuilder[] builders)
        {
            AppendItemsWithSeperator(it, ", ", appendItem, builders);
        }

        public static void AppendItemsWithSeperator<T>(IEnumerable<T> it, String seperator, AppendItemConfirmHandler<T> appendItem, params StringBuilder[] builders)
        {
            Boolean appendSeperator = false;
            foreach (T item in it)
            {
                if (appendSeperator)
                {
                    foreach (var sb in builders)
                        sb.Append(seperator);
                }
                appendSeperator = appendItem(item);
            }
        }

        public static void AppendItemsWithSeperator<T>(IEnumerable<T> it, String seperator, AppendItemHandler<T> appendItem, params StringBuilder[] builders)
        {
            AppendItemsWithSeperator(it, seperator, delegate(T item)
            {
                appendItem(item);
                return true;
            }, builders);
        }

        public static StringBuilder CreateBuilder()
        {
            return new StringBuilder();
        }

        public static String ToString(Object[] objects)
        {
            return Join(", ", objects);
        }

        public static String Join(String seperator, Object[] objects)
        {
            int len = objects.Length;
            StringBuilder sb = new StringBuilder(len * 2);
            if (len > 0)
            {
                sb.Append(objects[0]);
                for (int i = 1; i < len; i++)
                    sb.Append(seperator).Append(objects[i]);
            }
            return sb.ToString();
        }

        public static String Join(String seperator, IEnumerator objects)
        {
            StringBuilder sb = new StringBuilder();
            if (objects.MoveNext())
                sb.Append(objects.Current);
            while (objects.MoveNext())
                sb.Append(seperator).Append(objects.Current);
            return sb.ToString();
        }
    }
}
