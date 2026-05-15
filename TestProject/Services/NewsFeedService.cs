using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

public class NewsFeedService
{
    private readonly IHttpClientFactory _httpClientFactory;
    public NewsFeedService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<NewsArticle>> GetArticlesAsync(string feedUrl)
    {
        if (string.IsNullOrWhiteSpace(feedUrl)) 
            return new List<NewsArticle>();

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10); // Setting a hard timeout so a hanging external RSS feed doesn't block our page from loading for the user.
            var xmlContent = await client.GetStringAsync(feedUrl);
            var xmlDoc = XDocument.Parse(xmlContent);
            return xmlDoc.Descendants("item").Select(ParseArticleElement).Where(article => article != null).OrderByDescending(article => article.PublishedDate).ToList();
        }
        catch
        {
            return new List<NewsArticle>(); // If the external feed goes down or returns malformed XML, return an empty list rather than crashing the page.
        }
    }

    private NewsArticle ParseArticleElement(XElement item)
    {
        try
        {
            var title = item.Element("title")?.Value?.Trim();
            var rawDescription = item.Element("description")?.Value;
            var cleanDescription = StripHtmlAndScripts(rawDescription);

            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(cleanDescription))
            {
                return null; // No article shown if it has absolutely no readable content.
            }

            var imageUrl = item.Element("image")?.Value?.Trim(); // Look exclusively for the standard <image> tag.
            var link = item.Element("link")?.Value?.Trim();
            var rawDate = item.Element("pubDate")?.Value?.Trim();
            var publishedDate = DateTime.TryParse(rawDate, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var parsedDate) ? parsedDate : DateTime.MinValue; // Parse to Universal Time so our dates don't shift unpredictably depending on the server's local timezone.

            return new NewsArticle
            {
                Title = title,
                Description = cleanDescription,
                ImageUrl = imageUrl,
                Link = link,
                PublishedDate = publishedDate
            };
        }
        catch
        {
            return null; // A single malformed XML node shouldn't ruin the entire feed list.
        }
    }

    private static string StripHtmlAndScripts(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var withoutScripts = Regex.Replace(input, @"<(script|style)[^>]*>.*?</\1>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline); // Strip script and style tags completely so their inner code doesn't show up as plain text
        var withoutTags = Regex.Replace(withoutScripts, "<.*?>", string.Empty); // Removing all remaining HTML tags
        var normalizedText = Regex.Replace(withoutTags, @"\s+", " "); // Normalizing whitespace (collapses multiple spaces, tabs, and newlines into a single space)
        return normalizedText.Trim();
    }
}