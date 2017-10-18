using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Exceptions;
using Jint.Native;
using Jint.Native.Function;

namespace Flantter.MilkyWay.Models.Filter
{
    public static class FilterFunctions
    {
        public static Dictionary<string, Function> Functions = new Dictionary<string, Function>();

        public static void Register(string functionName, int argumentCount, Delegate dele)
        {
            if (Functions.ContainsKey(functionName))
                return;

            var function = new Function(functionName, argumentCount, dele);
            Functions.Add(functionName, function);
        }

        public static void Unregister(string functionName)
        {
            if (!Functions.ContainsKey(functionName))
                return;

            Functions.Remove(functionName);
        }

        public static bool Invoke(string functionName, bool defaultValue, params object[] param)
        {
            if (!Functions.ContainsKey(functionName))
                return defaultValue;

            var function = Functions[functionName];
            var target = function.Delegate.Target as ScriptFunctionInstance;
            if (target == null)
                return defaultValue;

            try
            {
                var jsparams = param.Select(x => JsValue.FromObject(target.Engine, x)).ToArray();
                var result = (JsValue) function.Delegate.DynamicInvoke(JsValue.Undefined, jsparams);
                return result.AsBoolean();
            }
            catch
            {
            }

            return defaultValue;
        }

        public struct Function
        {
            public string Name;
            public int ArgumentCount;
            public Delegate Delegate;

            public Function(string name, int nrgumentCount, Delegate dele)
                : this()
            {
                Name = name;
                ArgumentCount = nrgumentCount;
                Delegate = dele;
            }
        }
    }

    public struct Token
    {
        #region Priority List

        public static readonly List<TokenId> Priority1 = new List<TokenId>
        {
            TokenId.Multiplication, // *
            TokenId.Division, // /
            TokenId.Modulo // %
        };

        public static readonly List<TokenId> Priority2 = new List<TokenId>
        {
            TokenId.Plus, // +
            TokenId.Minus // -
        };

        public static readonly List<TokenId> Priority3 = new List<TokenId>
        {
            TokenId.Function, // 関数
            TokenId.Equal, // ==
            TokenId.NotEqual, // !=
            TokenId.LessThanEqual, // <=
            TokenId.GreaterThanEqual, // >=
            TokenId.LessThan, // <
            TokenId.GreaterThan, // >
            TokenId.Contains, // Contains
            TokenId.StartsWith, // StartsWith
            TokenId.EndsWith, // EndsWith
            TokenId.RegexMatch, // RegexMatch
            TokenId.In, // In
            TokenId.NotContains, // !Contains
            TokenId.NotStartsWith, // !StartsWith
            TokenId.NotEndsWith, // !EndsWith
            TokenId.NotRegexMatch, // !RegexMatch
            TokenId.NotIn // !In 
        };

        public static readonly List<TokenId> Priority4 = new List<TokenId>
        {
            TokenId.And, // &&
            TokenId.Or, // ||
            TokenId.Exclamation // !
        };

        public static readonly List<TokenId> PriorityOther = new List<TokenId>
        {
            TokenId.String, // "こ↑こ↓のぶぶん"
            TokenId.Numeric, // 数字(整数)
            TokenId.Space, // 空白
            TokenId.Boolean, // ブール代数
            TokenId.Literal, // いろいろ
            TokenId.Null, // Null
            TokenId.Function, // 関数
            TokenId.LiteralExpression, // いろいろ変換した後のリテラル
            TokenId.NumericArrayExpression, // 数字配列のリテラル
            TokenId.StringArrayExpression, // 文字列配列のリテラル
            TokenId.ExpressionParam // Expressionのパラメータ
        };

        #endregion

        public enum TokenId
        {
            // 優先度 0
            Period, // .
            OpenBracket, // (
            CloseBracket, // )
            OpenSquareBracket, // [
            CloseSquareBracket, // ]
            Comma, // ,

            // 優先度 1
            Multiplication, // *
            Division, // /
            Modulo, // %

            // 優先度 2
            Plus, // +
            Minus, // -

            // 優先度 3
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
            In, // In
            NotContains, // !Contains
            NotStartsWith, // !StartsWith
            NotEndsWith, // !EndsWith
            NotRegexMatch, // !RegexMatch
            NotIn, // !In

            // 優先度 4
            And, // &&
            Or, // ||
            Exclamation, // !

            // いろいろ
            String, // "こ↑こ↓のぶぶん"
            Numeric, // 数字(整数)
            Space, // 空白
            Boolean, // ブール代数
            Literal, // いろいろ
            Null, // Null
            Function, // 関数

            // ???
            LiteralExpression, // いろいろ変換した後のリテラル
            NumericArrayExpression, // 数字配列のリテラル
            StringArrayExpression, // 文字列配列のリテラル
            ExpressionParam // Expressionのパラメータ
        }

        public TokenId Type;
        public object Value;
        public int Pos;

        public Token(TokenId type, int pos)
            : this()
        {
            Type = type;
            Value = null;
            Pos = pos;
        }

