using DanskeCurrencyArsKur.ExchangeRates;

namespace DanskeCurrencyArsKur.Tests
{
    public class HardcodedExchangeRateProviderTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private HardcodedExchangeRateProvider _sut;
#pragma warning restore CS8618

        [SetUp]
        public void Setup() => _sut = new HardcodedExchangeRateProvider();

        [Test]
        public void GetUiDescription_ContainsWordHardcoded()
        {
            var descr = _sut.GetUiDescription();

            Assert.That(descr, Contains.Substring("Hardcoded"));
        }

        [Test]
        public void GetMainCurrencyCode_DKK()
        {
            var descr = _sut.GetMainCurrencyCode();

            Assert.That(descr, Is.EqualTo("DKK"));
        }

        [Test]
        public async Task GetMoneyToMainRates_SameAsInProvidedTableDividedBy100()
        {
            Assert.That(await _sut.GetMoneyToMainRate("EUR"), Is.EqualTo(7.4394m));
            Assert.That(await _sut.GetMoneyToMainRate("USD"), Is.EqualTo(6.6311m));
            Assert.That(await _sut.GetMoneyToMainRate("GBP"), Is.EqualTo(8.5285m));
            Assert.That(await _sut.GetMoneyToMainRate("SEK"), Is.EqualTo(0.7610m));
            Assert.That(await _sut.GetMoneyToMainRate("NOK"), Is.EqualTo(0.7840m));
            Assert.That(await _sut.GetMoneyToMainRate("CHF"), Is.EqualTo(6.8358m));
            Assert.That(await _sut.GetMoneyToMainRate("JPY"), Is.EqualTo(0.05974m));
        }

        [Test]
        public async Task GetMainToMoneyRates_SameAsInProvidedTableDividedBy100AndInverted()
        {
            Assert.That(await _sut.GetMainToMoneyRate("EUR"), Is.EqualTo(1 / 7.4394m));
            Assert.That(await _sut.GetMainToMoneyRate("USD"), Is.EqualTo(1 / 6.6311m));
            Assert.That(await _sut.GetMainToMoneyRate("GBP"), Is.EqualTo(1 / 8.5285m));
            Assert.That(await _sut.GetMainToMoneyRate("SEK"), Is.EqualTo(1 / 0.7610m));
            Assert.That(await _sut.GetMainToMoneyRate("NOK"), Is.EqualTo(1 / 0.7840m));
            Assert.That(await _sut.GetMainToMoneyRate("CHF"), Is.EqualTo(1 / 6.8358m));
            Assert.That(await _sut.GetMainToMoneyRate("JPY"), Is.EqualTo(1 / 0.05974m));
        }
    }
}
