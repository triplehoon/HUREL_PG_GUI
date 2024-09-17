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
        public static void StartFpgaDaq()
        {
            Trace.WriteLine("FpgaDaq Start");
            
        }

        public static void StopFpgaDaq()
        {
            Trace.WriteLine("FpgaDaq Stop");
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
