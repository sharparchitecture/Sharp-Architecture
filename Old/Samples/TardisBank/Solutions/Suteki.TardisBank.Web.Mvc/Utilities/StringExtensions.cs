namespace Suteki.TardisBank.Web.Mvc.Utilities
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        public static string Join(this string[] values, string joinText)
        {
            var result = new StringBuilder();

            if (values.Length == 0) return string.Empty;

            result.Append(values[0]);

            for (int i = 1; i < values.Length; i++)
            {
                result.Append(joinText);
                result.Append(values[i]);
            }

            return result.ToString();
        }

        public static string TrimWithElipsis(this string text, int length)
        {
            if (text.Length <= length) return text;
            return text.Substring(0, length) + "...";
        }

        /// <summary>
        /// replacement for String.Format
        /// </summary>
        public static string With(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        /// <summary>
        /// prettily renders property names
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Pretty(this string text)
        {
            return DeCamel(text).Replace("_", " ");
        }

        public static void PrettyTest()
        {
            Console.WriteLine("hello_worldIAmYourNemesis".Pretty());
        }

        /// <summary>
        /// turns HelloWorld into Hello World
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string DeCamel(this string text)
        {
            return Regex.Replace(text, @"([A-Z])", @" $&").Trim();
        }

        public static void DeCamelTest()
        {
            Console.WriteLine("HelloWorldIAmYourNemesis".DeCamel());
        }
    }
}