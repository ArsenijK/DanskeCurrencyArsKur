namespace DanskeCurrencyArsKur.UI
{
    public class InvalidUserInputException : Exception
    {
        public InvalidUserInputException(string message) : base(message) { }
    }
}
