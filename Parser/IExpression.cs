using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser
{
    public interface IExpression<T> where T : ITestable
    {
        bool TestValidity(T element);
    }

    public interface ITestable
    {
        string GetString(int id);
        int GetInteger(int id);
    }

    public enum Operations { And, Or }
    public class LogicalElement<T> : TreeElement, IExpression<T>
         where T : ITestable
    {
        public IExpression<T> A;
        public Operations Operation;
        public IExpression<T> B;
        public LogicalElement(IExpression<T> a, Operations op, IExpression<T> b)
            : base(TreeCode.Expression)
        {
            IsExpr = true;
            this.A = a;
            this.B = b;
            this.Operation = op;
        }

        public override string ToString()
        {
            return "(" + A.ToString() + (Operation == Operations.And ? " and " : " or ") + B.ToString() + ")";
        }

        public bool TestValidity(T element)
        {
            if (Operation == Operations.Or)
            {
                if (A.TestValidity(element)) return true;
                return B.TestValidity(element);
            }
            else /* AND */
            {
                if (!A.TestValidity(element)) return false;
                return B.TestValidity(element);
            }
        }
    }

   

    public class NotExpressionElement<T> : TreeElement, IExpression<T> where T : ITestable
    {
        public IExpression<T> A;
        public NotExpressionElement(IExpression<T> a)
            : base(TreeCode.Expression)
        {
            IsExpr = true;
            this.A = a;
        }

        public override string ToString()
        {
            return "(not " + A.ToString() + ")";
        }

        public bool TestValidity(T element)
        {
            return !A.TestValidity(element);
        }
    }
}
