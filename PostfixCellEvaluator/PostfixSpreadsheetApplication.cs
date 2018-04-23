namespace PostfixSpreadsheet
{
    using System;
    using System.IO;

    /// <summary>
    ///     The entry point into the application. Validates user input and renders the output spreadsheet.
    /// </summary>
    internal class PostfixSpreadsheetApplication
    {
        private const string _ERR_INVALID_INPUT = "Spreadsheet program received invalid arguments. Please refer to documentation for correct program input.";
        private const string _ERR_FILE_PATH = "Could not find file at path: {0}";

        public static void Main(string[] userArgs)
        {
            if (!Validate(userArgs))
            {
                return;
            }

            string fileName = userArgs[0];
            string fullFilePath = GetFullFilePath(fileName);

            string fileContent = File.ReadAllText(fullFilePath);
            var postfixSpreadsheet = new PostfixSpreadsheet(fileContent);
            Console.WriteLine(postfixSpreadsheet.Render());
        }

        /// <summary>
        ///     Ensures that the user arguments are not erroneus and point to the correct resources.
        /// </summary>
        private static bool Validate(string[] userArgs)
        {
            return ValidateArguments(userArgs) && ValidateFileLocation(userArgs);
        }

        /// <summary>
        ///     Did the user provide the right number and format of arguments via the command line?
        /// </summary>
        private static bool ValidateArguments(string[] userArgs)
        {
            bool isValid = (userArgs.Length == 1) && !string.IsNullOrEmpty(userArgs[0]);

            if (!isValid)
            {
                Console.WriteLine(_ERR_INVALID_INPUT);
            }

            return isValid;
        }

        /// <summary>
        ///     Does the user's file exist at the correct path?
        /// </summary>
        private static bool ValidateFileLocation(string[] userArgs)
        {
            string fileName = userArgs[0];
            string filePath = GetFullFilePath(fileName);

            bool fileExists = File.Exists(filePath);

            if (!fileExists)
            {
                Console.WriteLine(_ERR_FILE_PATH, filePath);
            }

            return fileExists;
        }

        /// <summary>
        ///     Returns the absolute path to the input file.
        /// </summary>
        private static string GetFullFilePath(string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), fileName);
        }
    }
}
