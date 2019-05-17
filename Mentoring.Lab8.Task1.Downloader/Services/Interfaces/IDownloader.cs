using System.Threading.Tasks;

namespace Mentoring.Lab8.Task1.Downloader.Services
{
    public interface IDownloader
    {
        int MaxDeepLevel { get; }
        Task DownloadAsync(string url);
    }
}
