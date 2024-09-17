using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PG.Fpga
{
    public struct DaqData
    {
        //SEC_TIME = typecast(reshape(chunkData(i: i + 3,:), [], 1), 'uint32');

        //CH_NUMBER = typecast(reshape(chunkData(i + 4:i + 5,:), [], 1), 'uint16');
        //PRE_DATA = typecast(reshape(chunkData(i + 6:i + 7,:), [], 1), 'uint16');
        //V_PULSE_DATA = typecast(reshape(chunkData(i + 10:i + 11,:), [], 1), 'uint16');
        //T_PULSE_TIME = typecast(reshape(chunkData(i + 12:i + 15,:), [], 1), 'uint32');

        public UInt32 secTime;
        public UInt16 chNumber;
        public UInt16 preData;
        public UInt16 vPulseData;
        public UInt32 tPulseTime;

        public int channel
        {
            get
            {
                return chNumber;
            }
        }
        public long timestamp
        {
            get
            {
                return (long)(chNumber == 144 ? secTime * 1e10 + tPulseTime * 10.0 : secTime * 1e10 + tPulseTime * 8.0);
            }
        }
        public double value
        {
            get
            {
                return vPulseData;
            }
        }
    }
}
