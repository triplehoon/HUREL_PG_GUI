using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public readonly UInt32 secTime;
        public readonly UInt16 chNumber;
        public readonly UInt16 preData;
        public readonly UInt16 vPulseData;
        public readonly UInt32 tPulseTime;

        public DaqData(byte[] chunkData)
        {
            Debug.Assert(chunkData.Length == 20);
            secTime = BitConverter.ToUInt32(chunkData, 0);
            chNumber = BitConverter.ToUInt16(chunkData, 4);
            preData = BitConverter.ToUInt16(chunkData, 6);
            vPulseData = BitConverter.ToUInt16(chunkData, 10);
            tPulseTime = BitConverter.ToUInt32(chunkData, 12);
        }

        public int channel
        {
            get
            {
                return chNumber;
            }
        }
        // in nanoseconds
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
