using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Exceptions
{
    public class AppServiceConnectionException : Exception
    {
        public AppServiceConnectionException(string msg, string status) : base(msg)
        {
        }

        public string Status { get; set; }
    }
}
