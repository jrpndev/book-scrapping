namespace BookScraper.Models;

public sealed class Book
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Rating { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}


