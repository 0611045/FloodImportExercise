using System;

namespace JBAExercise.Console
{
    /// <summary>
    /// Error log. Just prints errors to the Console.
    /// </summary>
    internal class ErrorLog : IDisposable
    {
        private JbaPrecipitationFileReader _reader;

        internal ErrorLog(JbaPrecipitationFileReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// Write to the log.
        /// </summary>
        /// <param name="message"></param>
        internal void Report(string message)
        {
            System.Console.WriteLine($"Error: Line Number: {_reader.LineNumber} | {message} | Raw: {_reader.CurrentLine}");
        }

        public void Dispose()
        {
            _reader = null;
        }
    }
}
