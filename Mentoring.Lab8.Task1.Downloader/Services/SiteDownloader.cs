using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ConcurrentCollections;
using HtmlAgilityPack;
using Mentoring.Lab8.Task1.Downloader.Models;

namespace Mentoring.Lab8.Task1.Downloader.Services
{
    public class SiteDownloader : IDownloader
    {
        public const string HtmlMediaType = "text/html";

        private readonly IDictionary<ConstraintType, IEnumerable<IConstraint>> _constraints;
        private readonly ISaver _saver;
        private readonly ILogger _logger;

        public SiteDownloader(IEnumerable<IConstraint> constraints,
            ISaver saver,
            ILogger logger,
            int maxDeepLevel = 0)
        {
            if (constraints == null)
            {
                throw new ArgumentNullException(nameof(constraints));
            }

            _constraints = new Dictionary<ConstraintType, IEnumerable<IConstraint>>
            {
                [ConstraintType.File] = constraints.Where(c => (c.ConstraintType & ConstraintType.File) != 0).ToArray(),
                [ConstraintType.Url] = constraints.Where(c => (c.ConstraintType & ConstraintType.Url) != 0).ToArray()
            };

            _saver = saver ?? throw new ArgumentNullException(nameof(saver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (maxDeepLevel < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDeepLevel));
            }

            MaxDeepLevel = maxDeepLevel;
        }

        public int MaxDeepLevel { get; }

        public async Task DownloadAsync(string url)
        {
            var uri = new Uri(url);

            var downloadedLinks = new ConcurrentHashSet<Uri>();
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = uri;
                await LoadUrlAsync(httpClient, downloadedLinks, httpClient.BaseAddress, 0);
            }
        }

        private async Task LoadUrlAsync(HttpClient client, ConcurrentHashSet<Uri> downloadedLinks, Uri uri, int level)
        {
            if (level > MaxDeepLevel
                || !IsValidScheme(uri.Scheme)
                || !downloadedLinks.Add(uri))
            {
                return;
            }

            HttpResponseMessage headResponse;
            try
            {
                headResponse = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));
            }
            catch (Exception e)
            {
                _logger.LogError($"Not loaded url: {uri}. Error message: {e.Message}");
                return;
            }

            if (!headResponse.IsSuccessStatusCode)
            {
                _logger.LogError($"Not loaded url: {uri}. Status code: {headResponse.StatusCode}");
                return;
            }

            if (headResponse.Content.Headers.ContentType?.MediaType == HtmlMediaType)
            {
                await ProcessHtmlAsync(client, downloadedLinks, uri, level);
            }
            else
            {
                await ProcessFileAsync(client, uri);
            }
        }

        private async Task ProcessHtmlAsync(HttpClient client, ConcurrentHashSet<Uri> downloadedLinks, Uri uri, int level)
        {
            _logger.LogInfo($"Found html: {uri}");

            if (!IsValidUri(uri, ConstraintType.Url))
            {
                return;
            }

            var response = await client.GetAsync(uri);
            var document = new HtmlDocument();
            var htmlStream = await response.Content.ReadAsStreamAsync();
            document.Load(htmlStream, Encoding.UTF8);

            _logger.LogInfo($"Loaded html: {uri}");

            await _saver.SaveHtmlAsync(uri, document);

            var internalLinks = document.DocumentNode.Descendants()
                .SelectMany(d => d.Attributes.Where(IsAttributeWithLink));

            var internalLinkTasks = new List<Task>();
            foreach (var internalLink in internalLinks)
            {
                var internalLinkTask = LoadUrlAsync(client, downloadedLinks, new Uri(client.BaseAddress, internalLink.Value), level + 1);
                internalLinkTasks.Add(internalLinkTask);
            }

            await Task.WhenAll(internalLinkTasks);
        }

        private async Task ProcessFileAsync(HttpClient client, Uri uri)
        {
            _logger.LogInfo($"Found file: {uri}");

            if (!IsValidUri(uri, ConstraintType.File))
            {
                return;
            }

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(uri);
            }
            catch (Exception e)
            {
                _logger.LogError($"Not loaded file: {uri}. Error message: {e.Message}");
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Not loaded file: {uri}. Status code: {response.StatusCode}");
                return;
            }

            _logger.LogInfo($"Loaded file: {uri}");

            var fileStream = await response.Content.ReadAsStreamAsync();
            await _saver.SaveFileAsync(uri, fileStream);
        }

        private bool IsValidScheme(string scheme)
        {
            return "http" == scheme || "https" == scheme;
        }

        private bool IsValidUri(Uri uri, ConstraintType constraintType)
        {
            return _constraints[constraintType].All(c => c.IsValid(uri));
        }

        private bool IsAttributeWithLink(HtmlAttribute attribute)
        {
            return attribute.Name == "src" || attribute.Name == "href";
        }
    }
}
