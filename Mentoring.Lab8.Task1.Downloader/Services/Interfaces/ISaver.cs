using System;
using System.IO;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Mentoring.Lab8.Task1.Downloader.Services
{
    public interface ISaver
    {
        Task SaveHtmlAsync(Uri uri, HtmlDocument document);
        Task SaveFileAsync(Uri uri, Stream fileStream);
    }
}
