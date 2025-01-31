﻿using DanskeCurrencyArsKur.Common;
using DanskeCurrencyArsKur.ExchangeRates;
using NSubstitute;
using System.Xml;

namespace DanskeCurrencyArsKur.Tests
{
    public class LietuvosBankasExchangeRateProviderTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private LietuvosBankasExchangeRateProvider _sut;
        private IHttpRequest _httpRequestMock;
#pragma warning restore CS8618

        [SetUp]
        public void Setup()
            => _sut = new LietuvosBankasExchangeRateProvider(_httpRequestMock = Substitute.For<IHttpRequest>());

        [Test]
        public void GetUiDescription_ContainsLietuvosBankas()
        {
            var descr = _sut.GetUiDescription();

            Assert.That(descr, Contains.Substring("Lietuvos bankas"));
        }

        [Test]
        public void GetMainCurrencyCode_EUR()
        {
            var descr = _sut.GetMainCurrencyCode();

            Assert.That(descr, Is.EqualTo("EUR"));
        }

        [Test]
        public async Task GetMainToMoneyRateCad_IsSameAsXml()
        {
            _httpRequestMock.GetStringAsync("https://www.lb.lt//webservices/FxRates/FxRates.asmx/getCurrentFxRates?tp=EU").Returns(responseExample);

            var rate = await _sut.GetMainToMoneyRate("CAD");

            Assert.That(rate, Is.EqualTo(1.5065m));
        }

        [Test]
        public async Task GetMoneyToMainRateCad_IsInvertedFromXml()
        {
            _httpRequestMock.GetStringAsync("https://www.lb.lt//webservices/FxRates/FxRates.asmx/getCurrentFxRates?tp=EU").Returns(responseExample);

            var rate = await _sut.GetMoneyToMainRate("CAD");

            Assert.That(rate, Is.EqualTo(1 / 1.5065m));
        }

        [Test]
        public void GetMoneyToMainRate_InvalidCurrency_ThrowsUnknownCurrencyCodeException()
        {
            _httpRequestMock.GetStringAsync("https://www.lb.lt//webservices/FxRates/FxRates.asmx/getCurrentFxRates?tp=EU").Returns(responseExample);

            Assert.ThrowsAsync<UnknownCurrencyCodeException>(() => _sut.GetMainToMoneyRate("BLA"));
        }

        [Test]
        public void GetMoneyToMainRate_IHttpRequestThrowsTaskCanceledException_ThrowsExchangeServiceUnavailableException()
        {
            var ex = new TaskCanceledException();
            _httpRequestMock.GetStringAsync("https://www.lb.lt//webservices/FxRates/FxRates.asmx/getCurrentFxRates?tp=EU").Returns(Task.FromException<string>(ex));

            var thrown = Assert.ThrowsAsync<ExchangeServiceUnavailableException>(() => _sut.GetMoneyToMainRate("DKK"));
            Assert.That(thrown.InnerException, Is.EqualTo(ex));
        }

        [Test]
        public void GetMainToMoneyRate_IHttpRequestThrowsHttpException_ThrowsExchangeServiceUnavailableException()
        {
            var ex = new HttpRequestException();
            _httpRequestMock.GetStringAsync("https://www.lb.lt//webservices/FxRates/FxRates.asmx/getCurrentFxRates?tp=EU").Returns(Task.FromException<string>(ex));

            var thrown = Assert.ThrowsAsync<ExchangeServiceUnavailableException>(() => _sut.GetMainToMoneyRate("USD"));
            Assert.That(thrown.InnerException, Is.EqualTo(ex));
        }

        [Test]
        public void GetMainToMoneyRate_IHttpRequestReturnsNull_ThrowsExchangeServiceUnavailableException()
        {
            _httpRequestMock.GetStringAsync("https://www.lb.lt//webservices/FxRates/FxRates.asmx/getCurrentFxRates?tp=EU").Returns(Task.FromResult<string>(null!));

            Assert.ThrowsAsync<ExchangeServiceUnavailableException>(() => _sut.GetMainToMoneyRate("NOK"));
        }

        [Test]
        public void GetMainToMoneyRate_IHttpRequestReturnsInvalidXml_ThrowsExchangeServiceUnavailableException()
        {
            _httpRequestMock.GetStringAsync("https://www.lb.lt//webservices/FxRates/FxRates.asmx/getCurrentFxRates?tp=EU").Returns("NotAnXml");

            var thrown = Assert.ThrowsAsync<ExchangeServiceUnavailableException>(() => _sut.GetMainToMoneyRate("CZK"));
            Assert.That(thrown.InnerException, Is.InstanceOf<XmlException>());
        }

