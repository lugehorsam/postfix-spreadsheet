namespace PostfixSpreadsheet
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     Represents a pointer from one <see cref="PostfixCell" /> cell to another.
    ///     The token it is constructed from must be a single letter succeeded by a number of arbitrary size.
    /// </summary>
    internal class PostfixCellPointer
    {
        /// <summary>
        ///     The column of the cell this pointer is located in.
        /// </summary>
        private readonly int _column;

        private readonly int _pointedColumn;

        private readonly int _pointedRow;

        /// <summary>
        ///     The row of the cell this pointer is located in.
        /// </summary>
        private readonly int _row;

        /// <summary>
        ///     Constructs a pointer from a token. Expects the token to have been validated by <see cref="IsCellPointer" />.
        /// </summary>
        public PostfixCellPointer(int ownerRow, int ownerColumn, string cellToken)
        {
            if (!IsCellPointer(cellToken))
            {
                throw new ArgumentException("Tried to construct cell pointer from invalid token.");
            }

            _row = ownerRow;
            _column = ownerColumn;

            _pointedRow = GetRowFromToken(cellToken.Substring(1));
            _pointedColumn = GetColumnFromToken(cellToken[0]);
        }

        /// <summary>
        ///     The row being pointed to.
        /// </summary>
        public int PointedRow
        {
            get { return _pointedRow; }
        }

        /// <summary>
        ///     The column being pointed to.
        /// </summary>
        public int PointedColumn
        {
            get { return _pointedColumn; }
        }

        /// <summary>
        ///     Returns the value of the cell this points to.
        ///     Returns null if the cell could not be found or was circular.
        /// </summary>
        public float? GetValue(PostfixCell[,] allCells, string linkToken)
        {
            PostfixCell linkedCell = GetCellPointedTo(allCells, linkToken);

            if (linkedCell == null)
            {
                return null;
            }

            bool thisPointsToSelf = (linkedCell.Row == _row) && (linkedCell.Column == _column);
            bool otherPointsToThis = linkedCell.ContainsPointerTo(_row, _column);

            //Avoid infinite loops
            if (otherPointsToThis || thisPointsToSelf)
            {
                return null;
            }

            return linkedCell.Evaluate(allCells);
        }

        /// <summary>
        ///     Does the provided token attempt to represent a pointer to another cell?
        /// </summary>
        public static bool IsCellPointer(string cellToken)
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
        ///     Returns the cell pointed to by this cell. Input must be a syntactically valid cell link.
        ///     If the cell link points to a nonexistant cell, returns null.
        /// </summary>
        private PostfixCell GetCellPointedTo(PostfixCell[,] allCells, string cellPointer)
        {
            int numRows = allCells.GetLength(0);
            int numCols = allCells.GetLength(1);

            bool linkedCellOutOfRange = (_pointedRow >= numRows) || (_pointedColumn >= numCols);

            return linkedCellOutOfRange ? null : allCells[_pointedRow, _pointedColumn];
        }

        /// <summary>
        ///     Given a letter, returns an integer representing the index of the letter in the alphabet.
        ///     Case insensitive. Zero-based.
        /// </summary>
        private static int GetColumnFromToken(char letter)
        {
            return char.ToUpper(letter) - 65;
        }

        /// <summary>
        ///     Determines the row being pointed to by the provided token.
        /// </summary>
        private static int GetRowFromToken(string cellToken)
        {
            int rowIndex;

            if (int.TryParse(cellToken, out rowIndex))
            {
                return rowIndex - 1;
            }

            return -1;
        }
    }
}
