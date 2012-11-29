using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser
{
    public enum TreeCode { Or = 0, And = 1, Not = 2, LBracket = 3, RBracket = 4, Condition = 5, End = 6, Expression = 7 };
    public class TreeElement
    {
        public bool IsMarked = false;
        public bool IsExpr = false;
        public TreeCode TreeCode;

        public TreeElement(TreeCode code)
        { this.TreeCode = code; }
        public override string ToString()
        {
            return "Token " + (IsMarked ? "<" : "") + " " + TreeCode.ToString();
        }
    }

    public class ConditionElement : TreeElement
    {
        public Parser.LexAnalyzer.Token Left, Operation, Right;

        public ConditionElement(Parser.LexAnalyzer.Token left, Parser.LexAnalyzer.Token operation, Parser.LexAnalyzer.Token right)
            : base(TreeCode.Condition)
        {
            this.Left = left;
            this.Right = right;
            this.Operation = operation;
        }
    }
   
}
