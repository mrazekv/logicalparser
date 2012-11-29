using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Parser
{
    public class LLAnalyzer<T> where T : ITestable
    {
        private Queue<TreeElement> InputQueue = new Queue<TreeElement>();
        private Queue<LexAnalyzer.Token> TokenQueue = new Queue<LexAnalyzer.Token>();
        private Stack<TreeElement> LLStack = new Stack<TreeElement>();

        private IPropertyFinder finder;

        /// <summary>
        /// Create new syntactic analyzer
        /// </summary>
        /// <param name="lex">Lexical analyzer output</param>
        /// <param name="finder">Table with information about variables</param>
        public LLAnalyzer(LexAnalyzer lex, IPropertyFinder finder)
        {
            this.finder = finder;
            this.TokenQueue = lex.Tokens;
            CreateInputQueue();
        }

        /// <summary>
        /// Parses tokens to input queue
        /// </summary>
        private void CreateInputQueue()
        {
            while (TokenQueue.Count > 0)
            {
                PushIntoInput(TokenQueue.Dequeue());
            }
        }

        /// <summary>
        /// Parse token to element
        /// </summary>
        /// <param name="t">Token to parse</param>
        private void PushIntoInput(LexAnalyzer.Token t)
        {
            switch (t.Type)
            {
                case LexAnalyzer.TokenTypes.Id:
                    string upper = t.Value.ToUpper();
                    switch (upper) /* Keywords parses to specials treeelements, in other case parse it as condition */
                    {
                        case "AND":
                            InputQueue.Enqueue(new TreeElement(TreeCode.And));
                            break;
                        case "OR":
                            InputQueue.Enqueue(new TreeElement(TreeCode.Or));
                            break;
                        case "NOT":
                            InputQueue.Enqueue(new TreeElement(TreeCode.Not));
                            break;
                        default:
                            ParseCondition(t);
                            break;
                    }
                    break;
                case LexAnalyzer.TokenTypes.LBracket:
                    InputQueue.Enqueue(new TreeElement(TreeCode.LBracket));
                    break;

                case LexAnalyzer.TokenTypes.RBracket:
                    InputQueue.Enqueue(new TreeElement(TreeCode.RBracket));
                    break;
                case LexAnalyzer.TokenTypes.End:
                    InputQueue.Enqueue(new TreeElement(TreeCode.End));
                    break;
                default:
                    throw new SyntacticException("Unexpected token " + t.Type.ToString());
            }
        }

        /// <summary>
        /// Parsing condition in format [id] [operation] [id]
        /// If you want another modification, you may change this function
        /// </summary>
        /// <param name="left"></param>
        private void ParseCondition(LexAnalyzer.Token left)
        {
            var operation = TokenQueue.Dequeue();
            var right = TokenQueue.Dequeue();
            InputQueue.Enqueue(new ConditionElement(left, operation, right));
        }


        
       /// <summary>
       /// Posible operation in LL table
       /// </summary>
        private enum LLOperation { In, Out, Eq, NotNot, NA };

        /// <summary>
        /// Construct tree
        /// </summary>
        public IExpression<T> Analyze()
        {
            LLStack.Push(new TreeElement(TreeCode.End));

            var actual = InputQueue.Dequeue();
            TreeElement term = LLStack.Peek();
            while (actual.TreeCode != TreeCode.End || term.TreeCode!=TreeCode.End)
            {
                LLOperation operation = GetLLOperation(GetStackTop(), actual);
                switch (operation)
                {
                    case LLOperation.In:
                        /* Mark element after nearest term */
                        if (term == LLStack.First()) // Last element is terminal
                        {
                            actual.IsMarked = true;
                        }
                        else
                        {
                            TreeElement last = null;
                            foreach (var v in LLStack)
                            {
                                if (v == term)
                                    break;
                                last = v;
                            }
                            last.IsMarked = true;
                        }
                        /* Insert current symbol */
                        LLStack.Push(actual);
                        /* Read next symbol */
                        actual = InputQueue.Dequeue();
                        break;
                    case LLOperation.Eq:
                        /* Insert actual to the top */
                        LLStack.Push(actual);
                        /* Read next symbol */
                        actual = InputQueue.Dequeue();
                        break;
                    case LLOperation.Out:
                        PopOperation(actual);
                        break;
                    case LLOperation.NotNot:
                        /* 2x not - remove them */
                        LLStack.Pop();
                        /* Read next symbol */
                        actual = InputQueue.Dequeue();
                        break;
                    case LLOperation.NA:
                        throw new SyntacticException();
                }
                term = LLStack.Where((x) => { return !x.IsExpr; }).First();
            }
            return LLStack.Pop() as IExpression<T>;
        }

        private void PopOperation(TreeElement actual)
        {
            var expr = PopExpression();
            int len = expr.Count;

            /* ( E ) */
            if (len == 3 && expr[0].TreeCode == TreeCode.LBracket && expr[1].TreeCode == TreeCode.Expression && expr[2].TreeCode == TreeCode.RBracket)
            {
                    expr[1].IsMarked = false;
                    LLStack.Push(expr[1]);
                return;
            }

            /* c */
            else if (len == 1 && expr[0].TreeCode == TreeCode.Condition)
            {
                LLStack.Push(new ConditionExpressionElement<T>(expr[0] as ConditionElement, this.finder));
            }

            /* a OR b */
            else if (len == 3 && expr[0].TreeCode == TreeCode.Expression && expr[1].TreeCode == TreeCode.Or && expr[2].TreeCode == TreeCode.Expression)
            {
                LLStack.Push(new LogicalElement<T>(expr[0] as IExpression<T>, Operations.Or, expr[2] as IExpression<T>));
            }

                
            /* a AND b */
            else if (len == 3 && expr[0].TreeCode == TreeCode.Expression && expr[1].TreeCode == TreeCode.And && expr[2].TreeCode == TreeCode.Expression)
            {
                LLStack.Push(new LogicalElement<T>(expr[0] as IExpression<T>, Operations.And, expr[2] as IExpression<T>));
            }

            /* not A */
            else if (len == 2 && expr[0].TreeCode == TreeCode.Not && expr[1].TreeCode == TreeCode.Expression)
            {
                LLStack.Push(new NotExpressionElement<T>(expr[1] as IExpression<T>));            
            }

            else
            {
                throw new SyntacticException();
            }


        }

        /// <summary>
        /// Recursive fill expression from stack
        /// </summary>
        /// <param name="actual"></param>
        /// <returns></returns>
        private List<TreeElement> PopExpression()
        {
            TreeElement peek = LLStack.Pop();
            List<TreeElement> ret;

            if (peek.IsMarked)
            {
                ret = new List<TreeElement>();
                ret.Add(peek);
            }
            else
            {
                ret = PopExpression();
                ret.Add(peek);
            }
            return ret;
        }



        /**
         * 	    or	and 	not	(	)	c	$
         * or	>	<	    <	<	>	<	>
         * and	>	>	    <	<	>	<	>
         * not	>	>   	-	<	>	<	>
         * (	<	<   	<	<	=	<	
         * )	>	>	    >		>		>
         * c	>	>	    >		>		>
         * $	<	<   	<	<		<	
        */
        private LLOperation[][] LLTable = new LLOperation[][] {            
            new LLOperation[] {LLOperation.Out,LLOperation.In,LLOperation.In,LLOperation.In,LLOperation.Out,LLOperation.In,LLOperation.Out}, // Or
            new LLOperation[] {LLOperation.Out,LLOperation.Out,LLOperation.In,LLOperation.In,LLOperation.Out,LLOperation.In,LLOperation.Out}, // And
            new LLOperation[] {LLOperation.Out,LLOperation.Out,LLOperation.NotNot,LLOperation.In,LLOperation.Out,LLOperation.In,LLOperation.Out}, // Not
            new LLOperation[] {LLOperation.In,LLOperation.In,LLOperation.In,LLOperation.In,LLOperation.Eq,LLOperation.In, LLOperation.NA }, // (
            new LLOperation[] {LLOperation.Out,LLOperation.Out,LLOperation.Out,LLOperation.NA ,LLOperation.Out, LLOperation.NA,LLOperation.Out}, // )
            new LLOperation[] {LLOperation.Out,LLOperation.Out,LLOperation.Out, LLOperation.NA,LLOperation.Out,LLOperation.NA ,LLOperation.Out}, // condition
            new LLOperation[] {LLOperation.In,LLOperation.In,LLOperation.In,LLOperation.In,LLOperation.NA ,LLOperation.In,LLOperation.NA }  // $
        };
        private LLOperation GetLLOperation(TreeElement stack, TreeElement actual)
        {
            Debug.WriteLine("Comparation st: " + stack.ToString() + " , actual= " + actual.ToString());
            int tc = (int)stack.TreeCode;
            int ac = (int)actual.TreeCode;
            if (tc < 0 || tc > (int)TreeCode.End || ac < 0 || ac > (int)TreeCode.End)
                throw new SyntacticException();

            return LLTable[tc][ac];
        }

        /// <summary>
        /// Gets first symbol from stack which is not marked and is not expression
        /// </summary>
        /// <returns></returns>
        private TreeElement GetStackTop()
        {
            return LLStack.Where(x => { return !x.IsExpr; }).First();
        }
    }

    [Serializable]
    public class SyntacticException : Exception
    {
        public SyntacticException() { }
        public SyntacticException(string message) : base(message) { }
        public SyntacticException(string message, Exception inner) : base(message, inner) { }
        protected SyntacticException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
