using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Mentoring.Lab8.Task1.Downloader.Services
{
    public class Saver : ISaver
    {
        private readonly DirectoryInfo _rootDirectoryInfo;
        private readonly ILogger _logger;

        public Saver(string rootDirectory, ILogger logger)
        {
            if (string.IsNullOrEmpty(rootDirectory))
            {
                throw new ArgumentNullException(nameof(rootDirectory));
            }

            if (!Directory.Exists(rootDirectory))
            {
                throw new ArgumentException($"Directory name: {rootDirectory}. Directory not found.");
            }

            _rootDirectoryInfo = new DirectoryInfo(rootDirectory);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task SaveHtmlAsync(Uri uri, HtmlDocument document)
        {
            var directoryPath = GetLocation(uri);
            Directory.CreateDirectory(directoryPath);
            var htmlFileName = GeDocumentFileName(document);
            htmlFileName = GetValidFileName(htmlFileName);

            var htmlFullPath = Path.Combine(directoryPath, htmlFileName);
            using (var documentStream = GetDocumentStream(document))
            {
                try
                {
                    await SaveToFileAsync(htmlFullPath, documentStream);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Url: {uri}, file path: {htmlFullPath}. Failed save file, error message: {e.Message}");
                }
            }
        }

        public async Task SaveFileAsync(Uri uri, Stream fileStream)
        {
            var fileFullPath = GetLocation(uri);

            var directoryPath = Path.GetDirectoryName(fileFullPath);
            Directory.CreateDirectory(directoryPath);

            if (File.Exists(fileFullPath))
            {
                fileFullPath = Path.Combine(fileFullPath, Guid.NewGuid().ToString());
            }

            try
            {
                await SaveToFileAsync(fileFullPath, fileStream);
            }
            catch (Exception e)
            {
                _logger.LogError($"Url: {uri}, file path: {fileFullPath}. Failed save file, error message: {e.Message}");
            }
        }

        private string GetLocation(Uri uri)
        {
            return $"{Path.Combine(_rootDirectoryInfo.FullName, uri.Host)}{uri.LocalPath.Replace("/", @"\")}";
        }

        private async Task SaveToFileAsync(string fileFullPath, Stream fileStream)
        {
            using (var file = File.Create(fileFullPath))
            {
                await fileStream.CopyToAsync(file);
            }
        }

        private string GeDocumentFileName(HtmlDocument document)
        {
            return document.DocumentNode.Descendants("title").FirstOrDefault()?.InnerText + ".html";
        }

        private string GetValidFileName(string filename)
        {
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            var newFileNameChars = filename.Where(c => !invalidFileNameChars.Contains(c))
                .ToArray();

            return new string(newFileNameChars);
        }

        private Stream GetDocumentStream(HtmlDocument document)
        {
            var memoryStream = new MemoryStream();

            document.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }
    }
}
