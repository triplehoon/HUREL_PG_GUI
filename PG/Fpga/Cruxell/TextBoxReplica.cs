using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG.Fpga.Cruxell
{
    internal class TextBoxReplica
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public TextBoxReplica(string name)
        {       
            Name = name;
            Text = string.Empty;
        }
    }
}
