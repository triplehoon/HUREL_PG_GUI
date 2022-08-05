using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUREL.PG.MultiSlit
{
    public partial class CRUXELL_Original
    {
        // Data Structure

        public class Variables_FPGA
        {
            public int CountMode_0x11;
            public int TrigVref_0x12;
            public int IntervalTime_0x13;
            public int TRIG1_PreMargin_0x14;
            public int TRIG1_PostMargin_tb_0x15;
            public int TRIG2_PreMargin_0x16;
            public int TRIG2_PostMargin_0x17;
            public int ADC_Rate_0x18;
            public int tb_0x19;
            public int tb_0x1a;
        }

        public class PGdists
        {
            public double[] cnt_row1 = new double[36];
            public double[] cnt_row2 = new double[36];
            public double[] cnt_row3 = new double[36];
            public double[] cnt_row4 = new double[36];

            public double[] cnt_top = new double[36];
            public double[] cnt_bot = new double[36];

            public double[] cnt_PGdist72 = new double[72];
            public double[] cnt_PGdist71 = new double[71];
        }

        public enum CountMode
        {
            Continue,
            TRIG1,
            TRIG2
        }

        public class CounterData
        {
            public double[] ChCounts = new double[144];
            public int Time_Start;
            public int Time_End;

            public DateTime RealTime_Start;
            public DateTime RealTime_End;

            public double ChCounts_Sum;
            public double TriggerADCHeight;
        }

        public class FPGADataModel_71Ch
        {
            public double[] ChannelCount = new double[71];
            public int TriggerInputStartTime;
            public int TriggerInputEndTime;
            public double TriggerADCHeight;

            public DateTime StartEpochTime;
            public DateTime EndEpochTime;
        }

        public class FPGADataModelTemp
        {
            public int[] ChannelCount = new int[144];
            public int TriggerInputStartTime;
            public int TriggerInputEndTime;
            //public double TriggerADCHeight;

            public bool isBeamOn;
            public int ChannelCountSum;

            public DateTime StartEpochTime;
            public DateTime EndEpochTime;
        }

    }
}
