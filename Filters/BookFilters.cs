using BookScraper.Models;
using BookScraper.Options;

namespace BookScraper.Filters;

public static class BookFilters
{
    public static List<Book> Apply(IEnumerable<Book> books, FilterOptions filters)
    {
        var query = books;
        if (filters.MinPrice.HasValue)
            query = query.Where(b => b.Price >= filters.MinPrice.Value);
        if (filters.MaxPrice.HasValue)
            query = query.Where(b => b.Price <= filters.MaxPrice.Value);
        if (filters.Rating.HasValue)
            query = query.Where(b => b.Rating == filters.Rating.Value);
        return query.ToList();
    }
}


