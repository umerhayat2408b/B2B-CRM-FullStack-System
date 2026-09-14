using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using B2B_PRO.Models;

namespace B2B_PRO.Services
{
    public class ScraperService
    {
        private readonly HttpClient _httpClient;

        public ScraperService()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };
            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(12);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }

        public async Task<LeadsData> ScrapePageAsync(string url)
        {
            var lead = new LeadsData
            {
                Website = url,
                Email = "Not Found",
                Phone = "Not Found",
                Name = "Unknown Business"
            };

            try
            {
                // 1. Homepage Download karein
                string html = await _httpClient.GetStringAsync(url);

                // Title extraction
                var titleMatch = Regex.Match(html, @"<title>\s*(.*?)\s*</title>", RegexOptions.IgnoreCase);
                if (titleMatch.Success)
                {
                    string title = titleMatch.Groups[1].Value.Trim();
                    lead.Name = title.Length > 100 ? title.Substring(0, 100) : title;
                }
                else
                {
                    lead.Name = new Uri(url).Host.Replace("www.", "").Split('.')[0].ToUpper();
                }

                // Homepage se data nikalen
                var (email, phone) = ExtractDataFromHtml(html);
                lead.Email = string.IsNullOrEmpty(email) ? "Not Found" : email;
                lead.Phone = string.IsNullOrEmpty(phone) ? "Not Found" : phone;

                // 2. Agar Homepage par email ya phone missing hai, to sub-pages scan karein
                if (lead.Email == "Not Found" || lead.Phone == "Not Found")
                {
                    // "Contact Us" aur "About Us" donon ke links dhoondein
                    string contactUrl = FindSubPageUrl(html, url, "contact");
                    string aboutUrl = FindSubPageUrl(html, url, "about");

                    // Contact Page Scan
                    if (!string.IsNullOrEmpty(contactUrl))
                    {
                        try
                        {
                            string contactHtml = await _httpClient.GetStringAsync(contactUrl);
                            var (cEmail, cPhone) = ExtractDataFromHtml(contactHtml);
                            if (lead.Email == "Not Found" && !string.IsNullOrEmpty(cEmail)) lead.Email = cEmail;
                            if (lead.Phone == "Not Found" && !string.IsNullOrEmpty(cPhone)) lead.Phone = cPhone;
                        }
                        catch { }
                    }

                    // About Page Scan (Agar phir bhi email/phone na mila ho)
                    if ((lead.Email == "Not Found" || lead.Phone == "Not Found") && !string.IsNullOrEmpty(aboutUrl))
                    {
                        try
                        {
                            string aboutHtml = await _httpClient.GetStringAsync(aboutUrl);
                            var (aEmail, aPhone) = ExtractDataFromHtml(aboutHtml);
                            if (lead.Email == "Not Found" && !string.IsNullOrEmpty(aEmail)) lead.Email = aEmail;
                            if (lead.Phone == "Not Found" && !string.IsNullOrEmpty(aPhone)) lead.Phone = aPhone;
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping {url}: {ex.Message}");
            }

            return lead;
        }

        private (string Email, string Phone) ExtractDataFromHtml(string html)
        {
            // 1. Script, Style, SVG aur HTML Tags ko Saaf Karein taake hidden code/timestamps na ayein
            string cleanText = Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
            cleanText = Regex.Replace(cleanText, @"<style[^>]*>[\s\S]*?</style>", "", RegexOptions.IgnoreCase);
            cleanText = Regex.Replace(cleanText, @"<svg[^>]*>[\s\S]*?</svg>", "", RegexOptions.IgnoreCase);

            // HTML tags strip kar dein taake plain text bache
            string plainText = Regex.Replace(cleanText, @"<[^>]+>", " ");

            // 2. Email Extraction
            var emailMatch = Regex.Match(plainText, @"[a-zA-Z0-9_\-\.]+@[a-zA-Z0-9_\-\.]+\.[a-zA-Z]{2,5}");
            string email = emailMatch.Success ? emailMatch.Value : "";

            if (email.EndsWith(".png") || email.EndsWith(".jpg") || email.EndsWith(".gif") || email.Contains("wixpress") || email.Contains("example.com"))
            {
                email = "";
            }

            // 3. Smart Phone Regex (Validates Landline, Helplines & Mobile numbers)
            var phoneRegex = new Regex(@"(\+?\d{1,3}[-.\s]?)?\(?\d{2,5}\)?[-.\s]?\d{3,4}[-.\s]?\d{3,4}");
            var matches = phoneRegex.Matches(plainText);

            string phone = "";
            foreach (Match m in matches)
            {
                string val = m.Value.Trim();

                // Digits extract karein verification ke liye
                string digitsOnly = Regex.Replace(val, @"\D", "");

                // Filter Out Garbage / Timestamps:
                // Phone number ki length 10 se 13 digits ki honi chahiye aur saal/timestamps (2024, 2025, 2026) se start nahi honi chahiye
                if (digitsOnly.Length >= 10 && digitsOnly.Length <= 13)
                {
                    if (!digitsOnly.StartsWith("2024") && !digitsOnly.StartsWith("2025") && !digitsOnly.StartsWith("2026"))
                    {
                        phone = val;
                        break; // Perfect phone number mil gaya!
                    }
                }
            }

            return (email, phone);
        }

        private string FindSubPageUrl(string html, string baseUrl, string keyword)
        {
            // Dynamic check for 'about' or 'contact' in links
            var matches = Regex.Matches(html, $@"href=""([^""]*{keyword}[^""]*)""", RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                string path = matches[0].Groups[1].Value;
                if (path.StartsWith("http")) return path;

                try
                {
                    Uri baseUri = new Uri(baseUrl);
                    Uri myUri = new Uri(baseUri, path);
                    return myUri.ToString();
                }
                catch { }
            }
            return null;
        }
    }
}
