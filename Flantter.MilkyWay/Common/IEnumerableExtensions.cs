using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Common
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// 目的の値に最も近い値を返します
        /// </summary>
        public static T Nearest<T>(
            this IEnumerable<T> self,
            T target
        ) where T : IComparable
        {
            var min = self.Min(c => Math.Abs((dynamic)c - (dynamic)target));
            return self.First(c => Math.Abs((dynamic)c - (dynamic)target) == min);
        }
    }
}
