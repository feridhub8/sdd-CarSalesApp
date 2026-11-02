using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using WebApi.Data.ExchangeRate.Models.V1;
using WebApi.Data.ExchangeRate.Repositories.V1.Base;

namespace WebApi.Data.ExchangeRate.Repositories.V1;

// The Central Bank of Azerbaijan's daily file. It is a public file and not a documented API,
// so anything unexpected in it counts as no answer rather than a rate.
public sealed class ExchangeRateCbarRepository : IExchangeRateSourceRepository
{
    private const decimal MinimumRate = 0.1m;
    private const decimal MaximumRate = 100m;
    private const string DateFormat = "dd.MM.yyyy";
    private static readonly TimeSpan BakuOffset = TimeSpan.FromHours(4);

    private readonly IHttpClientFactory httpClientFactory;
    private readonly ExchangeRateOptions options;

    public ExchangeRateCbarRepository(IHttpClientFactory httpClientFactory, IOptions<ExchangeRateOptions> options)
    {
        this.httpClientFactory = httpClientFactory;
        this.options = options.Value;
    }

    public async Task<SourceRates> GetLatestAsync(CancellationToken ct = default)
    {
        // Asking for today gives the latest working day on weekends and holidays too.
        var today = DateTime.UtcNow.Add(BakuOffset).ToString(DateFormat, CultureInfo.InvariantCulture);
        var client = this.httpClientFactory.CreateClient(ExchangeRateOptions.HttpClientName);

        using var response = await client.GetAsync(string.Format(CultureInfo.InvariantCulture, this.options.SourceUrl, today), ct);
        response.EnsureSuccessStatusCode();

        await using var content = await response.Content.ReadAsStreamAsync(ct);
        return await ParseAsync(content, ct);
    }

    private static async Task<SourceRates> ParseAsync(Stream content, CancellationToken ct)
    {
        using var reader = XmlReader.Create(content, new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
        var document = await XDocument.LoadAsync(reader, LoadOptions.None, ct);

        var root = document.Root ?? throw new FormatException("The file has no root element.");
        if (!DateOnly.TryParseExact(root.Attribute("Date")?.Value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            throw new FormatException("The file has no valid date.");
        }

        return new SourceRates(ReadRate(root, "USD"), ReadRate(root, "EUR"), date);
    }

    private static decimal ReadRate(XElement root, string code)
    {
        var currency = root.Descendants("Valute").SingleOrDefault(element => element.Attribute("Code")?.Value == code)
            ?? throw new FormatException($"The file has no {code}.");

        if (currency.Element("Nominal")?.Value.Trim() != "1")
        {
            throw new FormatException($"{code} is not quoted per one unit.");
        }

        var parsed = decimal.TryParse(currency.Element("Value")?.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var rate);
        if (!parsed || rate is < MinimumRate or > MaximumRate)
        {
            throw new FormatException($"{code} has no usable rate.");
        }

        return rate;
    }
}
