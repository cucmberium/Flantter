// The MIT License (MIT)
//
// Copyright (c) 2014 azyobuzin
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.


// from https://github.com/CoreTweet/CoreTweetSupplement/blob/master/CoreTweetSupplement/CoreTweetSupplement.cs


using Flantter.MilkyWay.Models.Twitter.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter
{
    public static class ExtractTextParts
    {
        private static string CharFromInt(uint code)
        {
            if (code <= char.MaxValue) return ((char)code).ToString();

            code -= 0x10000;
            return new string(new[]
            {
                (char)((code / 0x400) + 0xD800),
                (char)((code % 0x400) + 0xDC00)
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
                    case "nbsp": sb.Append(' '); break;
                    case "lt": sb.Append('<'); break;
                    case "gt": sb.Append('>'); break;
                    case "amp": sb.Append('&'); break;
                    case "quot": sb.Append('"'); break;
                    case "apos": sb.Append('\''); break;
                    default:
                        if (s[0] == '#')
                        {
                            var code = s[1] == 'x'
                                ? uint.Parse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                                : uint.Parse(s.Substring(1), CultureInfo.InvariantCulture);
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


        private static List<DoubleUtf16Char> EnumerateChars(string str)
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

        private static string ToString(IList<DoubleUtf16Char> source, int start)
        {
            var sourceLen = source.Count;
            var arr = new char[sourceLen * 2];
            var strLen = 0;
            for (var i = start; i < sourceLen; i++)
            {
                var x = source[i];
                arr[strLen++] = x.X;
                if (char.IsHighSurrogate(x.X))
                    arr[strLen++] = x.Y;
            }
            return new string(arr, 0, strLen);
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

        private static IEnumerable<T> Slice<T>(this T[] source, int start)
        {
            var len = source.Length;
            for (var i = start; i < len; i++)
                yield return source[i];
        }

        private static IEnumerable<T> Slice<T>(this T[] source, int start, int count)
        {
            var end = start + count;
            for (var i = start; i < end; i++)
                yield return source[i];
        }

        /// <summary>
        /// Enumerates parts split into Tweet Entities.
        /// </summary>
        /// <param name="text">The text such as <see cref="Status.Text"/>, <see cref="DirectMessage.Text"/> and <see cref="User.Description"/>.</param>
        /// <param name="entities">The <see cref="Entities"/> instance.</param>
        /// <returns>An <see cref="IEnumerable{ITextPart}"/> whose elements are parts of <paramref name="text"/>.</returns>
        public static IEnumerable<TextPart> EnumerateTextParts(string text, Entities entities)
        {
            if (entities == null)
            {
                yield return new TextPart()
                {
                    RawText = text,
                    Text = HtmlDecode(text)
                };
                yield break;
            }

            var list = new LinkedList<TextPart>(
                (entities.HashTags)
                    .Select(e => new TextPart()
                    {
                        Type = TextPartType.Hashtag,
                        Start = e.Start,
                        End = e.End,
                        RawText = "#" + e.Tag,
                        Text = "#" + e.Tag,
                        Entity = e,
                    })
                    .Concat(
                        (entities.Urls)
                            .Select(e => new TextPart()
                            {
                                Type = TextPartType.Url,
                                Start = e.Start,
                                End = e.End,
                                RawText = e.Url,
                                Text = e.DisplayUrl,
                                Entity = e,

                            })
                    )
                    .Concat(
                        (entities.UserMentions)
                            .Select(e => new TextPart()
                            {
                                Type = TextPartType.UserMention,
                                Start = e.Start,
                                End = e.End,
                                RawText = "@" + e.ScreenName,
                                Text = "@" + e.ScreenName,
                                Entity = e,
                            })
                    )
                    .OrderBy(part => part.Start)
            );

            if (list.Count == 0)
            {
                yield return new TextPart()
                {
                    RawText = text,
                    Text = HtmlDecode(text)
                };
                yield break;
            }

            var current = list.First;
            var chars = EnumerateChars(text);

            while (true)
            {
                var start = current.Previous?.Value.End ?? 0;
                var count = current.Value.Start - start;
                if (count > 0)
                {
                    var output = ToString(chars, start, count);
                    yield return new TextPart()
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
            if (lastStart < chars.Count)
            {
                var lastOutput = ToString(chars, lastStart);
                yield return new TextPart()
                {
                    RawText = lastOutput,
                    Text = HtmlDecode(lastOutput)
                };
            }
        }

    }

    /// <summary>
    /// Types of <see cref="ITextPart"/>.
    /// </summary>
    public enum TextPartType
    {
        /// <summary>
        /// Plain text, which is related to no entity.
        /// <see cref="ITextPart.Entity"/> will be <c>null</c>.
        /// </summary>
        Plain,

        /// <summary>
        /// Hashtag.
        /// <see cref="ITextPart.Entity"/> will be a <see cref="HashtagEntity" /> instance.
        /// </summary>
        Hashtag,

        /// <summary>
        /// Cashtag.
        /// <see cref="ITextPart.Entity"/> will be a <see cref="CashtagEntity" /> instance.
        /// </summary>
        Cashtag,

        /// <summary>
        /// URL.
        /// <see cref="ITextPart.Entity"/> will be a <see cref="UrlEntity" /> instance.
        /// </summary>
        Url,

        /// <summary>
        /// User mention.
        /// <see cref="ITextPart.Entity"/> will be a <see cref="UserMentionEntity" /> instance.
        /// </summary>
        UserMention
    }

    /// <summary>
    /// A part of text.
    /// </summary>
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
            this.X = x;
            this.Y = default(char);
        }

        public DoubleUtf16Char(char x, char y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
