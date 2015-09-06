using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Exceptions
{
    public class FilterCompileException : Exception
    {
        public FilterCompileException(ErrorCode errorCode, string message, Exception innerException, string param = "")
            : base(message, innerException)
        {
            this.Error = errorCode;
            this.Param = param;
        }

        public ErrorCode Error { get; set; }
        public string Param { get; set; }

        public enum ErrorCode
        {
            InternalError,
            EqualMustUseWithOtherTokens,
            AndMustUseWithOtherTokens,
            VerticalBarMustUseWithOtherTokens,
            FilterEndWithBacksrash,
            StringNotEnd,
            CloseBracketPositionIsWrong,
            CloseBracketCountAndOpenBracketCountIsDiffer,
            CloseBracketNotExist,
            LiteralCannotAccessDirectly,
            LiteralEndWithPeriod,
            WrongLiteral,
            FailedToTokenize,
            WrongOperation,
        }
    }

    public class SuggestionTokenNotFoundException : Exception
    {
        public SuggestionTokenNotFoundException() { }

        public SuggestionTokenNotFoundException(string message) : base(message) { }

        public SuggestionTokenNotFoundException(string message, Exception inner) : base(message) { }
    }
}
