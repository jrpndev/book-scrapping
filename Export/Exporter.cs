using System.Text;
using System.Text.Json;
using BookScraper.Models;

namespace BookScraper.Export;

public static class Exporter
{
    public static readonly JsonSerializerOptions JsonOptionsIndented = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    public static void ExportJson(List<Book> books, string path)
    {
        var json = JsonSerializer.Serialize(books, JsonOptionsIndented);
        File.WriteAllText(path, json, Encoding.UTF8);
    }

    public static void ExportXml(List<Book> books, string path)
    {
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Book>));
        using var fs = File.Create(path);
        serializer.Serialize(fs, books);
    }
}


