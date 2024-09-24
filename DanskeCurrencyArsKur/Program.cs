using DanskeCurrencyArsKur.Common;
using DanskeCurrencyArsKur.ExchangeRates;
using DanskeCurrencyArsKur.UI;
using Microsoft.Extensions.DependencyInjection;

namespace DanskeCurrencyArsKur
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection() // TODO: add logging
                .AddSingleton<IMainApp, MainApp>()
                .AddSingleton<IUserInput, CmdArgsUserInput>() // TODO: split interfaces/domain and particular implementations to different projects, and tests accordingly
                .AddSingleton<IUserOutput, ConsoleUserOutput>()
                .AddSingleton<IExchangeRateProvider, LietuvosBankasExchangeRateProvider>() // I made it so if previous one fails - next one is tried (in case you can't access API)
                .AddSingleton<IExchangeRateProvider, HardcodedExchangeRateProvider>()
                .AddSingleton<IHttpRequest, HttpRequest>()
                .AddSingleton<ICmdArgsProvider, CmdArgsProvider>()
                .BuildServiceProvider();

            var app = serviceProvider.GetRequiredService<IMainApp>();
            await app.Execute();
        }
    }
}