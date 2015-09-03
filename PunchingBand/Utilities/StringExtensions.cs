using System.Text.RegularExpressions;

namespace PunchingBand.Utilities
{
    public static class StringExtensions
    {
        public static string SplitCamelCase(this string input)
        {
            return Regex.Replace(input, "([A-Z])", " $1").Trim();
        }
    }
}
