using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser
{
    public enum ConditionOperations { Eq, Greater, Less, GE, LE, And, Or };
    public class ConditionExpressionElement<T> : TreeElement, IExpression<T> where T : ITestable
    {
        private bool IsString = false;
        private string LeftName = "??";
        private int LeftId { get; set; }
        private string RightString = "";
        private int RightValue = 0;
        private ConditionOperations operation;
        public ConditionExpressionElement(ConditionElement element, IPropertyFinder finder)
            : base(TreeCode.Expression)
        {
            if (element.Left.Type == LexAnalyzer.TokenTypes.Id)
            {
                LeftName = element.Left.Value.ToUpper();
                LeftId = finder.GetId(LeftName, out IsString);
            }
            else
            {
                throw new SyntacticException("Unexpected token " + element.Operation.ToString());
            }

            if (IsString)
            {
                RightString = element.Right.Value;
                switch (element.Operation.Type)
                {
                    case LexAnalyzer.TokenTypes.Equal:
                        operation = ConditionOperations.Eq;
                        break;
                    default:
                        throw new SyntacticException("Unexpected token " + element.Operation.ToString());
                }
            }
            else
            {
                GetRightNmr(element.Right);
                switch (element.Operation.Type)
                {
                    case LexAnalyzer.TokenTypes.Equal:
                        operation = ConditionOperations.Eq;
                        break;
                    case LexAnalyzer.TokenTypes.GE:
                        operation = ConditionOperations.GE;
                        break;

                    case LexAnalyzer.TokenTypes.Greater:
                        operation = ConditionOperations.Greater;
                        break;
                    case LexAnalyzer.TokenTypes.Less:
                        operation = ConditionOperations.Less;
                        break;
                    case LexAnalyzer.TokenTypes.LE:
                        operation = ConditionOperations.LE;
                        break;
                    case LexAnalyzer.TokenTypes.And:
                        operation = ConditionOperations.And;
                        break;
                    case LexAnalyzer.TokenTypes.Or:
                        operation = ConditionOperations.Or;
                        break;
                    default:
                        throw new SyntacticException("Unexpected token " + element.Operation.ToString());
                }
            }
            IsExpr = true;
        }

        private void GetRightNmr(LexAnalyzer.Token token)
        {
            if (token.Type == LexAnalyzer.TokenTypes.Number)
            {
                RightValue = Convert.ToInt32(token.Value);
            }
            else if (token.Type == LexAnalyzer.TokenTypes.Hex)
            {
                RightValue = Convert.ToInt32(token.Value.Substring(2), 16); // skips 0x prefix
            }
            else
                throw new SyntacticException("Unexpected token " + token.ToString());
        }

        public override string ToString()
        {
            if (IsString)
            {
                return LeftName + " = " + RightString;
            }
            StringBuilder s = new StringBuilder();
            s.Append(LeftName);
            switch (operation)
            {
                case ConditionOperations.And:
                    s.Append("&");
                    break;
                case ConditionOperations.Or:
                    s.Append("|");
                    break;
                case ConditionOperations.Eq:
                    s.Append("=");
                    break;
                case ConditionOperations.Less:
                    s.Append("<");
                    break;
                case ConditionOperations.LE:
                    s.Append("<=");
                    break;
                case ConditionOperations.Greater:
                    s.Append(">");
                    break;
                case ConditionOperations.GE:
                    s.Append(">=");
                    break;
            }
            s.Append(RightValue.ToString());
            return s.ToString();
        }

        public bool TestValidity(T element)
        {
            if (IsString)
            {
                if (operation == ConditionOperations.Eq)
                    return element.GetString(LeftId) == RightString;
                return false;
            }
            else
            {
                int value = element.GetInteger(LeftId);
                switch (operation)
                {
                    case ConditionOperations.And:
                        return (value & RightValue) == RightValue;
                    case ConditionOperations.Or:
                        return (value | RightValue) != 0;
                    case ConditionOperations.Eq:
                        return value == RightValue;
                    case ConditionOperations.Less:
                        return value < RightValue;
                    case ConditionOperations.LE:
                        return value <= RightValue;
                    case ConditionOperations.Greater:
                        return value > RightValue;
                    case ConditionOperations.GE:
                        return value >= RightValue;
                }
                return false;
            }
        }
    }
}
