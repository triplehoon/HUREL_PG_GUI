using HUREL.PG;
using HUREL.PG.Ncc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using PG;
using PG.Fpga;
using PG.Orm;
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
        for (int i = 0; i < 100; i++)
        {
            
            if (CruxellWrapper.GetDataCount() != 0)
            {
                // comma separated values                
                Console.WriteLine("Data count: " + CruxellWrapper.GetDataCount().ToString("N0"));
                Console.WriteLine("Sample data: ");
                daqData = CruxellWrapper.GetDaqData()[CruxellWrapper.GetDataCount() - 1];
                Console.WriteLine("secTime: " + daqData.secTime.ToString("N0"));
                //Console.WriteLine("chNumber: " + daqData.chNumber.ToString("N0"));
                Console.WriteLine("chNumber: " + daqData.chNumber.ToString("D3"));

                Console.WriteLine("preData: " + daqData.preData.ToString("N0"));
                Console.WriteLine("vPulseData: " + daqData.vPulseData.ToString("D4"));
                //Console.WriteLine("vPulseData: " + ((int)Math.Round(daqData.vPulseData)).ToString("D4"));

                Console.WriteLine("tPulseTime: " + daqData.tPulseTime.ToString("N0"));
                Console.WriteLine("-------Save values-------");
                Console.WriteLine("channel: " + daqData.channel.ToString("D3"));
                Console.WriteLine("timestamp [ns]: " + daqData.timestamp.ToString("N0"));
                
                Console.WriteLine("value [mV]: " + ((int)Math.Round(daqData.value)).ToString("D4"));

                //Console.WriteLine("value [mV]: " + daqData.value.ToString("NO"));
                //Console.WriteLine("value [mV]: " + Math.Round(daqData.value).ToString("Fo"));
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
    }
    static void TestCreateConnectionWithDb()
    {

    }
    static void Main(string[] args)
    {      
        // Set up the dependency injection container
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDbContext<PgDbContext>();

        // Build the service provider and use it
        var serviceProvider = serviceCollection.BuildServiceProvider();
       
        // Resolve the DbContext and use it
        using (PgDbContext dbContext = serviceProvider.GetRequiredService<PgDbContext>())
        {
            // read session data
            SessionInfo session = dbContext.SessionInfos.FirstOrDefault();
            // print session data
            if (session != null) {
                Console.WriteLine("Session ID: " + session.SessionId);
                Console.WriteLine("Session Name: " + session.PatientNumber);
                Console.WriteLine("Session Description: " + session.Date);
            }
            else
            {
                Console.WriteLine("Session data is empty");
            }
        }

        //TestFpgaDaq();

        serviceProvider.Dispose();
    }

}
