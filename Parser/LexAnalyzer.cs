using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Parser
{
    public class LexAnalyzer
    {
        public enum TokenTypes { Equal, Less, Greater, LE, GE, Or, And, Id, Number, LBracket, RBracket, End, Hex };
        public struct Token
        {
            public string Value;
            public TokenTypes Type;
            public Token(TokenTypes t)
            {
                this.Type = t;
                this.Value = "";
            }
            public Token(TokenTypes t, string v)
            {
                this.Type = t;
                this.Value = v;
            }

        }
        private enum states { START, TEXT, ZERO, HEX, NMR, GE, LE };

        public Queue<Token> Tokens = new Queue<Token>();

        public void Parse(string p)
        {
            bool moveNext;
            p += " "; // adds space to accept last terminal
            int index = 0;
            char[] data = p.ToCharArray();
            int max = data.Length;
            states state = states.START;
            string buffer = "";

            while (index < max)
            {
                moveNext = true;
                char c = data[index];

                switch (state)
                {
                    case states.START:
                        if (c == '(')
                        {
                            Debug.WriteLine("Token (");
                            Tokens.Enqueue(new Token(TokenTypes.LBracket));

                        }
                        else if (c == ')')
                        {
                            Debug.WriteLine("Token )");
                            Tokens.Enqueue(new Token(TokenTypes.RBracket));
                        }
                        else if (c == '=')
                        {
                            Debug.WriteLine("Token =");
                            Tokens.Enqueue(new Token(TokenTypes.Equal));
                        }
                        else if (char.IsLetter(c))
                        {
                            buffer = c.ToString();
                            state = states.TEXT;
                        }
                        else if (c == '0')
                        {
                            buffer = c.ToString();
                            state = states.ZERO;
                        }
                        else if (char.IsNumber(c))
                        {
                            buffer = c.ToString();
                            state = states.NMR;
                        }
                        else if (c == '>')
                        {
                            state = states.GE;
                        }
                        else if (c == '<')
                        {
                            state = states.LE;
                        }
                        else if (char.IsWhiteSpace(c))
                        {

                        }
                        else
                        {
                            throw new FormatException("LEX: Unexpected char " + c.ToString());
                        }
                        break;
                    case states.TEXT:
                        if (char.IsLetterOrDigit(c))
                        {
                            buffer += c.ToString();
                        }
                        else
                        {
                            Debug.WriteLine(string.Format("Token \"{0}\"", buffer));
                            Tokens.Enqueue(new Token(TokenTypes.Id, buffer));
                            state = states.START;
                            moveNext = false;
                        }
                        break;
                    case states.ZERO:
                        if (char.IsDigit(c))
                        {
                            buffer += c.ToString();
                            state = states.NMR;
                        }
                        else if (c == 'x' || c == 'X')
                        {
                            buffer += c.ToString();
                            state = states.HEX;
                        }
                        else
                        {
                            Debug.WriteLine("Token NMR " + buffer);
                            Tokens.Enqueue(new Token(TokenTypes.Number, buffer));
                            state = states.START;
                            moveNext = false;
                        }
                        break;
                    case states.HEX:
                        if (char.IsDigit(c))
                        {
                            buffer += c.ToString();
                        }
                        else
                        {
                            Debug.WriteLine("Token hexNMR " + buffer);
                            Tokens.Enqueue(new Token(TokenTypes.Hex, buffer));
                            state = states.START;
                            moveNext = false;
                        }
                        break;
                    case states.NMR:
                        if (char.IsDigit(c))
                        {
                            buffer += c.ToString();
                        }
                        else
                        {
                            Tokens.Enqueue(new Token(TokenTypes.Number, buffer));
                            state = states.START;
                            moveNext = false;
                        }
                        break;
                    case states.GE:
                        if (c == '=')
                        {
                            Tokens.Enqueue(new Token(TokenTypes.GE));
                            state = states.START;
                        }
                        else
                        {
                            Tokens.Enqueue(new Token(TokenTypes.Greater));
                            state = states.START;
                            moveNext = false;

                        }
                        break;
                    case states.LE:
                        if (c == '=')
                        {
                            Tokens.Enqueue(new Token(TokenTypes.LE));
                            state = states.START;
                        }
                        else
                        {
                            Tokens.Enqueue(new Token(TokenTypes.Less));
                            state = states.START;
                            moveNext = false;

                        }
                        break;
                }



                if (moveNext)
                    index++;
            }

            Tokens.Enqueue(new Token(TokenTypes.End));
        }


    }
}
