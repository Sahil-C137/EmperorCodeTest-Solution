

# Emperor Code Test - Solution

This is just a brief explanation of the changes I made during this coding test. This should help you understand what I have done and what steps I have taken. The objective was to integrate an external XML feed into the /news/ page, with the feed URL managed through the Umbraco CMS back-office.

**Admin login: admin@admin.com / MW>Y5XSxG?**


## Files Added or Modified

### `NewsArticle.cs`

A model representing a single parsed article from the feed.

-   `Title`, `Description`, `ImageUrl`, `Link`, `PublishedDate` mapped from XML elements
-   `TruncatedDescription` truncates to 100 characters with an ellipsis
-   `FormattedDate` formats dates as `12th November 2025` with correct ordinal suffixes
-   `HasValidImageUrl` checks that the image URL is an absolute HTTP/HTTPS address
-   `HasValidLink` applies the same validation to article links

### `NewsFeedService.cs`
The service that fetches and parses the XML feed. Registered in `Program.cs` via dependency injection and injected directly into the Razor view.
 - Uses IHttpClientFactory (best practice to avoids socket exhaustion)
 - 10-second timeout to protect against slow/unresponsive feeds
 - Entire fetch wrapped in try/catch to returns an empty list on any failure (network error, timeout, malformed XML,

### `ExternalNewsPage.cshtml`

The Razor view. Injects `NewsFeedService`, reads the `feedUrl` property from the Umbraco content model, and renders articles using the site's existing CSS classes.
There were some pre-existing hardcoded article cards instead of deleating them I renders the live feed articles alongside them, treating the static cards as production components rather than removing them.

### `Program.cs`

Two additions for service registration: `IHttpClientFactory` and `NewsFeedService`.

----------

## Edge Cases Handled

The implementation was tested against a dedicated XML feed with 18 items covering the following scenarios.
Here is the link to the XML file used for the testing the articles: https://raw.githubusercontent.com/Sahil-C137/XML-Test-Cases/refs/heads/main/test-feed.xml
1.  Empty pubDate
2.  Invalid date ("banana")
3.  Future date (year 2100)
4.  DateTime.MinValue display
5.  CDATA with HTML tags
6.  Encoded HTML / XSS
7.  media:content image
8.  Relative image URL
9.  Invalid link URL
10.  Excessive whitespace
11.  Fully empty item
12.  Multiple <image> tags
13.  Unicode / emoji title
14.  HTML entities (& etc.)
15.  Slow/unavailable feed
16.  Malformed XML
17.  Single bad item
18.  Missing image field

----------

## Summary

The implementation satisfies all stated requirements:

-   Feed URL is managed in the Umbraco CMS back-office
    
-   Each article displays image, title, truncated description, formatted date, and read more link
    
-   Dates formatted with correct English ordinal suffixes (1st, 2nd, 3rd, 11th, 12th...)
    
-   Articles sorted by descending date
    
-   Feed failures (timeout, malformed XML, bad items) handled
    
-   XSS and injection risks from feed content mitigated
    
-   Placeholder shown for missing or invalid images
