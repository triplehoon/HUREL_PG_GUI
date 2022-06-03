using AsyncAwaitBestPractices.MVVM;
using GalaSoft.MvvmLight.Command;
using HUREL_PG_GUI.Models;
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

namespace HUREL_PG_GUI.ViewModels
{
    public class VM_LineScanning : ViewModelBase
    {
        // Event Transfer (Model -> ViewModel)
        public static EventTransfer _EventTransfer;
        public static bool isStart = false;

        // Real-time Monitoring Objects
        private List<PlanStruct_SMC> Plan_SMC = new List<PlanStruct_SMC>();
        private List<PlanStruct_SMC> Plan_SMC_SelectedField = new List<PlanStruct_SMC>();
        private List<PGStruct> PG_raw = new List<PGStruct>();
        private List<PlanPGMergedDataStruct> MergedData;
        private List<SpotMapStruct> spotMap;
        private List<BeamRangeMapStruct> beamRangeMap;

        // Code Verification Objects (Not used)
        private List<PGStruct> PG_Verification = new List<PGStruct>();

        // Configuration
        public static Configuration_SMC _Configuration_SMC = new Configuration_SMC();        

        // Visualized Objects
        public ObservableCollection<SpotMapStruct> VM_SpotMap { get; set; }
        public ObservableCollection<BeamRangeMapStruct> VM_BeamRangeMap { get; set; }

        public VM_LineScanning()
        {
            //Console.WriteLine("VM_LineScanning Initialized");
            ConfigurationSetting_SMC();

            _EventTransfer = new EventTransfer();
            _EventTransfer.GeneratedEvent += VM_GenerateEvent;
        }  

        public class Configuration_SMC
        {
            // Path
            public string Path_Datas;
            public string Path_VerifyDatas; // for easy dialog open

            // Object
            public List<RangeInPMMAStruct> RangeInPMMA = new List<RangeInPMMAStruct>();
        }

        #region Binding

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


