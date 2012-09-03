//
// LX.EasyDb.DbOperationBlock.cs
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
using System.Data;

namespace LX.EasyDb
{
    /// <summary>
    /// Represents one execution of a text command.
    /// </summary>
    public class DbOperationBlock : IDbOperationBlock
    {
        /// <summary>
        /// Gets the text command to run.
        /// </summary>
        public String CommandText { get; private set; }

        /// <summary>
        /// Gets the collection of parameters.
        /// </summary>
        public IDbDataParameter[] Parameters { get; private set; }

        /// <summary>
        /// Gets the type indicates or specifies how the command is interpreted.
        /// </summary>
        public CommandType CommandType { get; private set; }

        /// <summary>
        /// Gets the type of this command.
        /// </summary>
        public DbOperationType Operation { get; private set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="operation">the type of this command</param>
        /// <param name="command">the text command to run</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        public DbOperationBlock(DbOperationType operation, String command, IDbDataParameter[] parameters, CommandType commandType)
        {
            CommandText = command;
            Parameters = parameters;
            CommandType = commandType;
            Operation = operation;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="operation">the type of this command</param>
        /// <param name="command">the <see cref="System.Data.IDbCommand"/> to run</param>
        /// <param name="parameters">the collection of parameters</param>
        /// <param name="commandType">the type indicates or specifies how the command is interpreted</param>
        public DbOperationBlock(DbOperationType operation, String command, IDataParameterCollection parameters, CommandType commandType)
            : this(operation, command, ConvertParameters(parameters), commandType)
        { }

        private static IDbDataParameter[] ConvertParameters(IDataParameterCollection col)
        {
            IDbDataParameter[] ps = new IDbDataParameter[col.Count];
            col.CopyTo(ps, 0);
            return ps;
        }
    }
}
