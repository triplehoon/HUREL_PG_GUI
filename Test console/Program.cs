using HUREL.PG;
using HUREL.PG.NccHelper;
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
        CruxellWrapper.StartFpgaDaq(@"C:\HUREL");
        // wait for 30 seconds for every 100 ms
        Console.WriteLine("Start reading data from FPGA");
        DaqData daqData;
        for (int i = 0; i < 200; i++)
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
    static void TestCreateConnectionWithDb()
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
            SessionInfo? session = dbContext.SessionInfos.FirstOrDefault();
            // print session data
            if (session != null)
            {
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

    static void ReadLogData()
    {
        // log folder set C:\HUREL\PG\MultiSlit\TestLog\59120_230822
        string logFolder = @"C:\HUREL\PG\MultiSlit\TestLog\59120_230822";

        List<string> files = Directory.GetFiles(logFolder).ToList();
       
        string? idtFile = files.Find(x => x.Contains("idt_config"));

        if (idtFile != null)
        {
            Console.WriteLine("IDT file: " + idtFile);
        }
        else
        {
            Console.WriteLine("IDT file is not found");
        }


        // read log data
        NccLogParameter logParameter = Ncc.GetNccLogParameter(idtFile);

        // print log data
        Console.WriteLine("Log data");
        // coefficient
        Console.WriteLine("Coefficient: " + logParameter.coeff_x + ", " + logParameter.coeff_y);

        // read map record and specific record
        List<NccLayer> layers = new List<NccLayer>();
        foreach (string file in files)
        {
            if (file.Contains("map_record") && file.Contains("xdr"))
            {
                // find specific xdr change record to specif
                string specifFile = file.Replace("map_record", "map_specif");
                Console.WriteLine("Map record: " + file);
                Console.WriteLine("Map specif: " + specifFile);

                NccLayer nccLayer = new NccLayer(file, specifFile, logParameter.coeff_x, logParameter.coeff_y);


                layers.Add(nccLayer);
            }
        }

        layers.Sort(Ncc.SortLayer);

        // print layer data
        foreach (NccLayer item in layers)
        {
            Console.WriteLine(item);
        }
        foreach (NccLayer item in layers)
        {
            foreach (NccLogSpot spot in item.LogSpots)
            {

               Console.WriteLine(spot);
            }
        }

    }
    // Test NccPgSession
    static async void TestNccPgSession()
    {
        // create NccPgSession
        NccPgSession nccPgSession = new NccPgSession(patientId: "JohnDoe",
                                                     sessionDescription: "Test session",
                                                     calibrationFilePath: @"C:\HUREL\PG\MultiSlit\TestLog\calibration\eWindow_230921.mat",
                                                     pldPath: @"C:\HUREL\PG\MultiSlit\TestLog\data\PLD_1C_ASO.pld",
                                                     pld3dPath: @"C:\HUREL\PG\MultiSlit\TestLog\data\3DplotRange1C_ASO.pld");

        nccPgSession.StartSession();
        nccPgSession.StartFtpStream("logs", true);
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        Task task = Task.Run(() =>
        {
            nccPgSession.UpdateLogData(tokenSource.Token);
        });
        while (true)
        {
            // the x key is pressed
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.X)
            {
                tokenSource.Cancel();
                await task;
                break;
            }

            // write ncc pg session info to console
            Console.WriteLine(nccPgSession);
            Console.WriteLine("Log data count: " + nccPgSession.SessionLogSpots.Count);
            if (nccPgSession.SessionLogSpots.Count > 0)
            {
                Console.WriteLine("Log data: " + nccPgSession.SessionLogSpots.Last().ToString());
            }


            nccPgSession.UpdateDbContext();

            System.Threading.Thread.Sleep(100);
        }
        nccPgSession.StopFtpStream();
        nccPgSession.StopSession();
    }
    static void Main(string[] args)
    {
        TestNccPgSession();


    }

}
