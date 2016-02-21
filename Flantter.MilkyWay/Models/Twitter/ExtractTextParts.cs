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
        private static string CharFromInt(int code)
        {
            if (code <= char.MaxValue) return ((char)code).ToString();

            code -= 0x10000;
            return new string(new[]
            {
                (char)((code / 0x400) + 0xD800),
                (char)((code % 0x400) + 0xDC00)
            });
        }

        private static readonly Regex HtmlDecodeCharFromIntRegex = new Regex("&#([0-9]+);", RegexOptions.Compiled);
        private static readonly Regex HtmlDecodeHexNumberRegex = new Regex("&#x([0-9a-f]+);", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static string HtmlDecode(string source)
        {
            if (!source.Contains("&")) return source;
            var result = HtmlDecodeCharFromIntRegex.Replace(source, match => CharFromInt(int.Parse(match.Groups[1].Value)));
            result = HtmlDecodeHexNumberRegex.Replace(result, match => CharFromInt(int.Parse(match.Groups[1].Value, NumberStyles.HexNumber)));
            return result.Replace("&nbsp;", " ")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&")
                .Replace("&quot;", "\"")
                .Replace("&apos;", "'");
        }

        private static IEnumerable<string> EnumerateChars(string str)
        {
            for (var i = 0; i < str.Length; i++)
            {
                if (char.IsSurrogatePair(str, i))
                {
                    yield return new string(new[] { str[i], str[++i] });
                }
                else
                {
                    yield return str[i].ToString();
                }
            }
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

        private class TextPart : ITextPart
        {
            public TextPartType Type { get; set; }
            internal int Start { get; set; }
            internal int End { get; set; }
            public string RawText { get; set; }
            public string Text { get; set; }
            public object Entity { get; set; }
        }

        /// <summary>
        /// Enumerates parts split into Tweet Entities.
        /// </summary>
        /// <param name="text">The text such as <see cref="Status.Text"/>, <see cref="DirectMessage.Text"/> and <see cref="User.Description"/>.</param>
        /// <param name="entities">The <see cref="Entities"/> instance.</param>
        /// <returns>An <see cref="IEnumerable{ITextPart}"/> whose elements are parts of <paramref name="text"/>.</returns>
        public static IEnumerable<ITextPart> EnumerateTextParts(string text, Entities entities)
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
            var chars = EnumerateChars(text).ToArray();

            while (true)
            {
                var start = current.Previous?.Value.End ?? 0;
                var count = current.Value.Start - start;
                if (count > 0)
                {
                    var output = string.Concat(chars.Slice(start, count));
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
            if (lastStart < chars.Length)
            {
                var lastOutput = string.Concat(chars.Slice(lastStart));
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
    public interface ITextPart
    {
        /// <summary>
        /// The type of this instance.
        /// </summary>
        TextPartType Type { get; }

        /// <summary>
        /// The raw text.
        /// </summary>
        string RawText { get; }

        /// <summary>
        /// The decoded text.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// The base entity information.
        /// </summary>
        object Entity { get; }
    }

}
