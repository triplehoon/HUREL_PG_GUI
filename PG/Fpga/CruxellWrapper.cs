using PG.Fpga.Cruxell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PG.Fpga
{
    public static class CruxellWrapper
    {
        private static bool IsFirstTime = true;
        private static CruxellBase CruxellBase = new CruxellBase();

        public static bool GetFpgaStatus()
        {
            if( CruxellBase.EndPointsComboBox.Items.Count > 0 )
            {
                return true;
            }
            return false;
        }
        public static void StartFpgaDaq(string fileName = "")
        {
            if (CruxellBase.bRunning)
            {
                Trace.WriteLine("FpgaDaq is already running");
                return;
            }
            if (fileName == "")
            {
                // filename as YYYYMMDD_HHMMSS
                fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                fileName += "_binary";
            }
            Trace.WriteLine("FpgaDaq Start");
            CruxellBase.TB_file_name.Text = fileName;
            CruxellBase.start_stop_usb();
        }

        public static void StopFpgaDaq()
        {
            if (!CruxellBase.bRunning)
            {
                Trace.WriteLine("FpgaDaq is not running");
                return;
            }
            Trace.WriteLine("FpgaDaq Stop");
            CruxellBase.start_stop_usb();

        }

        public static int GetDataCount()
        {
            return CruxellBase.DaqDataList.Count;
        }
        public static List<DaqData> GetDaqData() {
            return CruxellBase.DaqDataList;
        }

        public static void TestWriteData()
        {
            List<DaqData> daqDatas = CruxellBase.DaqDataList;
            // write as csv file
            string fileName = "test_convert.csv";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, false))
            {
                file.WriteLine("channel,timestamp,value");
                foreach (DaqData daqData in daqDatas)
                {
                    file.WriteLine($"{daqData.channel},{daqData.timestamp},{daqData.value}");
                }
            }
            fileName = "test_raw.csv";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, false))
            {
                //    datatmp = [double(CH_NUMBER(1:falseIdx-1)), double(SEC_TIME(1:falseIdx-1)), double(T_PULSE_TIME(1:falseIdx-1)),  double(V_PULSE_DATA(1:falseIdx-1))];
                file.WriteLine("channel,secTime,tPulseTime,vPulseData");
                foreach (DaqData daqData in daqDatas)
                {
                    file.WriteLine($"{daqData.chNumber},{daqData.secTime},{daqData.tPulseTime},{daqData.vPulseData}");
                }
            }

        }

        public static void PrintSettingValues()
        {
            Trace.WriteLine("FpgaDaq PrintSettingValues");
            CruxellBase.struct_ini.PrintIniValues();
        }
        public static void PrintDeviceList()
        {
            List<string> devices = CruxellBase.DevicesComboBox.Items;
            foreach (string device in devices)
            {
                Trace.WriteLine(device);
            }
            List<string> endpoints = CruxellBase.EndPointsComboBox.Items;
            foreach (string endpoint in endpoints)
            {
                Trace.WriteLine(endpoint);
            }
        }

    }
}
