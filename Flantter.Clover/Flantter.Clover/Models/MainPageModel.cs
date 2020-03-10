using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.Clover.Models
{
    public class MainPageModel
    {
        private MainPageModel()
        {
        }

        public static MainPageModel Instance { get; } = new MainPageModel();
    }
}
