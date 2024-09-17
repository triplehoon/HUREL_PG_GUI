using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG.Fpga
{
    public struct DaqData
    {
        int channel;
        long timestamp; // ns 
        int value; // mV
    }
}
