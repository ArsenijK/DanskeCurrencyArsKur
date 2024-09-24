using DanskeCurrencyArsKur.ExchangeRates;
using DanskeCurrencyArsKur.UI;
using NSubstitute;

namespace DanskeCurrencyArsKur.Tests
{
    public class MainAppTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private MainApp _sut;
        private IUserInput _userInputMock;
        private IUserOutput _userOutputMock;
        private IExchangeRateProvider _firstExchangeRateProvider;
        private IExchangeRateProvider _secondExchangeRateProvider;
#pragma warning restore CS8618

        [SetUp]
        public void Setup()
        {
            _sut = new MainApp(
                _userInputMock = Substitute.For<IUserInput>(),
                _userOutputMock = Substitute.For<IUserOutput>(),
                [
                    _firstExchangeRateProvider = Substitute.For<IExchangeRateProvider>(),
                    _secondExchangeRateProvider = Substitute.For<IExchangeRateProvider>()
                ]
            );
        }

        [Test]
        public async Task SameCurrency_OutputsInitialAmount()
        {
            _userInputMock.GetExchangeOperationInfo().Returns(("Bla", "Bla", 7.7m));

            await _sut.Execute();

            _userOutputMock.Received(1).ShowExchangedResult(Arg.Is(7.7m));
            Assert.That(_userOutputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_userInputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_firstExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(0));
            Assert.That(_secondExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task EurToDk1ThroughDkk_OutputsRateFromFirstErp()
        {
            _userInputMock.GetExchangeOperationInfo().Returns(("Eur", "Dkk", 1));
            _firstExchangeRateProvider.GetMainCurrencyCode().Returns("DKK");
            _firstExchangeRateProvider.GetMoneyToMainRate("EUR").Returns(7.4394m);

            await _sut.Execute();

            _userOutputMock.Received(1).ShowExchangedResult(Arg.Is(7.4394m));
            Assert.That(_userOutputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_userInputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_firstExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(2)); // get main currency and one rate, because other one is main
            Assert.That(_secondExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task DkToEurMinus2ThroughDkk_OutputsRateFromFirstErp()
        {
            _userInputMock.GetExchangeOperationInfo().Returns(("Dkk", "Eur", -2));
            _firstExchangeRateProvider.GetMainCurrencyCode().Returns("DKK");
            _firstExchangeRateProvider.GetMainToMoneyRate("EUR").Returns(0.1344m);

            await _sut.Execute();

            _userOutputMock.Received(1).ShowExchangedResult(Arg.Is(-0.2688m));
            Assert.That(_userOutputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_userInputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_firstExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(2)); // get main currency and one rate, because other one is main
            Assert.That(_secondExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task EurToUsd10ThroughDkk_OutputsConvertedWithFirstErp()
        {
            _userInputMock.GetExchangeOperationInfo().Returns(("Eur", "Usd", 10));
            _firstExchangeRateProvider.GetMainCurrencyCode().Returns("DKK");
            _firstExchangeRateProvider.GetMoneyToMainRate("EUR").Returns(3m);
            _firstExchangeRateProvider.GetMainToMoneyRate("USD").Returns(4m);

            await _sut.Execute();

            _userOutputMock.Received(1).ShowExchangedResult(Arg.Is(120m));
            Assert.That(_userOutputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_userInputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_firstExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(3)); // get main currency and two rates
            Assert.That(_secondExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task FirstErpThrowsExchangeServiceUnavailableException_OutputsExceptionAndSecondErpResult()
        {
            var ex = new ExchangeServiceUnavailableException(new ApplicationException());
            _userInputMock.GetExchangeOperationInfo().Returns(("Eur", "Usd", 100));
            _firstExchangeRateProvider.GetUiDescription().Returns("The Description");
            _firstExchangeRateProvider.GetMainCurrencyCode().Returns("DKK");
            _firstExchangeRateProvider.GetMoneyToMainRate("EUR").Returns(3m);
            _firstExchangeRateProvider.GetMainToMoneyRate("USD").Returns(Task.FromException<decimal>(ex));
            _secondExchangeRateProvider.GetMainCurrencyCode().Returns("SEK");
            _secondExchangeRateProvider.GetMoneyToMainRate("EUR").Returns(0.5m);
            _secondExchangeRateProvider.GetMainToMoneyRate("USD").Returns(0.2m);

            await _sut.Execute();

            _userOutputMock.Received(1).ShowExchangedResult(Arg.Is(10m));
            _userOutputMock.Received(1).ShowExchangeServiceUnavailableException("The Description", ex);
            Assert.That(_userOutputMock.ReceivedCalls().Count(), Is.EqualTo(2));
            Assert.That(_userInputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_firstExchangeRateProvider.ReceivedCalls().Count(), Is.AtMost(4)); // also needs description for error
            Assert.That(_secondExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(3)); // get main currency and two rates
        }

        [Test]
        public async Task FirstErpThrowsUnknownCurrencyCodeException_OutputsExceptionAndSecondErpResult()
        {
            var ex = new UnknownCurrencyCodeException("Bla");
            _userInputMock.GetExchangeOperationInfo().Returns(("Eur", "Usd", 0.1m));
            _firstExchangeRateProvider.GetUiDescription().Returns("Another Description");
            _firstExchangeRateProvider.GetMainCurrencyCode().Returns("NOK");
            _firstExchangeRateProvider.GetMoneyToMainRate("EUR").Returns(3m);
            _firstExchangeRateProvider.GetMainToMoneyRate("USD").Returns(Task.FromException<decimal>(ex));
            _secondExchangeRateProvider.GetMainCurrencyCode().Returns("USD");
            _secondExchangeRateProvider.GetMoneyToMainRate("EUR").Returns(0.5m);

            await _sut.Execute();

            _userOutputMock.Received(1).ShowExchangedResult(Arg.Is(0.05m));
            _userOutputMock.Received(1).ShowUnknownCurrencyCodeException("Another Description", ex);
            Assert.That(ex.Message, Contains.Substring("Bla"));
            Assert.That(_userOutputMock.ReceivedCalls().Count(), Is.EqualTo(2));
            Assert.That(_userInputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_firstExchangeRateProvider.ReceivedCalls().Count(), Is.AtMost(4)); // also needs description for error
            Assert.That(_secondExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(2)); // get main currency and one rate
        }

        [Test]
        public async Task EurToUsd10ThroughGBPButRatesAre0_Outputs0()
        {
            _userInputMock.GetExchangeOperationInfo().Returns(("Eur", "Usd", 10));
            _firstExchangeRateProvider.GetMainCurrencyCode().Returns("GBP");
            _firstExchangeRateProvider.GetMoneyToMainRate("EUR").Returns(0m);
            _firstExchangeRateProvider.GetMainToMoneyRate("USD").Returns(0m);

            await _sut.Execute();

            _userOutputMock.Received(1).ShowExchangedResult(Arg.Is(0m));
            Assert.That(_userOutputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_userInputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_firstExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(3)); // get main currency and two rates
            Assert.That(_secondExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task EurToUsd0ThroughGBP_Outputs0()
        {
            _userInputMock.GetExchangeOperationInfo().Returns(("Eur", "Usd", 0));
            _firstExchangeRateProvider.GetMainCurrencyCode().Returns("GBP");
            _firstExchangeRateProvider.GetMoneyToMainRate("EUR").Returns(2m);
            _firstExchangeRateProvider.GetMainToMoneyRate("USD").Returns(3m);

            await _sut.Execute();

            _userOutputMock.Received(1).ShowExchangedResult(Arg.Is(0m));
            Assert.That(_userOutputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_userInputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_firstExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(3)); // get main currency and two rates
            Assert.That(_secondExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task InputThrowsInvalidUserInputException_OutputsException()
        {
            var ex = new InvalidUserInputException("Bla");
            _userInputMock.GetExchangeOperationInfo().Returns(Task.FromException<(string, string, decimal)>(ex));

            await _sut.Execute();

            _userOutputMock.Received(1).ShowInvalidUserInputException(ex);
            Assert.That(_userOutputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_userInputMock.ReceivedCalls().Count(), Is.EqualTo(1));
            Assert.That(_firstExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(0));
            Assert.That(_secondExchangeRateProvider.ReceivedCalls().Count(), Is.EqualTo(0));
        }
    }
}