using HUREL.PG.Ncc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUREL.PG.MultiSlit
{
    public static class MultislitControl
    {
        
        public static NccSession CurrentSession { get; private set; }

        static public CRUXELL_Original FPGAControl;

        static MultislitControl()
        {
            FPGAControl = new CRUXELL_Original();
            CurrentSession = new NccSession();
        }

        static void InitiateNcc()
        {
            Ncc.LogFileSync.OpenFtpSession();
        }
        static public bool IsMonitoring { get; private set; }

        static async Task MonitoringRunFtpAndFpgaLoop(bool isTest = false)
        {
            if (!CurrentSession.IsReadyToStartSession)
            {
                return;
            }
            // Make local path
            DirectoryInfo mainDataFolder = new DirectoryInfo(".\\data");
            if (mainDataFolder.Exists == false)
            {
                mainDataFolder.Create();
            }
            string folderName = DateTime.Now.ToString("yyyyMMddHHmm") + "_" + "NCC" + "_" + CurrentSession.PatientId + "_" + CurrentSession.PatientName  + "_" + CurrentSession.PlanFileName;
            DirectoryInfo dataFolderName = new DirectoryInfo(".\\data\\" + folderName);
            if (dataFolderName.Exists == false)
            {
                dataFolderName.Create();
            }
            else
            {
                int i = 0;
                dataFolderName = new DirectoryInfo(".\\data\\" + folderName + "(" + i + ")");
                while (dataFolderName.Exists == false)
                {
                    ++i;
                    dataFolderName = new DirectoryInfo(".\\data\\" + folderName + "(" + i + ")");
                }
                dataFolderName.Create();
            }
            DirectoryInfo logFileFodler = new DirectoryInfo(dataFolderName.FullName + "\\log");
            if (logFileFodler.Exists == false)
            {
                logFileFodler.Create();
            }

            Task syncTask = LogFileSync.SyncAndDownloadLogFile(logFileFodler.FullName, isTest);

            string status = "";
            if (!isTest)
            {
                bool isFPGAstart = await Task.Run(() => FPGAControl.Command_MonitoringStart(out status, logFileFodler.FullName + "\\data.bin")).ConfigureAwait(false);
            }
            IsMonitoring = true;

            Task readLogFileLoop = ReadLogFilesLoop(logFileFodler.FullName);
            Task readPGDataLoop = Task.Run(() => ReadPgDataLoop());


            await syncTask;
            await readLogFileLoop;
            await readPGDataLoop;
            //await mergeAndDrawDataLoop;
        }
        static private Mutex loopMutex = new Mutex(false, "loop mutex");
        static private async Task ReadLogFilesLoop(string folderName)
        {            
            while (IsMonitoring)
            {
                List<string> logFiles = Directory.GetFiles(folderName).ToList();
                if (!CurrentSession.IsConfigLogFileLoad)
                {
                    string? selectedConfigFile = logFiles.Find(x => x.Contains("config"));
                    if (selectedConfigFile == null)
                    {
                        Debug.WriteLine("Cannot find config file");
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            CurrentSession.LoadConfigLogFile(selectedConfigFile);
                        });
                    }

                }

                List<string> selectedLogFiles = logFiles.FindAll(x => x.Contains("record"));


                foreach (string logFile in selectedLogFiles)
                {
                    string specifFile = logFile.Replace("record", "specif");
                    await Task.Run(()=> { CurrentSession.LoadRecordSpecifLogFile(logFile, specifFile); });

                }

                Thread.Sleep(1);
            }
        }
        private static void ReadPgDataLoop()
        {
            while (IsMonitoring)
            {

                CurrentSession.SetMultiSlitPg(FPGAControl.PG_raw);
                Thread.Sleep(1);
            }
        }


    }
}
