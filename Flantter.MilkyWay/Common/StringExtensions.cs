using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Flantter.MilkyWay.Common
{
    public static class StringExtensions
    {
        public static int LengthInTextElements(this string self)
        {
            return new StringInfo(self).LengthInTextElements;
        }

        public static IEnumerable<string> GetTextElementEnumerable(this string self)
        {
            var e = StringInfo.GetTextElementEnumerator(self);
            while (e.MoveNext())
                yield return (string) e.Current;
        }

        public static string SubstringByTextElements(this string self, int startIndex, int length)
        {
            var sub = self.GetTextElementEnumerable().Skip(startIndex).Take(length).ToArray();
            if (sub.Length != length)
                throw new ArgumentOutOfRangeException();

            return string.Concat(sub);
        }

        public static string EscapeEntity(this string text, bool escapeAllEntity = false)
        {
            var newtext = text
                .Replace("&", "&amp;")
                .Replace(">", "&gt;")
                .Replace("<", "&lt;");

            if (escapeAllEntity)
                newtext = newtext
                    .Replace("\"", "&quot;")
                    .Replace("'", "&apos;");

            return newtext;
        }

        public static string ResolveEntity(this string text, bool resolveAllEntity = true)
        {
            var newtext = text
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&");

            if (resolveAllEntity)
                newtext = newtext
                    .Replace("&quot;", "\"")
                    .Replace("&apos;", "'");

            return newtext;
        }
    }
}