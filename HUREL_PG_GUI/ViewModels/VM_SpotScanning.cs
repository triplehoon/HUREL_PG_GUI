using AsyncAwaitBestPractices.MVVM;
using GalaSoft.MvvmLight.CommandWpf;
using HUREL_PG_GUI.Models;
using MSPGC_GUI.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using HUREL.PG.Ncc;
using System.Collections.Concurrent;

namespace HUREL_PG_GUI.ViewModels
{
    public class VM_SpotScanning : ViewModelBase
    {

        // Real-time Monitoring Objects
        private List<PlanStruct_NCC> Plan_NCC = new List<PlanStruct_NCC>();
        private List<LogStruct_NCC> Log_NCC = new List<LogStruct_NCC>();
        private List<PGStruct> PG = new List<PGStruct>();
        private List<PlanLogMergedDataStruct> PlanLogMergedData = new List<PlanLogMergedDataStruct>();
        private List<PlanLogPGMergedDataStruct> MergedData = new List<PlanLogPGMergedDataStruct>();
        private List<SpotMapStruct> spotMap;
        private List<BeamRangeMapStruct> beamRangeMap;
        // Configuration
        public static Configuration_NCC _Configuration_NCC = new Configuration_NCC();

        public class Configuration_NCC
        {
            // Path
            public string Path_Datas;
            public string Path_remote; // Log File Sync
            public string Path_local;  // Log File Sync

            // Parameters
            public List<GapPeakAndRangeStruct> GapPeakAndRange = new List<GapPeakAndRangeStruct>();
        }

        // Visualized Objects
        public ObservableCollection<BeamRangeMapStruct> VM_BeamRangeMap { get; set; }
        public ObservableCollection<SpotMapStruct> VM_SpotMap { get; set; }


        //static public CRUXELLMSPGC FPGAControl;

        static public DateTime FirstTuningBeamTime = new DateTime(0); // 0528

        public VM_SpotScanning()
        {
            ConfigurationSetting_NCC();
        }
        private void ConfigurationSetting_NCC()
        {
            // Path
            string DefaultPath = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName).FullName;
            _Configuration_NCC.Path_Datas = Path.Combine(DefaultPath, "Datas");
            //_Configuration_NCC.Path_remote = @"/00. remote/";
            //_Configuration_NCC.Path_local = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\01. OLD\02. FTP\RebexTinySftpServer-1.0.11\data\00. local\";
            _Configuration_NCC.Path_remote = @"/PBSdata/test/clinical/tr3/planId/beamId/fractionId/";
            _Configuration_NCC.Path_local = @"C:\Users\JungJaerin\Desktop\FTP\";

            // Object
            _Configuration_NCC.GapPeakAndRange = GapPeakAndRangeRead(Path.Combine(_Configuration_NCC.Path_Datas, "gapPeakAndRange.txt"));
        }