        [Test]
        public void GetMainToMoneyRate_IHttpRequestReturnsXmlWithoutFxRateElements_ThrowsUnknownCurrencyCodeException()
        {
            _httpRequestMock.GetStringAsync("https://www.lb.lt//webservices/FxRates/FxRates.asmx/getCurrentFxRates?tp=EU")
                .Returns("<?xml version=\"1.0\" encoding=\"utf-8\"?><FxRates xmlns=\"http://www.lb.lt/WebServices/FxRates\"></FxRates>");

            Assert.ThrowsAsync<UnknownCurrencyCodeException>(() => _sut.GetMainToMoneyRate("BGN"));
        }

        [Test]
        public void GetMainToMoneyRate_IHttpRequestReturnsXmlWithoutCcyInsideFxRate_ThrowsExchangeServiceUnavailableException()
        {
            _httpRequestMock.GetStringAsync("https://www.lb.lt//webservices/FxRates/FxRates.asmx/getCurrentFxRates?tp=EU")
                .Returns(@"<?xml version=""1.0"" encoding=""utf-8""?>
<FxRates xmlns=""http://www.lb.lt/WebServices/FxRates"">
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Amt>10</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>JPY</Ccy>
      <Amt>159.58</Amt>
    </CcyAmt>
  </FxRate>
</FxRates>");

            var thrown = Assert.ThrowsAsync<ExchangeServiceUnavailableException>(() => _sut.GetMainToMoneyRate("JPY"));
            Assert.That(thrown.InnerException?.Message, Contains.Substring("Ccy element"));
        }

        [Test]
        public void GetMainToMoneyRate_IHttpRequestReturnsXmlWithoutAmtInsideFxRate_ThrowsExchangeServiceUnavailableException()
        {
            _httpRequestMock.GetStringAsync("https://www.lb.lt//webservices/FxRates/FxRates.asmx/getCurrentFxRates?tp=EU")
                .Returns(@"<?xml version=""1.0"" encoding=""utf-8""?>
<FxRates xmlns=""http://www.lb.lt/WebServices/FxRates"">
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
    </CcyAmt>
    <CcyAmt>
      <Ccy>INR</Ccy>
      <Amt>92.8955</Amt>
    </CcyAmt>
  </FxRate>
</FxRates>");

            var thrown = Assert.ThrowsAsync<ExchangeServiceUnavailableException>(() => _sut.GetMainToMoneyRate("INR"));
            Assert.That(thrown.InnerException?.Message, Contains.Substring("Amt element"));
        }

        [Test]
        public void GetMainToMoneyRate_IHttpRequestReturnsXmlWhereFirstAmtIsZero_ThrowsExchangeServiceUnavailableException()
        {
            _httpRequestMock.GetStringAsync("https://www.lb.lt//webservices/FxRates/FxRates.asmx/getCurrentFxRates?tp=EU")
                .Returns(@"<?xml version=""1.0"" encoding=""utf-8""?>
<FxRates xmlns=""http://www.lb.lt/WebServices/FxRates"">
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>0</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>TRY</Ccy>
      <Amt>37.9745</Amt>
    </CcyAmt>
  </FxRate>
</FxRates>");

            var thrown = Assert.ThrowsAsync<ExchangeServiceUnavailableException>(() => _sut.GetMainToMoneyRate("TRY"));
            Assert.That(thrown.InnerException?.Message, Contains.Substring("Amt element"));
        }

        [Test]
        public void GetMainToMoneyRate_IHttpRequestReturnsXmlWhereSecondAmtIsZero_ThrowsExchangeServiceUnavailableException()
        {
            _httpRequestMock.GetStringAsync("https://www.lb.lt//webservices/FxRates/FxRates.asmx/getCurrentFxRates?tp=EU")
                .Returns(@"<?xml version=""1.0"" encoding=""utf-8""?>
<FxRates xmlns=""http://www.lb.lt/WebServices/FxRates"">
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>RUB</Ccy>
      <Amt>0</Amt>
    </CcyAmt>
  </FxRate>
</FxRates>");

            var thrown = Assert.ThrowsAsync<ExchangeServiceUnavailableException>(() => _sut.GetMainToMoneyRate("RUB"));
            Assert.That(thrown.InnerException?.Message, Contains.Substring("Amt element"));
        }

        private const string responseExample = @"<?xml version=""1.0"" encoding=""utf-8""?>
<FxRates xmlns=""http://www.lb.lt/WebServices/FxRates"">
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>AUD</Ccy>
      <Amt>1.6274</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>BGN</Ccy>
      <Amt>1.9558</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>BRL</Ccy>
      <Amt>6.1976</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>CAD</Ccy>
      <Amt>1.5065</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>CHF</Ccy>
      <Amt>0.9448</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>CNY</Ccy>
      <Amt>7.8438</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>CZK</Ccy>
      <Amt>25.0980</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>DKK</Ccy>
      <Amt>7.4581</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>GBP</Ccy>
      <Amt>0.835180</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>HKD</Ccy>
      <Amt>8.6576</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>HUF</Ccy>
      <Amt>394.68</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>IDR</Ccy>
      <Amt>16886.09</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>ILS</Ccy>
      <Amt>4.2010</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>INR</Ccy>
      <Amt>92.8955</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>ISK</Ccy>
      <Amt>151.70</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>JPY</Ccy>
      <Amt>159.58</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>KRW</Ccy>
      <Amt>1486.62</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>MYR</Ccy>
      <Amt>4.6733</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>MXN</Ccy>
      <Amt>21.5915</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>NOK</Ccy>
      <Amt>11.6860</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>NZD</Ccy>
      <Amt>1.7770</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>PHP</Ccy>
      <Amt>62.30</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>PLN</Ccy>
      <Amt>4.2750</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>RON</Ccy>
      <Amt>4.9742</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>SEK</Ccy>
      <Amt>11.3620</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>SGD</Ccy>
      <Amt>1.4357</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>THB</Ccy>
      <Amt>36.6540</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>TRY</Ccy>
      <Amt>37.9745</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>USD</Ccy>
      <Amt>1.1119</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-09-23</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>ZAR</Ccy>
      <Amt>19.3253</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2022-03-01</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>RUB</Ccy>
      <Amt>117.2010</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>AED</Ccy>
      <Amt>4.074530</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>AFN</Ccy>
      <Amt>78.381120</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>ALL</Ccy>
      <Amt>99.797130</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>AMD</Ccy>
      <Amt>430.327960</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>ARS</Ccy>
      <Amt>1052.218480</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>AZN</Ccy>
      <Amt>1.8859</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>BAM</Ccy>
      <Amt>1.960280</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>BDT</Ccy>
      <Amt>132.567330</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>BHD</Ccy>
      <Amt>0.418150</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>BYN</Ccy>
      <Amt>3.630460</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>BOB</Ccy>
      <Amt>7.665610</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>CLP</Ccy>
      <Amt>1012.559210</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>COP</Ccy>
      <Amt>4543.381750</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>DJF</Ccy>
      <Amt>197.5475</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>DZD</Ccy>
      <Amt>148.336570</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>EGP</Ccy>
      <Amt>53.9366</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>ETB</Ccy>
      <Amt>122.152750</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>GEL</Ccy>
      <Amt>2.984150</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>GNF</Ccy>
      <Amt>9569.2531</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>YER</Ccy>
      <Amt>277.509450</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>IQD</Ccy>
      <Amt>1453.2485</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>IRR</Ccy>
      <Amt>46595.473380</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>JOD</Ccy>
      <Amt>0.785860</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>KES</Ccy>
      <Amt>142.828810</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>KGS</Ccy>
      <Amt>94.4358</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>KWD</Ccy>
      <Amt>0.3387</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>KZT</Ccy>
      <Amt>534.401630</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>LBP</Ccy>
      <Amt>99342.2925</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>LYD</Ccy>
      <Amt>5.281010</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>LKR</Ccy>
      <Amt>332.8882</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>MAD</Ccy>
      <Amt>10.774120</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>MDL</Ccy>
      <Amt>19.255320</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>MGA</Ccy>
      <Amt>5075.498120</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>MKD</Ccy>
      <Amt>61.607750</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>MNT</Ccy>
      <Amt>3751.267030</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>MZN</Ccy>
      <Amt>70.893010</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>PAB</Ccy>
      <Amt>1.109350</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>PEN</Ccy>
      <Amt>4.146970</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>PKR</Ccy>
      <Amt>309.480920</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>QAR</Ccy>
      <Amt>4.043580</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>RSD</Ccy>
      <Amt>117.019780</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>SAR</Ccy>
      <Amt>4.162840</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>SYP</Ccy>
      <Amt>14423.214030</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>TJS</Ccy>
      <Amt>11.836760</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>TMT</Ccy>
      <Amt>3.882390</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>TND</Ccy>
      <Amt>3.372090</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>TWD</Ccy>
      <Amt>35.371620</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>TZS</Ccy>
      <Amt>3014.103950</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>UAH</Ccy>
      <Amt>45.622020</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>UYU</Ccy>
      <Amt>44.701260</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>UZS</Ccy>
      <Amt>14031.613480</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>VES</Ccy>
      <Amt>40.542480</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>VND</Ccy>
      <Amt>27600.6280</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>XAF</Ccy>
      <Amt>663.635360</Amt>
    </CcyAmt>
  </FxRate>
  <FxRate>
    <Tp>EU</Tp>
    <Dt>2024-08-29</Dt>
    <CcyAmt>
      <Ccy>EUR</Ccy>
      <Amt>1</Amt>
    </CcyAmt>
    <CcyAmt>
      <Ccy>XOF</Ccy>
      <Amt>655.387340</Amt>
    </CcyAmt>
  </FxRate>
</FxRates>";
    }
}
