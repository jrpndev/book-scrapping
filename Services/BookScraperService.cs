using BookScraper.Models;
using HtmlAgilityPack;

namespace BookScraper.Services;

public sealed class BookScraperService
{
    private readonly Uri _baseUri;
    private readonly HttpClient _httpClient;

    public BookScraperService(string baseUrl, HttpClient httpClient)
    {
        _baseUri = new Uri(EnsureTrailingSlash(baseUrl));
        _httpClient = httpClient;
    }

    public async Task<List<Book>> ScrapeAsync(IEnumerable<string> categories)
    {
        var categoryMap = await LoadCategoryMapAsync();
        var result = new List<Book>();
        foreach (var category in categories)
        {
            if (!categoryMap.TryGetValue(category.Trim(), out var relUrl))
            {
                Console.Error.WriteLine($"Category not found on site: {category}");
                continue;
            }
            var absoluteCategoryUrl = new Uri(_baseUri, relUrl);
            Console.WriteLine($"Scraping category: {category} → {absoluteCategoryUrl}");
            var books = await ScrapeCategoryAsync(category, absoluteCategoryUrl);
            result.AddRange(books);
        }
        return result;
    }

    private async Task<Dictionary<string, string>> LoadCategoryMapAsync()
    {
        var html = await GetStringAsync(_baseUri);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var categoryNodes = doc.DocumentNode.SelectNodes("//div[@class='side_categories']//ul//li//a");
        if (categoryNodes != null)
        {
            foreach (var node in categoryNodes)
            {
                var name = node.InnerText.Trim();
                var href = node.GetAttributeValue("href", string.Empty);
                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(href))
                {
                    if (string.Equals(name, "Books", StringComparison.OrdinalIgnoreCase)) continue;
                    map[name] = href;
                }
            }
        }
        return map;
    }

    private async Task<List<Book>> ScrapeCategoryAsync(string categoryName, Uri categoryUrl)
    {
        var books = new List<Book>();
        Uri? nextPage = categoryUrl;
        while (nextPage != null)
        {
            string html;
            try
            {
                html = await GetStringAsync(nextPage);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to load {nextPage}: {ex.Message}");
                break;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var productNodes = doc.DocumentNode.SelectNodes("//article[contains(@class,'product_pod')]");
            if (productNodes != null)
            {
                foreach (var node in productNodes)
                {
                    var titleNode = node.SelectSingleNode(".//h3/a");
                    var priceNode = node.SelectSingleNode(".//p[@class='price_color']");
                    var ratingNode = node.SelectSingleNode(".//p[contains(@class,'star-rating')]");
                    if (titleNode == null || priceNode == null || ratingNode == null) continue;

                    var title = titleNode.GetAttributeValue("title", string.Empty).Trim();
                    var href = titleNode.GetAttributeValue("href", string.Empty);
                    var url = new Uri(categoryUrl, href);
                    var priceText = priceNode.InnerText.Trim();
                    var price = ParsePrice(priceText);
                    var rating = ParseRating(ratingNode.GetAttributeValue("class", string.Empty));

                    books.Add(new Book
                    {
                        Title = title,
                        Price = price,
                        Rating = rating,
                        Category = categoryName,
                        Url = url.ToString()
                    });
                }
            }

            var nextNode = doc.DocumentNode.SelectSingleNode("//li[@class='next']/a");
            if (nextNode != null)
            {
                var href = nextNode.GetAttributeValue("href", string.Empty);
                nextPage = new Uri(categoryUrl, href);
            }
            else
            {
                nextPage = null;
            }
        }
        return books;
    }

    private static int ParseRating(string classAttr)
    {
        if (string.IsNullOrWhiteSpace(classAttr)) return 0;
        if (classAttr.Contains("One", StringComparison.OrdinalIgnoreCase)) return 1;
        if (classAttr.Contains("Two", StringComparison.OrdinalIgnoreCase)) return 2;
        if (classAttr.Contains("Three", StringComparison.OrdinalIgnoreCase)) return 3;
        if (classAttr.Contains("Four", StringComparison.OrdinalIgnoreCase)) return 4;
        if (classAttr.Contains("Five", StringComparison.OrdinalIgnoreCase)) return 5;
        return 0;
    }

    private static decimal ParsePrice(string priceText)
    {
        var cleaned = new string(priceText.Where(ch => char.IsDigit(ch) || ch == '.' || ch == ',').ToArray());
        if (decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }
        return 0m;
    }

    private async Task<string> GetStringAsync(Uri url)
    {
        using var resp = await _httpClient.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }

    private static string EnsureTrailingSlash(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "https://books.toscrape.com/";
        return url.EndsWith('/') ? url : url + "/";
    }
}


