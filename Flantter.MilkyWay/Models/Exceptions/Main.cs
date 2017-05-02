using System;

namespace Flantter.MilkyWay.Models.Exceptions
{
    public class FilterCompileException : Exception
    {
        public enum ErrorCode
        {
            InternalError,
            EqualMustUseWithOtherTokens,
            AndMustUseWithOtherTokens,
            VerticalBarMustUseWithOtherTokens,
            FilterEndWithBacksrash,
            StringTokenIncomplete,
            CloseBracketPositionIsWrong,
            CloseBracketCountAndOpenBracketCountDiffer,
            CloseBracketNotExist,
            OpenBracketNotExist,
            LiteralCannotAccessDirectly,
            LiteralEndWithPeriod,
            WrongLiteral,
            FailedToTokenize,
            WrongOperation,
            ArrayIncomplete,
            WrongArray,
            ArrayPositionIsWrong
        }

        public FilterCompileException(ErrorCode errorCode, string message, Exception innerException, string param = "")
            : base(message, innerException)
        {
            Error = errorCode;
            Param = param;
        }

        public ErrorCode Error { get; set; }
        public string Param { get; set; }
    }

    public class SuggestionTokenNotFoundException : Exception
    {
        public SuggestionTokenNotFoundException()
        {
        }

        public SuggestionTokenNotFoundException(string message) : base(message)
        {
        }
    }
}