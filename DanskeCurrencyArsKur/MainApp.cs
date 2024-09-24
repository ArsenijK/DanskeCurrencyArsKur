using DanskeCurrencyArsKur.ExchangeRates;
using DanskeCurrencyArsKur.UI;

namespace DanskeCurrencyArsKur
{
    public class MainApp : IMainApp
    {
        private readonly IUserInput _userInput;
        private readonly IUserOutput _userOutput;
        private readonly IEnumerable<IExchangeRateProvider> _exchangeRateProviders;

        public MainApp(
            IUserInput userInput,
            IUserOutput userOutput,
            IEnumerable<IExchangeRateProvider> exchangeRateProviders)
        {
            _userInput = userInput;
            _userOutput = userOutput;
            _exchangeRateProviders = exchangeRateProviders;
        }

        public async Task Execute()
        {
            try
            {
                var (currencyCodeFrom, currencyCodeTo, exchangeAmount) = await _userInput.GetExchangeOperationInfo();
                currencyCodeFrom = currencyCodeFrom.ToUpper();
                currencyCodeTo = currencyCodeTo.ToUpper();
                foreach (var erp in _exchangeRateProviders)
                    try
                    {
                        _userOutput.ShowExchangedResult(await ConvertUsing(erp, currencyCodeFrom, currencyCodeTo, exchangeAmount));
                        return;
                    }
                    catch (ExchangeServiceUnavailableException esuEx)
                    {
                        _userOutput.ShowExchangeServiceUnavailableException(erp.GetUiDescription(), esuEx);
                        continue;
                    }
                    catch (UnknownCurrencyCodeException uccEx)
                    {
                        _userOutput.ShowUnknownCurrencyCodeException(erp.GetUiDescription(), uccEx);
                        continue;
                    }
            }
            catch (InvalidUserInputException iuiEx) // if resource consumption and speed is important (back-end or in a loop)
                                                   // I would not use exceptions for user input, but for this app should do
            {
                _userOutput.ShowInvalidUserInputException(iuiEx);
            }
        }

        private async Task<decimal> ConvertUsing(IExchangeRateProvider erp, string currencyCodeFrom, string currencyCodeTo, decimal exchangeAmount)
        {
            if (currencyCodeFrom == currencyCodeTo)
                return exchangeAmount;

            string erpCurrency = erp.GetMainCurrencyCode();

            decimal initialCurrencyToMainRate
                = erpCurrency == currencyCodeFrom
                    ? 1
                    : await erp.GetMoneyToMainRate(currencyCodeFrom);

            decimal erpMainToFinalMoneyRate
                = erpCurrency == currencyCodeTo
                    ? 1
                    : await erp.GetMainToMoneyRate(currencyCodeTo);

            return Math.Round(exchangeAmount * initialCurrencyToMainRate * erpMainToFinalMoneyRate, 4); // TODO : should we round and how much? maybe depending on currency? add test, hen answer is known
        }
    }
}
