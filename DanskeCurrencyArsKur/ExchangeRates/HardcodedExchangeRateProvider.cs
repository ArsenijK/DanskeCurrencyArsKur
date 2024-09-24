namespace DanskeCurrencyArsKur.ExchangeRates
{
    public class HardcodedExchangeRateProvider : IExchangeRateProvider
    {
        private readonly Dictionary<string, decimal> _rates = new Dictionary<string, decimal>()
        {
            { "EUR", 743.94m / 100 },
            { "USD", 663.11m / 100 },
            { "GBP", 852.85m / 100 },
            { "SEK", 76.10m / 100 },
            { "NOK", 78.40m / 100 },
            { "CHF", 683.58m / 100 },
            { "JPY", 5.9740m / 100 }
        };

        public string GetUiDescription() => "Hardcoded exchange rates";

        public string GetMainCurrencyCode() => "DKK";

        public async Task<decimal> GetMainToMoneyRate(string moneyCurrencyCode)
            => 1 / await GetMoneyToMainRate(moneyCurrencyCode);

        public Task<decimal> GetMoneyToMainRate(string moneyCurrencyCode)
            => _rates.TryGetValue(moneyCurrencyCode, out var rate)
                ? Task.FromResult(rate)
                : Task.FromException<decimal>(new UnknownCurrencyCodeException(moneyCurrencyCode));
    }
}
