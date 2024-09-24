using DanskeCurrencyArsKur.Common;
using DanskeCurrencyArsKur.UI;
using NSubstitute;

namespace DanskeCurrencyArsKur.Tests
{
    public class CmdArgsUserInputTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private CmdArgsUserInput _sut;
        private ICmdArgsProvider _cmdArgsProvider;
#pragma warning restore CS8618

        [SetUp]
        public void Setup()
            => _sut = new CmdArgsUserInput(_cmdArgsProvider = Substitute.For<ICmdArgsProvider>());

        [Test]
        public async Task NormalArgs_Parsed()
        {
            _cmdArgsProvider.GetCommandLineArgs().Returns(["Tests.exe", "EUR/DKK", "1"]);

            var (CurrencyCodeFrom, CurrencyCodeTo, ExchangeAmount) = await _sut.GetExchangeOperationInfo();

            Assert.That(CurrencyCodeFrom, Is.EqualTo("EUR"));
            Assert.That(CurrencyCodeTo, Is.EqualTo("DKK"));
            Assert.That(ExchangeAmount, Is.EqualTo(1m));
        }

        [Test]
        public async Task WithNegativeAmount_Parsed()
        {
            _cmdArgsProvider.GetCommandLineArgs().Returns(["Tests.exe", "SEK/NOK", "-123.456"]);

            var (CurrencyCodeFrom, CurrencyCodeTo, ExchangeAmount) = await _sut.GetExchangeOperationInfo();

            Assert.That(CurrencyCodeFrom, Is.EqualTo("SEK"));
            Assert.That(CurrencyCodeTo, Is.EqualTo("NOK"));
            Assert.That(ExchangeAmount, Is.EqualTo(-123.456m));
        }

        [Test]
        public void TooLittleArgs_ThrowsInvalidUserInputException()
        {
            _cmdArgsProvider.GetCommandLineArgs().Returns(["Tests.exe", "EUR/DKK"]);

            var thrown = Assert.ThrowsAsync<InvalidUserInputException>(() => _sut.GetExchangeOperationInfo());
            Assert.That(thrown.Message, Contains.Substring("arguments")); // we can decide on whole texts or keywords or nothing in these places
        }

        [Test]
        public void TooManyArgs_ThrowsInvalidUserInputException()
        {
            _cmdArgsProvider.GetCommandLineArgs().Returns(["Tests.exe", "EUR/DKK", "100", "200"]);

            var thrown = Assert.ThrowsAsync<InvalidUserInputException>(() => _sut.GetExchangeOperationInfo());
            Assert.That(thrown.Message, Contains.Substring("arguments"));
        }

        [Test]
        public void InvalidDecimalNumber_ThrowsInvalidUserInputException()
        {
            _cmdArgsProvider.GetCommandLineArgs().Returns(["Tests.exe", "EUR/DKK", "1A"]);

            var thrown = Assert.ThrowsAsync<InvalidUserInputException>(() => _sut.GetExchangeOperationInfo());
            Assert.That(thrown.Message, Contains.Substring("decimal"));
        }

        [Test]
        public void WrongSlashForCurrencyPair_ThrowsInvalidUserInputException()
        {
            _cmdArgsProvider.GetCommandLineArgs().Returns(["Tests.exe", "EUR\\DKK", "10"]);

            var thrown = Assert.ThrowsAsync<InvalidUserInputException>(() => _sut.GetExchangeOperationInfo());
            Assert.That(thrown.Message, Contains.Substring("/"));
        }

        [Test]
        public void MoreHanOneSlashForCurrencyPair_ThrowsInvalidUserInputException()
        {
            _cmdArgsProvider.GetCommandLineArgs().Returns(["Tests.exe", "EUR/DKK/", "10"]);

            var thrown = Assert.ThrowsAsync<InvalidUserInputException>(() => _sut.GetExchangeOperationInfo());
            Assert.That(thrown.Message, Contains.Substring("/"));
        }

        [Test]
        public void TooShortCurrencyCode_ThrowsInvalidUserInputException()
        {
            _cmdArgsProvider.GetCommandLineArgs().Returns(["Tests.exe", "EU/DKK", "10"]);

            var thrown = Assert.ThrowsAsync<InvalidUserInputException>(() => _sut.GetExchangeOperationInfo());
            Assert.That(thrown.Message, Contains.Substring("ISO"));
        }

        [Test]
        public void TooLongCurrencyCode_ThrowsInvalidUserInputException()
        {
            _cmdArgsProvider.GetCommandLineArgs().Returns(["Tests.exe", "EUR/DKKR", "10"]);

            var thrown = Assert.ThrowsAsync<InvalidUserInputException>(() => _sut.GetExchangeOperationInfo());
            Assert.That(thrown.Message, Contains.Substring("ISO"));
        }
    }
}
