using System;

namespace Mentoring.Lab8.Task1.Downloader.Services
{
    public class Logger : ILogger
    {
        private readonly bool _isInfoEnabled;

        public Logger(bool isInfoEnabled = true)
        {
            this._isInfoEnabled = isInfoEnabled;
        }

        public void LogInfo(string message)
        {
            if (_isInfoEnabled)
            {
                Console.WriteLine($"[Info] {message}");
            }
        }

        public void LogError(string message)
        {
            Console.WriteLine($"[Error] {message}");
        }
    }
}
