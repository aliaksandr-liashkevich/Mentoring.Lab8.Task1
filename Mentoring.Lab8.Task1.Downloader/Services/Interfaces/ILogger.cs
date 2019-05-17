namespace Mentoring.Lab8.Task1.Downloader.Services
{
    public interface ILogger
    {
        void LogInfo(string message);
        void LogError(string message);
    }
}
