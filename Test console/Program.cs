using HUREL.PG;
using HUREL.PG.Ncc;
using PG;
using PG.Fpga;
using System.Runtime.CompilerServices;
using static PG.PgSession;




class TestClass
{
    private static void TestCruxellLoading()
    {
        CruxellWrapper.PrintSettingValues();        
    }
    private static void TestDeviceLoading()
    {
        CruxellWrapper.PrintDeviceList();
    }
    private static void TestFpgaDaq()
    {
        CruxellWrapper.StartFpgaDaq();
        // wait for 30 seconds for every 100 ms
        Console.WriteLine("Start reading data from FPGA");
        DaqData daqData;
        for (int i = 0; i < 300; i++)
        {
            
            if (CruxellWrapper.GetDataCount() != 0)
            {
                // comma separated values                
                Console.WriteLine("Data count: " + CruxellWrapper.GetDataCount().ToString("N0"));
                Console.WriteLine("Sample data: ");
                daqData = CruxellWrapper.GetDaqData()[CruxellWrapper.GetDataCount() - 1];
                Console.WriteLine("secTime: " + daqData.secTime.ToString("N0"));
                Console.WriteLine("chNumber: " + daqData.chNumber.ToString("N0"));
                Console.WriteLine("preData: " + daqData.preData.ToString("N0"));
                Console.WriteLine("vPulseData: " + daqData.vPulseData.ToString("N0"));
                Console.WriteLine("tPulseTime: " + daqData.tPulseTime.ToString("N0"));
                Console.WriteLine("-------Save values-------");
                Console.WriteLine("channel: " + daqData.channel.ToString("N0"));
                Console.WriteLine("timestamp [ns]: " + daqData.timestamp.ToString("N0"));
                Console.WriteLine("value [mV]: " + daqData.value.ToString("N0"));
            }
            System.Threading.Thread.Sleep(100);
            // check console is write
            if (Console.CursorTop > 5)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 11);
            }
        }
        Console.SetCursorPosition(0, Console.CursorTop + 11);
        Console.WriteLine("Stop reading data from FPGA");
        // cursor to the end of the console
        CruxellWrapper.StopFpgaDaq();
        // comma separated values                
        Console.WriteLine("Data count: " + CruxellWrapper.GetDataCount().ToString("N0"));
        Console.WriteLine("Sample data: ");
        daqData = CruxellWrapper.GetDaqData()[CruxellWrapper.GetDataCount() - 1];
        Console.WriteLine("secTime: " + daqData.secTime.ToString("N0"));
        Console.WriteLine("chNumber: " + daqData.chNumber.ToString("N0"));
        Console.WriteLine("preData: " + daqData.preData.ToString("N0"));
        Console.WriteLine("vPulseData: " + daqData.vPulseData.ToString("N0"));
        Console.WriteLine("tPulseTime: " + daqData.tPulseTime.ToString("N0"));
        Console.WriteLine("-------Save values-------");
        Console.WriteLine("channel: " + daqData.channel.ToString("N0"));
        Console.WriteLine("timestamp [ns]: " + daqData.timestamp.ToString("N0"));
        Console.WriteLine("value [mV]: " + daqData.value.ToString("N0"));

        Console.WriteLine("Test write data");
        CruxellWrapper.TestWriteData();
        Console.WriteLine("Done");
    }
    static void TestSessionCreation()
    {
        PgSession session = new PgSession(eSessionType.NCC);
    }
    static void Main(string[] args)
    {
        TestFpgaDaq();
        TestSessionCreation();
    }
}