        public Token(TokenId type, object value, int pos)
            : this()
        {
            Type = type;
            Value = value;
            Pos = pos;
        }
    }

    public static class Compiler
    {
        public static Delegate Compile(string filterString, bool defaultValue = true)
        {
            Debug.WriteLine("\n-- Compile Filter --\n");
            var paramExpr = Expression.Parameter(typeof(Status));

            #region Check FilterString

            Debug.WriteLine("\n-- String Check --\n");
            if (!filterString.StartsWith("("))
                filterString = "(" + filterString + ")";

            #endregion

            #region Tokenize

            Debug.WriteLine("\n-- Tokenize --\n");
            var tokenQueue = new List<Token>(Tokenizer.Tokenize(filterString));

            foreach (var t in tokenQueue)
                if (t.Value != null)
                    Debug.WriteLine(t.Type + " : " + t.Value);
                else
                    Debug.WriteLine(t.Type);

            #endregion

            #region TokenAnalyze

            Debug.WriteLine("\n-- Token Analyze --\n");
            var tokenAnalyzer = new TokenAnalyzer(tokenQueue, paramExpr);
            tokenAnalyzer.TokenAnalyze();

            foreach (var t in tokenAnalyzer.PolandQueue)
                if (t.Value != null)
                    Debug.WriteLine(t.Type + " : " + t.Value);
                else
                    Debug.WriteLine(t.Type);

            #endregion

            #region PolandTokenCompile

            Debug.WriteLine("\n-- Poland Token Compile --\n");
            var polandTokenCompile = new PolandTokenCompiler(tokenAnalyzer.PolandQueue);
            polandTokenCompile.PolandTokenCompile(defaultValue);
            var filter = Expression.Lambda<Func<Status, bool>>(polandTokenCompile.CompiledExpression, paramExpr);

            Debug.WriteLine("\n-- Poland Token Compile --\n");
            Debug.WriteLine(polandTokenCompile.CompiledExpression.ToString());
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
            private const string InString = "In";

            private const string Tokens = ".,[]=<>!&|()\" \t\r\n+-*/";
            private const string NumericToken = "1234567890";

            public static IEnumerable<Token> Tokenize(string filter)
            {
                var strPos = 0;
                var keywordToken = Token.TokenId.And;
                object value = null;

                do
                {
                    int begin;
                    switch (filter[strPos])
                    {
                        case '.':
                            yield return new Token(Token.TokenId.Period, strPos++);
                            break;

                        case ',':
                            yield return new Token(Token.TokenId.Comma, strPos++);
                            break;

                        case '=':
                            if (++strPos >= filter.Length || filter[strPos] != '=')
                                throw new FilterCompileException(
                                    FilterCompileException.ErrorCode.EqualMustUseWithOtherTokens,
                                    "'=' must use with other tokens", null);

                            yield return new Token(Token.TokenId.Equal, strPos++ - 1);
                            break;

                        case '<':
                            if (++strPos < filter.Length && filter[strPos] == '=')
                                yield return new Token(Token.TokenId.LessThanEqual, strPos++ - 1);
                            else
                                yield return new Token(Token.TokenId.LessThan, strPos - 1);

                            break;

                        case '>':
                            if (++strPos < filter.Length && filter[strPos] == '=')
                                yield return new Token(Token.TokenId.GreaterThanEqual, strPos++ - 1);
                            else
                                yield return new Token(Token.TokenId.GreaterThan, strPos - 1);

                            break;

                        case '!':
                            begin = strPos;

                            if (++strPos < filter.Length && filter[strPos] == '=')
                            {
                                yield return new Token(Token.TokenId.NotEqual, begin);
                                strPos++;
                            }
                            else if (TryGetKeyword(filter, ref strPos, ref keywordToken, ref value) &&
                                     keywordToken != Token.TokenId.Null && keywordToken != Token.TokenId.Boolean)
                            {
                                yield return new Token(keywordToken + 5, begin);
                                strPos++;
                            }
                            else
                            {
                                yield return new Token(Token.TokenId.Exclamation, begin);
                                strPos = ++begin;
                            }
                            break;

                        case 'C':
                        case 'S':
                        case 'E':
                        case 'R':
                        case 'F':
                        case 'T':
                        case 'N':
                        case 'I':
                            begin = strPos;

                            if (TryGetKeyword(filter, ref strPos, ref keywordToken, ref value))
                            {
                                yield return new Token(keywordToken, value, begin);
                                strPos++;
                            }
                            else
                            {
                                strPos = begin;
                                goto default;
                            }

                            break;

                        case '&':
                            if (++strPos >= filter.Length || filter[strPos] != '&')
                                throw new FilterCompileException(
                                    FilterCompileException.ErrorCode.AndMustUseWithOtherTokens,
                                    "'&' must use with other tokens", null);
                            else
                                yield return new Token(Token.TokenId.And, strPos++ - 1);

                            break;

                        case '|':
                            if (++strPos >= filter.Length || filter[strPos] != '|')
                                throw new FilterCompileException(
                                    FilterCompileException.ErrorCode.VerticalBarMustUseWithOtherTokens,
                                    "'|' must use with other tokens", null);
                            else
                                yield return new Token(Token.TokenId.Or, strPos++ - 1);

                            break;

                        case '(':
                            yield return new Token(Token.TokenId.OpenBracket, strPos++);
                            break;
                        case ')':
                            yield return new Token(Token.TokenId.CloseBracket, strPos++);
                            break;

                        case '"':
                            begin = strPos;
                            yield return new Token(Token.TokenId.String, GetFilterString(filter, ref strPos), begin);
                            strPos++;
                            break;

                        case '[':
                            yield return new Token(Token.TokenId.OpenSquareBracket, strPos++);
                            break;
                        case ']':
                            yield return new Token(Token.TokenId.CloseSquareBracket, strPos++);
                            break;

                        case '+':
                            yield return new Token(Token.TokenId.Plus, strPos++);
                            break;
                        case '-':
                            yield return new Token(Token.TokenId.Minus, strPos++);
                            break;
                        case '*':
                            yield return new Token(Token.TokenId.Multiplication, strPos++);
                            break;
                        case '/':
                            yield return new Token(Token.TokenId.Division, strPos++);
                            break;
                        case '%':
                            yield return new Token(Token.TokenId.Modulo, strPos++);
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
                            begin = strPos;
                            yield return new Token(Token.TokenId.Numeric,
                                long.Parse(GetFilterNumeric(filter, ref strPos)), begin);
                            strPos++;
                            break;
                        case ' ':
                        case '\t':
                        case '\r':
                        case '\n':
                            yield return new Token(Token.TokenId.Space, strPos++);
                            break;
                        default:
                            begin = strPos;

                            if (TryGetFunction(filter, ref strPos, ref keywordToken, ref value))
                                yield return new Token(keywordToken, value, begin);
                            else
                                do
                                {
                                    if (Tokens.Contains(filter[strPos].ToString()))
                                    {
                                        yield return new Token(Token.TokenId.Literal,
                                            filter.Substring(begin, strPos - begin), begin);
                                        break;
                                    }
                                    if (strPos + 1 >= filter.Length)
                                    {
                                        strPos++;
                                        yield return new Token(Token.TokenId.Literal,
                                            filter.Substring(begin, strPos - begin), begin);
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
                var begin = cursor++;
                while (cursor < filter.Length)
                {
                    if (filter[cursor] == '\\')
                    {
                        if (cursor + 1 == filter.Length)
                            throw new FilterCompileException(FilterCompileException.ErrorCode.FilterEndWithBacksrash,
                                "Filter ends with backsrash", null);
                        if (filter[cursor + 1] == '"' || filter[cursor + 1] == '\\')
                            cursor++;
                    }
                    else if (filter[cursor] == '"')
                    {
                        return filter.Substring(begin + 1, cursor - begin - 1)
                            .Replace("\\\"", "\"")
                            .Replace("\\\\", "\\");
                    }
                    cursor++;
                }
                throw new FilterCompileException(FilterCompileException.ErrorCode.StringTokenIncomplete,
                    "string token is incomplete", null);
            }

            private static string GetFilterNumeric(string filter, ref int cursor)
            {
                var begin = cursor;

                while (cursor < filter.Length)
                {
                    if (NumericToken.Contains(filter[cursor].ToString()))
                    {
                        cursor++;
                        continue;
                    }

                    return filter.Substring(begin, cursor-- - begin);
                }
                return filter.Substring(begin, cursor-- - begin);
            }

            private static bool TryGetFunction(string filter, ref int cursor, ref Token.TokenId token, ref object value)
            {
                var strPos = cursor;
                var begin = strPos;

                do
                {
                    if (Tokens.Contains(filter[strPos].ToString()))
                        if (filter[strPos].ToString() == "(")
                        {
                            var commaCount = 0;
                            var tempStrPos = strPos + 1;
                            var existLiteral = false;
                            do
                            {
                                if (filter[tempStrPos].ToString() == " " || filter[tempStrPos].ToString() == "\n" ||
                                    filter[tempStrPos].ToString() == "\r" || filter[tempStrPos].ToString() == "\t")
                                {
                                    tempStrPos++;
                                    continue;
                                }

                                if (filter[tempStrPos].ToString() == ",")
                                    commaCount++;
                                else if (filter[tempStrPos].ToString() == ")")
                                    break;
                                else
                                    existLiteral = true;

                                tempStrPos++;
                            } while (tempStrPos < filter.Length);

                            cursor = strPos;
                            token = Token.TokenId.Function;
                            value = filter.Substring(begin, strPos - begin) + "," +
                                    (commaCount + (existLiteral ? 1 : 0));
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    strPos++;
                } while (strPos < filter.Length);

                return false;
            }

            private static bool TryGetKeyword(string filter, ref int cursor, ref Token.TokenId token, ref object value)
            {
                if (cursor + ContainsString.Length < filter.Length && filter.Substring(cursor, ContainsString.Length)
                        .Contains(ContainsString))
                {
                    token = Token.TokenId.Contains;
                    cursor += ContainsString.Length - 1;
                    return true;
                }
                if (cursor + StartsWithString.Length < filter.Length &&
                    filter.Substring(cursor, StartsWithString.Length).Contains(StartsWithString))
                {
                    token = Token.TokenId.StartsWith;
                    cursor += StartsWithString.Length - 1;
                    return true;
                }
                if (cursor + EndsWithString.Length < filter.Length && filter.Substring(cursor, EndsWithString.Length)
                        .Contains(EndsWithString))
                {
                    token = Token.TokenId.EndsWith;
                    cursor += EndsWithString.Length - 1;
                    return true;
                }
                if (cursor + RegexMatchString.Length < filter.Length &&
                    filter.Substring(cursor, RegexMatchString.Length).Contains(RegexMatchString))
                {
                    token = Token.TokenId.RegexMatch;
                    cursor += RegexMatchString.Length - 1;
                    return true;
                }
                if (cursor + BooleanTrueString.Length < filter.Length && filter
                        .Substring(cursor, BooleanTrueString.Length)
                        .Contains(BooleanTrueString))
                {
                    token = Token.TokenId.Boolean;
                    value = true;
                    cursor += BooleanTrueString.Length - 1;
                    return true;
                }
                if (cursor + BooleanFalseString.Length < filter.Length && filter
                        .Substring(cursor, BooleanFalseString.Length)
                        .Contains(BooleanFalseString))
                {
                    token = Token.TokenId.Boolean;
                    value = false;
                    cursor += BooleanFalseString.Length - 1;
                    return true;
                }
                if (cursor + NullString.Length < filter.Length &&
                    filter.Substring(cursor, NullString.Length).Contains(NullString))
                {
                    token = Token.TokenId.Null;
                    value = null;
                    cursor += NullString.Length - 1;
                    return true;
                }
                if (cursor + InString.Length < filter.Length &&
                    filter.Substring(cursor, InString.Length).Contains(InString))
                {
                    token = Token.TokenId.In;
                    value = null;
                    cursor += InString.Length - 1;
                    return true;
                }

                return false;
            }
        }

        internal class TokenAnalyzer
        {
            private readonly ParameterExpression _paramExpr;
            private readonly List<Token> _tempQueue;

            private readonly List<Token> _tokenQueue;
            private int _closeBracketCount;
            private int _openBracketCount;

            public TokenAnalyzer(IEnumerable<Token> tokens, ParameterExpression paramExpr)
            {
                _paramExpr = paramExpr;

                PolandQueue = new List<Token>();
                _tokenQueue = new List<Token>();
                _tempQueue = new List<Token>();

                _openBracketCount = 0;
                _closeBracketCount = 0;

                foreach (var token in tokens)
                {
                    if (token.Type == Token.TokenId.Space)
                        continue;

                    _tokenQueue.Add(token);
                }
            }

            public List<Token> PolandQueue { get; set; }

            public void TokenAnalyze()
            {
                int cursor;

                for (cursor = 0; cursor < _tokenQueue.Count; cursor++)
                {
                    var cursorToken = _tokenQueue[cursor];

                    switch (cursorToken.Type)
                    {
                        case Token.TokenId.Numeric:
                        case Token.TokenId.String:
                        case Token.TokenId.Boolean:
                        case Token.TokenId.Null:
                            PolandQueue.Add(cursorToken);
                            break;
                        case Token.TokenId.Literal:
                            PolandQueue.Add(new Token
                            {
                                Type = Token.TokenId.LiteralExpression,
                                Pos = -1,
                                Value = LiteralExpression(ref cursor)
                            });
                            break;
                        case Token.TokenId.OpenSquareBracket:
                            var type = string.Empty;
                            var value = ArrayExpression(ref cursor, ref type);

                            if (type == "Numeric")
                                PolandQueue.Add(new Token
                                {
                                    Type = Token.TokenId.NumericArrayExpression,
                                    Pos = -1,
                                    Value = value
                                });
                            else if (type == "String")
                                PolandQueue.Add(new Token
                                {
                                    Type = Token.TokenId.StringArrayExpression,
                                    Pos = -1,
                                    Value = value
                                });

                            break;
                        case Token.TokenId.Plus:
                        case Token.TokenId.Minus:
                        case Token.TokenId.Multiplication:
                        case Token.TokenId.Division:
                        case Token.TokenId.Modulo:
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
                        case Token.TokenId.In:
                        case Token.TokenId.NotContains:
                        case Token.TokenId.NotStartsWith:
                        case Token.TokenId.NotEndsWith:
                        case Token.TokenId.NotRegexMatch:
                        case Token.TokenId.NotIn:
                        case Token.TokenId.And:
                        case Token.TokenId.Or:
                        case Token.TokenId.Exclamation:
                        case Token.TokenId.Function:
                            _tempQueue.Add(cursorToken);
                            TempQueueCheckPriority();
                            break;
                        case Token.TokenId.OpenBracket:
                            _tempQueue.Add(cursorToken);
                            _openBracketCount++;
                            break;
                        case Token.TokenId.CloseBracket:
                            _tempQueue.Add(cursorToken);
                            _closeBracketCount++;
                            if (_closeBracketCount > _openBracketCount)
                                throw new FilterCompileException(
                                    FilterCompileException.ErrorCode.CloseBracketPositionIsWrong,
                                    "Close backet position is wrong", null);
                            else
                                TempQueueAnnihilationBracket();
                            break;
                        case Token.TokenId.Comma:
                        case Token.TokenId.Period:
                        case Token.TokenId.CloseSquareBracket:
                        case Token.TokenId.Space:
                        case Token.TokenId.LiteralExpression:
                        case Token.TokenId.NumericArrayExpression:
                        case Token.TokenId.StringArrayExpression:
                        case Token.TokenId.ExpressionParam:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (_openBracketCount != _closeBracketCount)
                    throw new FilterCompileException(
                        FilterCompileException.ErrorCode.CloseBracketCountAndOpenBracketCountDiffer,
                        "Close bracket count and open bracket count differ", null);

                for (var tempQueueCursor = _tempQueue.Count - 1; tempQueueCursor >= 0; tempQueueCursor--)
                {
                    if (_tempQueue[tempQueueCursor].Type == Token.TokenId.OpenBracket ||
                        _tempQueue[tempQueueCursor].Type == Token.TokenId.CloseBracket)
                        throw new FilterCompileException(
                            FilterCompileException.ErrorCode.CloseBracketCountAndOpenBracketCountDiffer,
                            "Close bracket count and open bracket count differ", null);

                    PolandQueue.Add(_tempQueue[tempQueueCursor]);
                }
                _tempQueue.Clear();
            }

            private void TempQueueCheckPriority()
            {
                if (_tempQueue.Count < 2)
                    return;

                var cursor = _tempQueue.Count - 2;
                var previousPriority = SearchPriority(_tempQueue[cursor].Type);
                var nextPriority = SearchPriority(_tempQueue[cursor + 1].Type);

                if (previousPriority == 0)
                    return;

                if (previousPriority > nextPriority)
                    return;

                PolandQueue.Add(_tempQueue[cursor]);
                _tempQueue.RemoveAt(cursor);
            }

            private int SearchPriority(Token.TokenId token)
            {
                if (token == Token.TokenId.OpenBracket || token == Token.TokenId.CloseBracket)
                    return 0;
                if (Token.Priority1.Contains(token))
                    return 1;
                if (Token.Priority2.Contains(token))
                    return 2;
                if (Token.Priority3.Contains(token))
                    return 3;
                if (Token.Priority4.Contains(token))
                    return 4;

                throw new FilterCompileException(FilterCompileException.ErrorCode.InternalError, "Internal error",
                    null);
            }

            private void TempQueueAnnihilationBracket()
            {
                if (_tempQueue.Count <= 1)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.InternalError, "Internal error",
                        null);

                if (_tempQueue[_tempQueue.Count - 1].Type != Token.TokenId.CloseBracket)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.CloseBracketNotExist,
                        "Close bracket doesnt exist", null);

                var existOpenBracket = false;
                int cursor;
                for (cursor = _tempQueue.Count - 2; cursor >= 0; cursor--)
                    if (_tempQueue[cursor].Type == Token.TokenId.OpenBracket)
                    {
                        existOpenBracket = true;
                        break;
                    }
                    else if (_tempQueue[cursor].Type == Token.TokenId.CloseBracket)
                    {
                        throw new FilterCompileException(FilterCompileException.ErrorCode.CloseBracketPositionIsWrong,
                            "Close backet position is wrong", null);
                    }
                    /*else if (Token.Priority_Other.Contains(tempQueue[cursor].Type))
                    {
                        throw new FilterCompileException(FilterCompileException.ErrorCode.InternalError, "Internal error", null);
                    }*/
                    else
                    {
                        PolandQueue.Add(_tempQueue[cursor]);
                        _tempQueue.Remove(_tempQueue[cursor]);
                    }

                if (existOpenBracket == false)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.OpenBracketNotExist,
                        "Open bracket doesnt exist", null);

                var openBracketPosition = cursor;
                var closeBracketPosition = _tempQueue.Count - 1;

                _tempQueue.RemoveAt(closeBracketPosition);
                _tempQueue.RemoveAt(openBracketPosition);
            }

            private object LiteralExpression(ref int cursor)
            {
                Expression literal = _paramExpr;
                do
                {
                    var literalString = _tokenQueue[cursor].Value as string;

                    if (literalString == "CreateAt")
                        literal = Expression.Convert(Expression.Property(literal, literalString), typeof(string));
                    else if (literalString == "Count")
                        literal = Expression.Convert(Expression.Property(literal, literalString), typeof(long));
                    else if (literalString == "FavoriteCount")
                        literal = Expression.Convert(Expression.Property(literal, literalString), typeof(long));
                    else if (literalString == "RetweetCount")
                        literal = Expression.Convert(Expression.Property(literal, literalString), typeof(long));
                    else if (literalString == "FavouritesCount")
                        literal = Expression.Convert(Expression.Property(literal, literalString), typeof(long));
                    else if (literalString == "FollowersCount")
                        literal = Expression.Convert(Expression.Property(literal, literalString), typeof(long));
                    else if (literalString == "FriendsCount")
                        literal = Expression.Convert(Expression.Property(literal, literalString), typeof(long));
                    else if (literalString == "ListedCount")
                        literal = Expression.Convert(Expression.Property(literal, literalString), typeof(long));
                    else if (literalString == "StatusesCount")
                        literal = Expression.Convert(Expression.Property(literal, literalString), typeof(long));
                    else
                        literal = Expression.Property(literal, literalString);

                    if (_tokenQueue.Count > cursor + 2 && _tokenQueue[cursor + 1].Type == Token.TokenId.Period &&
                        _tokenQueue[cursor + 2].Type == Token.TokenId.Literal)
                        cursor += 2;
                    else
                        break;
                } while (true);

                return literal;
            }

            private object ArrayExpression(ref int cursor, ref string type)
            {
                var arrayType = string.Empty;

                if (_tokenQueue.Count <= cursor + 1)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.ArrayIncomplete,
                        "Array token is incomplete", null);

                cursor++;

                if (_tokenQueue[cursor].Type == Token.TokenId.Numeric)
                    arrayType = "Numeric";
                else if (_tokenQueue[cursor].Type == Token.TokenId.String)
                    arrayType = "String";

                var arrayExpressionList = new List<Expression>();

                do
                {
                    if (arrayType == "Numeric" && _tokenQueue[cursor].Type == Token.TokenId.Numeric)
                        arrayExpressionList.Add(Expression.Constant(_tokenQueue[cursor].Value));
                    else if (arrayType == "String" && _tokenQueue[cursor].Type == Token.TokenId.String)
                        arrayExpressionList.Add(Expression.Constant(_tokenQueue[cursor].Value));
                    else
                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongArray, "Wrong array",
                            null);

                    if (_tokenQueue.Count <= cursor + 1)
                        throw new FilterCompileException(FilterCompileException.ErrorCode.ArrayIncomplete,
                            "Array token is incomplete", null);

                    if (_tokenQueue[cursor + 1].Type == Token.TokenId.CloseSquareBracket)
                    {
                        cursor++;
                        break;
                    }
                    if (_tokenQueue.Count > cursor + 2 && _tokenQueue[cursor + 1].Type == Token.TokenId.Comma)
                        cursor += 2;
                    else
                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongArray, "Wrong array",
                            null);
                } while (true);

                type = arrayType;

                if (arrayType == "Numeric")
                    return Expression.NewArrayInit(typeof(long), arrayExpressionList);
                if (arrayType == "String")
                    return Expression.NewArrayInit(typeof(string), arrayExpressionList);

                throw new FilterCompileException(FilterCompileException.ErrorCode.InternalError, "Internal error",
                    null);
            }
        }

        internal class PolandTokenCompiler
        {
            private static readonly MethodInfo ContainsMethod = typeof(string).GetMethods("Contains").First();
            private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethods("StartsWith").First();
            private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethods("EndsWith").First();
            private readonly List<Token> _polandQueue;
            private readonly List<Token> _tempQueue;

            public Expression CompiledExpression;

            public PolandTokenCompiler(IEnumerable<Token> tokens)
            {
                _polandQueue = new List<Token>();
                _tempQueue = new List<Token>();

                foreach (var t in tokens)
                    _polandQueue.Add(t);
            }

            public void PolandTokenCompile(bool defaultValue = true)
            {
                foreach (var token in _polandQueue)
                    switch (token.Type)
                    {
                        case Token.TokenId.Numeric:
                        case Token.TokenId.String:
                        case Token.TokenId.Boolean:
                        case Token.TokenId.LiteralExpression:
                        case Token.TokenId.NumericArrayExpression:
                        case Token.TokenId.StringArrayExpression:
                        case Token.TokenId.ExpressionParam:
                        case Token.TokenId.Null:
                            _tempQueue.Add(token);
                            break;
                        case Token.TokenId.Plus:
                        case Token.TokenId.Minus:
                        case Token.TokenId.Multiplication:
                        case Token.TokenId.Division:
                        case Token.TokenId.Modulo:
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
                        case Token.TokenId.In:
                        case Token.TokenId.NotContains:
                        case Token.TokenId.NotStartsWith:
                        case Token.TokenId.NotEndsWith:
                        case Token.TokenId.NotRegexMatch:
                        case Token.TokenId.NotIn:
                        case Token.TokenId.And:
                        case Token.TokenId.Or:
                            PolandTokenOperate(token.Type);
                            break;
                        case Token.TokenId.Exclamation:
                            PolandTokenOperateExclamation();
                            break;
                        case Token.TokenId.Function:
                            PolandTokenOperateFunction(token, defaultValue);
                            break;
                        default:
                            throw new FilterCompileException(FilterCompileException.ErrorCode.InternalError,
                                "Internal error", null);
                    }
                if (_tempQueue.Count == 0)
                {
                    CompiledExpression = Expression.Constant(true);
                }
                else if (_tempQueue.Count > 1)
                {
                    throw new FilterCompileException(FilterCompileException.ErrorCode.InternalError, "Internal error",
                        null);
                }
                else
                {
                    if (_tempQueue[_tempQueue.Count - 1].Type == Token.TokenId.Boolean)
                        CompiledExpression = Expression.Constant((bool) _tempQueue[_tempQueue.Count - 1].Value);
                    else
                        CompiledExpression = _tempQueue[_tempQueue.Count - 1].Value as Expression;

                    if (CompiledExpression == null)
                        CompiledExpression = Expression.Constant(defaultValue);
                }
            }

            private void PolandTokenOperateFunction(Token token, bool defaultValue)
            {
                var data = ((string) token.Value).Split(',');
                var functionName = data.ElementAt(0);
                var functionArgCount = int.Parse(data.ElementAt(1));

                if (_tempQueue.Count < functionArgCount)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.InternalError,
                        "Argument count is wrong", null);


                var param = new Expression[functionArgCount];
                for (var i = 0; i < functionArgCount; i++)
                    switch (_tempQueue[i].Type)
                    {
                        case Token.TokenId.Boolean:
                            param[i] = Expression.Constant((bool) _tempQueue[i].Value, typeof(object));
                            break;
                        case Token.TokenId.Numeric:
                            param[i] = Expression.Constant((long) _tempQueue[i].Value, typeof(object));
                            break;
                        case Token.TokenId.String:
                            param[i] = Expression.Constant((string) _tempQueue[i].Value, typeof(object));
                            break;
                        case Token.TokenId.LiteralExpression:
                        case Token.TokenId.ExpressionParam:
                        case Token.TokenId.NumericArrayExpression:
                        case Token.TokenId.StringArrayExpression:
                            param[i] = Expression.Convert(_tempQueue[i].Value as Expression, typeof(object));
                            break;
                        case Token.TokenId.Null:
                            param[i] = Expression.Constant(null, typeof(object));
                            break;
                        default:
                            throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation,
                                "Wrong operation", null);
                    }

                var expressionResult = Expression.Call(typeof(FilterFunctions), "Invoke", null,
                    Expression.Constant(functionName), Expression.Constant(defaultValue),
                    Expression.NewArrayInit(typeof(object), param));

                _tempQueue.RemoveRange(_tempQueue.Count - functionArgCount, functionArgCount);
                _tempQueue.Add(new Token {Pos = -1, Type = Token.TokenId.ExpressionParam, Value = expressionResult});
            }