        private string gantryAngle;
        public string GantryAngle
        {
            get
            { 
                return gantryAngle; 
            }
            set
            {
                gantryAngle = value;
                OnPropertyChanged(nameof(GantryAngle));
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


        private int plan_TotalSegment = 0;
        public int Plan_TotalSegment
        {
            get
            {
                return plan_TotalSegment;
            }
            set
            {
                plan_TotalSegment = value;
                OnPropertyChanged(nameof(Plan_TotalSegment));
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


        private string loadedFileName_DICOM_RP;
        public string LoadedFileName_DICOM_RP
        {
            get
            {
                return loadedFileName_DICOM_RP;
            }
            set
            {
                loadedFileName_DICOM_RP = value;
                OnPropertyChanged(nameof(LoadedFileName_DICOM_RP));
            }
        }


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


        private int currentLayer;
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
            if (isMonitoring == false) // When Monitoring Start
            {
                string status = "";
                VMStatus = "FPGA setting...";

                //VM_MainWindow.FPGAControl.start_stop_usb
                //bool isFPGAstart = await Task.Run(() => VM_MainWindow.FPGAControl.Command_MonitoringStart(out status)).ConfigureAwait(false);
                //bool isPGdistUpdate = await Task.Run(() => PGdistUpdate());

                //await Task.Run(() => FPGAControl.start_stop_usb()).ConfigureAwait(false);

                VMStatus = status;
                IsMonitoring = true;
            }
            else // When Monitoring Stop
            {
                string status = "";

                //bool isFPGAstart = await Task.Run(() => VM_MainWindow.FPGAControl.Command_MonitoringStart(out status)).ConfigureAwait(false);
                //bool isPGdisUpdate = await Task.Run(() => PGdistUpdate());

                //await Task.Run(() => FPGAControl.start_stop_usb()).ConfigureAwait(false);

                VMStatus = "Idle";
                IsMonitoring = false;
            }
        }


        private AsyncCommand _LoadDICOMCommand;
        public ICommand LoadDICOMCommand
        {
            get
            {
                return _LoadDICOMCommand ?? (_LoadDICOMCommand = new AsyncCommand(LoadDICOMAsync));
            }
        }
        private async Task LoadDICOMAsync()
        {
            Plan_SMC = new List<PlanStruct_SMC>();
            PlanFileClass_SMC Class = new PlanFileClass_SMC();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = _Configuration_SMC.Path_VerifyDatas;
            dialog.Filter = "SMC RT Plan Dicom File (*.dcm)|*.dcm";
            DialogResult result = dialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                System.Windows.Forms.MessageBox.Show("Plan_SMC File Load Canceled", $"Plan_SMC File Load Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dialog.FileName.Length > 0)
            {
                await Task.Run(() => Plan_SMC = Class.LoadPlanFile(dialog.FileName));
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Load Dicom File", "Error");
            }

            PatientName = "";
            PatientID = Convert.ToString(Plan_SMC.Last().a1_PatientNumber);

            int gantryAngle_temp = 270;

            Plan_SMC_SelectedField = Plan_SMC.FindAll(x => x.a4_GantryAngle == gantryAngle_temp);
            GantryAngle = Convert.ToString(Plan_SMC_SelectedField.Last().a4_GantryAngle);
            Plan_TotalLayer = Plan_SMC_SelectedField.Last().a3_LayerNumber;

            Is_PlanFileLoaded = true;
        }


        private RelayCommand _LoadPlanFileCommand;
        public ICommand LoadPlanFileCommand
        {
            get
            {
                return _LoadPlanFileCommand ?? (_LoadPlanFileCommand = new RelayCommand(LoadPlanFile));
            }
        }
        private void LoadPlanFile()
        {

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

        #endregion

        #region Sub-Function

        private List<RangeInPMMAStruct> LoadRangeInPMMA(string path)
        {
            List<RangeInPMMAStruct> RangeInPMMA = new List<RangeInPMMAStruct>();

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
                {
                    string lines = null;
                    string[] tempString = null;

                    while ((lines = sr.ReadLine()) != null)
                    {
                        RangeInPMMAStruct temp = new RangeInPMMAStruct();

                        tempString = lines.Split("\t");

                        temp.Energy = Convert.ToDouble(tempString[0]);
                        temp.Range = Convert.ToDouble(tempString[1]);

                        RangeInPMMA.Add(temp);
                    }
                }
            }
            return RangeInPMMA;
        }

        private void VM_GenerateEvent(object sender, EventTransfer.AnalysisEventHandlerArgs e)
        {
            //Task.Run(() => VM_SMCRealTimeAnalysis_Start());
            Trace.WriteLine("VM으로 event 전달되었음!");

            // Task.Run(() => VM_SMCPostProcessingAnalysis_Start());
        }

        private async Task VM_SMCRealTimeAnalysis_Start()
        {
            await System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                SMCAnalysisClass Class_SMC = new SMCAnalysisClass();
                MergedData = await Task.Run(() => Class_SMC.GenerateMergedData_RealTime(Plan_SMC_SelectedField, PG_raw));

                int StartLayer = 0;
                int LastLayer = MergedData.Last().a3_LayerNumber;

                Task<List<SpotMapStruct>> SpotMapTask = Class_SMC.GenerateSpotMap(MergedData, LastLayer);
                Task<List<BeamRangeMapStruct>> BeamRangeMapTask = Class_SMC.GenerateBeamRangeMap(MergedData, 7, StartLayer, LastLayer, 1);

                spotMap = await SpotMapTask;
                beamRangeMap = await BeamRangeMapTask;

                VM_SpotMap = new ObservableCollection<SpotMapStruct>();
                spotMap.ForEach(x => VM_SpotMap.Add(x));

                VM_BeamRangeMap = new ObservableCollection<BeamRangeMapStruct>();
                beamRangeMap.ForEach(x => VM_BeamRangeMap.Add(x));

                OnPropertyChanged(nameof(VM_SpotMap));
                OnPropertyChanged(nameof(VM_BeamRangeMap));

                FinishedLayerRatio = Math.Round((double)100 * LastLayer / Plan_TotalLayer);
                OnPropertyChanged(nameof(FinishedLayerRatio));
            }));
        }

        private void ConfigurationSetting_SMC()
        {
            // Path
            string DefaultPath = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName).FullName;
            _Configuration_SMC.Path_Datas = Path.Combine(DefaultPath, "Datas");
            //_Configuration_SMC.Path_VerifyDatas = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\99. FunctionTest\02. (완료) Shift and merge 디버깅\검증 자료";
            _Configuration_SMC.Path_VerifyDatas = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\99. FunctionTest\05. SMC\02. DICOM Data";

            // Object
            _Configuration_SMC.RangeInPMMA = LoadRangeInPMMA(Path.Combine(_Configuration_SMC.Path_Datas, "RangeInPMMA.txt"));
        }

