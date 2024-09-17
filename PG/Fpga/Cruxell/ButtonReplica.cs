using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG.Fpga.Cruxell
{
    internal class ButtonReplica
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public ButtonReplica(string name)
        {       
            Name = name;
            Text = string.Empty;
        }

    }
}
