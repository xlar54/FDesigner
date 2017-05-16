using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleExpressionEvaluator
{
    public class ExpressionIterator
    {
        private const string supportedMathematicalOpertors = "+-*/";
        private const string supportedConstructions = "()";
        private const string supportedDecimalParts = ".1234567890 ";

        private bool IsASupportedOperator(char theOperator)
        {
            return supportedMathematicalOpertors.Contains(theOperator.ToString());
        }

        private bool IsASupportedCharacter(char theCharacter)
        {
            return supportedDecimalParts.Contains(theCharacter.ToString());
        }

        private void ValidateExpression(string expression)
        {
            //a check to make sure the expression comprises only numbers, spaces, brackets and supported operators, nothing else
            char[] characters = expression.ToCharArray();
            foreach (char ch in characters)
            {
                if (!Char.IsDigit(ch))
                {
                    if (!supportedMathematicalOpertors.Contains(ch.ToString()))
                    {
                        if (!supportedConstructions.Contains(ch.ToString()))
                        {
                            if (!supportedDecimalParts.Contains(ch.ToString()))
                            {
                                throw new Exception("Unsupported character found in expression: " + ch.ToString());
                            }
                        }
                    }
                }
            }
        }

        public decimal EvaluateStringExpression(string expression)
        {
            decimal result = 0;
            try
            {
                //store either the operator and relevant left and right operand numbers, or evaluate all 3 as a resulting number and store the
                //result as an operand to the next evaluation of the wider context, do until all such expressions are completed and an overall result appears
                ValidateExpression(expression);

                Stack<char> theOperators = new Stack<char>();
                Stack<decimal> numbers = new Stack<decimal>();
                string adjustedForUnaries = AdjustForUnaries(expression);
                char[] items = adjustedForUnaries.ToCharArray();

                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] != ' ') //we are not interested in processing spaces
                    {
                        if (items[i] == '(')
                        {
                            theOperators.Push(items[i]);
                        }
                        else if (items[i] == ')')
                        {
                            while (theOperators.Peek() != '(')
                            {
                                //here we execute the evaluations of all previously stored numbers with their respective operators until
                                //we encounter the next batch of operators and operands on the stack indicated by (
                                numbers.Push(MathematicEvaluation(theOperators.Pop(), numbers.Pop(), numbers.Pop()));
                            }
                            theOperators.Pop();
                        }
                        else
                        if (IsASupportedCharacter(items[i]))
                        {
                            StringBuilder sb = new StringBuilder();
                            //capture all the digits to build the number
                            sb.Append(items[i]);
                            while (i + 1 < items.Length && IsASupportedCharacter(items[i + 1]))
                            {
                                sb.Append(items[i + 1]);
                                i = i + 1;
                            }
                            numbers.Push(Convert.ToDecimal(sb.ToString()));
                        }
                        else
                        if (IsASupportedOperator(items[i]))
                        {
                            //do we need to execute an evaluation because the last encountered operator has higher precedence?
                            while (theOperators.Count > 0 && SecondOperatorHasPrecedence(items[i], theOperators.Peek()))
                            {
                                numbers.Push(MathematicEvaluation(theOperators.Pop(), numbers.Pop(), numbers.Pop()));
                            }
                            theOperators.Push(items[i]);
                        }
                    }
                }

                //apply remaining sets (operand operator operand) 
                while (theOperators.Count > 0)
                {
                    numbers.Push(MathematicEvaluation(theOperators.Pop(), numbers.Pop(), numbers.Pop()));
                }

                result = numbers.Pop();
            }
            catch (Exception)
            {
                throw new Exception("Error in ExpressionIterator.EvaluateStringExpression - expression " + expression + " does not parse!");
            }
            return result;
        }

        private string AdjustForUnaries(string expression)
        {
            //unary minus can appear against any number, but can follow another operator
            //also mathematical meaning of things like -- equating to + need to be taken into account

            string adjustedForUnaries = expression;

            adjustedForUnaries = adjustedForUnaries.Replace("  ", " ");

            adjustedForUnaries = adjustedForUnaries.Replace("+-", "+ 0 -");
            adjustedForUnaries = adjustedForUnaries.Replace("+ -", "+ 0 -");

            adjustedForUnaries = adjustedForUnaries.Replace("--", " + ");
            adjustedForUnaries = adjustedForUnaries.Replace("- -", " + ");

            adjustedForUnaries = FindAndReplaceForUnaries(adjustedForUnaries, "(-");
            adjustedForUnaries = FindAndReplaceForUnaries(adjustedForUnaries, "( -");
            adjustedForUnaries = adjustedForUnaries.Replace("**", "*");
            adjustedForUnaries = adjustedForUnaries.Replace("//", "/");

            adjustedForUnaries = FindAndReplaceForUnaries(adjustedForUnaries, "*-");
            adjustedForUnaries = FindAndReplaceForUnaries(adjustedForUnaries, "* -");
            adjustedForUnaries = FindAndReplaceForUnaries(adjustedForUnaries, "/-");
            adjustedForUnaries = FindAndReplaceForUnaries(adjustedForUnaries, "/ -");

            return adjustedForUnaries;
        }

        private string FindAndReplaceForUnaries(string expression, string supportedUnary)
        {
            //put (0-n) around any unary operator and value found to cope with *-1, * -1, etc
            string result = expression;
            if (result.Contains(supportedUnary))
            {
                int startPoint = result.IndexOf(supportedUnary);
                if (startPoint > 0)
                {
                    int nextChar = startPoint + supportedUnary.Length;
                    string value = string.Empty;
                    StringBuilder sb = new StringBuilder();
                    while (nextChar < result.Length && IsASupportedCharacter(result[nextChar]))
                    {
                        sb.Append(result[nextChar]);
                        nextChar += 1;
                    }
                    value = supportedUnary[0] + "(0-" + sb.ToString() + ")";
                    string newvalue = string.Empty;
                    if (nextChar == result.Length)
                    {
                        newvalue = result.Substring(0, startPoint);
                        newvalue = newvalue + value;
                    }
                    else
                    {
                        newvalue = result.Substring(0, startPoint);
                        newvalue = newvalue + value;
                        if (nextChar < result.Length)
                        {
                            newvalue = newvalue + result.Substring(nextChar);
                        }
                    }
                    result = newvalue;
                }
            }
            return result;
        }

        private bool SecondOperatorHasPrecedence(char operatorA, char operatorB)
        {
            bool result = true;
            if (operatorB == '(' || operatorB == ')')
            {
                result = false;
            }
            else
            if ((operatorA == '*' || operatorA == '/') && (operatorB == '+' || operatorB == '-'))
            {
                result = false;
            }
            return result;
        }


        private decimal MathematicEvaluation(char theOperator, decimal b, decimal a)
        {
            decimal result = 0;
            switch (theOperator)
            {
                case '*':
                    {
                        result = a * b;
                        break;
                    }
                case '/':
                    {
                        if (b == 0)
                        {
                            throw new Exception("Cannot divide by zero");
                        }
                        result = a / b;
                        break;
                    }
                case '+':
                    {
                        result = a + b;
                        break;
                    }
                case '-':
                    {
                        result = a - b;
                        break;
                    }
            }
            return result;
        }
    }
    // Generally - performance - if the ExpressionIterator was to be used by multiple clients, a multi-threaded assembly to accept many simultaneous
    //  requests to resolve expressions could be accommodated with a new instance of the ExpressionIterator class per thread created

    // Extensibility - 

    //  ExpressionIterator currently supports limited operators as per requirements, including unary -, and supports decimal numbers
    //  Extension of support for more operators and construction controllers (the other kinds of mathematical braces and functions other than ())
    //    can be achieved by extension of the three 'supported' constants defined at the top of the ExpressionIterator class, and supporting new code
    //    within the main algorithm that decides what to do with a particular character. To support other mathematical construction symbols within formulae
    //    over and above simple brackets, the main logic of the algorithm would need to be enhanced to support the other construction characters. 
    //    A more generic 'IamEnteringAConstructionBrace' function could be added instead of the hard coded '(', and an accompanying 
    //                   'ClosingConstructionSymbolFound' function could be added instead of the hard coded ')' check. The stack operations of 
    //    Pop, Peek, Push would continue for all nested construction characters in more complex mathematical formulae

    //    Supports expressions of format: 
    //      5*(-2)
    //      (5 + 5) * (3 + 2) - 1
    //      (5 + 5) * (3 + 2) - -1 
    //      (5 + 5.25) * (1.75 + 2.25) - -1
    //      (5 + -5) * (3 + 2) - 1
    //      (6 + 0 ) * (3 * -2) 
    //      (6 *-2 ) * (3 * -2) 

    // Trade-offs? 
    // Solution uses an algorithm that walks through the expression left to right, and nests any bracketed sub-expressions to stack operations
    //  so that execution of (leftParam operator rightParam) succeeds according to mathematical rules of precedence

    // Summary - a bit Heath-Robinson but seems to have some future lifespan and maintainability - though like any algorithm, you gotta know what 
    //   its doing.
}