        #endregion

        ////////////////////////////////////////////////////////

        #region Code/GUI Verification

        #region Command

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
            Console.WriteLine($"===== SMC Real-time Monitoring Mode Code Verificaiton =====");
            Console.Write($"Plan File Loading...");


            // --- Plan File Load --- //
            Plan_SMC = new List<PlanStruct_SMC>();
            Plan_SMC_SelectedField = new List<PlanStruct_SMC>();
            PlanFileClass_SMC Class_PlanLoad = new PlanFileClass_SMC();
            var Data_RP_Path_First = "D:\\OneDrive - 한양대학교\\01. Research\\01. 통합 제어 프로그램\\99. FunctionTest\\05. SMC\\02. DICOM Data\\";
            var Data_RP_Path_Middle = "3G_270D_S_2Gy";
            var Data_RP_Name = "RP1.2.752.243.1.1.20210507152916139.2800.28424.dcm";
            var FullPath_RP = Data_RP_Path_First + Data_RP_Path_Middle + "\\" + Data_RP_Name;
            await Task.Run(() =>
            {
                Plan_SMC = Class_PlanLoad.LoadPlanFile(FullPath_RP);
                Plan_SMC_SelectedField = Plan_SMC.FindAll(x => x.a2_FieldNumber == 2); // 1: Sphere, 2: Cubic
                Plan_TotalLayer = Plan_SMC_SelectedField.Last().a3_LayerNumber;
            });
            LoadedFileName_DICOM_RP = Data_RP_Name;
            Is_PlanFileLoaded = true;

            Console.SetCursorPosition(10, 1);
            Console.Write($"Loaded, ");

            Console.Write($"PG Data Loading...");


            // --- Binary File Load --- //
            PG_Verification = new List<PGStruct>();
            PGDataClass Class_PGLoad = new PGDataClass();
            var Data_PG_Path_First = "D:\\OneDrive - 한양대학교\\01. Research\\01. 통합 제어 프로그램\\99. FunctionTest\\05. SMC\\01. PG Data\\";
            var Data_PG_Name = "data_shallowCub2Gy_Shift5-10.bin";
            var FullPath_PG = Data_PG_Path_First + Data_PG_Name;
            await Task.Run(() =>
            {
                PG_Verification = Class_PGLoad.LoadPGData(FullPath_PG);
            });
            // LoadedFileName_PG = FullPath_PG;
            // IsPGDataLoaded_PostProcessing = true;

            Console.SetCursorPosition(26, 1);
            Console.WriteLine($"Loaded       ");
            Console.WriteLine($"All File Loaded!");
        }

                
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
            Console.WriteLine($"");
            Console.WriteLine($"*** Code Verification Start! ***");

