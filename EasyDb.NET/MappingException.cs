//
// LX.EasyDb.MappingException.cs
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

namespace LX.EasyDb
{
    /// <summary>
    /// Represents exceptions occurs in O-R mappings.
    /// </summary>
    public class MappingException : Exception
    {
        /// <summary>
        /// Creates a instance.
        /// </summary>
        public MappingException()
            : base()
        { }

        /// <summary>
        /// Creates a instance with the given message.
        /// </summary>
        public MappingException(String message)
            : base(message)
        { }
    }
}
