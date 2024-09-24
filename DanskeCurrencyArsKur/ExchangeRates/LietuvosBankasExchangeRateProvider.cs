using DanskeCurrencyArsKur.Common;
using System.Xml;
using System.Xml.Linq;

namespace DanskeCurrencyArsKur.ExchangeRates
{
    public class LietuvosBankasExchangeRateProvider : IExchangeRateProvider
    {
        private readonly IHttpRequest _httpRequest;

        public LietuvosBankasExchangeRateProvider(IHttpRequest httpRequest) => _httpRequest = httpRequest;

        public string GetUiDescription() => "Lietuvos bankas web service";

        public string GetMainCurrencyCode() => "EUR";

        public async Task<decimal> GetMoneyToMainRate(string moneyCurrencyCode) => 1 / await GetMainToMoneyRate(moneyCurrencyCode);

        public async Task<decimal> GetMainToMoneyRate(string moneyCurrencyCode) // TODO: method is pretty long, but to split need to decide how to handle edge cases and split into private methods accordingly
        {
            try
            {
                var response = await _httpRequest.GetStringAsync("https://www.lb.lt//webservices/FxRates/FxRates.asmx/getCurrentFxRates?tp=EU");
                var xml = XElement.Parse(response ?? throw new ApplicationException("Expected XML response, but it was NULL")); // for efficiency we can use XmlReader or XPathNavigator, also we could cache response for some time
                var fxRate = xml.Descendants("{http://www.lb.lt/WebServices/FxRates}FxRate")
                    .Where(fxr => fxr.Elements("{http://www.lb.lt/WebServices/FxRates}CcyAmt")
                            .Any(ccyAmt => ccyAmt.Element("{http://www.lb.lt/WebServices/FxRates}Ccy")?.Value == "EUR")
                        && fxr.Elements("{http://www.lb.lt/WebServices/FxRates}CcyAmt")
                            .Any(ccyAmt => ccyAmt.Element("{http://www.lb.lt/WebServices/FxRates}Ccy")?.Value == moneyCurrencyCode))
                    .SingleOrDefault() // maybe if there are multiple we can sort by date or something?
                        ?? throw new UnknownCurrencyCodeException(moneyCurrencyCode);

                decimal finalRate = 1;
                foreach (var ccyAmt in fxRate.Elements("{http://www.lb.lt/WebServices/FxRates}CcyAmt"))
                {
                    string ccy = (string)(ccyAmt.Element("{http://www.lb.lt/WebServices/FxRates}Ccy") ?? throw new ApplicationException("Expected Ccy element inside CcyAmt, but did not find any"));
                    decimal amt = (decimal)(ccyAmt.Element("{http://www.lb.lt/WebServices/FxRates}Amt") ?? throw new ApplicationException("Expected Amt element inside CcyAmt, but did not find any"));
                    if (0 == amt)
                        throw new ApplicationException("Expected Amt element to be non zero");

                    if (ccy == "EUR")
                        finalRate /= amt; // currently main amount of EUR is always 1, but in case it becomes something else like 1000 EUR or 0.001 EUR
                    else if (ccy == moneyCurrencyCode)
                        finalRate *= amt;
                }
                return finalRate;
            }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or XmlException or ApplicationException)
            {
                throw new ExchangeServiceUnavailableException(ex);
            }
        }
    }
}
