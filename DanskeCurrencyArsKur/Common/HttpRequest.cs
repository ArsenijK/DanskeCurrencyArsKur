namespace DanskeCurrencyArsKur.Common
{
    public class HttpRequest : IHttpRequest
    {
        public async Task<string> GetStringAsync(string url)
        {
            using HttpClient client = new();
            return await client.GetStringAsync(url);
        }
    }
}
