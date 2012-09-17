//
// LX.EasyDb.Enumerable.cs
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

namespace LX.EasyDb
{
    /// <summary>
    /// Provides a set of static methods for querying objects that implement <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/> or <see cref="System.Collections.IEnumerable"/>.
    /// </summary>
    public static class Enumerable
    {
        /// <summary>
        /// Returns the first element of a sequence.
        /// </summary>
        /// <param name="source">the <see cref="System.Collections.IEnumerable"/> to return the first element of</param>
        /// <returns>the first element in the specified sequence</returns>
        public static Object First(IEnumerable source)
        {
            return First(source, true, false);
        }

        /// <summary>
        /// Returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <param name="source">the <see cref="System.Collections.IEnumerable"/> to return the first element of</param>
        /// <returns>null if <paramref name="source"/> is empty; otherwise, the first element in <paramref name="source"/></returns>
        public static Object FirstOrDefault(IEnumerable source)
        {
            return First(source, false, false);
        }

        /// <summary>
        /// Returns the first element of a sequence.
        /// </summary>
        /// <typeparam name="T">the type of the elements of <paramref name="source"/></typeparam>
        /// <param name="source">the <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/> to return the first element of</param>
        /// <returns>the first element in the specified sequence</returns>
        public static T First<T>(IEnumerable<T> source)
        {
            return First(source, true, false);
        }

        /// <summary>
        /// Returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="T">the type of the elements of <paramref name="source"/></typeparam>
        /// <param name="source">the <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/> to return the first element of</param>
        /// <returns>default (T) if <paramref name="source"/> is empty; otherwise, the first element in <paramref name="source"/></returns>
        public static T FirstOrDefault<T>(IEnumerable<T> source)
        {
            return First(source, false, false);
        }

        /// <summary>
        /// Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.
        /// </summary>
        /// <param name="source">the <see cref="System.Collections.IEnumerable"/> to return the single element of</param>
        /// <returns>the single element of the input sequence</returns>
        public static Object Single(IEnumerable source)
        {
            return First(source, true, true);
        }

        /// <summary>
        /// Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.
        /// </summary>
        /// <param name="source">the <see cref="System.Collections.IEnumerable"/> to return the single element of</param>
        /// <returns>the single element of the input sequence, or null if the sequence contains no elements</returns>
        public static Object SingleOrDefault(IEnumerable source)
        {
            return First(source, false, true);
        }

        /// <summary>
        /// Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.
        /// </summary>
        /// <typeparam name="T">the type of the elements of <paramref name="source"/></typeparam>
        /// <param name="source">the <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/> to return the single element of</param>
        /// <returns>the single element of the input sequence</returns>
        public static T Single<T>(IEnumerable<T> source)
        {
            return First(source, true, true);
        }

        /// <summary>
        /// Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.
        /// </summary>
        /// <typeparam name="T">the type of the elements of <paramref name="source"/></typeparam>
        /// <param name="source">the <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/> to return the single element of</param>
        /// <returns>the single element of the input sequence, or default(T) if the sequence contains no elements</returns>
        public static T SingleOrDefault<T>(IEnumerable<T> source)
        {
            return First(source, false, true);
        }

        private static Object First(IEnumerable source, Boolean throwEmpty, Boolean throwMoreThanOne)
        {
            Object ret = null;
            Int32 gotu = 0;
            foreach (var item in source)
            {
                if (gotu == 0)
                    ret = item;
                gotu++;
            }
            if (gotu == 0 && throwEmpty)
                throw new InvalidOperationException("The enumerable source is empty.");
            if (gotu > 1 && throwMoreThanOne)
                throw new InvalidOperationException("The enumerable source has more than one element.");
            return ret;
        }

        private static T First<T>(IEnumerable<T> source, Boolean throwEmpty, Boolean throwMoreThanOne)
        {
            T ret = default(T);
            Int32 gotu = 0;
            foreach (var item in source)
            {
                if (gotu == 0)
                    ret = item;
                gotu++;
            }
            if (gotu == 0 && throwEmpty)
                throw new InvalidOperationException("The enumerable source is empty.");
            if (gotu > 1 && throwMoreThanOne)
                throw new InvalidOperationException("The enumerable source has more than one element.");
            return ret;
        }

        /// <summary>
        /// Creates a <see cref="System.Collections.Generic.IList&lt;T&gt;"/> from an <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/>.
        /// </summary>
        /// <typeparam name="T">the type of the elements of <paramref name="source"/></typeparam>
        /// <param name="source">the <see cref="System.Collections.Generic.IEnumerable&lt;T&gt;"/> to create a <see cref="System.Collections.Generic.IList&lt;T&gt;"/> from</param>
        /// <returns>a <see cref="System.Collections.Generic.IList&lt;T&gt;"/> that contains elements from the input sequence</returns>
        public static IList<T> ToList<T>(IEnumerable<T> source)
        {
            return new List<T>(source);
        }

        /// <summary>
        /// Creates a <see cref="System.Collections.IList"/> from an <see cref="System.Collections.IEnumerable"/>.
        /// </summary>
        /// <param name="source">the <see cref="System.Collections.IList"/> to create a <see cref="System.Collections.IEnumerable"/> from</param>
        /// <returns>a <see cref="System.Collections.IList"/> that contains elements from the input sequence</returns>
        public static IList ToList(IEnumerable source)
        {
            ArrayList list = new ArrayList();
            foreach (var item in source)
            {
                list.Add(item);
            }
            return list;
        }
    }
}
