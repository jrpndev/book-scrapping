## Book Scraper

Aplicação em C# (.NET) para fazer scraping do site `https://books.toscrape.com/`, aplicar filtros configuráveis e exportar os resultados para JSON e XML, além de enviar os dados filtrados para um endpoint REST (por padrão `https://httpbin.org/post`).

### Requisitos
- .NET SDK 9.0 (ou 8.0+ com compatibilidade)

### Configuração
Parâmetros podem ser definidos de 3 formas (precedência: CLI > variáveis de ambiente > `appsettings.json`):
- Arquivo `appsettings.json`
- Variáveis de ambiente com prefixo `BOOKSCRAPER_`
- Parâmetros de linha de comando

Estrutura no `appsettings.json`:
```json
{
  "Scraper": {
    "BaseUrl": "https://books.toscrape.com/",
    "Categories": ["Travel", "Mystery", "Science"],
    "Filters": { "MinPrice": null, "MaxPrice": null, "Rating": null },
    "Output": { "JsonPath": "books.json", "XmlPath": "books.xml" },
    "Api": { "Endpoint": "https://httpbin.org/post", "TimeoutSeconds": 30 }
  }
}
```

Exemplos via variáveis de ambiente (WSL/Linux):
```bash
export BOOKSCRAPER_Scraper__Filters__MinPrice=10
export BOOKSCRAPER_Scraper__Filters__Rating=4
export BOOKSCRAPER_Scraper__Categories__0=Travel
export BOOKSCRAPER_Scraper__Categories__1=Mystery
export BOOKSCRAPER_Scraper__Categories__2=Science
```

Exemplos via CLI:
```bash
dotnet run --project . -- Scraper:Filters:MinPrice=10 Scraper:Filters:Rating=4 Scraper:Categories:0=Travel Scraper:Categories:1=Mystery Scraper:Categories:2=Science
```

Também há chaves planas de fallback: `baseUrl`, `categories` (CSV), `minPrice`, `maxPrice`, `rating`, `jsonPath`, `xmlPath`, `endpoint`, `timeoutSeconds`.

### Execução
```bash
dotnet restore
dotnet run --project .
```

### Saída
- `books.json`: JSON indentado com os livros filtrados
- `books.xml`: XML equivalente
- Console: logs básicos, status do POST e resumo (quantidade e títulos exemplo)

### Observações
- São percorridas todas as páginas de 3 categorias escolhidas. Caso informe mais que 3, serão consideradas as 3 primeiras; se menos, será completado com defaults.
- Tratamento de erros básico com mensagens no console.
- Parsing de HTML via HtmlAgilityPack.

