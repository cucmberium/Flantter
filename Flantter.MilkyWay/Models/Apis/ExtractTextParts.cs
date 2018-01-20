using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Flantter.MilkyWay.Models.Apis.Objects;

namespace Flantter.MilkyWay.Models.Apis
{
    public static class ExtractTextParts
    {
        private static string CharFromInt(uint code)
        {
            if (code <= char.MaxValue) return ((char) code).ToString();

            code -= 0x10000;
            return new string(new[]
            {
                (char) (code / 0x400 + 0xD800),
                (char) (code % 0x400 + 0xDC00)
            });
        }

        private static string HtmlDecode(string source)
        {
            if (source.IndexOf('&') == -1) return source;
            var sb = new StringBuilder(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                int semicolonIndex;
                if (source[i] != '&'
                    || (semicolonIndex = source.IndexOf(';', i + 3)) == -1)
                {
                    sb.Append(source[i]);
                    continue;
                }

                var s = source.Substring(i + 1, semicolonIndex - i - 1);
                switch (s)
                {
                    case "nbsp":
                        sb.Append(' ');
                        break;
                    case "lt":
                        sb.Append('<');
                        break;
                    case "gt":
                        sb.Append('>');
                        break;
                    case "amp":
                        sb.Append('&');
                        break;
                    case "quot":
                        sb.Append('"');
                        break;
                    case "apos":
                        sb.Append('\'');
                        break;
                    default:
                        if (s[0] == '#')
                        {
                            var code = s[1] == 'x'
                                ? uint.Parse(s.Substring(2), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo)
                                : uint.Parse(s.Substring(1), NumberFormatInfo.InvariantInfo);
                            sb.Append(CharFromInt(code));
                        }
                        else
                        {
                            sb.Append('&').Append(s).Append(';');
                        }

                        break;
                }

                i = semicolonIndex;
            }

            return sb.ToString();
        }

        private static List<DoubleUtf16Char> GetCodePoints(string str)
        {
            var result = new List<DoubleUtf16Char>(str.Length);
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                result.Add(char.IsHighSurrogate(c)
                    ? new DoubleUtf16Char(c, str[++i])
                    : new DoubleUtf16Char(c));
            }

            return result;
        }

        private static string ToString(IList<DoubleUtf16Char> source, int start, int count)
        {
            var arr = new char[count * 2];
            var end = start + count;
            var strLen = 0;
            for (var i = start; i < end; i++)
            {
                var x = source[i];
                arr[strLen++] = x.X;
                if (char.IsHighSurrogate(x.X))
                    arr[strLen++] = x.Y;
            }

            return new string(arr, 0, strLen);
        }

        public static IEnumerable<TextPart> EnumerateTextParts(string text, Entities entities)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var chars = GetCodePoints(text);
            return EnumerateTextParts(chars, entities, 0, chars.Count);
        }

        private static IEnumerable<TextPart> EnumerateTextParts(IList<DoubleUtf16Char> chars, Entities entities,
            int startIndex, int endIndex)
        {
            if (startIndex == endIndex) yield break;

            if (entities == null)
            {
                var text = ToString(chars, startIndex, endIndex - startIndex);
                yield return new TextPart
                {
                    RawText = text,
                    Text = HtmlDecode(text)
                };
                yield break;
            }

            var list = new LinkedList<TextPart>(
                (entities.HashTags ?? Enumerable.Empty<HashtagEntity>())
                .Select(e => new TextPart
                {
                    Type = TextPartType.Hashtag,
                    Start = e.Start,
                    End = e.End,
                    RawText = "#" + e.Tag,
                    Text = "#" + e.Tag,
                    Entity = e
                })
                .Concat(
                    (entities.Urls ?? Enumerable.Empty<UrlEntity>())
                    .Select(e => new TextPart
                    {
                        Type = TextPartType.Url,
                        Start = e.Start,
                        End = e.End,
                        RawText = e.Url,
                        Text = e.DisplayUrl,
                        Entity = e
                    })
                )
                .Concat(
                    (entities.UserMentions ?? Enumerable.Empty<UserMentionEntity>())
                    .Select(e => new TextPart
                    {
                        Type = TextPartType.UserMention,
                        Start = e.Start,
                        End = e.End,
                        RawText = e.Id.ToString(),
                        Text = "@" + e.ScreenName,
                        Entity = e
                    })
                )
                .Where(e => e.Start >= startIndex && e.Start < endIndex)
                .OrderBy(part => part.Start)
            );

            if (list.Count == 0)
            {
                var text = ToString(chars, startIndex, endIndex - startIndex);
                yield return new TextPart
                {
                    RawText = text,
                    Text = HtmlDecode(text)
                };
                yield break;
            }

            var current = list.First;

            while (true)
            {
                var start = current.Previous?.Value.End ?? startIndex;
                var count = current.Value.Start - start;
                if (count > 0)
                {
                    var output = ToString(chars, start, count);
                    yield return new TextPart
                    {
                        RawText = output,
                        Text = HtmlDecode(output)
                    };
                }

                yield return current.Value;

                if (current.Next == null) break;
                current = current.Next;
            }

            var lastStart = current.Value.End;
            if (lastStart < endIndex)
            {
                var lastOutput = ToString(chars, lastStart, endIndex - lastStart);
                yield return new TextPart
                {
                    RawText = lastOutput,
                    Text = HtmlDecode(lastOutput)
                };
            }
        }
    }

    public enum TextPartType
    {
        Plain,
        Hashtag,
        Cashtag,
        Url,
        UserMention,
        Emoji
    }

    public class TextPart
    {
        public TextPartType Type { get; set; }
        internal int Start { get; set; }
        internal int End { get; set; }
        public string RawText { get; set; }
        public string Text { get; set; }
        public object Entity { get; set; }
    }

    internal struct DoubleUtf16Char
    {
        public char X;
        public char Y;

        public DoubleUtf16Char(char x)
        {
            X = x;
            Y = default(char);
        }

        public DoubleUtf16Char(char x, char y)
        {
            X = x;
            Y = y;
        }
    }
}