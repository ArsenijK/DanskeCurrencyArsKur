using DanskeCurrencyArsKur.ExchangeRates;

namespace DanskeCurrencyArsKur.UI
{
    public class ConsoleUserOutput : IUserOutput
    {
        public void ShowExchangedResult(decimal result)
            => Console.WriteLine(result);

        public void ShowInvalidUserInputException(InvalidUserInputException ex)
            => Console.WriteLine("Usage: Exchange <currency pair> <amount to exchange>"); // used phrase from excercise, but it's not mentioned here that '/' should be used, so unclear to the user

        public void ShowExchangeServiceUnavailableException(string exchangeServiceDescription, ExchangeServiceUnavailableException esuEx)
            => Console.WriteLine($"Exchange service '{exchangeServiceDescription}' unavailable"); // translations can be used here

        public void ShowUnknownCurrencyCodeException(string exchangeServiceDescription, UnknownCurrencyCodeException ex)
            => Console.WriteLine($"Exchange service '{exchangeServiceDescription}' doesn't recognize provided currency code '{ex.CurrencyCode}'");
    }
}
