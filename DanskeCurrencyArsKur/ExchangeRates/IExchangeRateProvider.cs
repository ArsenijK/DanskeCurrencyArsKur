namespace DanskeCurrencyArsKur.ExchangeRates
{
    public interface IExchangeRateProvider
    {
        /// <summary>
        /// Used to identify this particular provider amongst others, can be shown to user
        /// </summary>
        string GetUiDescription();
        string GetMainCurrencyCode();

        /// <exception cref="UnknownCurrencyCodeException">
        /// <exception cref="ExchangeServiceUnavailableException">
        Task<decimal> GetMainToMoneyRate(string moneyCurrencyCode);

        /// <exception cref="UnknownCurrencyCodeException">
        /// <exception cref="ExchangeServiceUnavailableException">
        Task<decimal> GetMoneyToMainRate(string moneyCurrencyCode);
    }
}
