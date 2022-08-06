using AsyncAwaitBestPractices.MVVM;
using GalaSoft.MvvmLight.CommandWpf;
using HUREL_PG_GUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace HUREL_PG_GUI.ViewModels
{
    public class VM_SpotScanning_PostProcessing : ViewModelBase
    {
        #region Data struct
        static public List<PlanLogPGMergedDataStruct> MergedData = new List<PlanLogPGMergedDataStruct>();
        static string InitialPath = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\99. FunctionTest\02. (완료) Shift and merge 디버깅\검증 자료";
        static public string Directory_DebugFile = @"D:\OneDrive - 한양대학교\바탕 화면\temp";

        public List<PlanStruct_NCC> PostProcessing_PlanFile_NCC = new List<PlanStruct_NCC>();
        public List<PGStruct> PostProcessing_PG = new List<PGStruct>();
        static public List<LogStruct_NCC> LogDatasPostProcessing_NCC = new List<LogStruct_NCC>();

        public List<GapPeakAndRangeStruct> GapPeakAndRange = new List<GapPeakAndRangeStruct>();

        #endregion

        public static DateTime FirstTuningBeamTime = new DateTime(0); // 0528

        public VM_SpotScanning_PostProcessing()
        {
            Data_SelectType = Data_SelectTypes.Data_Patient;
            StaticParameterSetting();

            Trace.WriteLine("VM_SpotScanning_PostProcessing Initialized");
        }

        private void StaticParameterSetting()
        {
           
        }

        #region Binding


        public ObservableCollection<BeamRangeMapStruct> VM_BeamRangeMap { get; set; }
        private List<BeamRangeMapStruct> beamRangeMap;

        public ObservableCollection<SpotMapStruct> VM_SpotMap { get; set; }
        private List<SpotMapStruct> spotMap;

        public ObservableCollection<BeamRangeMapStruct> TestMap { get; set; }


        public enum Data_SelectTypes
        {
            Data_Patient,
            Data_Individual
        }

        public static Data_SelectTypes data_SelectType;
        public Data_SelectTypes Data_SelectType
        {
            get
            {
                return data_SelectType;
            }
            set
            {
                data_SelectType = value;
                OnPropertyChanged(nameof(Data_SelectType));
                Trace.WriteLine($"Data Select Mode: {data_SelectType}");
            }
        }


        private string measuredDate;
        public string MeasuredDate
        {
            get
            {
                return measuredDate;
            }
            set
            {
                measuredDate = value;
                OnPropertyChanged(nameof(MeasuredDate));
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


        private string cameraPosition;
        public string CameraPosition
        {
            get
            {
                return cameraPosition;
            }
            set
            {
                cameraPosition = value;
                OnPropertyChanged(nameof(CameraPosition));
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


        private string _DICOM_PrescribedDose;
        public string DICOM_PrescribedDose
        {
            get
            {
                return _DICOM_PrescribedDose;
            }
            set
            {
                _DICOM_PrescribedDose = value + " Gy";
                OnPropertyChanged(nameof(DICOM_PrescribedDose));
            }
        }


        private string _DICOM_CurrentFraction;
        public string DICOM_CurrentFraction
        {
            get
            {
                return _DICOM_CurrentFraction;
            }
            set
            {
                _DICOM_CurrentFraction = value;
                OnPropertyChanged(nameof(DICOM_CurrentFraction));
            }
        }


        private string _DICOM_TotalFraction;
        public string DICOM_TotalFraction
        {
            get
            {
                return _DICOM_TotalFraction;
            }
            set
            {
                _DICOM_TotalFraction = value;
                OnPropertyChanged(nameof(DICOM_TotalFraction));
            }
        }


        private string loadedFileName_Plan;
        public string LoadedFileName_Plan
        {
            get
            {
                return loadedFileName_Plan;
            }
            set
            {
                loadedFileName_Plan = value;
                OnPropertyChanged(nameof(LoadedFileName_Plan));
            }
        }


        private string loadedFileName_Log;
        public string LoadedFileName_Log
        {
            get
            {
                return loadedFileName_Log;
            }
            set
            {
                loadedFileName_Log = value;
                OnPropertyChanged(nameof(LoadedFileName_Log));
            }
        }


        private string loadedFileName_PG;
        public string LoadedFileName_PG
        {
            get
            {
                return loadedFileName_PG;
            }
            set
            {
                loadedFileName_PG = value;
                OnPropertyChanged(nameof(LoadedFileName_PG));
            }
        }


        private string loadedFileName_DICOM;
        public string LoadedFileName_DICOM
        {
            get
            {
                return loadedFileName_DICOM;
            }
            set
            {
                loadedFileName_DICOM = value;
                OnPropertyChanged(nameof(LoadedFileName_DICOM));
            }
        }

        #endregion

        #region View Status(Converter)

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


        private int spotMap_SelectedLayer;
        public int SpotMap_SelectedLayer
        {
            get
            {
                return spotMap_SelectedLayer;
            }
            set
            {
                spotMap_SelectedLayer = value;
                OnPropertyChanged(nameof(SpotMap_SelectedLayer));
            }
        }


        private int beamRangeMap_Selected_StartLayer;
        public int BeamRangeMap_Selected_StartLayer
        {
            get
            {
                return beamRangeMap_Selected_StartLayer;
            }
            set
            {
                beamRangeMap_Selected_StartLayer = value;
                OnPropertyChanged(nameof(BeamRangeMap_Selected_StartLayer));
            }
        }


        private int beamRangeMap_Selected_EndLayer;
        public int BeamRangeMap_Selected_EndLayer
        {
            get
            {
                return beamRangeMap_Selected_EndLayer;
            }
            set
            {
                beamRangeMap_Selected_EndLayer = value;
                OnPropertyChanged(nameof(BeamRangeMap_Selected_EndLayer));
            }
        }


        private bool isPlanFileLoaded_PostProcessing;
        public bool IsPlanFileLoaded_PostProcessing
        {
            get
            {
                return isPlanFileLoaded_PostProcessing;
            }
            set
            {
                isPlanFileLoaded_PostProcessing = value;
                OnPropertyChanged(nameof(IsPlanFileLoaded_PostProcessing));
            }
        }


        private bool isLogFileLoaded_PostProcessing;
        public bool IsLogFileLoaded_PostProcessing
        {
            get
            {
                return isLogFileLoaded_PostProcessing;
            }
            set
            {
                isLogFileLoaded_PostProcessing = value;
                OnPropertyChanged(nameof(IsLogFileLoaded_PostProcessing));
            }
        }


        private bool isPGDataLoaded_PostProcessing;
        public bool IsPGDataLoaded_PostProcessing
        {
            get
            {
                return isPGDataLoaded_PostProcessing;
            }
            set
            {
                isPGDataLoaded_PostProcessing = value;
                OnPropertyChanged(nameof(IsPGDataLoaded_PostProcessing));
            }
        }


        private bool isDICOMFileLoaded_PostProcessing;
        public bool IsDICOMFileLoaded_PostProcessing
        {
            get
            {
                return isDICOMFileLoaded_PostProcessing;
            }
            set
            {
                isDICOMFileLoaded_PostProcessing = value;
                OnPropertyChanged(nameof(IsDICOMFileLoaded_PostProcessing));
            }
        }


        private bool isReadyPostProcessingAnalysis;
        public bool IsReadyPostProcessingAnalysis
        {
            get
            {
                return isReadyPostProcessingAnalysis;
            }
            set
            {
                isReadyPostProcessingAnalysis = value;
                OnPropertyChanged(nameof(IsReadyPostProcessingAnalysis));
            }
        }


        private bool isPostProcessingAnalysising;
        public bool IsPostProcessingAnalysising
        {
            get
            {
                return isPostProcessingAnalysising;
            }
            set
            {
                isPostProcessingAnalysising = value;
                OnPropertyChanged(nameof(IsPostProcessingAnalysising));
            }
        }

        #endregion

        #region Command

        private RelayCommand _DrawSpotMapCommand;
        public ICommand DrawSpotMapCommand
        {
            get
            {
                return _DrawSpotMapCommand ?? (_DrawSpotMapCommand = new RelayCommand(DrawSpotMap));
            }
        }
        private async void DrawSpotMap()
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

            Task<List<SpotMapStruct>> SpotMapTask = Class.GenerateSpotMap(MergedData, spotMap_SelectedLayer - 1);
            spotMap = await SpotMapTask;

            VM_SpotMap = new ObservableCollection<SpotMapStruct>();
            spotMap.ForEach(x => VM_SpotMap.Add(x));
            OnPropertyChanged(nameof(VM_SpotMap));

            sw.Stop();
            Trace.WriteLine($"{sw.ElapsedMilliseconds} ms");
        }


        private RelayCommand _DrawBeamRageMapCommand;
        public ICommand DrawBeamRageMapCommand
        {
            get
            {
                return _DrawBeamRageMapCommand ?? (_DrawBeamRageMapCommand = new RelayCommand(DrawBeamRageMap));
            }
        }
        private async void DrawBeamRageMap()
        {
            #region Test Code(2022-02-17), Un-Activated

            //for (int j = 0; j < 10; j++)
            //{
            //    Trace.WriteLine($"{j}: Beam Range Map Speed Evaluation Start");

            //    for (int i = 1; i < 12; i++)
            //    {
            //        Stopwatch sw = new Stopwatch();
            //        sw.Start();

            //        NCCAnalysisClass Class = new NCCAnalysisClass();
            //        Task<List<BeamRangeMapStruct>> BeamRangeMapTask = Class.GenerateBeamRangeMap(MergedData, 7, 0, i - 1, 1);
            //        beamRangeMap = await BeamRangeMapTask;

            //        VM_BeamRangeMap = new ObservableCollection<BeamRangeMapStruct>();
            //        beamRangeMap.ForEach(x => VM_BeamRangeMap.Add(x));
            //        OnPropertyChanged(nameof(VM_BeamRangeMap));

            //        sw.Stop();
            //        Trace.WriteLine($"{sw.ElapsedMilliseconds}");
            //    }
            //    Trace.WriteLine($"");
            //}

            #endregion

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            NCCAnalysisClass Class = new NCCAnalysisClass();

            // Option 1: Possible
            Task<List<BeamRangeMapStruct>> BeamRangeMapTask = Class.GenerateBeamRangeMap(MergedData, 7, beamRangeMap_Selected_StartLayer - 1, beamRangeMap_Selected_EndLayer - 1, 1);
            beamRangeMap = await BeamRangeMapTask;
            // Option 2: ImPossible ???
            //beamRangeMap = await Task.Run(() => Class.GenerateBeamRangeMap(MergedData, 7, StartLayer, LastLayer, 1));

            VM_BeamRangeMap = new ObservableCollection<BeamRangeMapStruct>();
            beamRangeMap.ForEach(x => VM_BeamRangeMap.Add(x));
            OnPropertyChanged(nameof(VM_BeamRangeMap));

            //sw.Stop();
            //Trace.WriteLine($"{sw.ElapsedMilliseconds} ms");
        }


        private RelayCommand _Select_PatientDataCommand;
        public ICommand Select_PatientDataCommand
        {
            get
            {
                return _Select_PatientDataCommand ?? (_Select_PatientDataCommand = new RelayCommand(Select_PatientData));
            }
        }
        private void Select_PatientData()
        {
            Data_SelectType = Data_SelectTypes.Data_Patient;
        }


        private RelayCommand _Select_IndividualDataCommand;
        public ICommand Select_IndividualDataCommand
        {
            get
            {
                return _Select_IndividualDataCommand ?? (_Select_IndividualDataCommand = new RelayCommand(Select_IndividualData));
            }
        }
        private void Select_IndividualData()
        {
            Data_SelectType = Data_SelectTypes.Data_Individual;
        }


        private RelayCommand _LoadPatientDatasCommand;
        public ICommand LoadPatientDatasCommand
        {
            get
            {
                return _LoadPatientDatasCommand ?? (_LoadPatientDatasCommand = new RelayCommand(LoadPatientDatas));
            }
        }
        private void LoadPatientDatas()
        {

        }


        private RelayCommand _PlanFileLoadCommand;
        public ICommand PlanFileLoadCommand
        {
            get
            {
                return _PlanFileLoadCommand ?? (_PlanFileLoadCommand = new RelayCommand(PlanFileLoad));
            }
        }
        private void PlanFileLoad()
        {
            PostProcessing_PlanFile_NCC = new List<PlanStruct_NCC>(); // Data 생성
            

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = InitialPath;
            dialog.Filter = "Pld|*.pld|Text|*.txt|All|*.*";
            dialog.ShowDialog();

            PostProcessing_PlanFile_NCC = PlanFileClass_NCC.LoadPlanFile(dialog.FileName, true); ////////////////////////////////////

            if (PostProcessing_PlanFile_NCC != null)
            {
                // True: 출력창에 Plan_NCC file 정보 표출 // False: 정보 출력 안함
                Debug_WriteLine_PlanFileInformation(PostProcessing_PlanFile_NCC, true);
                Plan_TotalLayer = PostProcessing_PlanFile_NCC.Last().LayerNumber + 1;
                GantryAngle = Convert.ToString(270);

                LoadedFileName_Plan = dialog.FileName.Split('\\').Last();
                IsPlanFileLoaded_PostProcessing = true;

                CheckForAnalysisReady();
            }
            else
            {
                Trace.WriteLine("");
                Trace.WriteLine("Plan_NCC File = null");
                Trace.WriteLine("");
            }
        }


        private AsyncCommand _LogFilesLoadCommand;
        public ICommand LogFilesLoadCommand
        {
            get
            {
                return _LogFilesLoadCommand ?? (_LogFilesLoadCommand = new AsyncCommand(LogFilesLoad));
            }
        }
        private async Task LogFilesLoad()
        {
            LogDatasPostProcessing_NCC = new List<LogStruct_NCC>();
            NCCLogFileLoad Class = new NCCLogFileLoad();

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = InitialPath + "\\";
            DialogResult result = dialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            string SelectedPath = dialog.SelectedPath;
            List<string> XdrFiles_SelectedPath = Directory.GetFiles(SelectedPath, "*.xdr").ToList();

            if (XdrFiles_SelectedPath.Count == 0)
            {
                MessageBox.Show($"No log file('*.xdr') in the selected directory", $"Log_NCC File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Stopwatch sw = Stopwatch.StartNew();    
            await Task.Run(() => LogDatasPostProcessing_NCC = Class.LogDatasLoad_PostProcessing(SelectedPath)).ConfigureAwait(false);
            sw.Stop();
            Trace.WriteLine($"Log loading time: {sw.Elapsed.TotalMilliseconds} [ms]");
            LoadedFileName_Log = SelectedPath.Split('\\').Last();
            IsLogFileLoaded_PostProcessing = true;
            CheckForAnalysisReady();
        }


        private AsyncCommand _PGDataLoadCommand;
        public ICommand PGDataLoadCommand
        {
            get
            {
                return _PGDataLoadCommand ?? (_PGDataLoadCommand = new AsyncCommand(PGDataLoad));
            }
        }
        private async Task PGDataLoad()
        {
            PostProcessing_PG = new List<PGStruct>(); // Data 생성
            PGDataClass Class = new PGDataClass();            // Function 호출

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = InitialPath;
            dialog.Filter = "Bin|*.bin|Text|*.txt|All|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                //MessageBox.Show("PG Data Load Canceled", $"PG Data Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await Task.Run(() => PostProcessing_PG = Class.LoadPGData(dialog.FileName)); ///////////////////////////////////////////

            if (PostProcessing_PG.Count != 0)
            {
                // True: 출력창에 PG Data 정보 표출 // False: 정보 출력 안함 
                Debug_WriteLine_PGDataInformation(PostProcessing_PG, false);

                MeasuredDate = "2022-02-21   05:44";
                CameraPosition = "(0, 30, 0), (0, 20, 10)";

                LoadedFileName_PG = dialog.FileName.Split('\\').Last();
                IsPGDataLoaded_PostProcessing = true;

                CheckForAnalysisReady();
            }
            else
            {
                MessageBox.Show($"Select proper PG(*.bin) file.", $"Invalid PG data loaded!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private RelayCommand _DICOMFileLoadCommand;
        public ICommand DICOMFileLoadCommand
        {
            get
            {
                return _DICOMFileLoadCommand ?? (_DICOMFileLoadCommand = new RelayCommand(DICOMFileLoad));
            }
        }
        private void DICOMFileLoad()
        {
            LoadedFileName_DICOM = "This function is implemented soon...";
            IsDICOMFileLoaded_PostProcessing = true;
            CheckForAnalysisReady();
        }


        private AsyncCommand _PostProcessingAnalysisStartCommand;
        public ICommand PostProcessingAnalysisStartCommand
        {
            get
            {
                return _PostProcessingAnalysisStartCommand ?? (_PostProcessingAnalysisStartCommand = new AsyncCommand(PostProcessingAnalysisStart));
            }
        }
        private async Task PostProcessingAnalysisStart()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (IsReadyPostProcessingAnalysis == true)
            {
                IsPostProcessingAnalysising = true;

                NCCAnalysisClass Class = new NCCAnalysisClass();
                MergedData = await Task.Run(() => Class.GenerateMergedData_PostProcessing(PostProcessing_PlanFile_NCC, LogDatasPostProcessing_NCC, PostProcessing_PG)); // static data

                int StartLayer = 0;
                int LastLayer = MergedData.Last().Log_LayerNumber;

                //Task<List<SpotMapStruct>> SpotMapTask = Class.GenerateSpotMap(MergedData, SpotMap_SelectedLayer);
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

                BeamRangeMap_Selected_StartLayer = StartLayer + 1;
                BeamRangeMap_Selected_EndLayer = LastLayer + 1;
                IsPostProcessingAnalysising = false;

                #region Debug
                //string DebugFileName = "Debug_PG71_Split(50, 30).csv";
                //string DebugPath_excel = VM_SpotScanning_PostProcessing.Directory_DebugFile + "\\" + DebugFileName;
                //using (StreamWriter file = new StreamWriter(DebugPath_excel))
                //{
                //    for (int i = 0; i < MergedData.Count(); i++)
                //    {
                //        for (int j = 0; j < 71; j++)
                //        {
                //            file.Write($"{MergedData[i].ChannelCount_71[j]}, ");
                //        }
                //        file.WriteLine($"");
                //    }
                //}
                #endregion

                Trace.WriteLine("Code Finish");

                sw.Stop();
                Trace.WriteLine($"{sw.ElapsedMilliseconds} msec");
            }
            else
            {
                MessageBox.Show($"Error Code: JJR0003");
            }
        }

        public async void DrawBeamRangeMap(int StartLayer, int LastLayer)
        {
            NCCAnalysisClass Class = new NCCAnalysisClass();

            // Option 1: Possible
            Task<List<BeamRangeMapStruct>> BeamRangeMapTask = Class.GenerateBeamRangeMap(MergedData, 7, StartLayer - 1, LastLayer - 1, 1);
            beamRangeMap = await BeamRangeMapTask;
            // Option 2: ImPossible ???
            //beamRangeMap = await Task.Run(() => Class.GenerateBeamRangeMap(MergedData, 7, StartLayer, LastLayer, 1));

            VM_BeamRangeMap = new ObservableCollection<BeamRangeMapStruct>();
            beamRangeMap.ForEach(x => VM_BeamRangeMap.Add(x));
            OnPropertyChanged(nameof(VM_BeamRangeMap));
        }


        private RelayCommand _ViewClearCommand;
        public ICommand ViewClearCommand
        {
            get
            {
                return _ViewClearCommand ?? (_ViewClearCommand = new RelayCommand(ViewClear));
            }
        }
        private void ViewClear()
        {
            PostProcessing_PlanFile_NCC = null;
            PostProcessing_PG = null;
            LogDatasPostProcessing_NCC = null;

            LoadedFileName_Plan = "";
            LoadedFileName_Log = "";
            LoadedFileName_PG = "";
            LoadedFileName_DICOM = "";

            IsPlanFileLoaded_PostProcessing = false;
            IsLogFileLoaded_PostProcessing = false;
            IsPGDataLoaded_PostProcessing = false;
            IsDICOMFileLoaded_PostProcessing = false;

            IsPostProcessingAnalysising = false;
            IsReadyPostProcessingAnalysis = false;

            CheckForAnalysisReady();

            VM_SpotMap = new ObservableCollection<SpotMapStruct>();
            VM_SpotMap.Add(new SpotMapStruct(-10000, -10000, 10, new SolidColorBrush(Color.FromScRgb(1, 1, 1, 1))));

            VM_BeamRangeMap = new ObservableCollection<BeamRangeMapStruct>();
            VM_BeamRangeMap.Add(new BeamRangeMapStruct(-10000, -10000, 10, new SolidColorBrush(Color.FromScRgb(1, 1, 1, 1))));

            GantryAngle = "";
            MeasuredDate = "";
            CameraPosition = "";

            Plan_TotalLayer = 1;
            SpotMap_SelectedLayer = 1;
            BeamRangeMap_Selected_StartLayer = 1;
            BeamRangeMap_Selected_EndLayer = 1;

            OnPropertyChanged(nameof(VM_SpotMap));
            OnPropertyChanged(nameof(VM_BeamRangeMap));

            DataClear();
        }


        private AsyncCommand _TestCommand;
        public ICommand TestCommand
        {
            get
            {
                return _TestCommand ?? (_TestCommand = new AsyncCommand(Test));
            }
        }
        private async Task Test()
        {
            // Plan File Load
            PostProcessing_PlanFile_NCC = new List<PlanStruct_NCC>();
            PlanFileClass_NCC Class1 = new PlanFileClass_NCC();
            PostProcessing_PlanFile_NCC = PlanFileClass_NCC.LoadPlanFile("C:\\Users\\HUREL\\Desktop\\20220528 NCC 재린\\Spot\\3DplotMultiSBox2Gy.pld", true);
            Debug_WriteLine_PlanFileInformation(PostProcessing_PlanFile_NCC, true);
            LoadedFileName_Plan = "3DplotMultiSBox2Gy.pld";
            IsPlanFileLoaded_PostProcessing = true;
            Plan_TotalLayer = PostProcessing_PlanFile_NCC.Last().LayerNumber + 1;

            // Log File Load
            LogDatasPostProcessing_NCC = new List<LogStruct_NCC>();
            NCCLogFileLoad Class2 = new NCCLogFileLoad();
            //string SelectedPath = "D:\\OneDrive - 한양대학교\\01. Research\\01. 통합 제어 프로그램\\99. FunctionTest\\02. (완료) Shift and merge 디버깅\\검증 자료\\02. Log\\Box5Gy_QuadLocalShift_1";
            string SelectedPath = "C:\\Users\\HUREL\\Desktop\\20220528 NCC 재린\\Log\\20220528_095649_18";
            await Task.Run(() => LogDatasPostProcessing_NCC = Class2.LogDatasLoad_PostProcessing(SelectedPath));
            //LoadedFileName_Log = "Box5Gy_QuadLocalShift_1";
            LoadedFileName_Log = "Box2Gy";
            IsLogFileLoaded_PostProcessing = true;

            // PG data Load
            PostProcessing_PG = new List<PGStruct>();
            PGDataClass Class3 = new PGDataClass();
            //string FileName = "D:\\OneDrive - 한양대학교\\01. Research\\01. 통합 제어 프로그램\\99. FunctionTest\\02. (완료) Shift and merge 디버깅\\검증 자료\\03. PG\\Box5Gy_QuadLocalShift_1.bin";
            string FileName = "C:\\Users\\HUREL\\Desktop\\20220528 NCC 재린\\PG\\data (18).bin";
            await Task.Run(() => PostProcessing_PG = Class3.LoadPGData(FileName));
            Debug_WriteLine_PGDataInformation(PostProcessing_PG, false);
            //LoadedFileName_PG = "Box5Gy_QuadLocalShift_1.bin";
            LoadedFileName_PG = "data (18).bin";
            IsPGDataLoaded_PostProcessing = true;

            DICOMFileLoad();
            CheckForAnalysisReady();

            await PostProcessingAnalysisStart();
        }

        #endregion

        #region Sub-Function

        private void CheckForAnalysisReady()
        {
            if (isPlanFileLoaded_PostProcessing == true && isLogFileLoaded_PostProcessing == true && isPGDataLoaded_PostProcessing == true && isDICOMFileLoaded_PostProcessing == true)
            {
                IsReadyPostProcessingAnalysis = true;
            }
        }

        private void Debug_WriteLine_PlanFileInformation(List<PlanStruct_NCC> Plan, bool flag)
        {
            (int TotalLayer, int TotalSpot, double TotalMU) = (0, 0, 0);

            if (flag)
            {
                TotalLayer = Plan.Last().LayerNumber + 1;

                for (int i = 0; i < TotalLayer; i++)
                {
                    int SpotinLayer = (from Spots in Plan
                                       where Spots.LayerNumber == i
                                       select Spots).ToList()[0].LayerSpotCount;
                    TotalSpot += SpotinLayer;
                }

                for (int i = 0; i < TotalLayer; i++)
                {
                    double MUinLayer = (from Spots in Plan
                                        where Spots.LayerNumber == i
                                        select Spots).ToList()[0].LayerMU;
                    TotalMU += MUinLayer;
                }

                Trace.WriteLine("");
                Trace.WriteLine("Loaded Plan_NCC File Information");
                Trace.WriteLine($" - Total Layer: {TotalLayer}");
                Trace.WriteLine($" - Total Spot: {TotalSpot}");
                Trace.WriteLine($" - Total MU: {TotalMU}");
            }
        }

        private void Debug_WriteLine_PGDataInformation(List<PGStruct> PGData, bool flag)
        {
            if (flag == true)
            {
                Trace.WriteLine("");
                Trace.WriteLine("Loaded PG Data Information");
                Trace.WriteLine($" - Total Spot: {PGData.Count()}");
            }
        }

        private void DataClear()
        {
            PostProcessing_PlanFile_NCC = new List<PlanStruct_NCC>();
            LogDatasPostProcessing_NCC = new List<LogStruct_NCC>();
            PostProcessing_PG = new List<PGStruct>();
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
        #endregion

    }
}
