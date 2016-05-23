using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Plugin
{
    public static class Debug
    {
        public static void Log(object log)
        {
            System.Diagnostics.Debug.WriteLine(log);
        }
    }

    public static class Filter
    {
        public static void RegisterFunction(string functionName, int argumentCount, Delegate dele)
        {
            Flantter.MilkyWay.Models.Filter.FilterFunctions.Register(functionName, argumentCount, dele);
        }

        public static void UnregisterFunction(string functionName)
        {
            Flantter.MilkyWay.Models.Filter.FilterFunctions.Unregister(functionName);
        }
    }
}
