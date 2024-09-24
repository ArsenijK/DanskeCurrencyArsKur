namespace DanskeCurrencyArsKur.UI
{
    public interface IUserInput
    {
        /// <exception cref="InvalidUserInputException" />
        Task<(string CurrencyCodeFrom, string CurrencyCodeTo, decimal ExchangeAmount)> GetExchangeOperationInfo();
    }
}
