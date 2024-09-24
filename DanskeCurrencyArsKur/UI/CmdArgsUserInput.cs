using DanskeCurrencyArsKur.Common;

namespace DanskeCurrencyArsKur.UI
{
    public class CmdArgsUserInput : IUserInput
    {
        private readonly ICmdArgsProvider _cmdArgsProvider;

        public CmdArgsUserInput(ICmdArgsProvider cmdArgsProvider) => _cmdArgsProvider = cmdArgsProvider;

        public Task<(string CurrencyCodeFrom, string CurrencyCodeTo, decimal ExchangeAmount)> GetExchangeOperationInfo()
        {
            var cmdArgs = _cmdArgsProvider.GetCommandLineArgs();
            if (cmdArgs.Length != 3)
                return CreateInvalidUserInputException("Invalid number of arguments");

            if (!decimal.TryParse(cmdArgs[2], out var exchangeAmount))
                return CreateInvalidUserInputException("Second argument must be a valid decimal amount");

            var currencies = cmdArgs[1].Split('/');
            if (currencies.Length != 2)
                return CreateInvalidUserInputException("Currencies must be separated by one '/'");

            if (currencies[0].Length != 3 || currencies[1].Length != 3)
                return CreateInvalidUserInputException("Currency symbols must adhere to ISO 4217 (3 characters)");

            return Task.FromResult((currencies[0], currencies[1], exchangeAmount));
        }

        private static Task<(string CurrencyCodeFrom, string CurrencyCodeTo, decimal ExchangeAmount)> CreateInvalidUserInputException(string message)
            => Task.FromException<(string CurrencyCodeFrom, string CurrencyCodeTo, decimal ExchangeAmount)>(new InvalidUserInputException(message));
    }
}
