using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BookScraper.Export;
using BookScraper.Filters;
using BookScraper.Models;
using BookScraper.Options;
using BookScraper.Services;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables(prefix: "BOOKSCRAPER_")
    .AddCommandLine(args)
    .Build();
    
Console.WriteLine("Current directory: " + Directory.GetCurrentDirectory());

Console.WriteLine("Files in current directory:");
foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
{
    Console.WriteLine(" - " + Path.GetFileName(file));
}

var cats = configuration.GetSection("Scraper:Categories").Get<string[]>();

Console.WriteLine("Categories from config: " + string.Join(", ", cats ?? Array.Empty<string>()));

var options = ScraperOptions.FromConfiguration(configuration);

Console.WriteLine("Starting Book Scraper...")
;
Console.WriteLine($"Base URL: {options.BaseUrl}");
Console.WriteLine($"Categories: {string.Join(", ", options.Categories)}");
Console.WriteLine($"Filters → MinPrice: {options.Filters.MinPrice?.ToString() ?? "-"}, MaxPrice: {options.Filters.MaxPrice?.ToString() ?? "-"}, Rating: {options.Filters.Rating?.ToString() ?? "-"}");

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("book-scraper", "1.0"));

var scraper = new BookScraperService(options.BaseUrl, httpClient);
List<Book> allBooks;
try
{
    allBooks = await scraper.ScrapeAsync(options.Categories);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error while scraping: {ex.Message}");
    return;
}

var filtered = BookFilters.Apply(allBooks, options.Filters);
Console.WriteLine($"Scraped: {allBooks.Count} books; After filters: {filtered.Count}");

try
{
    Exporter.ExportJson(filtered, options.Output.JsonPath);
    Exporter.ExportXml(filtered, options.Output.XmlPath);
    Console.WriteLine($"Exported JSON → {options.Output.JsonPath}");
    Console.WriteLine($"Exported XML  → {options.Output.XmlPath}");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to export files: {ex.Message}");
}

try
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(options.Api.TimeoutSeconds));
    var json = JsonSerializer.Serialize(filtered, Exporter.JsonOptionsIndented);
    using var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await httpClient.PostAsync(options.Api.Endpoint, content, cts.Token);
    Console.WriteLine($"POST {options.Api.Endpoint} → Status: {(int)response.StatusCode} {response.StatusCode}");
    Console.WriteLine("Summary sent:");
    Console.WriteLine($"- Count: {filtered.Count}");
    var sample = string.Join(", ", filtered.Take(5).Select(b => b.Title));
    Console.WriteLine($"- Sample titles: {sample}");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to POST results: {ex.Message}");
}
