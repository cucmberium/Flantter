using System.Collections.Generic;
using System.Linq;
using Flantter.MilkyWay.Models.Exceptions;

namespace Flantter.MilkyWay.Models.Twitter
{
    public static class SuggestionService
    {
        public static SuggestionToken GetTokenFromPosition(IEnumerable<SuggestionToken> tokens, int pos)
        {
            for (var i = tokens.Count() - 1; i >= 0; i--)
                if (tokens.ElementAt(i).Pos <= pos)
                    return tokens.ElementAt(i);

            throw new SuggestionTokenNotFoundException();
        }

        public static IEnumerable<SuggestionToken> Tokenize(string text)
        {
            var tokens = TokenizeImpl("(" + text + ")").ToList();
            tokens.RemoveAt(0);
            tokens.RemoveAt(tokens.Count - 1);

            foreach (var token in tokens)
                token.Pos -= 1;

            return tokens;
        }

        private static IEnumerable<SuggestionToken> TokenizeImpl(string text)
        {
            var strPos = 0;
            const string tokens = "@#:.=<>!&|()\" \t\r\n";
            do
            {
                int begin;
                switch (text[strPos])
                {
                    case '#':
                        strPos++;
                        begin = strPos;
                        do
                        {
                            if (tokens.Contains(text[strPos].ToString()))
                            {
                                yield return new SuggestionToken(SuggestionToken.SuggestionTokenId.HashTag,
                                    text.Substring(begin, strPos - begin), begin - 1, strPos - (begin - 1));
                                break;
                            }
                            strPos++;
                        } while (strPos < text.Length);
                        break;
                    case '@':
                        strPos++;
                        begin = strPos;
                        do
                        {
                            if (tokens.Contains(text[strPos].ToString()))
                            {
                                yield return new SuggestionToken(SuggestionToken.SuggestionTokenId.ScreenName,
                                    text.Substring(begin, strPos - begin), begin - 1, strPos - (begin - 1));
                                break;
                            }
                            strPos++;
                        } while (strPos < text.Length);
                        break;
                    case ':':
                        strPos++;
                        begin = strPos;
                        do
                        {
                            if (tokens.Contains(text[strPos].ToString()))
                            {
                                yield return new SuggestionToken(SuggestionToken.SuggestionTokenId.Emoji,
                                    text.Substring(begin, strPos - begin), begin - 1, strPos - (begin - 1));
                                break;
                            }
                            strPos++;
                        } while (strPos < text.Length);
                        break;
                    case '.':
                    case '=':
                    case '<':
                    case '>':
                    case '!':
                    case '&':
                    case '|':
                    case '(':
                    case ')':
                    case '\"':
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        yield return new SuggestionToken(SuggestionToken.SuggestionTokenId.Literal, strPos);
                        strPos++;
                        break;
                    default:
                        begin = strPos;
                        do
                        {
                            if (tokens.Contains(text[strPos].ToString()))
                            {
                                yield return new SuggestionToken(SuggestionToken.SuggestionTokenId.String,
                                    text.Substring(begin, strPos - begin), begin, strPos - begin);
                                break;
                            }
                            strPos++;
                        } while (strPos < text.Length);
                        break;
                }
            } while (strPos < text.Length);
        }

        public class SuggestionToken
        {
            public enum SuggestionTokenId
            {
                String,
                HashTag,
                ScreenName,
                Emoji,
                Literal
            }

            public int Length;
            public int Pos;

            public SuggestionTokenId Type;
            public string Value;

            public SuggestionToken(SuggestionTokenId type, int pos)
            {
                Type = type;
                Value = string.Empty;
                Pos = pos;
                Length = 1;
            }

            public SuggestionToken(SuggestionTokenId type, int pos, int length)
            {
                Type = type;
                Value = string.Empty;
                Pos = pos;
                Length = length;
            }

            public SuggestionToken(SuggestionTokenId type, string value, int pos, int length)
            {
                Type = type;
                Value = value;
                Pos = pos;
                Length = length;
            }
        }
    }
}