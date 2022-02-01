using System;

namespace JBAExercise.Console
{
    internal class ErrorLog : IDisposable
    {
        private JbaPrecipitationFileReader _reader;

        internal ErrorLog(JbaPrecipitationFileReader reader)
        {
            _reader = reader;
        }

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
