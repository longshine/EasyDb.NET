//
// LX.EasyDb.Helper.cs
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
#if !NET20
using System.Linq;
#endif
using System.Reflection;
using System.Text;

namespace LX.EasyDb
{
    delegate Boolean AppendItemConfirmHandler<T>(T item);
    delegate void AppendItemHandler<T>(T item);

    static class StringHelper
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
            Int32 loc = qualifiedName.LastIndexOf(".");
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
            Boolean appendSeperator = false;
            foreach (T item in it)
            {
                if (appendSeperator)
                {
                    foreach (var sb in builders)
                        sb.Append(seperator);
                }
                else
                    appendSeperator = true;
                appendItem(item);
            }
        }

        public static StringBuilder CreateBuilder()
        {
            return new StringBuilder();
        }

        public static String ToString(IEnumerable<Object> objects)
        {
            return Join(", ", objects);
        }

        public static String Join(String seperator, IEnumerable<Object> objects)
        {
            Boolean first = true;
            StringBuilder sb = new StringBuilder();
            foreach (Object obj in objects)
            {
                if (first)
                    first = false;
                else
                    sb.Append(seperator);
                sb.Append(obj);
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

    static class ReflectHelper
    {
        public static T GetAttribute<T>(MemberInfo mi) where T : Attribute
        {
            Object[] attrs = mi.GetCustomAttributes(typeof(T), true);
            return attrs.Length > 0 ? (T)attrs[0] : default(T);
        }

        public static Boolean HasAttribute<T>(MemberInfo mi) where T : Attribute
        {
            return mi.IsDefined(typeof(T), true);
        }

        public static IEnumerable<FieldInfo> GetSettableFields(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        }

        public static Type GetMemberType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                default:
                    return null;
            }
        }

        public static Type GetSubTypeInNamespace<T>(Assembly asm, String ns)
        {
            Type type = typeof(T);
            return Array.Find(asm.GetTypes(), delegate(Type t)
            {
                return (ns == null || String.Equals(t.Namespace, ns)) && t.IsSubclassOf(type);
            });
        }

        public static void GetTypeNames(String typeFullName, out String className, out String assemblyName)
        {
            String[] tmp = typeFullName.Split(new Char[] { ',' }, 2);
            className = tmp[0];
            assemblyName = (tmp.Length > 1) ? tmp[1].Trim() : String.Empty;
        }

        public static Assembly LoadAssembly(String assemblyName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            if (!String.IsNullOrEmpty(assemblyName) && !assemblyName.Equals(asm.FullName.Split(',')[0]))
                asm = Assembly.Load(assemblyName);
            return asm;
        }

        public static T CreateInstance<T>(String typeFullName)
        {
            String className, assemblyName;
            ReflectHelper.GetTypeNames(typeFullName, out className, out assemblyName);
            Assembly asm = ReflectHelper.LoadAssembly(assemblyName);
            return (T)asm.CreateInstance(className);
        }
    }
}
