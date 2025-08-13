using Microsoft.Extensions.Configuration;

namespace BookScraper.Options;

public sealed class FilterOptions
{
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? Rating { get; set; }
}

public sealed class OutputOptions
{
    public string JsonPath { get; set; } = "books.json";
    public string XmlPath { get; set; } = "books.xml";
}

public sealed class ApiOptions
{
    public string Endpoint { get; set; } = "https://httpbin.org/post";
    public int TimeoutSeconds { get; set; } = 30;
}

public sealed class ScraperOptions
{
    public string BaseUrl { get; set; } = "https://books.toscrape.com/";
    public List<string> Categories { get; set; } = new() { "Travel", "Mystery", "Science" };
    public FilterOptions Filters { get; set; } = new();
    public OutputOptions Output { get; set; } = new();
    public ApiOptions Api { get; set; } = new();

    public static ScraperOptions FromConfiguration(IConfiguration configuration)
    {
        var opt = new ScraperOptions();
        var section = configuration.GetSection("Scraper");
        if (section.Exists())
        {
            section.Bind(opt);
        }
        else
        {
            opt.BaseUrl = configuration["baseUrl"] ?? opt.BaseUrl;
            var cats = configuration["categories"];
            if (!string.IsNullOrWhiteSpace(cats))
            {
                opt.Categories = cats
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            if (decimal.TryParse(configuration["minPrice"], out var minVal)) opt.Filters.MinPrice = minVal;
            if (decimal.TryParse(configuration["maxPrice"], out var maxVal)) opt.Filters.MaxPrice = maxVal;
            if (int.TryParse(configuration["rating"], out var rVal)) opt.Filters.Rating = rVal;
            opt.Output.JsonPath = configuration["jsonPath"] ?? opt.Output.JsonPath;
            opt.Output.XmlPath = configuration["xmlPath"] ?? opt.Output.XmlPath;
            opt.Api.Endpoint = configuration["endpoint"] ?? opt.Api.Endpoint;
            if (int.TryParse(configuration["timeoutSeconds"], out var t)) opt.Api.TimeoutSeconds = t;
        }

        if (opt.Categories.Count < 3)
        {
            var defaults = new List<string> { "Travel", "Mystery", "Science" };
            foreach (var d in defaults)
            {
                if (opt.Categories.Count >= 3) break;
                if (!opt.Categories.Contains(d, StringComparer.OrdinalIgnoreCase)) opt.Categories.Add(d);
            }
        }
        opt.Categories = opt.Categories.Distinct(StringComparer.OrdinalIgnoreCase).Take(3).ToList();
        return opt;
    }
}


