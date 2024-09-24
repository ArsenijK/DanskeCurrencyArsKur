namespace DanskeCurrencyArsKur.Common
{
    public interface IHttpRequest
    {
        Task<string> GetStringAsync(string url);
    }
}