            public void PolandTokenOperateExclamation()
            {
                if (_tempQueue.Count < 1)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "Wrong operation",
                        null);

                var frontToken = _tempQueue[_tempQueue.Count - 1];
                if (!Token.PriorityOther.Contains(frontToken.Type))
                    throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "Wrong operation",
                        null);

                Expression frontExpression;

                switch (frontToken.Type)
                {
                    case Token.TokenId.Boolean:
                        frontExpression = Expression.Constant((bool) frontToken.Value);
                        break;
                    case Token.TokenId.LiteralExpression:
                        frontExpression = frontToken.Value as MemberExpression;
                        break;
                    case Token.TokenId.ExpressionParam:
                        frontExpression = frontToken.Value as Expression;
                        break;
                    default:
                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation,
                            "Wrong operation", null);
                }

                Expression expressionResult =
                    Expression.Equal(frontExpression, Expression.Constant(false, typeof(bool)));

                _tempQueue.Remove(frontToken);
                _tempQueue.Add(new Token {Pos = -1, Type = Token.TokenId.ExpressionParam, Value = expressionResult});
            }

            public void PolandTokenOperate(Token.TokenId tokenId)
            {
                if (_tempQueue.Count < 2)
                    throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "Wrong operation",
                        null);

                var backToken = _tempQueue[_tempQueue.Count - 2];
                var frontToken = _tempQueue[_tempQueue.Count - 1];
                if (!Token.PriorityOther.Contains(backToken.Type))
                    throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "Wrong operation",
                        null);
                if (!Token.PriorityOther.Contains(frontToken.Type))
                    throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation, "Wrong operation",
                        null);

                Expression expressionResult = null;
                Expression backExpression;
                Expression frontExpression;

                switch (backToken.Type)
                {
                    case Token.TokenId.Boolean:
                        backExpression = Expression.Constant((bool) backToken.Value);
                        break;
                    case Token.TokenId.Numeric:
                        backExpression = Expression.Constant((long) backToken.Value);
                        break;
                    case Token.TokenId.String:
                        backExpression = Expression.Constant((string) backToken.Value);
                        break;
                    case Token.TokenId.LiteralExpression:
                    case Token.TokenId.ExpressionParam:
                        backExpression = backToken.Value as Expression;
                        break;
                    case Token.TokenId.Null:
                        backExpression = Expression.Constant(null, typeof(object));
                        break;
                    case Token.TokenId.StringArrayExpression:
                    case Token.TokenId.NumericArrayExpression:
                        throw new FilterCompileException(FilterCompileException.ErrorCode.ArrayPositionIsWrong,
                            "Array position is wrong", null);
                    default:
                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation,
                            "Wrong operation", null);
                }
                switch (frontToken.Type)
                {
                    case Token.TokenId.Boolean:
                        frontExpression = Expression.Constant((bool) frontToken.Value);
                        break;
                    case Token.TokenId.Numeric:
                        frontExpression = Expression.Constant((long) frontToken.Value);
                        break;
                    case Token.TokenId.String:
                        frontExpression = Expression.Constant((string) frontToken.Value);
                        break;
                    case Token.TokenId.LiteralExpression:
                    case Token.TokenId.ExpressionParam:
                    case Token.TokenId.NumericArrayExpression:
                    case Token.TokenId.StringArrayExpression:
                        frontExpression = frontToken.Value as Expression;
                        break;
                    case Token.TokenId.Null:
                        frontExpression = Expression.Constant(null, typeof(object));
                        break;
                    default:
                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation,
                            "Wrong operation", null);
                }

                switch (tokenId)
                {
                    case Token.TokenId.Plus:
                        expressionResult = Expression.Add(backExpression, frontExpression);
                        break;
                    case Token.TokenId.Minus:
                        expressionResult = Expression.Subtract(backExpression, frontExpression);
                        break;
                    case Token.TokenId.Multiplication:
                        expressionResult = Expression.Multiply(backExpression, frontExpression);
                        break;
                    case Token.TokenId.Division:
                        expressionResult = Expression.Divide(backExpression, frontExpression);
                        break;
                    case Token.TokenId.Modulo:
                        expressionResult = Expression.Modulo(backExpression, frontExpression);
                        break;
                    case Token.TokenId.Equal:
                        expressionResult = Expression.Equal(backExpression, frontExpression);
                        break;
                    case Token.TokenId.NotEqual:
                        expressionResult = Expression.NotEqual(backExpression, frontExpression);
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
                        expressionResult = Expression.Call(typeof(Regex), "IsMatch", null, backExpression,
                            frontExpression);
                        break;
                    case Token.TokenId.In:
                        if (frontToken.Type == Token.TokenId.NumericArrayExpression)
                            expressionResult = Expression.Call(typeof(Enumerable), "Contains", new[] {typeof(long)},
                                frontExpression, backExpression);
                        else if (frontToken.Type == Token.TokenId.StringArrayExpression)
                            expressionResult = Expression.Call(typeof(Enumerable), "Contains", new[] {typeof(string)},
                                frontExpression, backExpression);

                        break;
                    case Token.TokenId.NotContains:
                        expressionResult = Expression.Equal(
                            Expression.Call(backExpression, ContainsMethod, frontExpression),
                            Expression.Constant(false));
                        break;
                    case Token.TokenId.NotStartsWith:
                        expressionResult = Expression.Equal(
                            Expression.Call(backExpression, StartsWithMethod, frontExpression),
                            Expression.Constant(false));
                        break;
                    case Token.TokenId.NotEndsWith:
                        expressionResult = Expression.Equal(
                            Expression.Call(backExpression, EndsWithMethod, frontExpression),
                            Expression.Constant(false));
                        break;
                    case Token.TokenId.NotRegexMatch:
                        expressionResult = Expression.Equal(
                            Expression.Call(typeof(Regex), "IsMatch", null, backExpression, frontExpression),
                            Expression.Constant(false));
                        break;
                    case Token.TokenId.NotIn:
                        if (frontToken.Type == Token.TokenId.NumericArrayExpression)
                            expressionResult =
                                Expression.Equal(
                                    Expression.Call(typeof(Enumerable), "Contains", new[] {typeof(long)},
                                        frontExpression, backExpression), Expression.Constant(false));
                        else if (frontToken.Type == Token.TokenId.StringArrayExpression)
                            expressionResult =
                                Expression.Equal(
                                    Expression.Call(typeof(Enumerable), "Contains", new[] {typeof(string)},
                                        frontExpression, backExpression), Expression.Constant(false));

                        break;
                    case Token.TokenId.And:
                        expressionResult = Expression.AndAlso(backExpression, frontExpression);
                        break;
                    case Token.TokenId.Or:
                        expressionResult = Expression.OrElse(backExpression, frontExpression);
                        break;
                    default:
                        throw new FilterCompileException(FilterCompileException.ErrorCode.WrongOperation,
                            "Wrong operation", null);
                }

                _tempQueue.Remove(frontToken);
                _tempQueue.Remove(backToken);
                _tempQueue.Add(new Token {Pos = -1, Type = Token.TokenId.ExpressionParam, Value = expressionResult});
            }
        }
    }
}