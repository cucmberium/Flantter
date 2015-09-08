using Flantter.MilkyWay.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter
{
    public static class SuggestionService
    {
        
        public class SuggestionToken
        {
            public enum SuggestionTokenId
            {
                String,
                HashTag,
                ScreenName,
                Literal
            }

            public SuggestionTokenId Type;
            public string Value;
            public int Pos;
            public int Length;

            public SuggestionToken(SuggestionTokenId Type, int Pos)
            {
                this.Type = Type;
                this.Value = string.Empty;
                this.Pos = Pos;
                this.Length = 1;
            }

            public SuggestionToken(SuggestionTokenId Type, int Pos, int Length)
            {
                this.Type = Type;
                this.Value = string.Empty;
                this.Pos = Pos;
                this.Length = Length;
            }

            public SuggestionToken(SuggestionTokenId Type, string Value, int Pos, int Length)
            {
                this.Type = Type;
                this.Value = Value;
                this.Pos = Pos;
                this.Length = Length;
            }
        }

        public static SuggestionToken GetTokenFromPosition(IEnumerable<SuggestionToken> tokens, int pos)
        {
            for (var i = tokens.Count() - 1; i >= 0; i--)
            {
                if (tokens.ElementAt(i).Pos < pos)
                    return tokens.ElementAt(i);
            }

            throw new SuggestionTokenNotFoundException();
        }

        public static IEnumerable<SuggestionToken> Tokenize(string text)
        {
            var tokens = TokenizeImpl("(" + text.Replace("\r\n", "\n") + ")").ToList();
            tokens.RemoveAt(0); tokens.RemoveAt(tokens.Count - 1);

            foreach (var token in tokens)
                token.Pos -= 1;

            return tokens;
        }

        private static IEnumerable<SuggestionToken> TokenizeImpl(string text)
        {
            int strPos = 0, begin;
            const string Tokens = "@#.=<>!&|()\" \t\r\n";
            do
            {
                switch (text[strPos])
                {
                    case '#':
                        strPos++;
                        begin = strPos;
                        do
                        {
                            if (Tokens.Contains(text[strPos].ToString()))
                            {
                                yield return new SuggestionToken(SuggestionToken.SuggestionTokenId.HashTag, text.Substring(begin, strPos - begin), begin - 1, strPos - (begin - 1));
                                break;
                            }
                            strPos++;
                        } while (strPos < text.Length);
                        break;
                    case '@':
                        //yield return new SuggestionToken(SuggestionToken.SuggestionTokenId.ScreenName, strPos);
                        strPos++;
                        begin = strPos;
                        do
                        {
                            if (Tokens.Contains(text[strPos].ToString()))
                            {
                                yield return new SuggestionToken(SuggestionToken.SuggestionTokenId.ScreenName, text.Substring(begin, strPos - begin), begin - 1, strPos - (begin - 1));
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
                            if (Tokens.Contains(text[strPos].ToString()))
                            {
                                yield return new SuggestionToken(SuggestionToken.SuggestionTokenId.String, text.Substring(begin, strPos - begin), begin, strPos - begin);
                                break;
                            }
                            strPos++;
                        } while (strPos < text.Length);
                        break;
                }
            } while (strPos < text.Length);
        }
    }
}
