using DanskeCurrencyArsKur.ExchangeRates;

namespace DanskeCurrencyArsKur.UI
{
    public interface IUserOutput
    {
        void ShowExchangedResult(decimal result);
        void ShowInvalidUserInputException(InvalidUserInputException ex);
        void ShowUnknownCurrencyCodeException(string exchangeServiceDescription, UnknownCurrencyCodeException ex);
        void ShowExchangeServiceUnavailableException(string exchangeServiceDescription, ExchangeServiceUnavailableException esuEx);
    }
}
