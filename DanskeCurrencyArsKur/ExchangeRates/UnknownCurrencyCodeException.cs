namespace DanskeCurrencyArsKur.ExchangeRates
{
    public class UnknownCurrencyCodeException : Exception
    {
        public UnknownCurrencyCodeException(string currencyCode) : base($"Unknown currency code: {currencyCode}")
            => CurrencyCode = currencyCode;

        public string CurrencyCode { get; }
    }
}
