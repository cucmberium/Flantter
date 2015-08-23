using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Exceptions;
using Flantter.MilkyWay.Models.Twitter.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Filter
{
    struct Token
    {
        public enum TokenId
        {
            // 優先度 0
            Period, // .
            OpenBracket, // (
            CloseBracket, // )
            // OpenSquareBracket, // [
            // CloseSquareBracket, // ]

            // 優先度 1
            Equal, // ==
            NotEqual, // !=
            LessThanEqual, // <=
            GreaterThanEqual, // >=
            LessThan, // <
            GreaterThan, // >
            Contains, // Contains
            StartsWith, // StartsWith
            EndsWith, // EndsWith
            RegexMatch, // RegexMatch
            NotContains, // !Contains
            NotStartsWith, // !StartsWith
            NotEndsWith, // !EndsWith
            NotRegexMatch, // !RegexMatch

            // 優先度 2
            And, // &&
            Or, // ||
            Exclamation, // !

            // 優先度3
            String, // "こ↑こ↓のぶぶん"
            Numeric, // 数字(整数)
            Space, // 空白
            Boolean, // ブール代数
            Literal, // いろいろ
            Null, // Null

            // ???
            LiteralParam, // いろいろ変換した後のリテラル
            ExpressionParam // Expressionのパラメータ
        }

        public TokenId Type;
        public object Value;
        public int Pos;

        public Token(TokenId Type, int Pos)
            : this()
        {
            this.Type = Type;
            this.Value = null;
            this.Pos = Pos;
        }

        public Token(TokenId Type, object Value, int Pos)
            : this()
        {
            this.Type = Type;
            this.Value = Value;
            this.Pos = Pos;
        }
    }

    public static class Compiler
    {
        public static Delegate Compile(string filterString)
        {
            System.Diagnostics.Debug.WriteLine("\n-- Compile Filter --\n");

            var paramExpr = Expression.Parameter(typeof(Status));

            #region Check FilterString
            System.Diagnostics.Debug.WriteLine("\n-- String Check --\n");
            if (!filterString.StartsWith("("))
                filterString = "(" + filterString + ")";
            #endregion

            #region Tokenize
            System.Diagnostics.Debug.WriteLine("\n-- Tokenize --\n");
            var tokenQueue = new List<Token>(Tokenizer.Tokenize(filterString));
#if DEBUG
            foreach (var t in tokenQueue)
            {
                if (t.Value != null)
                    System.Diagnostics.Debug.WriteLine(t.Type.ToString() + " : " + t.Value.ToString());
                else
                    System.Diagnostics.Debug.WriteLine(t.Type.ToString());
            }
#endif
            #endregion

            #region TokenAnalyze
            System.Diagnostics.Debug.WriteLine("\n-- Token Analyze --\n");
            var tokenAnalyze = new TokenAnalyzer(tokenQueue, paramExpr);
            tokenAnalyze.TokenQueueToPolandQueue();

#if DEBUG
            foreach (var t in tokenAnalyze.PolandQueue)
            {
                if (t.Value != null)
                    System.Diagnostics.Debug.WriteLine(t.Type.ToString() + " : " + t.Value.ToString());
                else
                    System.Diagnostics.Debug.WriteLine(t.Type.ToString());
            }
#endif
            #endregion

            #region PolandTokenCompile
            System.Diagnostics.Debug.WriteLine("\n-- Poland Token Compile --\n");
            var PolandTokenCompile = new PolandTokenCompiler(tokenAnalyze.PolandQueue);
            PolandTokenCompile.PolandTokenCompile();
            var filter = Expression.Lambda<Func<Status, bool>>(PolandTokenCompile.CompiledExpression, paramExpr);
            System.Diagnostics.Debug.WriteLine(PolandTokenCompile.CompiledExpression.ToString());
            return filter.Compile();
            #endregion
        }

        internal static class Tokenizer
        {
            private const string ContainsString = "Contains";
            private const string StartsWithString = "StartsWith";
            private const string EndsWithString = "EndsWith";
            private const string RegexMatchString = "RegexMatch";
            private const string BooleanTrueString = "True";
            private const string BooleanFalseString = "False";
            private const string NullString = "Null";

            public static IEnumerable<Token> Tokenize(string filter)
            {
                int strPos = 0;

                do
                {
                    switch (filter[strPos])
                    {
                        case '.':
                            yield return new Token(Token.TokenId.Period, strPos);
                            strPos++;
                            break;
                        /*case ',':
                            yield return new Token(Token.TokenId.Comma, StrPos);
                            StrPos++;
                            break;*/
                        case '=':
                            strPos++;
                            if (strPos < filter.Length && filter[strPos] == '=')
                            {
                                yield return new Token(Token.TokenId.Equal, strPos);
                                strPos++;
                                break;
                            }
                            else
                            {
                                strPos--;
                                throw new FilterCompileException(FilterCompileException.ErrorCode.EqualMustUseWithOtherTokens, "「=」単体では使用できません", null, "");
                            }
                        case '<':
                            strPos++;
                            if (strPos < filter.Length && filter[strPos] == '=')
                            {
                                yield return new Token(Token.TokenId.LessThanEqual, strPos);
                                strPos++;
                                break;
                            }
                            else
                            {
                                yield return new Token(Token.TokenId.LessThan, strPos);
                                break;
                            }
                        case '>':
                            strPos++;
                            if (strPos < filter.Length && filter[strPos] == '=')
                            {
                                yield return new Token(Token.TokenId.GreaterThanEqual, strPos);
                                strPos++;
                                break;
                            }
                            else
                            {
                                yield return new Token(Token.TokenId.GreaterThan, strPos);
                                break;
                            }
                        case '!':
                            strPos++;
                            if (strPos < filter.Length && filter[strPos] == '=')
                            {
                                yield return new Token(Token.TokenId.NotEqual, strPos);
                                strPos++;
                                break;
                            }
                            else
                            {
                                if (strPos + ContainsString.Length < filter.Length && filter.Substring(strPos, ContainsString.Length).Contains(ContainsString))
                                {
                                    yield return new Token(Token.TokenId.NotContains, strPos);
                                    strPos += ContainsString.Length;
                                    break;
                                }
                                else if (strPos + StartsWithString.Length < filter.Length && filter.Substring(strPos, StartsWithString.Length).Contains(StartsWithString))
                                {
                                    yield return new Token(Token.TokenId.NotStartsWith, strPos);
                                    strPos += ContainsString.Length;
                                    break;
                                }
                                else if (strPos + EndsWithString.Length < filter.Length && filter.Substring(strPos, EndsWithString.Length).Contains(EndsWithString))
                                {
                                    yield return new Token(Token.TokenId.NotEndsWith, strPos);
                                    strPos += EndsWithString.Length;
                                    break;
                                }
                                else if (strPos + RegexMatchString.Length < filter.Length && filter.Substring(strPos, RegexMatchString.Length).Contains(RegexMatchString))
                                {
                                    yield return new Token(Token.TokenId.NotRegexMatch, strPos);
                                    strPos += RegexMatchString.Length;
                                    break;
                                }
                                else
                                {
                                    yield return new Token(Token.TokenId.Exclamation, strPos);
                                    break;
                                }
                            }
                        case 'C':
                            if (strPos + ContainsString.Length < filter.Length && filter.Substring(strPos, ContainsString.Length).Contains(ContainsString))
                            {
                                yield return new Token(Token.TokenId.Contains, strPos);
                                strPos += ContainsString.Length;
                                break;
                            }
                            else
                            {
                                goto default;
                            }
                        case 'S':
                            if (strPos + StartsWithString.Length < filter.Length && filter.Substring(strPos, StartsWithString.Length).Contains(StartsWithString))
                            {
                                yield return new Token(Token.TokenId.StartsWith, strPos);
                                strPos += StartsWithString.Length;
                                break;
                            }
                            else
                            {
                                goto default;
                            }
                        case 'E':
                            if (strPos + EndsWithString.Length < filter.Length && filter.Substring(strPos, EndsWithString.Length).Contains(EndsWithString))
                            {
                                yield return new Token(Token.TokenId.EndsWith, strPos);
                                strPos += EndsWithString.Length;
                                break;
                            }
                            else
                            {
                                goto default;
                            }
                        case 'R':
                            if (strPos + RegexMatchString.Length < filter.Length && filter.Substring(strPos, RegexMatchString.Length).Contains(RegexMatchString))
                            {
                                yield return new Token(Token.TokenId.RegexMatch, strPos);
                                strPos += RegexMatchString.Length;
                                break;
                            }
                            else
                            {
                                goto default;
                            }
                        case 'F':
                            if (strPos + BooleanFalseString.Length < filter.Length && filter.Substring(strPos, BooleanFalseString.Length).Contains(BooleanFalseString))
                            {
                                yield return new Token(Token.TokenId.Boolean, false, strPos);
                                strPos += BooleanFalseString.Length;
                                break;
                            }
                            else
                            {
                                goto default;
                            }
                        case 'T':
                            if (strPos + BooleanTrueString.Length < filter.Length && filter.Substring(strPos, BooleanTrueString.Length).Contains(BooleanTrueString))
                            {
                                yield return new Token(Token.TokenId.Boolean, true, strPos);
                                strPos += BooleanTrueString.Length;
                                break;
                            }
                            else
                            {
                                goto default;
                            }
                        case 'N':
                            if (strPos + NullString.Length < filter.Length && filter.Substring(strPos, NullString.Length).Contains(NullString))
                            {
                                yield return new Token(Token.TokenId.Null, true, strPos);
                                strPos += NullString.Length;
                                break;
                            }
                            else
                            {
                                goto default;
                            }
                        case '&':
                            strPos++;
                            if (strPos < filter.Length && filter[strPos] == '&')
                            {
                                yield return new Token(Token.TokenId.And, strPos);
                                strPos++;
                                break;
                            }
                            else
                            {
                                strPos--;
                                throw new FilterCompileException(FilterCompileException.ErrorCode.AndMustUseWithOtherTokens, "「&」単体では使用できません", null, "");
                            }
                        case '|':
                            strPos++;
                            if (strPos < filter.Length && filter[strPos] == '|')
                            {
                                yield return new Token(Token.TokenId.Or, strPos);
                                strPos++;
                                break;
                            }
                            else
                            {
                                strPos--;
                                throw new FilterCompileException(FilterCompileException.ErrorCode.VerticalBarMustUseWithOtherTokens, "「|」単体では使用できません", null, "");
                            }
                        case '(':
                            yield return new Token(Token.TokenId.OpenBracket, strPos);
                            strPos++;
                            break;
                        case ')':
                            yield return new Token(Token.TokenId.CloseBracket, strPos);
                            strPos++;
                            break;
                        case '"':
                            yield return new Token(Token.TokenId.String, GetFilterString(filter, ref strPos), strPos);
                            break;
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '0':
                            yield return new Token(Token.TokenId.Numeric, long.Parse(GetFilterNumeric(filter, ref strPos)), strPos);
                            break;
                        case ' ':
                        case '\t':
                        case '\r':
                        case '\n':
                            yield return new Token(Token.TokenId.Space, strPos);
                            strPos++;
                            break;
                        default:
                            int begin = strPos;
                            var Tokens = ".=<>!&|()\" \t\r\n";
                            do
                            {
                                if (Tokens.Contains(filter[strPos].ToString()))
                                {
                                    yield return new Token(Token.TokenId.Literal, filter.Substring(begin, strPos - begin), begin);
                                    break;
                                }
                                strPos++;
                            } while (strPos < filter.Length);
                            break;
                    }
                } while (strPos < filter.Length);
            }

            private static string GetFilterString(string filter, ref int cursor)
            {
                int begin = cursor++;
                while (cursor < filter.Length)
                {
                    if (filter[cursor] == '\\')
                    {
                        if (cursor + 1 == filter.Length)
                        {
                            throw new FilterCompileException(FilterCompileException.ErrorCode.FilterEndWithBacksrash, "バックスラッシュでクエリが終了しています", null, "");
                        }
                        else if (filter[cursor + 1] == '"' || filter[cursor + 1] == '\\')
                        {
                            cursor++;
                        }
                    }
                    else if (filter[cursor] == '"')
                    {
                        cursor++;
                        return filter.Substring(begin + 1, cursor - begin - 2).Replace("\\\"", "\"").Replace("\\\\", "\\");
                    }
                    cursor++;
                }
                throw new FilterCompileException(FilterCompileException.ErrorCode.StringNotEnd, "文字列が終了していません", null, "");
            }

            private static string GetFilterNumeric(string filter, ref int cursor)
            {
                int begin = cursor;
                var NumericToken = "1234567890";
                while (cursor < filter.Length)
                {
                    if (NumericToken.Contains(filter[cursor].ToString()))
                    {
                        cursor++;
                        continue;
                    }
                    else
                    {
                        return filter.Substring(begin, cursor - begin);
                    }
                }
                return filter.Substring(begin, cursor - begin);
            }
        }

        internal class TokenAnalyzer
        {
            public List<Token> TokenQueue { get; set; }
            public List<Token> PolandQueue { get; set; }

            private List<Token> tempQueue;

            private int openBracketCount;
            private int closeBracketCount;

            private ParameterExpression ParamExpr;

            public TokenAnalyzer(IEnumerable<Token> tokens, ParameterExpression ParamExpr)
            {
                this.ParamExpr = ParamExpr;

                PolandQueue = new List<Token>();
                TokenQueue = new List<Token>();

                tempQueue = new List<Token>();

                openBracketCount = 0;
                closeBracketCount = 0;

                foreach (var t in tokens)
                {
                    if (t.Type == Token.TokenId.Space)
                        continue;

                    TokenQueue.Add(t);
                }
            }

            public void TokenQueueToPolandQueue()
            {
                int cursor;

                for (cursor = 0; cursor < TokenQueue.Count; cursor++)
                {
                    var cursorToken = TokenQueue[cursor];

                    switch (cursorToken.Type)
                    {
                        case Token.TokenId.Numeric:
                        case Token.TokenId.String:
                        case Token.TokenId.Boolean:
                        case Token.TokenId.Null:
                            PolandQueue.Add(cursorToken);
                            break;
                        case Token.TokenId.Literal:
                            PolandQueue.Add(new Token { Type = Token.TokenId.LiteralParam, Pos = -1, Value = LiteralParam(ref cursor) });
                            break;
                        case Token.TokenId.Equal:
                        case Token.TokenId.NotEqual:
                        case Token.TokenId.LessThanEqual:
                        case Token.TokenId.GreaterThanEqual:
                        case Token.TokenId.LessThan:
                        case Token.TokenId.GreaterThan:
                        case Token.TokenId.Contains:
                        case Token.TokenId.StartsWith:
                        case Token.TokenId.EndsWith:
                        case Token.TokenId.RegexMatch:
                        case Token.TokenId.NotContains:
                        case Token.TokenId.NotStartsWith:
                        case Token.TokenId.NotEndsWith:
                        case Token.TokenId.NotRegexMatch:
                            tempQueue.Add(cursorToken);
                            break;
                        case Token.TokenId.And:
                        case Token.TokenId.Or:
                        case Token.TokenId.Exclamation:
                            tempQueue.Add(cursorToken);
                            TempQueueCheckPriority();
                            break;
                        case Token.TokenId.OpenBracket:
                            tempQueue.Add(cursorToken);
                            openBracketCount++;
                            break;
                        case Token.TokenId.CloseBracket:
                            tempQueue.Add(cursorToken);
                            closeBracketCount++;
                            if (closeBracketCount > openBracketCount)
                                throw new FilterCompileException(FilterCompileException.ErrorCode.CloseBracketPositionIsWrong, "不正な位置に閉じ括弧「)」が存在します", null);
                            else
                                TempQueueAnnihilationBracket();
                            break;
                    }
                }

                if (openBracketCount != closeBracketCount)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.CloseBracketCountAndOpenBracketCountIsDiffer, "開き括弧と閉じ括弧の数が一致しません", null);

                for (int tempQueueCursor = tempQueue.Count - 1; tempQueueCursor >= 0; tempQueueCursor--)
                {
                    if (tempQueue[tempQueueCursor].Type == Token.TokenId.OpenBracket || tempQueue[tempQueueCursor].Type == Token.TokenId.CloseBracket)
                        throw new FilterCompileException(FilterCompileException.ErrorCode.CloseBracketCountAndOpenBracketCountIsDiffer, "開き括弧と閉じ括弧の数が一致しません", null);

                    PolandQueue.Add(tempQueue[tempQueueCursor]);
                }
                tempQueue.Clear();
            }

            private void TempQueueCheckPriority()
            {
                if (tempQueue.Count <= 2)
                    return;

                int cursor = tempQueue.Count - 2;

                if (tempQueue[cursor].Type == Token.TokenId.CloseBracket || tempQueue[cursor].Type == Token.TokenId.OpenBracket)
                    return;
                else if (tempQueue[cursor].Type == Token.TokenId.And || tempQueue[cursor].Type == Token.TokenId.Or || tempQueue[cursor].Type == Token.TokenId.Exclamation)
                    return;
                else if (tempQueue[cursor].Type == Token.TokenId.String || tempQueue[cursor].Type == Token.TokenId.Numeric || tempQueue[cursor].Type == Token.TokenId.Literal || tempQueue[cursor].Type == Token.TokenId.LiteralParam || tempQueue[cursor].Type == Token.TokenId.Null)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.InternalError, "内部エラー", null);
                else
                {
                    PolandQueue.Add(tempQueue[cursor]);
                    tempQueue.RemoveAt(cursor);
                }
            }

            private void TempQueueAnnihilationBracket()
            {
                if (tempQueue.Count <= 1)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.InternalError, "内部エラー", null);

                if (tempQueue[tempQueue.Count - 1].Type != Token.TokenId.CloseBracket)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.CloseBracketNotExist, "開き括弧が存在しません", null);

                bool existOpenBracket = false;
                int cursor;
                for (cursor = tempQueue.Count - 2; cursor >= 0; cursor--)
                {
                    if (tempQueue[cursor].Type == Token.TokenId.OpenBracket)
                    {
                        existOpenBracket = true;
                        break;
                    }
                    else if (tempQueue[cursor].Type == Token.TokenId.CloseBracket)
                    {
                        throw new FilterCompileException(FilterCompileException.ErrorCode.CloseBracketPositionIsWrong, "不正な位置に閉じ括弧「)」が存在します", null);
                    }
                    else if (tempQueue[cursor].Type == Token.TokenId.String || tempQueue[cursor].Type == Token.TokenId.Numeric || tempQueue[cursor].Type == Token.TokenId.Literal || tempQueue[cursor].Type == Token.TokenId.LiteralParam || tempQueue[cursor].Type == Token.TokenId.Null)
                    {
                        throw new FilterCompileException(FilterCompileException.ErrorCode.InternalError, "内部エラー", null);
                    }
                    else
                    {
                        PolandQueue.Add(tempQueue[cursor]);
                        tempQueue.Remove(tempQueue[cursor]);
                        continue;
                    }
                }

                if (existOpenBracket == false)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.CloseBracketNotExist, "開き括弧が存在しません", null);

                int openBracketPosition = cursor;
                int closeBracketPosition = tempQueue.Count - 1;

                tempQueue.RemoveAt(closeBracketPosition);
                tempQueue.RemoveAt(openBracketPosition);
            }

            private object LiteralParam(ref int cursor)
            {
                if (TokenQueue[cursor].Type != Token.TokenId.Literal)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.InternalError, "内部エラー", null);
                
                var startCursorPosition = cursor;

                var literalString = TokenQueue[startCursorPosition].Value as string;

                #region CreateAt
                if (literalString == "CreateAt")
                {
                    cursor = startCursorPosition;
                    return Expression.Convert(Expression.Property(this.ParamExpr, "CreateAt"), typeof(string));
                }
                #endregion
                #region Entities
                else if (literalString == "Entities")
                {
                    startCursorPosition++;
                    if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                    {
                        throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalString + "\"に直接アクセスすることは出来ません", null, literalString);
                    }
                    else
                    {
                        startCursorPosition++;
                        if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                        {
                            throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                        }
                        else
                        {
                            var literalEntitiesString = TokenQueue[startCursorPosition].Value as string;
                            #region HashTags
                            if (literalEntitiesString == "HashTags")
                            {
                                startCursorPosition++;
                                if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                                {
                                    throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalEntitiesString + "\"に直接アクセスすることは出来ません", null, literalEntitiesString);
                                }
                                else
                                {
                                    startCursorPosition++;
                                    if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                                    {
                                        throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                                    }
                                    else
                                    {
                                        var literalHashTagEntitiesString = TokenQueue[startCursorPosition].Value as string;
                                        if (literalHashTagEntitiesString == "Count")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "Entities"), "HashTags"), "Count"), typeof(long));
                                        }
                                        else
                                        {
                                            throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalHashTagEntitiesString, null, literalHashTagEntitiesString);
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region Urls
                            else if (literalEntitiesString == "Urls")
                            {
                                startCursorPosition++;
                                if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                                {
                                    throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalEntitiesString + "\"に直接アクセスすることは出来ません", null, literalEntitiesString);
                                }
                                else
                                {
                                    startCursorPosition++;
                                    if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                                    {
                                        throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                                    }
                                    else
                                    {
                                        var literalUrlEntitiesString = TokenQueue[startCursorPosition].Value as string;
                                        if (literalUrlEntitiesString == "Count")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "Entities"), "Urls"), "Count"), typeof(long));
                                        }
                                        else
                                        {
                                            throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalUrlEntitiesString, null, literalUrlEntitiesString);
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region Medias
                            else if (literalEntitiesString == "Medias")
                            {
                                startCursorPosition++;
                                if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                                {
                                    throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalEntitiesString + "\"に直接アクセスすることは出来ません", null, literalEntitiesString);
                                }
                                else
                                {
                                    startCursorPosition++;
                                    if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                                    {
                                        throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                                    }
                                    else
                                    {
                                        var literalMediaEntitiesString = TokenQueue[startCursorPosition].Value as string;
                                        if (literalMediaEntitiesString == "Count")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "Entities"), "Medias"), "Count"), typeof(long));
                                        }
                                        else
                                        {
                                            throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalMediaEntitiesString, null, literalMediaEntitiesString);
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region UserMentions
                            else if (literalEntitiesString == "UserMentions")
                            {
                                startCursorPosition++;
                                if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                                {
                                    throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalEntitiesString + "\"に直接アクセスすることは出来ません", null, literalEntitiesString);
                                }
                                else
                                {
                                    startCursorPosition++;
                                    if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                                    {
                                        throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                                    }
                                    else
                                    {
                                        var literalUserMentionEntitiesString = TokenQueue[startCursorPosition].Value as string;
                                        if (literalUserMentionEntitiesString == "Count")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "Entities"), "UserMentions"), "Count"), typeof(long));
                                        }
                                        else
                                        {
                                            throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalUserMentionEntitiesString, null, literalUserMentionEntitiesString);
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region Other
                            else
                            {
                                throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalEntitiesString, null, literalEntitiesString);
                            }
                            #endregion
                        }
                    }
                }
                #endregion
                #region FavoriteCount
                else if (literalString == "FavoriteCount")
                {
                    cursor = startCursorPosition;
                    return Expression.Convert(Expression.Property(this.ParamExpr, "FavoriteCount"), typeof(long));
                }
                #endregion
                #region RetweetCount
                else if (literalString == "RetweetCount")
                {
                    cursor = startCursorPosition;
                    return Expression.Convert(Expression.Property(this.ParamExpr, "RetweetCount"), typeof(long));
                }
                #endregion
                #region InReplyToStatusId
                else if (literalString == "InReplyToStatusId")
                {
                    cursor = startCursorPosition;
                    return Expression.Property(this.ParamExpr, "InReplyToStatusId");
                }
                #endregion
                #region InReplyToScreenName
                else if (literalString == "InReplyToScreenName")
                {
                    cursor = startCursorPosition;
                    return Expression.Property(this.ParamExpr, "InReplyToScreenName");
                }
                #endregion
                #region InReplyToUserId
                else if (literalString == "InReplyToUserId")
                {
                    cursor = startCursorPosition;
                    return Expression.Property(this.ParamExpr, "InReplyToUserId");
                }
                #endregion
                #region Id
                else if (literalString == "Id")
                {
                    cursor = startCursorPosition;
                    return Expression.Property(this.ParamExpr, "Id");
                }
                #endregion
                #region Source
                else if (literalString == "Source")
                {
                    cursor = startCursorPosition;
                    return Expression.Property(this.ParamExpr, "Source");
                }
                #endregion
                #region Text
                else if (literalString == "Text")
                {
                    cursor = startCursorPosition;
                    return Expression.Property(this.ParamExpr, "Text");
                }
                #endregion
                #region User
                else if (literalString == "User")
                {
                    startCursorPosition++;
                    if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                    {
                        throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalString + "\"に直接アクセスすることは出来ません", null, literalString);
                    }
                    else
                    {
                        startCursorPosition++;
                        if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                        {
                            throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                        }
                        else
                        {
                            var literalUserString = TokenQueue[startCursorPosition].Value as string;
                            #region CreateAt
                            if (literalString == "CreateAt")
                            {
                                cursor = startCursorPosition;
                                return Expression.Convert(Expression.Property(Expression.Property(this.ParamExpr, "User"), "CreateAt"), typeof(string));
                            }
                            #endregion
                            #region Description
                            if (literalUserString == "Description")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "Description");
                            }
                            #endregion
                            #region FavoritesCount
                            else if (literalUserString == "FavouritesCount")
                            {
                                cursor = startCursorPosition;
                                return Expression.Convert(Expression.Property(Expression.Property(this.ParamExpr, "User"), "FavouritesCount"), typeof(long));
                            }
                            #endregion
                            #region FollowersCount
                            else if (literalUserString == "FollowersCount")
                            {
                                cursor = startCursorPosition;
                                return Expression.Convert(Expression.Property(Expression.Property(this.ParamExpr, "User"), "FollowersCount"), typeof(long));
                            }
                            #endregion
                            #region FriendsCount
                            else if (literalUserString == "FriendsCount")
                            {
                                cursor = startCursorPosition;
                                return Expression.Convert(Expression.Property(Expression.Property(this.ParamExpr, "User"), "FriendsCount"), typeof(long));
                            }
                            #endregion
                            #region Id
                            else if (literalUserString == "Id")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "Id");
                            }
                            #endregion
                            #region IsFollowRequestSent
                            else if (literalUserString == "IsFollowRequestSent")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "IsFollowRequestSent");
                            }
                            #endregion
                            #region IsMuting
                            else if (literalUserString == "IsMuting")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "IsMuting");
                            }
                            #endregion
                            #region IsProtected
                            else if (literalUserString == "IsProtected")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "IsProtected");
                            }
                            #endregion
                            #region IsVerified
                            else if (literalUserString == "IsVerified")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "IsVerified");
                            }
                            #endregion
                            #region IsVerified
                            else if (literalUserString == "IsVerified")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "IsVerified");
                            }
                            #endregion
                            #region Lang
                            else if (literalUserString == "Language")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "Language");
                            }
                            #endregion
                            #region ListedCount
                            else if (literalUserString == "ListedCount")
                            {
                                cursor = startCursorPosition;
                                return Expression.Convert(Expression.Property(Expression.Property(this.ParamExpr, "User"), "ListedCount"), typeof(long));
                            }
                            #endregion
                            #region Location
                            else if (literalUserString == "Location")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "Location");
                            }
                            #endregion
                            #region Name
                            else if (literalUserString == "Name")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "Name");
                            }
                            #endregion
                            #region ProfileBackgroundImageUrl
                            else if (literalUserString == "ProfileBackgroundImageUrl")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "ProfileBackgroundImageUrl");
                            }
                            #endregion
                            #region ProfileBannerUrl
                            else if (literalUserString == "ProfileBannerUrl")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "ProfileBannerUrl");
                            }
                            #endregion
                            #region ProfileImageUrl
                            else if (literalUserString == "ProfileImageUrl")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "ProfileImageUrl");
                            }
                            #endregion
                            #region ScreenName
                            else if (literalUserString == "ScreenName")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "ScreenName");
                            }
                            #endregion
                            #region StatusesCount
                            else if (literalUserString == "StatusesCount")
                            {
                                cursor = startCursorPosition;
                                return Expression.Convert(Expression.Property(Expression.Property(this.ParamExpr, "User"), "StatusesCount"), typeof(long));
                            }
                            #endregion
                            #region TimeZone
                            else if (literalUserString == "TimeZone")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "TimeZone");
                            }
                            #endregion
                            #region Url
                            else if (literalUserString == "Url")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "User"), "Url");
                            }
                            #endregion
                            #region Other
                            else
                            {
                                throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalUserString, null, literalUserString);
                            }
                            #endregion
                        }
                    }
                }
                #endregion
                #region IsFavorited
                else if (literalString == "IsFavorited")
                {
                    cursor = startCursorPosition;
                    return Expression.Property(this.ParamExpr, "IsFavorited");
                }
                #endregion
                #region IsRetweeted
                else if (literalString == "IsRetweeted")
                {
                    cursor = startCursorPosition;
                    return Expression.Property(this.ParamExpr, "IsRetweeted");
                }
                #endregion
                #region RetweetInformation
                else if (literalString == "RetweetInformation")
                {
                    startCursorPosition++;
                    if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                    {
                        throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalString + "\"に直接アクセスすることは出来ません", null, literalString);
                    }
                    else
                    {
                        startCursorPosition++;
                        if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                        {
                            throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                        }
                        else
                        {
                            var literalRetweetInformationString = TokenQueue[startCursorPosition].Value as string;
                            #region User
                            if (literalRetweetInformationString == "User")
                            {
                                startCursorPosition++;
                                if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                                {
                                    throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalRetweetInformationString + "\"に直接アクセスすることは出来ません", null, literalRetweetInformationString);
                                }
                                else
                                {
                                    startCursorPosition++;
                                    if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                                    {
                                        throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                                    }
                                    else
                                    {
                                        var literalUserString = TokenQueue[startCursorPosition].Value as string;
                                        #region CreateAt
                                        if (literalString == "CreateAt")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "CreateAt"), typeof(string));
                                        }
                                        #endregion
                                        #region Description
                                        if (literalUserString == "Description")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "Description");
                                        }
                                        #endregion
                                        #region FavoritesCount
                                        else if (literalUserString == "FavouritesCount")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "FavouritesCount"), typeof(long));
                                        }
                                        #endregion
                                        #region FollowersCount
                                        else if (literalUserString == "FollowersCount")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "FollowersCount"), typeof(long));
                                        }
                                        #endregion
                                        #region FriendsCount
                                        else if (literalUserString == "FriendsCount")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "FriendsCount"), typeof(long));
                                        }
                                        #endregion
                                        #region Id
                                        else if (literalUserString == "Id")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "Id");
                                        }
                                        #endregion
                                        #region IsFollowRequestSent
                                        else if (literalUserString == "IsFollowRequestSent")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "IsFollowRequestSent");
                                        }
                                        #endregion
                                        #region IsMuting
                                        else if (literalUserString == "IsMuting")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "IsMuting");
                                        }
                                        #endregion
                                        #region IsProtected
                                        else if (literalUserString == "IsProtected")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "IsProtected");
                                        }
                                        #endregion
                                        #region IsVerified
                                        else if (literalUserString == "IsVerified")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "IsVerified");
                                        }
                                        #endregion
                                        #region Language
                                        else if (literalUserString == "Language")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "Language");
                                        }
                                        #endregion
                                        #region ListedCount
                                        else if (literalUserString == "ListedCount")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "ListedCount"), typeof(long));
                                        }
                                        #endregion
                                        #region Location
                                        else if (literalUserString == "Location")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "Location");
                                        }
                                        #endregion
                                        #region Name
                                        else if (literalUserString == "Name")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "Name");
                                        }
                                        #endregion
                                        #region ProfileBackgroundImageUrl
                                        else if (literalUserString == "ProfileBackgroundImageUrl")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "ProfileBackgroundImageUrl");
                                        }
                                        #endregion
                                        #region ProfileBannerUrl
                                        else if (literalUserString == "ProfileBannerUrl")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "ProfileBannerUrl");
                                        }
                                        #endregion
                                        #region ProfileImageUrl
                                        else if (literalUserString == "ProfileImageUrl")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "ProfileImageUrl");
                                        }
                                        #endregion
                                        #region ScreenName
                                        else if (literalUserString == "ScreenName")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "ScreenName");
                                        }
                                        #endregion
                                        #region StatusesCount
                                        else if (literalUserString == "StatusesCount")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "StatusesCount"), typeof(long));
                                        }
                                        #endregion
                                        #region TimeZone
                                        else if (literalUserString == "TimeZone")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "TimeZone");
                                        }
                                        #endregion
                                        #region Url
                                        else if (literalUserString == "Url")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "User"), "Url");
                                        }
                                        #endregion
                                        #region Other
                                        else
                                        {
                                            throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalUserString, null, literalUserString);
                                        }
                                        #endregion
                                    }
                                }
                            }
                            #endregion
                            #region Id
                            if (literalRetweetInformationString == "Id")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "Id");
                            }
                            #endregion
                            #region CreatedAt
                            if (literalRetweetInformationString == "CreatedAt")
                            {
                                cursor = startCursorPosition;
                                return Expression.Convert(Expression.Property(Expression.Property(this.ParamExpr, "RetweetInformation"), "CreatedAt"), typeof(string));
                            }
                            #endregion
                            #region Other
                            else
                            {
                                throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalRetweetInformationString, null, literalRetweetInformationString);
                            }
                            #endregion
                        }
                    }
                }
                #endregion
                #region HasRetweetInformation
                else if (literalString == "HasRetweetInformation")
                {
                    cursor = startCursorPosition;
                    return Expression.Property(this.ParamExpr, "HasRetweetInformation");
                }
                #endregion
                #region QuotedStatusId
                else if (literalString == "QuotedStatusId")
                {
                    cursor = startCursorPosition;
                    return Expression.Property(this.ParamExpr, "QuotedStatusId");
                }
                #endregion
                #region QuotedStatus
                else if (literalString == "QuotedStatus")
                {
                    startCursorPosition++;
                    if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                    {
                        throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalString + "\"に直接アクセスすることは出来ません", null, literalString);
                    }
                    else
                    {
                        startCursorPosition++;
                        if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                        {
                            throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                        }
                        else
                        {
                            var literalQuotedStatusString = TokenQueue[startCursorPosition].Value as string;
                            #region CreatedAt
                            if (literalQuotedStatusString == "CreatedAt")
                            {
                                cursor = startCursorPosition;
                                return Expression.Convert(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "CreatedAt"), typeof(string));
                            }
                            #endregion
                            #region Entities
                            else if (literalQuotedStatusString == "Entities")
                            {
                                startCursorPosition++;
                                if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                                {
                                    throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalString + "\"に直接アクセスすることは出来ません", null, literalQuotedStatusString);
                                }
                                else
                                {
                                    startCursorPosition++;
                                    if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                                    {
                                        throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                                    }
                                    else
                                    {
                                        var literalEntitiesString = TokenQueue[startCursorPosition].Value as string;
                                        #region HashTags
                                        if (literalEntitiesString == "HashTags")
                                        {
                                            startCursorPosition++;
                                            if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                                            {
                                                throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalEntitiesString + "\"に直接アクセスすることは出来ません", null, literalEntitiesString);
                                            }
                                            else
                                            {
                                                startCursorPosition++;
                                                if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                                                {
                                                    throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                                                }
                                                else
                                                {
                                                    var literalHashTagEntitiesString = TokenQueue[startCursorPosition].Value as string;
                                                    if (literalHashTagEntitiesString == "Count")
                                                    {
                                                        cursor = startCursorPosition;
                                                        return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "Entities"), "HashTags"), "Count"), typeof(long));
                                                    }
                                                    else
                                                    {
                                                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalHashTagEntitiesString, null, literalHashTagEntitiesString);
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                        #region Urls
                                        else if (literalEntitiesString == "Urls")
                                        {
                                            startCursorPosition++;
                                            if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                                            {
                                                throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalEntitiesString + "\"に直接アクセスすることは出来ません", null, literalEntitiesString);
                                            }
                                            else
                                            {
                                                startCursorPosition++;
                                                if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                                                {
                                                    throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                                                }
                                                else
                                                {
                                                    var literalUrlEntitiesString = TokenQueue[startCursorPosition].Value as string;
                                                    if (literalUrlEntitiesString == "Count")
                                                    {
                                                        cursor = startCursorPosition;
                                                        return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "Entities"), "Urls"), "Count"), typeof(long));
                                                    }
                                                    else
                                                    {
                                                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalUrlEntitiesString, null, literalUrlEntitiesString);
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                        #region Medias
                                        else if (literalEntitiesString == "Medias")
                                        {
                                            startCursorPosition++;
                                            if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                                            {
                                                throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalEntitiesString + "\"に直接アクセスすることは出来ません", null, literalEntitiesString);
                                            }
                                            else
                                            {
                                                startCursorPosition++;
                                                if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                                                {
                                                    throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                                                }
                                                else
                                                {
                                                    var literalMediaEntitiesString = TokenQueue[startCursorPosition].Value as string;
                                                    if (literalMediaEntitiesString == "Count")
                                                    {
                                                        cursor = startCursorPosition;
                                                        return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "Entities"), "Medias"), "Count"), typeof(long));
                                                    }
                                                    else
                                                    {
                                                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalMediaEntitiesString, null, literalMediaEntitiesString);
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                        #region UserMentions
                                        else if (literalEntitiesString == "UserMentions")
                                        {
                                            startCursorPosition++;
                                            if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                                            {
                                                throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalEntitiesString + "\"に直接アクセスすることは出来ません", null, literalEntitiesString);
                                            }
                                            else
                                            {
                                                startCursorPosition++;
                                                if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                                                {
                                                    throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                                                }
                                                else
                                                {
                                                    var literalUserMentionEntitiesString = TokenQueue[startCursorPosition].Value as string;
                                                    if (literalUserMentionEntitiesString == "Count")
                                                    {
                                                        cursor = startCursorPosition;
                                                        return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "Entities"), "UserMentions"), "Count"), typeof(long));
                                                    }
                                                    else
                                                    {
                                                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalUserMentionEntitiesString, null, literalUserMentionEntitiesString);
                                                    }
                                                }
                                            }
                                        }
                                        #endregion
                                        #region Other
                                        else
                                        {
                                            throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalEntitiesString, null, literalEntitiesString);
                                        }
                                        #endregion
                                    }
                                }
                            }
                            #endregion
                            #region FavoriteCount
                            if (literalQuotedStatusString == "FavoriteCount")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "FavoriteCount");
                            }
                            #endregion
                            #region RetweetCount
                            if (literalQuotedStatusString == "RetweetCount")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "RetweetCount");
                            }
                            #endregion
                            #region InReplyToStatusId
                            if (literalQuotedStatusString == "InReplyToStatusId")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "InReplyToStatusId");
                            }
                            #endregion
                            #region InReplyToScreenName
                            if (literalQuotedStatusString == "InReplyToScreenName")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "InReplyToScreenName");
                            }
                            #endregion
                            #region InReplyToUserId
                            if (literalQuotedStatusString == "InReplyToUserId")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "InReplyToUserId");
                            }
                            #endregion
                            #region Id
                            if (literalQuotedStatusString == "Id")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "Id");
                            }
                            #endregion
                            #region Source
                            if (literalQuotedStatusString == "Source")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "Source");
                            }
                            #endregion
                            #region Text
                            if (literalQuotedStatusString == "Text")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "Text");
                            }
                            #endregion
                            #region User
                            if (literalQuotedStatusString == "User")
                            {
                                startCursorPosition++;
                                if (TokenQueue[startCursorPosition].Type != Token.TokenId.Period)
                                {
                                    throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralCannotAccessDirectly, "リテラル\"" + literalQuotedStatusString + "\"に直接アクセスすることは出来ません", null, literalQuotedStatusString);
                                }
                                else
                                {
                                    startCursorPosition++;
                                    if (TokenQueue[startCursorPosition].Type != Token.TokenId.Literal)
                                    {
                                        throw new FilterCompileException(FilterCompileException.ErrorCode.LiteralEndWithPeriod, "リテラルがピリオドで終了しています", null);
                                    }
                                    else
                                    {
                                        var literalUserString = TokenQueue[startCursorPosition].Value as string;
                                        #region CreateAt
                                        if (literalString == "CreateAt")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "CreateAt"), typeof(string));
                                        }
                                        #endregion
                                        #region Description
                                        if (literalUserString == "Description")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "Description");
                                        }
                                        #endregion
                                        #region FavoritesCount
                                        else if (literalUserString == "FavouritesCount")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "FavouritesCount"), typeof(long));
                                        }
                                        #endregion
                                        #region FollowersCount
                                        else if (literalUserString == "FollowersCount")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "FollowersCount"), typeof(long));
                                        }
                                        #endregion
                                        #region FriendsCount
                                        else if (literalUserString == "FriendsCount")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "FriendsCount"), typeof(long));
                                        }
                                        #endregion
                                        #region Id
                                        else if (literalUserString == "Id")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "Id");
                                        }
                                        #endregion
                                        #region IsFollowRequestSent
                                        else if (literalUserString == "IsFollowRequestSent")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "IsFollowRequestSent");
                                        }
                                        #endregion
                                        #region IsMuting
                                        else if (literalUserString == "IsMuting")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "IsMuting");
                                        }
                                        #endregion
                                        #region IsProtected
                                        else if (literalUserString == "IsProtected")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "IsProtected");
                                        }
                                        #endregion
                                        #region IsVerified
                                        else if (literalUserString == "IsVerified")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "IsVerified");
                                        }
                                        #endregion
                                        #region Language
                                        else if (literalUserString == "Language")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "Language");
                                        }
                                        #endregion
                                        #region ListedCount
                                        else if (literalUserString == "ListedCount")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "ListedCount"), typeof(long));
                                        }
                                        #endregion
                                        #region Location
                                        else if (literalUserString == "Location")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "Location");
                                        }
                                        #endregion
                                        #region Name
                                        else if (literalUserString == "Name")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "Name");
                                        }
                                        #endregion
                                        #region ProfileBackgroundImageUrl
                                        else if (literalUserString == "ProfileBackgroundImageUrl")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "ProfileBackgroundImageUrl");
                                        }
                                        #endregion
                                        #region ProfileBannerUrl
                                        else if (literalUserString == "ProfileBannerUrl")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "ProfileBannerUrl");
                                        }
                                        #endregion
                                        #region ProfileImageUrl
                                        else if (literalUserString == "ProfileImageUrl")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "ProfileImageUrl");
                                        }
                                        #endregion
                                        #region ScreenName
                                        else if (literalUserString == "ScreenName")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "ScreenName");
                                        }
                                        #endregion
                                        #region StatusesCount
                                        else if (literalUserString == "StatusesCount")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Convert(Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "StatusesCount"), typeof(long));
                                        }
                                        #endregion
                                        #region TimeZone
                                        else if (literalUserString == "TimeZone")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "TimeZone");
                                        }
                                        #endregion
                                        #region Url
                                        else if (literalUserString == "Url")
                                        {
                                            cursor = startCursorPosition;
                                            return Expression.Property(Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "User"), "Url");
                                        }
                                        #endregion
                                        #region Other
                                        else
                                        {
                                            throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalUserString, null, literalUserString);
                                        }
                                        #endregion
                                    }
                                }
                            }
                            #endregion
                            #region IsFavorited
                            if (literalQuotedStatusString == "IsFavorited")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "IsFavorited");
                            }
                            #endregion
                            #region IsRetweeted
                            if (literalQuotedStatusString == "IsRetweeted")
                            {
                                cursor = startCursorPosition;
                                return Expression.Property(Expression.Property(this.ParamExpr, "QuotedStatus"), "IsRetweeted");
                            }
                            #endregion
                            #region Other
                            else
                            {
                                throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalQuotedStatusString, null, literalQuotedStatusString);
                            }
                            #endregion
                        }
                    }
                }
                #endregion
                #region Other
                else
                {
                    throw new FilterCompileException(FilterCompileException.ErrorCode.WrongLiteral, "不正なリテラル:" + literalString, null, literalString);
                }
                #endregion

                throw new FilterCompileException(FilterCompileException.ErrorCode.FailedToTokenize, "リテラルの解析に失敗しました" + literalString, null, literalString);
            }
        }

        internal class PolandTokenCompiler
        {
            private List<Token> polandQueue;
            private List<Token> tempQueue;

            public Expression CompiledExpression;

            private readonly MethodInfo ContainsMethod = typeof(string).GetMethods("Contains").First();
            private readonly MethodInfo StartsWithMethod = typeof(string).GetMethods("StartsWith").First();
            private readonly MethodInfo EndsWithMethod = typeof(string).GetMethods("EndsWith").First();

            public PolandTokenCompiler(IEnumerable<Token> tokens)
            {
                polandQueue = new List<Token>();
                tempQueue = new List<Token>();

                foreach (var t in tokens)
                    polandQueue.Add(t);
            }

            public void PolandTokenCompile()
            {
                foreach (var token in polandQueue)
                {
                    switch (token.Type)
                    {
                        case Token.TokenId.Numeric:
                        case Token.TokenId.String:
                        case Token.TokenId.Boolean:
                        case Token.TokenId.LiteralParam:
                        case Token.TokenId.Null:
                            tempQueue.Add(token);
                            break;
                        case Token.TokenId.Equal:
                        case Token.TokenId.NotEqual:
                        case Token.TokenId.GreaterThan:
                        case Token.TokenId.GreaterThanEqual:
                        case Token.TokenId.LessThan:
                        case Token.TokenId.LessThanEqual:
                        case Token.TokenId.Contains:
                        case Token.TokenId.StartsWith:
                        case Token.TokenId.EndsWith:
                        case Token.TokenId.RegexMatch:
                        case Token.TokenId.NotContains:
                        case Token.TokenId.NotStartsWith:
                        case Token.TokenId.NotEndsWith:
                        case Token.TokenId.NotRegexMatch:
                        case Token.TokenId.And:
                        case Token.TokenId.Or:
                            PolandTokenOperate(token.Type);
                            break;
                        case Token.TokenId.Exclamation:
                            PolandTokenOperateExclamation(token.Type);
                            break;
                        default:
                            throw new FilterCompileException(FilterCompileException.ErrorCode.InternalError, "内部エラー", null);
                    }
                }
                if (tempQueue.Count == 0)
                {
                    this.CompiledExpression = Expression.Constant(true);
                }
                else if (tempQueue.Count > 1)
                {
                    throw new FilterCompileException(FilterCompileException.ErrorCode.InternalError, "内部エラー", null);
                }
                else
                {
                    if (tempQueue[tempQueue.Count - 1].Type == Token.TokenId.Boolean)
                        this.CompiledExpression = Expression.Constant((bool)tempQueue[tempQueue.Count - 1].Value);
                    else
                        this.CompiledExpression = tempQueue[tempQueue.Count - 1].Value as Expression;

                    if (this.CompiledExpression == null)
                        this.CompiledExpression = Expression.Constant(true);
                }
            }


            public void PolandTokenOperateExclamation(Token.TokenId tokenId)
            {
                if (tempQueue.Count < 1)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "不正な演算", null);

                var frontToken = tempQueue[tempQueue.Count - 1];
                if (!(frontToken.Type == Token.TokenId.Boolean || frontToken.Type == Token.TokenId.Numeric || frontToken.Type == Token.TokenId.String || frontToken.Type == Token.TokenId.LiteralParam || frontToken.Type == Token.TokenId.ExpressionParam))
                    throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "不正な演算", null);

                Expression expressionResult = null;
                Expression frontExpression = null;

                switch (frontToken.Type)
                {
                    case Token.TokenId.Boolean:
                        frontExpression = Expression.Constant((bool)frontToken.Value);
                        break;
                    case Token.TokenId.Numeric:
                        frontExpression = Expression.Constant((long)frontToken.Value);
                        break;
                    case Token.TokenId.String:
                        frontExpression = Expression.Constant((string)frontToken.Value);
                        break;
                    case Token.TokenId.LiteralParam:
                        frontExpression = frontToken.Value as MemberExpression;
                        break;
                    case Token.TokenId.ExpressionParam:
                        frontExpression = frontToken.Value as Expression;
                        break;
                    case Token.TokenId.Null:
                        frontExpression = Expression.Constant(null, typeof(object));
                        break;
                    default:
                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "不正な演算", null);
                }

                switch (tokenId)
                {
                    case Token.TokenId.Exclamation:
                        expressionResult = Expression.Equal(frontExpression, Expression.Constant(false, typeof(bool)));
                        break;
                    default:
                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "不正な演算", null);
                }

                tempQueue.Remove(frontToken);
                tempQueue.Add(new Token { Pos = -1, Type = Token.TokenId.ExpressionParam, Value = expressionResult });
            }

            public void PolandTokenOperate(Token.TokenId tokenId)
            {
                if (tempQueue.Count < 2)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "不正な演算", null);

                var backToken = tempQueue[tempQueue.Count - 2];
                var frontToken = tempQueue[tempQueue.Count - 1];
                if (!(backToken.Type == Token.TokenId.Boolean || backToken.Type == Token.TokenId.Numeric || backToken.Type == Token.TokenId.String || backToken.Type == Token.TokenId.LiteralParam || backToken.Type == Token.TokenId.ExpressionParam || backToken.Type == Token.TokenId.Null))
                    throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "不正な演算", null);
                if (!(frontToken.Type == Token.TokenId.Boolean || frontToken.Type == Token.TokenId.Numeric || frontToken.Type == Token.TokenId.String || frontToken.Type == Token.TokenId.LiteralParam || frontToken.Type == Token.TokenId.ExpressionParam || frontToken.Type == Token.TokenId.Null))
                    throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "不正な演算", null);

                Expression expressionResult = null;
                Expression backExpression = null;
                Expression frontExpression = null;

                switch (backToken.Type)
                {
                    case Token.TokenId.Boolean:
                        backExpression = Expression.Constant((bool)backToken.Value);
                        break;
                    case Token.TokenId.Numeric:
                        backExpression = Expression.Constant((long)backToken.Value);
                        break;
                    case Token.TokenId.String:
                        backExpression = Expression.Constant((string)backToken.Value);
                        break;
                    case Token.TokenId.LiteralParam:
                        backExpression = backToken.Value as Expression;
                        break;
                    case Token.TokenId.ExpressionParam:
                        backExpression = backToken.Value as Expression;
                        break;
                    case Token.TokenId.Null:
                        backExpression = Expression.Constant(null, typeof(object));
                        break;
                    default:
                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "不正な演算", null);
                }
                switch (frontToken.Type)
                {
                    case Token.TokenId.Boolean:
                        frontExpression = Expression.Constant((bool)frontToken.Value);
                        break;
                    case Token.TokenId.Numeric:
                        frontExpression = Expression.Constant((long)frontToken.Value);
                        break;
                    case Token.TokenId.String:
                        frontExpression = Expression.Constant((string)frontToken.Value);
                        break;
                    case Token.TokenId.LiteralParam:
                        frontExpression = frontToken.Value as Expression;
                        break;
                    case Token.TokenId.ExpressionParam:
                        frontExpression = frontToken.Value as Expression;
                        break;
                    case Token.TokenId.Null:
                        frontExpression = Expression.Constant(null, typeof(object));
                        break;
                    default:
                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "不正な演算", null);
                }

                switch (tokenId)
                {
                    case Token.TokenId.Equal:
                        expressionResult = Expression.Equal(backExpression, frontExpression);
                        break;
                    case Token.TokenId.NotEqual:
                        expressionResult = Expression.Equal(Expression.Equal(backExpression, frontExpression), Expression.Constant(false));
                        break;
                    case Token.TokenId.LessThanEqual:
                        expressionResult = Expression.LessThanOrEqual(backExpression, frontExpression);
                        break;
                    case Token.TokenId.GreaterThanEqual:
                        expressionResult = Expression.GreaterThanOrEqual(backExpression, frontExpression);
                        break;
                    case Token.TokenId.LessThan:
                        expressionResult = Expression.LessThan(backExpression, frontExpression);
                        break;
                    case Token.TokenId.GreaterThan:
                        expressionResult = Expression.GreaterThan(backExpression, frontExpression);
                        break;
                    case Token.TokenId.Contains:
                        expressionResult = Expression.Call(backExpression, ContainsMethod, frontExpression);
                        break;
                    case Token.TokenId.StartsWith:
                        expressionResult = Expression.Call(backExpression, StartsWithMethod, frontExpression);
                        break;
                    case Token.TokenId.EndsWith:
                        expressionResult = Expression.Call(backExpression, EndsWithMethod, frontExpression);
                        break;
                    case Token.TokenId.RegexMatch:
                        expressionResult = Expression.Call(typeof(Regex), "IsMatch", null, backExpression, frontExpression);
                        break;
                    case Token.TokenId.NotContains:
                        expressionResult = Expression.Equal(Expression.Call(backExpression, ContainsMethod, frontExpression), Expression.Constant(false));
                        break;
                    case Token.TokenId.NotStartsWith:
                        expressionResult = Expression.Equal(Expression.Call(backExpression, StartsWithMethod, frontExpression), Expression.Constant(false));
                        break;
                    case Token.TokenId.NotEndsWith:
                        expressionResult = Expression.Equal(Expression.Call(backExpression, EndsWithMethod, frontExpression), Expression.Constant(false));
                        break;
                    case Token.TokenId.NotRegexMatch:
                        expressionResult = Expression.Equal(Expression.Call(typeof(Regex), "IsMatch", null, backExpression, frontExpression), Expression.Constant(false));
                        break;
                    case Token.TokenId.And:
                        expressionResult = Expression.AndAlso(backExpression, frontExpression);
                        break;
                    case Token.TokenId.Or:
                        expressionResult = Expression.OrElse(backExpression, frontExpression);
                        break;
                    default:
                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "不正な演算", null);
                }

                tempQueue.Remove(frontToken);
                tempQueue.Remove(backToken);
                tempQueue.Add(new Token { Pos = -1, Type = Token.TokenId.ExpressionParam, Value = expressionResult });
            }
        }
    }
}
