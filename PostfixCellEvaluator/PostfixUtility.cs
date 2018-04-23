namespace PostfixSpreadsheet
{
    using System;

    /// <summary>
    ///    Utility for evaluating Reverse Polish Notation.
    /// </summary>
    internal static class PostfixUtility
    {
        /// <summary>
        ///     Operators recognized by this postfix expression.
        /// </summary>
        private const string _PLUS_OPERATOR = "+";
        private const string _MINUS_OPPERATOR = "-";
        private const string _DIVISION_OPERATOR = "/";
        private const string _MULTIPLICATION_OPERATOR = "*";

        private static readonly string[] _validOperators = {_PLUS_OPERATOR, _MINUS_OPPERATOR, _DIVISION_OPERATOR, _MULTIPLICATION_OPERATOR};

        /// <summary>
        ///     Is the operator recognized by this postfix expression?
        /// </summary>
        public static bool IsValidOperator(string argument)
        {
            return Array.IndexOf(_validOperators, argument) >= 0;
        }

        /// <summary>
        ///     Computes the value of the expression.
        /// </summary>
        public static float Evaluate(float leftOperand, float rightOperand, string @operator)
        {
            switch (@operator)
            {
                case _PLUS_OPERATOR:
                    return leftOperand + rightOperand;
                case _MINUS_OPPERATOR:
                    return leftOperand - rightOperand;
                case _DIVISION_OPERATOR:
                    return leftOperand / rightOperand;
                case _MULTIPLICATION_OPERATOR:
                    return leftOperand * rightOperand;
                default:
                    throw new ArgumentException("Could not evaluate postfix expression. Did not recognize operator: " + @operator);
            }
        }
    }
}
