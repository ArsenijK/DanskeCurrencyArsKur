namespace DanskeCurrencyArsKur.Common
{
    public class CmdArgsProvider : ICmdArgsProvider
    {
        public string[] GetCommandLineArgs() => Environment.GetCommandLineArgs();
    }
}