            // (1) 

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Task.Run(() => GetPG_raw(sw)).ConfigureAwait(false);

        }


        private RelayCommand _TestCommand3;
        public ICommand TestCommand3
        {
            get
            {
                return _TestCommand3 ?? (_TestCommand3 = new RelayCommand(Test3));
            }
        }
        private void Test3()
        {
            SMCAnalysisClass Class = new SMCAnalysisClass();
            Class.TestFunction();

            //EventGenerateTest Class2 = new EventGenerateTest();
            //Class2.FunctionTest();
        }

        #endregion

        #region Sub-Functions

        private void GetPG_raw(Stopwatch sw)
        {
            Console.WriteLine($"{sw.Elapsed} sec: PG Distribution Measurement Start");

            int TimeIndex = 1;
            int LayerNumber = 1;
            int TuningIndex = 1;
            bool isLayer = false;
            double Threshold = 2.5;
            int[] Trigger_temp = new int[2];

            Stopwatch sw_local = new Stopwatch();

            PGStruct tempPG = new PGStruct();

            for (int i = 0; i < PG_Verification.Count; i++)
            {
                WaitUS(sw, PG_Verification[i].TriggerInputStartTime);

                tempPG.TriggerInputStartTime = PG_Verification[i].TriggerInputStartTime;
                tempPG.TriggerInputEndTime = PG_Verification[i].TriggerInputEndTime;
                tempPG.ADC = PG_Verification[i].ADC;
                tempPG.ChannelCount = PG_Verification[i].ChannelCount;
                tempPG.SumCounts = PG_Verification[i].SumCounts;

                PG_raw.Add(tempPG);

                // ADC 사용하여 Trigger Event 생성하는 코드

                if (isLayer == false)
                {
                    if (tempPG.ADC < Threshold)
                    {
                        isLayer = true;
                        Trigger_temp[0] = tempPG.TriggerInputEndTime;

                        CurrentLayer = LayerNumber;
                        CurrentLayerRatio = Math.Round((double)100 * LayerNumber / Plan_TotalLayer);

                        sw_local.Stop();
                        Console.WriteLine($"{sw.Elapsed} sec: Layer Interval: {sw_local.ElapsedMilliseconds} msec");
                        sw_local = new Stopwatch();
                    }
                }
                else
                {
                    if (tempPG.ADC > Threshold)
                    {
                        isLayer = false;
                        Trigger_temp[1] = tempPG.TriggerInputEndTime;

                        if (Trigger_temp[1] - Trigger_temp[0] < 10000)
                        {
                            Console.WriteLine($"{sw.Elapsed} sec: [Irrdiation] Layer: {LayerNumber} Tuning: {TuningIndex} ----- {(Trigger_temp[1] - Trigger_temp[0]) / 1000} msec");
                            TuningIndex++;
                        }
                        else
                        {
                            Console.WriteLine($"{sw.Elapsed} sec: [Irrdiation] Layer: {LayerNumber}           ----- {(Trigger_temp[1] - Trigger_temp[0]) / 1000} msec");
                            sw_local.Start();
                            LayerNumber++;
                            TuningIndex = 1;
                            Task.Run(() => VM_SMCPostProcessingAnalysis_Start());
                        }

                    }
                }

                tempPG = new PGStruct();

                if (PG_Verification[i].TriggerInputStartTime > 5000000 * TimeIndex)
                {
                    Console.WriteLine($"{sw.Elapsed} sec: {sw.ElapsedMilliseconds / 1000} sec Elapsed");
                    TimeIndex++;
                }
            }
            Console.WriteLine($"{sw.Elapsed} sec: PG Distribution Measurement Finished");

            Console.WriteLine($"*** Code Verification Finished! ***");
            System.Windows.MessageBox.Show("Monitoring Finished");
        }

        public async Task VM_SMCPostProcessingAnalysis_Start()
        {
            await System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                //if (IsReadyPostProcessingAnalysis == true)
                //{
                //IsPostProcessingAnalysising = true;

                SMCAnalysisClass Class_SMC = new SMCAnalysisClass();
                MergedData = await Task.Run(() => Class_SMC.GenerateMergedData_RealTime(Plan_SMC_SelectedField, PG_raw));

                int StartLayer = 0;
                int LastLayer = MergedData.Last().a3_LayerNumber;

                Task<List<SpotMapStruct>> SpotMapTask = Class_SMC.GenerateSpotMap(MergedData, LastLayer);
                Task<List<BeamRangeMapStruct>> BeamRangeMapTask = Class_SMC.GenerateBeamRangeMap(MergedData, 7, StartLayer, LastLayer, 1);

                spotMap = await SpotMapTask;
                beamRangeMap = await BeamRangeMapTask;

                VM_SpotMap = new ObservableCollection<SpotMapStruct>();
                spotMap.ForEach(x => VM_SpotMap.Add(x));

                VM_BeamRangeMap = new ObservableCollection<BeamRangeMapStruct>();
                beamRangeMap.ForEach(x => VM_BeamRangeMap.Add(x));

                OnPropertyChanged(nameof(VM_SpotMap));
                OnPropertyChanged(nameof(VM_BeamRangeMap));

                FinishedLayerRatio = Math.Round((double)100 * LastLayer / Plan_TotalLayer);
                OnPropertyChanged(nameof(FinishedLayerRatio));

                sw.Stop();
                Trace.WriteLine($"{sw.ElapsedMilliseconds} msec");
            }));
        }

        private void WaitUS(Stopwatch sw, long us)
        {
            while (sw.ElapsedTicks / 10 < us) ;
        }

        #endregion

        #endregion       
    }

    /// <summary>
    /// Event(Model -> ViewModel)
    /// </summary>
    public class EventTransfer
    {
        public void RaiseEvent()
        {
            if (GeneratedEvent != null && VM_LineScanning.isStart != false)
            {
                GeneratedEvent(this, new AnalysisEventHandlerArgs());
            }
        }

        public event EventHandler<AnalysisEventHandlerArgs> GeneratedEvent;
        public class AnalysisEventHandlerArgs
        {
            public AnalysisEventHandlerArgs() { }
        }
    }
}
