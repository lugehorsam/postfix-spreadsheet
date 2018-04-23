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

        private readonly string _tokens;

        private readonly int _row;
        private readonly int _column;

        public int Row
        {
            get { return _row; }
        }

        public int Column
        {
            get { return _column; }
        }

        public PostfixCell(int row, int column, string tokens)
        {
            _row = row;
            _column = column;
            _tokens = tokens;
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

            var currentTokens = new Stack<float>();

            foreach (string token in cellTokens)
            {
                float operand;

                if (float.TryParse(token, out operand))
                {
                    currentTokens.Push(operand);
                }
                else if (PostfixCellPointer.IsCellPointer(token))
                {
                    var cellPointer = new PostfixCellPointer(_row, _column, token);
                    
                    float? linkedValue = cellPointer.GetValue(allCells, token);

                    if (linkedValue.HasValue)
                    {
                        currentTokens.Push(linkedValue.Value);
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (PostfixUtility.IsValidOperator(token))
                {
                    if (currentTokens.Count >= 2)
                    {
                        float rightOperand = currentTokens.Pop();
                        float leftOperand = currentTokens.Pop();
                        currentTokens.Push(PostfixUtility.Evaluate(leftOperand, rightOperand, token));
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            if (currentTokens.Count != 1)
            {
                return null;
            }

            return currentTokens.Pop();
        }

        /// <summary>
        /// Does this cell contain tokens that point to the provided row and column?
        /// </summary>
        public bool ContainsPointerTo(int row, int column)
        {
            string[] tokens = CreateCellTokens(_tokens);
            return tokens.Any(token => IsTokenPointerTo(token, row, column));
        }

        private bool IsTokenPointerTo(string token, int row, int column)
        {
            if (!PostfixCellPointer.IsCellPointer(token))
            {
                return false;
            }
                
            var pointer = new PostfixCellPointer(_row, _column, token);

            return pointer.PointedRow == row && pointer.PointedColumn == column;
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
