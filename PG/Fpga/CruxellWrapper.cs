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
        private static CruxellBase CruxellBase = new CruxellBase();
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