        private List<GapPeakAndRangeStruct> GapPeakAndRangeRead(string path)
        {
            List<GapPeakAndRangeStruct> GapPeakAndRange = new List<GapPeakAndRangeStruct>();

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
                {
                    string lines = null;
                    string[] tempString = null;

                    while ((lines = sr.ReadLine()) != null)
                    {
                        GapPeakAndRangeStruct temp = new GapPeakAndRangeStruct();

                        tempString = lines.Split("\t");

                        temp.Energy = Convert.ToDouble(tempString[0]);
                        temp.GapValue = Convert.ToDouble(tempString[1]);

                        GapPeakAndRange.Add(temp);
                    }
                }
            }
            return GapPeakAndRange;
        }


        #region Binding

        static public bool is_Watcherstart = false;
        public bool Is_Watcherstart
        {
            get
            {
                return is_Watcherstart;
            }
            set
            {
                is_Watcherstart = value;
                OnPropertyChanged(nameof(Is_Watcherstart));
            }
        }


        static public bool is_FTPstart = false;
        public bool Is_FTPstart
        {
            get
            {
                return is_FTPstart;
            }
            set
            {
                is_FTPstart = value;
                OnPropertyChanged(nameof(Is_FTPstart));
            }
        }


        private bool is_LogFileSync = false;
        public bool Is_LogFileSync
        {
            get
            {
                return is_LogFileSync;
            }
            set
            {
                is_LogFileSync = value;
                OnPropertyChanged(nameof(Is_LogFileSync));
            }
        }


        private string vmStatus;
        public string VMStatus
        {
            get
            {
                return vmStatus;
            }
            set
            {
                vmStatus = value;
                OnPropertyChanged(nameof(VMStatus));
            }
        }


        private bool isMonitoring;
        public bool IsMonitoring
        {
            get
            {
                return isMonitoring;
            }
            set
            {
                isMonitoring = value;
                OnPropertyChanged(nameof(IsMonitoring));
            }
        }


        private bool isConverting;
        public bool IsConverting
        {
            get
            {
                return isConverting;
            }
            set
            {
                isConverting = value;
                OnPropertyChanged(nameof(IsConverting));
            }
        }


        private string patientName;
        public string PatientName
        {
            get
            {
                return patientName;
            }
            set
            {
                patientName = value;
                OnPropertyChanged(nameof(PatientName));
            }
        }


        private string patientID;
        public string PatientID
        {
            get
            {
                return patientID;
            }
            set
            {
                patientID = value;
                OnPropertyChanged(nameof(PatientID));
            }
        }


        private int plan_TotalLayer = 1;
        public int Plan_TotalLayer
        {
            get
            {
                return plan_TotalLayer;
            }
            set
            {
                plan_TotalLayer = value;
                OnPropertyChanged(nameof(Plan_TotalLayer));
            }
        }


        private int plan_TotalSpot = 0;
        public int Plan_TotalSpot
        {
            get
            {
                return plan_TotalSpot;
            }
            set
            {
                plan_TotalSpot = value;
                OnPropertyChanged(nameof(Plan_TotalSpot));
            }
        }


        private double plan_TotalMU;
        public double Plan_TotalMU
        {
            get
            {
                return plan_TotalMU;
            }
            set
            {
                plan_TotalMU = value;
                OnPropertyChanged(nameof(Plan_TotalMU));
            }
        }


        private bool is_PlanFileLoaded;
        public bool Is_PlanFileLoaded
        {
            get
            {
                return is_PlanFileLoaded;
            }
            set
            {
                is_PlanFileLoaded = value;
                OnPropertyChanged(nameof(Is_PlanFileLoaded));
            }
        }

        // Progress Bar 관련
        private double finishedLayerRatio = 0;
        public double FinishedLayerRatio
        {
            get
            {
                return finishedLayerRatio;
            }
            set
            {
                finishedLayerRatio = value;
                OnPropertyChanged(nameof(FinishedLayerRatio));
            }
        }


        private double currentLayerRatio = 0;
        public double CurrentLayerRatio
        {
            get
            {
                return currentLayerRatio;
            }
            set
            {
                currentLayerRatio = value;
                OnPropertyChanged(nameof(CurrentLayerRatio));
            }
        }


        private int currentLayer = 0;
        public int CurrentLayer
        {
            get
            {
                return currentLayer;
            }
            set
            {
                currentLayer = value;
                OnPropertyChanged(nameof(CurrentLayer));
            }
        }

        #endregion

        #region Command
        private AsyncCommand _MonitoringStartCommand;
        public ICommand MonitoringStartCommand
        {
            get
            {
                return _MonitoringStartCommand ?? (_MonitoringStartCommand = new AsyncCommand(MonitoringStart));
            }
        }
        private async Task MonitoringStart()
        {
            if (isMonitoring == false)
            {
                if (!is_FTPstart)
                {
                    return;
                }

                Task monitoringRunFtpAndFpgaLoop = MonitoringRunFtpAndFpgaLoop();

                await monitoringRunFtpAndFpgaLoop.ConfigureAwait(false);
            }
            else
            {
                string status = "";

                //bool isFPGAstart = await Task.Run(() => FPGAControl.Command_MonitoringStart(out status)).ConfigureAwait(false);
                //await Task.Run(() => FPGAControl.start_stop_usb());
                bool isFPGAstart = await Task.Run(() => VM_MainWindow.FPGAControl.Command_MonitoringStart(out status, "")).ConfigureAwait(false);
                //bool isPGdisUpdate = await Task.Run(() => PGdistUpdate());
                LogFileSync.StopSyncAndDownloadLogFile();

                VMStatus = "Idle";
                IsMonitoring = false;
            }
        }

        private async Task MonitoringRunFtpAndFpgaLoop(bool isTest = false)
        {
            // Make local path
            DirectoryInfo mainDataFolder = new DirectoryInfo(".\\data");
            if (mainDataFolder.Exists == false)
            {
                mainDataFolder.Create();
            }
            string folderName = DateTime.Now.ToString("yyyyMMddHHmm") + "_" + "NCC" + "_" + patientID + "_" + patientName + "_" + planFileName;
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

            Task syncTask =  LogFileSync.SyncAndDownloadLogFile(logFileFodler.FullName, isTest);

            string status = "";
            if (!isTest)
            {
                bool isFPGAstart = await Task.Run(() => VM_MainWindow.FPGAControl.Command_MonitoringStart(out status, logFileFodler.FullName + "\\data.bin")).ConfigureAwait(false);
            }           
            VMStatus = status;
            IsMonitoring = true;

            Task readLogFileLoop = ReadLogFilesLoop(logFileFodler.FullName);
            Task readPGDataLoop = Task.Run(() =>ReadPgDataLoop());
            Task mergeAndDrawDataLoop = MergeAndDrawDataLoop();


            await syncTask;
            await readLogFileLoop;
            await readPGDataLoop;
            await mergeAndDrawDataLoop;
        }
        Mutex loopMutex = new Mutex(false, "loop mutex");
        private async Task ReadLogFilesLoop(string folderName)
        {
            NCCLogFileLoad Class = new NCCLogFileLoad();

            while (IsMonitoring)
            {
                loopMutex.WaitOne();
                try
                {
                    
                    await Task.Run(() => Log_NCC = Class.LogDatasLoad_PostProcessing(folderName)).ConfigureAwait(false);
                    Trace.WriteLine(Log_NCC.Count);
                    CurrentLayer = Log_NCC[Log_NCC.Count - 1].LayerNumber + 1;
                    
                }
                catch (Exception ex)
                {
                    //Trace.WriteLine("Log update error");
                    Log_NCC = null;
                }
                loopMutex.ReleaseMutex();

                Thread.Sleep(1);
            }
        }

        private void ReadPgDataLoop()
        {
            while (IsMonitoring)
            {
                loopMutex.WaitOne();
                try
                {
                    
                    PG = VM_MainWindow.FPGAControl.PG_raw;
                }
                catch
                {
                    PG = null;
                }
                loopMutex.ReleaseMutex();
                
                Thread.Sleep(1);
            }
        }


        private async Task MergeAndDrawDataLoop()
        {
            NCCAnalysisClass Class = new NCCAnalysisClass();

            while (IsMonitoring)
            {
                loopMutex.WaitOne();
                try
                {
                    if (PG == null || Log_NCC == null)
                    {
                        loopMutex.ReleaseMutex();
                        continue;
                    }
                    Task<List<PlanLogPGMergedDataStruct>> mergeTask = Class.GenerateMergedData_PostProcessing(Plan_NCC, Log_NCC, PG); // static data

                    MergedData = await mergeTask;
                    loopMutex.ReleaseMutex();

                    int StartLayer = 0;
                    int LastLayer = MergedData.Last().Log_LayerNumber;

                    Task<List<SpotMapStruct>> SpotMapTask = Class.GenerateSpotMap(MergedData, 0); // 초기에는 1번째 Layer가 보여지도록
                    Task<List<BeamRangeMapStruct>> BeamRangeMapTask = Class.GenerateBeamRangeMap(MergedData, 7, StartLayer, LastLayer, 1);

                    spotMap = await SpotMapTask;
                    beamRangeMap = await BeamRangeMapTask;

                    VM_SpotMap = new ObservableCollection<SpotMapStruct>();
                    spotMap.ForEach(x => VM_SpotMap.Add(x));

                    VM_BeamRangeMap = new ObservableCollection<BeamRangeMapStruct>();
                    beamRangeMap.ForEach(x => VM_BeamRangeMap.Add(x));

                    OnPropertyChanged(nameof(VM_SpotMap));
                    OnPropertyChanged(nameof(VM_BeamRangeMap));
                    await DrawSpotMap();
                    Thread.Sleep(1);
                }
                catch
                {
                    Trace.WriteLine("Merge And Draw Data Loop done");
                }
            }

        }

        private async Task DrawSpotMap()
        {
            #region Test Code(2022-02-17), Un-Activated

            //for (int j = 0; j < 10; j++)
            //{
            //    Trace.WriteLine($"{j}: Spot Map Speed Evaluation Start");

            //    for (int i = 1; i < 12; i++)
            //    {
            //        //Trace.WriteLine($"{i}: Start");

            //        Stopwatch sw = new Stopwatch();
            //        sw.Start();

            //        NCCAnalysisClass Class = new NCCAnalysisClass();

            //        Task<List<SpotMapStruct>> SpotMapTask = Class.GenerateSpotMap(MergedData, i - 1);
            //        spotMap = await SpotMapTask;

            //        VM_SpotMap = new ObservableCollection<SpotMapStruct>();
            //        spotMap.ForEach(x => VM_SpotMap.Add(x));
            //        OnPropertyChanged(nameof(VM_SpotMap));

            //        sw.Stop();
            //        Trace.WriteLine($"{sw.ElapsedMilliseconds}");

            //    }
            //    Trace.WriteLine($"");
            //}

            #endregion

            Stopwatch sw = new Stopwatch();
            sw.Start();

            NCCAnalysisClass Class = new NCCAnalysisClass();

            Task<List<SpotMapStruct>> SpotMapTask = Class.GenerateSpotMap(MergedData, CurrentLayer - 1);
            spotMap = await SpotMapTask;

            VM_SpotMap = new ObservableCollection<SpotMapStruct>();
            spotMap.ForEach(x => VM_SpotMap.Add(x));
            OnPropertyChanged(nameof(VM_SpotMap));

            sw.Stop();
            Trace.WriteLine($"{sw.ElapsedMilliseconds} ms");
        }

        private RelayCommand _LoadDICOMCommand;
        public ICommand LoadDICOMCommand
        {
            get
            {
                return _LoadDICOMCommand ?? (_LoadDICOMCommand = new RelayCommand(LoadDICOM));
            }
        }

        /// <summary>
        /// Loading pld, dicom files
        /// </summary>
        private void LoadDICOM()
        {
            // Plan File Load
            LoadPlanFile();
        }
        private void LoadPlanFile()
        {
            Plan_NCC = new List<PlanStruct_NCC>(); // Data 생성

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Pld|*.pld|Text|*.txt|All|*.*";
            dialog.ShowDialog();
            VMStatus = "Loading Plan File";
            Plan_NCC = PlanFileClass_NCC.LoadPlanFile(dialog.FileName, true); ////////////////////////////////////
            if (Plan_NCC != null)
            {
                PatientID = "00000";
                PatientName = "Test";
                VMStatus = "Plan File loaded";
                planFileName = Path.GetFileNameWithoutExtension(dialog.FileName);
                Plan_TotalLayer = Plan_NCC[Plan_NCC.Count - 1].LayerNumber;
                CurrentLayer = 1;
                Is_PlanFileLoaded = true;
            }
            else
            {
                Trace.WriteLine("");
                PatientID = "Fail";
                PatientName = "Fail";
                Trace.WriteLine("Plan_NCC File = null");
                Trace.WriteLine("");
            }
        }

        private string planFileName = "";


        private AsyncCommand _ResetPGdistCommand;
        public ICommand ResetPGdistCommand
        {
            get
            {
                return _ResetPGdistCommand ?? (_ResetPGdistCommand = new AsyncCommand(ResetPGdist));
            }
        }
        private async Task ResetPGdist()
        {
            // await Task.Run(() => VM_MainWindow.FPGAControl.Command_Reset_PGdist()).ConfigureAwait(false);
        }


        private AsyncCommand _FileConvertCommand;
        public ICommand FileConvertCommand
        {
            get
            {
                return _FileConvertCommand ?? (_FileConvertCommand = new AsyncCommand(FileConvert));
            }
        }
        private async Task FileConvert()
        {
            // bool isConverting = await Task.Run(() => VM_MainWindow.FPGAControl.Command_Convert()).ConfigureAwait(false);
        }



        private AsyncCommand _StartFtpCommand;
        public ICommand StartFtpCommand
        {
            get
            {
                return _StartFtpCommand ?? (_StartFtpCommand = new AsyncCommand(StartFtp));
            }
        }
        private async Task StartFtp()
        {
            VMStatus = "Start up ftp server";
            await Task.Run(() =>
            {
                (Is_FTPstart, VMStatus) = LogFileSync.OpenFtpSession("10.1.30.80", "clinical", "Madne55");
                Is_LogFileSync = Is_FTPstart;
            });

            //BindingLogFileSync();
        }


        private RelayCommand _PostProcessingViewCommand;
        public ICommand PostProcessingViewCommand
        {
            get
            {
                return _PostProcessingViewCommand ?? (_PostProcessingViewCommand = new RelayCommand(PostProcessingView));
            }
        }
        private void PostProcessingView()
        {
            if (VM_MainWindow.selectedView == "SpotScanningView")
            {
                if (VM_MainWindow.NCC_Post_View == null)
                {
                    VM_MainWindow.NCC_Post_View = new HUREL_PG_GUI.Views.Window_SpotScanningView_PostProcessing();
                    VM_MainWindow.NCC_Post_View.Show();
                }
                else if (VM_MainWindow.NCC_Post_View != null)
                {
                    VM_MainWindow.NCC_Post_View.Visibility = Visibility.Visible;
                }
            }
            else if (VM_MainWindow.selectedView == "LineScanningView")
            {
                if (VM_MainWindow.SMC_Post_View == null)
                {
                    VM_MainWindow.SMC_Post_View = new HUREL_PG_GUI.Views.Window_LineScanningView_PostProcessing();
                    VM_MainWindow.SMC_Post_View.Show();
                }
                else if (VM_MainWindow.SMC_Post_View != null)
                {
                    VM_MainWindow.SMC_Post_View.Visibility = Visibility.Visible;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Error Code 001");
            }
        }


        private RelayCommand _PositioningSystemViewCommand;
        public ICommand PositioningSystemViewCommand
        {
            get
            {
                return _PositioningSystemViewCommand ?? (_PositioningSystemViewCommand = new RelayCommand(PositioningSystemView));
            }
        }
        private void PositioningSystemView()
        {
            if (VM_MainWindow.PositioningSystem_View == null)
            {
                VM_MainWindow.PositioningSystem_View = new HUREL_PG_GUI.Views.Window_PositioningSystem();
                VM_MainWindow.PositioningSystem_View.Show();
            }
            else if (VM_MainWindow.PositioningSystem_View != null)
            {
                VM_MainWindow.PositioningSystem_View.Visibility = Visibility.Visible;
            }
            else
            {
                System.Windows.MessageBox.Show("Error Code [JJR]0005");
            }
        }

        #endregion


        ////////////////////////////////////////////////////////

        #region Code/GUI Verificaiton


        // 01. Binary, Plan File Load (완료, 2/22 23:56)
        private RelayCommand _TestCommand1;
        public ICommand TestCommand1
        {
            get
            {
                return _TestCommand1 ?? (_TestCommand1 = new RelayCommand(Test1));
            }
        }

        private async void Test1()
        {
            VMStatus = "Start up ftp server";

            // Make local path

            IsMonitoring = true;
            await ReadLogFilesLoop("./testLogFolder");
        }


        // 02. Log File Synchronization Start (완료, 2/23, 01:15)
        private RelayCommand _TestCommand2;
        public ICommand TestCommand2
        {
            get
            {
                return _TestCommand2 ?? (_TestCommand2 = new RelayCommand(Test2));
            }
        }
        private void Test2()
        {
            LogFileSync.StopSyncAndDownloadLogFile();

        }

        // 03. Real-time Code Verification Start
        private RelayCommand _TestCommand3;
        public ICommand TestCommand3
        {
            get
            {
                return _TestCommand3 ?? (_TestCommand3 = new RelayCommand(Test3));
            }
        }
        private async void Test3()
        {
       
        }
        
        #endregion




    }
}
