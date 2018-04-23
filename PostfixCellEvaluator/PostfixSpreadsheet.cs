namespace PostfixSpreadsheet
{
    using System;
    using System.Linq;

    /// <summary>
    ///     Given a string representing a spreadsheet in Postfix Notation, renders a string representing the evaluated
    /// </summary>
    internal class PostfixSpreadsheet
    {
        private readonly string _csvFileContent;

        public PostfixSpreadsheet(string csvFileContent)
        {
            _csvFileContent = csvFileContent;
        }

        /// <summary>
        ///     Returns a string in CSV format containing the evaluated contents of the input file.
        /// </summary>
        public string Render()
        {
            string[,] csvCells = GetCSVCells(_csvFileContent);

            PostfixCell[,] postfixCells = GetPostfixCells(csvCells);

            string fullString = string.Empty;

            int numRows = postfixCells.GetLength(0);
            int numCols = postfixCells.GetLength(1);

            for (var rowIndex = 0; rowIndex < numRows; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < numCols; columnIndex++)
                {
                    string renderedCell = postfixCells[rowIndex, columnIndex].Render(postfixCells);

                    fullString += renderedCell;

                    if (columnIndex != numCols - 1)
                    {
                        fullString += ",";
                    }
                }

                if (rowIndex != numRows - 1)
                {
                    fullString += Environment.NewLine;
                }
            }

            return fullString;
        }

        /// <summary>
        ///     Given a CSV string, returns a two dimensional array of strings, where each cell was separated by a comma in the
        ///     source string.
        /// </summary>
        private string[,] GetCSVCells(string csvFileContent)
        {
            if (string.IsNullOrEmpty(csvFileContent))
            {
                return new string[0, 0];
            }

            string[] csvRows = csvFileContent.Split('\n');

            int maxNumColumns = csvRows.Max(row => row.Split(',').Length);
            return CreateCellGrid(csvRows, maxNumColumns);
        }

        /// <summary>
        ///     Given rows of raw CSV strings and the width of the CSV grid, returns each individually parsed cell.
        ///     If any rows have columns that fall short of the <see cref="maxNumColumns" /> assigns an empty string
        ///     to the remaining undefined cells.
        /// </summary>
        private string[,] CreateCellGrid(string[] csvRows, int maxNumColumns)
        {
            var cellGrid = new string[csvRows.Length, maxNumColumns];

            for (var i = 0; i < csvRows.Length; i++)
            {
                string csvRow = csvRows[i];
                string[] csvColumns = csvRow.Split(',');

                for (var j = 0; j < maxNumColumns; j++)
                {
                    if (j >= csvColumns.Length)
                    {
                        cellGrid[i, j] = string.Empty;
                    }
                    else
                    {
                        cellGrid[i, j] = csvColumns[j];
                    }
                }
            }

            return cellGrid;
        }

        /// <summary>
        ///     Given a multidimensional array of strings, creates a multidimensional array of <see cref="PostfixCell" />.
        /// </summary>
        private static PostfixCell[,] GetPostfixCells(string[,] csvCells)
        {
            int numRows = csvCells.GetLength(0);
            int numCols = csvCells.GetLength(1);

            var postfixCells = new PostfixCell[numRows, numCols];

            for (var rowIndex = 0; rowIndex < numRows; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < numCols; columnIndex++)
                {
                    postfixCells[rowIndex, columnIndex] = new PostfixCell(rowIndex, columnIndex, csvCells[rowIndex, columnIndex]);
                }
            }

            return postfixCells;
        }
    }
}
