using System;

public class NewsArticle
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public string Link { get; set; }
    public DateTime PublishedDate { get; set; }

    public string TruncatedDescription
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Description)) return string.Empty;

            var cleanText = Description.Trim();
            return cleanText.Length > 100 ? $"{cleanText.Substring(0, 100)}..." : cleanText;
        }
    }

    public string FormattedDate
    {
        get
        {
            if (PublishedDate == DateTime.MinValue) return "Date unavailable";
            var suffix = GetOrdinalSuffix(PublishedDate.Day);
            return $"{PublishedDate.Day}{suffix} {PublishedDate:MMMM yyyy}";
        }
    }

    public bool HasValidImageUrl => IsValidUrl(ImageUrl);
    public bool HasValidLink => IsValidUrl(Link);

    private static bool IsValidUrl(string url)
    {
        return !string.IsNullOrWhiteSpace(url) &&
               Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static string GetOrdinalSuffix(int day)
    {
        if (day % 100 is >= 11 and <= 13) return "th";

        return (day % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
    }
}