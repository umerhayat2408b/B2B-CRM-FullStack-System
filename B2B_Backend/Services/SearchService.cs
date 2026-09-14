using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace B2B_PRO.Services
{
    public class SearchService
    {
        private readonly HttpClient _httpClient;

        private readonly Random _random = new Random();

        private readonly List<string> _userAgents = new()
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/137 Safari/537.36",

            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:139.0) Gecko/20100101 Firefox/139.0",

            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Edg/137.0.3296.52",

            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 Version/17.5 Safari/605.1.15",

            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 Chrome/136 Safari/537.36"
        };

        private readonly List<string> _referers = new()
        {
            "https://www.google.com/",
            "https://www.bing.com/",
            "https://duckduckgo.com/",
            "https://search.yahoo.com/",
            "https://www.ecosia.org/"
        };

        public SearchService()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression =
                    DecompressionMethods.GZip |
                    DecompressionMethods.Deflate |
                    DecompressionMethods.Brotli,

                AllowAutoRedirect = true,

                UseCookies = true,

                CookieContainer = new CookieContainer()
            };

            _httpClient = new HttpClient(handler);

            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            ApplyRandomHeaders();
        }

        private void ApplyRandomHeaders()
        {
            _httpClient.DefaultRequestHeaders.Clear();

            string ua =
                _userAgents[_random.Next(_userAgents.Count)];

            string referer =
                _referers[_random.Next(_referers.Count)];

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", ua);

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", referer);

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "Accept-Language",
                "en-US,en;q=0.9");

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "Cache-Control",
                "no-cache");

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
                "Pragma",
                "no-cache");
        }

        private async Task RandomDelay()
        {
            await Task.Delay(_random.Next(1500, 4000));
        }

        private async Task<string> DownloadPage(string url)
        {
            for (int attempt = 1; attempt <= 4; attempt++)
            {
                try
                {
         

                    Console.WriteLine("--------------------------------");
                    Console.WriteLine("Downloading:");
                    Console.WriteLine(url);
                    Console.WriteLine($"Attempt : {attempt}");

                    var response =
                        await _httpClient.GetAsync(url);

                    Console.WriteLine($"Status : {(int)response.StatusCode}");

                    if (!response.IsSuccessStatusCode)
                    {
                        await RandomDelay();
                        continue;
                    }

                    string html =
                        await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrWhiteSpace(html))
                    {
                        Console.WriteLine($"HTML Length : {html.Length}");

                        return html;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                await RandomDelay();
            }

            Console.WriteLine("FAILED : " + url);

            return "";
        }
        // =========================================
        // VALIDATE ORGANIC LINK
        // =========================================

        private bool IsValidOrganicLink(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return false;

            string lower = url.ToLower();

            string[] blocked =
            {
                "google.",
                "bing.",
                "duckduckgo.",
                "yahoo.",
                "facebook.",
                "instagram.",
                "linkedin.",
                "youtube.",
                "twitter.",
                "x.com",
                "reddit.",
                "quora.",
                "wikipedia.",
                "github.",
                "stackoverflow.",
                "amazon.",
                "blog.",
                "support.",
                "docs.",
                "community.",
                "cloudflare.",
                "buttondown.",
                "substack.",
                "medium.",
                "wordpress.",
                "blogspot."
            };

            foreach (var item in blocked)
            {
                if (lower.Contains(item))
                    return false;
            }

            string[] extensions =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".gif",
                ".svg",
                ".css",
                ".js",
                ".xml",
                ".pdf",
                ".zip",
                ".rar",
                ".exe"
            };

            foreach (var ext in extensions)
            {
                if (lower.EndsWith(ext))
                    return false;
            }

            return true;
        }

        // =========================================
        // CLEAN DOMAIN
        // =========================================

        private string CleanToDomainOnly(string url)
        {
            try
            {
                Uri uri = new Uri(url);

                return uri.Scheme + "://" + uri.Host.ToLower();
            }
            catch
            {
                return "";
            }
        }



        // =========================================
        // PARSE HTML
        // =========================================

        private HtmlDocument ParseHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();

            doc.OptionFixNestedTags = true;

            doc.LoadHtml(html);

            return doc;
        }

        // =========================================
        // EXTRACT LINKS
        // =========================================

        private IEnumerable<string> ExtractLinks(HtmlDocument doc)
        {
            var nodes =
                doc.DocumentNode.SelectNodes("//a[@href]");
            Console.WriteLine("Nodes = " + (nodes?.Count ?? 0));

            if (nodes == null)
                yield break;

            foreach (var node in nodes)
            {
                string href =
                    node.GetAttributeValue("href", "");

                if (IsValidOrganicLink(href))
                    yield return href;
            }
        }
        private async Task<List<string>> SearchBing(string keyword)
        {
            HashSet<string> websites =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int[] pages =
            {
1,11,21
};

            foreach (int page in pages)
            {
                try
                {
                    string url =
                        $"https://www.bing.com/search?q={Uri.EscapeDataString(keyword)}&first={page}";

                    Console.WriteLine("BING : " + url);

                    string html = await DownloadPage(url);
                    System.Diagnostics.Debug.WriteLine("--------------------------------");
                    System.Diagnostics.Debug.WriteLine(html.Contains("b_algo"));
                    System.Diagnostics.Debug.WriteLine(html.Contains("captcha"));
                    System.Diagnostics.Debug.WriteLine(html.Contains("verify"));
                    System.Diagnostics.Debug.WriteLine(html.Contains("Our systems have detected"));
                    System.Diagnostics.Debug.WriteLine("--------------------------------"); 

                    if (string.IsNullOrWhiteSpace(html))
                        continue;

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var nodes = doc.DocumentNode.SelectNodes(
                        "//li[contains(@class,'b_algo')]//h2/a");
                    // Color change karein taake fauran nazar aaye
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("=================================");
                    Console.WriteLine("Nodes = " + (nodes?.Count ?? 0));
                    Console.WriteLine("=================================");
                    Console.ResetColor(); // Color wapas normal karne ke liye

                    if (nodes == null)
                        continue;

                    foreach (var node in nodes)
                    {
                        string link = node.GetAttributeValue("href", "");

                        if (!IsValidOrganicLink(link))
                            continue;

                        websites.Add(link);

                        Console.WriteLine("BING FOUND : " + link);
                    }

                    // Backup selector
                    var backup = doc.DocumentNode.SelectNodes("//h2/a");

                    if (backup != null)
                    {
                        foreach (var node in backup)
                        {
                            string link = node.GetAttributeValue("href", "");

                            if (!IsValidOrganicLink(link))
                                continue;

                            websites.Add(link);
                        }
                    }

                    await Task.Delay(_random.Next(1500, 3500));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("BING ERROR");
                    Console.WriteLine(ex.Message);
                }
            }

            Console.WriteLine("BING TOTAL : " + websites.Count);

            return websites.ToList();
        }
        private async Task<List<string>> SearchDuckDuckGo(string keyword)
        {
            HashSet<string> websites =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int page = 0; page <= 60; page += 30)
            {
                try
                {
                    string url =
                        $"https://html.duckduckgo.com/html/?q={Uri.EscapeDataString(keyword)}&s={page}";

                    Console.WriteLine("DUCKDUCKGO : " + url);

                    string html = await DownloadPage(url);

                    if (string.IsNullOrWhiteSpace(html))
                        continue;

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var nodes =
                        doc.DocumentNode.SelectNodes("//a[contains(@class,'result__a')]");

                    if (nodes == null)
                        continue;

                    foreach (var node in nodes)
                    {
                        string href = node.GetAttributeValue("href", "");

                        if (href.Contains("uddg="))
                        {
                            int index = href.IndexOf("uddg=");

                            if (index >= 0)
                            {
                                href = href.Substring(index + 5);
                                href = Uri.UnescapeDataString(href);
                            }
                        }

                        if (!IsValidOrganicLink(href))
                            continue;

                        websites.Add(href);

                        Console.WriteLine("DDG FOUND : " + href);
                    }

                    // Backup selector
                    var backup =
                        doc.DocumentNode.SelectNodes("//a[@href]");

                    if (backup != null)
                    {
                        foreach (var node in backup)
                        {
                            string href = node.GetAttributeValue("href", "");

                            if (href.Contains("uddg="))
                            {
                                int index = href.IndexOf("uddg=");

                                if (index >= 0)
                                {
                                    href = href.Substring(index + 5);
                                    href = Uri.UnescapeDataString(href);
                                }
                            }

                            if (!IsValidOrganicLink(href))
                                continue;

                            websites.Add(href);
                        }
                    }

                    await Task.Delay(_random.Next(1500, 3500));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DUCKDUCKGO ERROR");
                    Console.WriteLine(ex.Message);
                }
            }

            Console.WriteLine("DUCKDUCKGO TOTAL : " + websites.Count);

            return websites.ToList();
        }
        // =========================================
        // MAIN SEARCH METHOD
        // =========================================

        public async Task<List<string>> SearchWebsites(string keyword)
        {
            Console.WriteLine("====================================");
            Console.WriteLine("SEARCH STARTED : " + keyword);
            Console.WriteLine("====================================");

            HashSet<string> websites =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Multiple Search Queries
            List<string> queries = new()
{
    keyword,
    keyword + " Pakistan",
    keyword + " official website"
};

            foreach (var q in queries)
            {
                Console.WriteLine("QUERY : " + q);

                var bing = await SearchBing(q);
                foreach (var item in bing)
                    websites.Add(item);

                var ddg = await SearchDuckDuckGo(q);
                foreach (var item in ddg)
                    websites.Add(item);

                await Task.Delay(Random.Shared.Next(1500, 3000));
            }
            Console.WriteLine($"RAW LINKS : {websites.Count}");

            List<string> verified = new();

            foreach (var url in websites)
            {
                string domain = CleanToDomainOnly(url);

                if (string.IsNullOrWhiteSpace(domain))
                    continue;

                bool ok = await VerifyWebsite(domain);

                if (ok)
                {
                    verified.Add(domain);
                    Console.WriteLine("VALID : " + domain);
                }

                await Task.Delay(Random.Shared.Next(300, 700));
            }

            verified = verified
                .Distinct()
                .OrderBy(x => x)
                .Take(600)
                .ToList();

            Console.WriteLine("====================================");
            Console.WriteLine($"FINAL WEBSITES : {verified.Count}");
            Console.WriteLine("====================================");

            foreach (var item in verified)
                Console.WriteLine(item);

            return verified;
        }
        // =========================================
        // VERIFY WEBSITE (SMART)
        // =========================================

        private async Task<bool> VerifyWebsite(string url)
        {
            try
            {
                // Pehle HEAD request
                var headRequest = new HttpRequestMessage(HttpMethod.Head, url);

                var headResponse = await _httpClient.SendAsync(headRequest);

                if (headResponse.IsSuccessStatusCode)
                    return true;
            }
            catch
            {
            }

            try
            {
                // Agar HEAD fail ho to GET request
                var getResponse = await _httpClient.GetAsync(url);

                if (getResponse.IsSuccessStatusCode)
                    return true;
            }
            catch
            {
            }

            try
            {
                // www add karke try kare
                if (!url.Contains("://www."))
                {
                    Uri uri = new Uri(url);

                    string wwwUrl = $"{uri.Scheme}://www.{uri.Host}";

                    var response = await _httpClient.GetAsync(wwwUrl);

                    if (response.IsSuccessStatusCode)
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }
    }
}