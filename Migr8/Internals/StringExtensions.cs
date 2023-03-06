namespace Migr8.Internals
{
    static class StringExtensions
    {
        public static string TrimUntil(this string str, char c)
        {
            var index = str.IndexOf(c);

            return index < 0 ? str : str.Substring(index + 1);
        }

        public static string TrimAfter(this string str, char c)
        {
            var index = str.IndexOf(c);

            return index < 0 ? str : str.Substring(0, index);
        }
    }
}