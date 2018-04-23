namespace PostfixSpreadsheet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     Represents a cell in the spreadsheet. Tokens contain either an operator or an operand in Postfix Notation, or a
    ///     link to another cell.
    /// </summary>
    internal class PostfixCell
    {
        /// <summary>
        ///     The ideal amount of whitespace between raw cell tokens.
        /// </summary>
        private const char _IDEAL_TOKEN_WHITESPACING = ' ';

        /// <summary>
        ///     What to render in the event of unexpected output.
        /// </summary>
        private const string _ERR_STRING = "#ERR";

        /// <summary>
        ///     The maximum number of decimal places to render to the user.
        ///     Does not impact fidelity of calculations.
        /// </summary>
        private const int _MAX_DECIMAL_PLACES = 3;

        private readonly int _column;

        private readonly int _row;

        private readonly string _tokens;

        public PostfixCell(int row, int column, string tokens)
        {
            _row = row;
            _column = column;
            _tokens = tokens;
        }

        public int Row
        {
            get { return _row; }
        }

        public int Column
        {
            get { return _column; }
        }

        /// <summary>
        ///     Given an array of all the cells on the spreadsheet, returns a string representing how this cell should appear.
        /// </summary>
        public string Render(PostfixCell[,] allCells)
        {
            float? evaluatedResult = Evaluate(allCells);
            return evaluatedResult.HasValue ? SanitizeOutputForRendering(evaluatedResult.Value) : _ERR_STRING;
        }

        /// <summary>
        ///     Links cells and evaluates tokens according to Postfix Notation. Returns null if this cell or a linked cell was
        ///     malformed.
        /// </summary>
        public float? Evaluate(PostfixCell[,] allCells)
        {
            string[] cellTokens = CreateCellTokens(_tokens);

            if (cellTokens.Length == 0)
            {
                return 0; //just render an empty cell as a zero.
            }

            var operandStack = new Stack<float>();

            foreach (string token in cellTokens)
            {
                float operand;

                if (float.TryParse(token, out operand))
                {
                    operandStack.Push(operand);
                }
                else if (!TryPushCellPointerToStack(token, operandStack, allCells) && !TryPushOperatorToStack(token, operandStack))
                {
                    return null;
                }
            }

            if (operandStack.Count != 1)
            {
                return null;
            }

            return operandStack.Pop();
        }

        /// <summary>
        ///     Does this cell contain tokens that point to the provided row and column?
        /// </summary>
        public bool ContainsPointerTo(int row, int column)
        {
            string[] tokens = CreateCellTokens(_tokens);
            return tokens.Any(token => IsTokenPointerTo(token, row, column));
        }

        /// <summary>
        ///     Tries to push an operator to the stack of operands.
        ///     Returns true if the token was recognized as an operator and pushed to the stack.
        ///     Else, returns false.
        /// </summary>
        private bool TryPushOperatorToStack(string token, Stack<float> operandStack)
        {
            if (PostfixUtility.IsValidOperator(token))
            {
                if (operandStack.Count >= 2)
                {
                    float rightOperand = operandStack.Pop();
                    float leftOperand = operandStack.Pop();
                    operandStack.Push(PostfixUtility.Evaluate(leftOperand, rightOperand, token));
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Tries to push a cell pointer to the stack of operands.
        ///     Returns true if the token was recognized as a valid cell pointer and pushed to the stack.
        ///     Else, returns false.
        /// </summary>
        private bool TryPushCellPointerToStack(string token, Stack<float> operandStack, PostfixCell[,] allCells)
        {
            if (PostfixCellPointer.IsCellPointer(token))
            {
                var cellPointer = new PostfixCellPointer(_row, _column, token);

                float? linkedValue = cellPointer.GetValue(allCells, token);
                if (linkedValue.HasValue)
                {
                    operandStack.Push(linkedValue.Value);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Returns true if the token is a valid cell pointer to the provided row and column.
        ///     Else returns false.
        /// </summary>
        private bool IsTokenPointerTo(string token, int row, int column)
        {
            if (!PostfixCellPointer.IsCellPointer(token))
            {
                return false;
            }

            var pointer = new PostfixCellPointer(_row, _column, token);

            return (pointer.PointedRow == row) && (pointer.PointedColumn == column);
        }

        /// <summary>
        ///     Given a string representing a single CSV cell, returns individual items separated by an arbitrary amount of
        ///     whitespace within that cell.
        /// </summary>
        private static string[] CreateCellTokens(string rawContent)
        {
            rawContent = SanitizeRawCellInput(rawContent);
            return rawContent.Split(new[] {_IDEAL_TOKEN_WHITESPACING}, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        ///     Trims excess (more than one) spaces from the raw cell content.
        /// </summary>
        private static string SanitizeRawCellInput(string rawCellInput)
        {
            var whitespaceMatcher = new Regex(@"\s+");
            return whitespaceMatcher.Replace(rawCellInput, _IDEAL_TOKEN_WHITESPACING.ToString());
        }

        /// <summary>
        ///     Rounds the value to render to the <see cref="_MAX_DECIMAL_PLACES" />.
        /// </summary>
        private static string SanitizeOutputForRendering(float cellOutput)
        {
            var roundedOutput = (float) Math.Round(cellOutput, _MAX_DECIMAL_PLACES);
            return roundedOutput.ToString();
        }
    }
}
