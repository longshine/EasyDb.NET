//
// LX.EasyDb.Criterion.MatchMode.cs
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

namespace LX.EasyDb.Criterion
{
    /// <summary>
    /// Represents an strategy for matching Strings using "like".
    /// </summary>
    public abstract class MatchMode
    {
        /// <summary>
        /// Match the entire String to the pattern.
        /// </summary>
        public static readonly MatchMode Exact = new MatchExactMode();
        /// <summary>
        /// Match the start of the String to the pattern.
        /// </summary>
        public static readonly MatchMode Start = new MatchStartMode();
        /// <summary>
        /// Match the end of the String to the pattern.
        /// </summary>
        public static readonly MatchMode End = new MatchEndMode();
        /// <summary>
        /// Match the pattern anywhere in the String.
        /// </summary>
        public static readonly MatchMode Anywhere = new MatchAnywhereMode();

        private static Dictionary<String, MatchMode> Instances = new Dictionary<String, MatchMode>();
        private String _name;

        static MatchMode()
        {
            Instances.Add(Exact._name, Exact);
            Instances.Add(Start._name, Start);
            Instances.Add(End._name, End);
            Instances.Add(Anywhere._name, Anywhere);
        }

        internal MatchMode(String name)
        {
            _name = name;
        }

        /// <summary>
        /// Converts the pattern, by appending/prepending "%".
        /// </summary>
        public abstract String ToMatchString(String pattern);

        /// <summary>
        /// </summary>
        public override String ToString()
        {
            return _name;
        }

        class MatchExactMode : MatchMode
        {
            public MatchExactMode() : base("EXACT") { }

            public override String ToMatchString(String pattern)
            {
                return pattern;
            }
        }

        class MatchStartMode : MatchMode
        {
            public MatchStartMode() : base("START") { }

            public override String ToMatchString(String pattern)
            {
                return pattern + "%";
            }
        }

        class MatchEndMode : MatchMode
        {
            public MatchEndMode() : base("END") { }

            public override String ToMatchString(String pattern)
            {
                return "%" + pattern;
            }
        }

        class MatchAnywhereMode : MatchMode
        {
            public MatchAnywhereMode() : base("ANYWHERE") { }

            public override String ToMatchString(String pattern)
            {
                return "%" + pattern + "%";
            }
        }
    }
}
