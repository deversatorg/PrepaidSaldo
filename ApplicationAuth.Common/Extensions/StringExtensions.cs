using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ApplicationAuth.Common.Extensions
{
    public static class StringExtensions
    {
        public static void ThrowsWhenNullOrEmpty(this string str, string paramName = null)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                if (string.IsNullOrEmpty(paramName))
                    throw new ArgumentNullException();
                else
                    throw new ArgumentNullException(paramName);
            }
        }

        public static string Сapitalize(this string str)
        {
            str.ThrowsWhenNullOrEmpty();

            return $"{char.ToUpper(str[0])}{str.Substring(1)}";
        }

        public static string GetFirstLine(this string str) => str.Split(Environment.NewLine.ToCharArray()).FirstOrDefault();

        public static string HumanizePascalCase(this string str) => Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1]));

        public static string FindMostSimilarString(this string target, string[] strings)
        {
            string mostSimilarString = string.Empty;
            int maxLCSLength = 0;

            foreach (string str in strings)
            {
                int lcsLength = LCSLength(target, str);

                if (lcsLength > maxLCSLength)
                {
                    maxLCSLength = lcsLength;
                    mostSimilarString = str;
                }
            }

            return mostSimilarString;
        }

        public static int LCSLength(string str1, string str2)
        {
            int[,] dp = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    if (str1[i - 1] == str2[j - 1])
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                    else
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                }
            }

            return dp[str1.Length, str2.Length];
        }
    }
}
