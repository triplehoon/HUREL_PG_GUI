using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HUREL_PG_GUI.Models
{
    public class PGDataClass
    {
        public List<PGStruct> LoadPGData(string path)
        {
            List<PGStruct> PGData = new List<PGStruct>();

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            if (path != "")
            {
                try
                {
                    using BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open));
                    {
                        long length = br.BaseStream.Length;
                        byte[] buffer = new byte[1024 * 1024 * 1000];
                        buffer = br.ReadBytes(Convert.ToInt32(length));

                        byte[] DATA_BUFFER = new byte[334];
                        ushort[] SDATA_BUFFER = new ushort[167];

                        int CurrentPos = 0;

                        while (CurrentPos < length)
                        {
                            if (buffer[CurrentPos] == 0xFE)
                            {
                                CurrentPos += 1;

                                if (buffer[CurrentPos] == 0xFE)
                                {
                                    PGStruct tempData = new PGStruct();

                                    for (int i = 0; i < 334; i++)
                                    {
                                        CurrentPos += 1;
                                        DATA_BUFFER[i] = buffer[CurrentPos];
                                    }
                                    Buffer.BlockCopy(DATA_BUFFER, 0, SDATA_BUFFER, 0, DATA_BUFFER.Length);

                                    for (int ch = 0; ch < 72; ch++)
                                    {
                                        tempData.ChannelCount[ch] = SDATA_BUFFER[ch];
                                    }
                                    for (int ch = 81; ch < 153; ch++)
                                    {
                                        tempData.ChannelCount[ch - 9] = SDATA_BUFFER[ch];
                                    }

                                    uint time_count = ((uint)SDATA_BUFFER[79] << 16) | SDATA_BUFFER[78];

                                    tempData.TriggerInputStartTime = Convert.ToInt32(time_count);
                                    tempData.TriggerInputEndTime = Convert.ToInt32(time_count + (((uint)((uint)SDATA_BUFFER[166] << 16) | ((uint)SDATA_BUFFER[165])) * 10));

                                    tempData.ADC = (double)SDATA_BUFFER[76] / 4096.0 * 5.0;

                                    tempData.SumCounts = tempData.ChannelCount.ToList().Sum();

                                    PGData.Add(tempData);
                                }
                                else
                                {
                                    CurrentPos += 1;
                                }
                            }
                            else
                            {
                                CurrentPos += 1;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error Check: {e}");
                    return null;
                }
            }

            //sw.Stop();
            //Trace.WriteLine($"");
            //Trace.WriteLine($"PG Data Load Time: {sw.ElapsedTicks / 1000} seconds");

            return PGData;
        }
    }

    /// <summary>
    /// (int[144]) ChannelCount // (int) TriggerInputStartTime // (int) TriggerInputEndTime // (double) ADC // (int) SumCounts
    /// </summary>
    public class PGStruct
    {
        public int[] ChannelCount = new int[144];
        public int TriggerInputStartTime;
        public int TriggerInputEndTime;

        public double ADC;

        public int Tick; // 0528 add

        public int SumCounts; // 합칠 때는 굳이 필요 없어보임 (SplitDataIntoSpots 함수)
    }

    /// <summary>
    /// (int[144]) ChannelCount // (int) TriggerInputStartTime // (int) TriggerInputEndTime // (double) ADC // (int) SumCounts // (long) PCTime // (long) Capacity
    /// </summary>
    public class PGStruct_CodeVerification : PGStruct
    {
        public long PCTime;
        public long Capacity;
    }
}
