namespace DanskeCurrencyArsKur.ExchangeRates
{
    public class ExchangeServiceUnavailableException : Exception
    {
        public ExchangeServiceUnavailableException(Exception innerException) : base("Exchange service is currently unavailable", innerException) { }
    }
}
