using System;
using System.Collections.Generic;

namespace LX.EasyDb.Criterion
{
    public class Junction : IExpression
    {
        public Junction(String op)
        {
            Op = op;
            Expressions = new List<IExpression>();
        }

        public IList<IExpression> Expressions { get; private set; }

        public Junction Add(IExpression exp)
        {
            Expressions.Add(exp);
            return this;
        }

        public String Op { get; private set; }

        public String ToSqlString(ICriteriaRender criteria)
        {
            return criteria.ToSqlString(this);
        }

        public override String ToString()
        {
            return '(' + StringHelper.Join(' ' + Op + ' ', Expressions.GetEnumerator()) + ')';
        }
    }

    class Conjunction : Junction
    {
        public Conjunction()
            : base("and")
        { }
    }

    class Disjunction : Junction
    {
        public Disjunction()
            : base("or")
        { }
    }
}
