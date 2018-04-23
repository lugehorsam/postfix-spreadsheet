namespace PostfixSpreadsheet
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     Represents a cell in the spreadsheet. Tokens contain either an operator or an operand in Postfix Notation, or a link to another cell.
    /// </summary>
    internal class PostfixCell
    {
        /// <summary>
        ///     The ideal amount of whitespace between raw cell tokens.
        /// </summary>
        private const char _IDEAL_TOKEN_WHITESPACING = ' ';

        /// <summary>
        /// What to render in the event of unexpected output.
        /// </summary>
        private const string _ERR_STRING = "#ERR";

        /// <summary>
        /// The maximum number of decimal places to render to the user.
        /// Does not impact fidelity of calculations.
        /// </summary>
        private const int _MAX_DECIMAL_PLACES = 3;

        private readonly string _tokens;

        public PostfixCell(string tokens)
        {
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
        ///     Links cells and evaluates tokens according to Postfix Notation. Returns null if this cell or a linked cell was malformed.
        /// </summary>
        private float? Evaluate(PostfixCell[,] allCells)
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
                else if (IsCellLink(token))
                {
                    float? linkedValue = EvaluateCellLink(allCells, token);

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
        /// Returns the value of the cell linked from this token.
        /// Returns null if the link could not be found or was circular.
        /// </summary>
        private float? EvaluateCellLink(PostfixCell[,] allCells, string currLink)
        {
            PostfixCell linkedCell = GetLinkedPostfixCell(allCells, currLink);

            //Avoid an infinite loop of cells referencing one another.
            if (linkedCell == null || linkedCell.LinksToCell(this, allCells))
            {
                return null;
            }

            return linkedCell.Evaluate(allCells);
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
        ///     Given a letter, returns an integer representing the index of the letter in the alphabet. Case insensitive.
        ///     Zero-based.
        /// </summary>
        private static int GetColumnIndexFromLetter(char letter)
        {
            return char.ToUpper(letter) - 65;
        }

        /// <summary>
        ///     Does the provided token attempt to represent a pointer to another cell?
        /// </summary>
        private static bool IsCellLink(string cellToken)
        {
            if (cellToken.Length <= 0)
            {
                return false;
            }

            string firstChar = cellToken[0].ToString();

            MatchCollection letterMatches = Regex.Matches(firstChar, @"[a-zA-Z]");
            return letterMatches.Count > 0;
        }

        /// <summary>
        ///     Determines the row being pointed to by the provided token.
        ///     Token is expected to be prevalidated as a numeric cell pointer.
        /// </summary>
        private static int GetRowIndex(string cellToken)
        {
            int rowIndex;

            if (int.TryParse(cellToken, out rowIndex))
            {
                return rowIndex - 1;
            }

            return -1;
        }
        
        /// <summary>
        ///     Does this cell have a token that points to the specifically provided cell?
        ///     Uses the array as a basis for determining this.
        /// </summary>
        private bool LinksToCell(PostfixCell otherCell, PostfixCell[,] allCells)
        {
            string[] cellElements = CreateCellTokens(_tokens);

            foreach (string cellElement in cellElements)
            {
                if (!IsCellLink(cellElement))
                {
                    continue;
                }

                PostfixCell linkedCell = GetLinkedPostfixCell(allCells, cellElement);
    
                if (linkedCell != null && linkedCell == otherCell)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Returns the cell pointed to by this cell.
        ///     Input must be a syntactically valid cell link, i.e., a single letter succeeded by a number of arbitrary size.
        ///     If the cell link points to a nonexistant cell, returns null.
        /// </summary>
        private static PostfixCell GetLinkedPostfixCell(PostfixCell[,] allCells, string cellLink)
        {
            int column = GetColumnIndexFromLetter(cellLink[0]);
            int row = GetRowIndex(cellLink.Substring(1));
            
            int numRows = allCells.GetLength(0);
            int numCols = allCells.GetLength(1);

            bool linkedCellOutOfRange = (row >= numRows) || (column >= numCols);
            
            return linkedCellOutOfRange ? null : allCells[row, column];
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
        /// Rounds the value to render to the <see cref="_MAX_DECIMAL_PLACES"/>.
        /// </summary>
        private static string SanitizeOutputForRendering(float cellOutput)
        {
            float roundedOutput = (float) Math.Round(cellOutput, _MAX_DECIMAL_PLACES);
            return roundedOutput.ToString();
        }
    }
}
