//
// LX.EasyDb.ICriteria.cs
//
// Authors:
//	Longshine He <longshinehe@users.sourceforge.net>
//
// Copyright (c) 2012 Longshine He
//
// This code is distributed in the hope that it will be useful,
// but WITHOUT WARRANTY OF ANY KIND.
//

using System.Collections.Generic;
using LX.EasyDb.Criterion;

namespace LX.EasyDb
{
    public interface ICriteria<T>
    {
        ICriteria<T> Add(IExpression condition);
        ICriteria<T> AddOrder(Order order);
        IEnumerable<T> List();
        T SingleOrDefault();
    }
}
