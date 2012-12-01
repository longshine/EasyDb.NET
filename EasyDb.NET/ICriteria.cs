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

using System;
using System.Collections;
using System.Collections.Generic;
using LX.EasyDb.Criterion;

namespace LX.EasyDb
{
    public interface ICriteria<T>
    {
        ICriteria<T> Add(IExpression condition);
        ICriteria<T> AddOrder(Order order);
        ICriteria<T> AddSelect(Select select);
        IEnumerable<T> List();
        IEnumerable<T> List(Int32 total, Int32 offset);
        Int32 Count();
        T SingleOrDefault();
    }

    public interface ICriteria
    {
        ICriteria Add(IExpression condition);
        ICriteria AddSelect(Select select);
        ICriteria AddOrder(Order order);
        IEnumerable List();
        IEnumerable List(Int32 total, Int32 offset);
        Int32 Count();
        Object SingleOrDefault();
    }
}
