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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace HUREL_PG_GUI.ViewModels
{
    public class VM_LineScanning_PostProcessing : ViewModelBase
    {
        #region Data Struct

        private List<PGStruct> PostProcessing_PG = new List<PGStruct>();
        private List<PlanStruct_SMC> Plan_SMC = new List<PlanStruct_SMC>();
        private List<PlanStruct_SMC> Plan_SMC_SelectedField = new List<PlanStruct_SMC>();

        private string InitialPath = @"D:\OneDrive - 한양대학교\01. Research\01. 통합 제어 프로그램\99. FunctionTest\05. SMC";
               
        static string DatasPath = @"..\..\..\Datas\";
        static public string Directory_DebugFile = @"D:\OneDrive - 한양대학교\바탕 화면\temp";

        static public List<RangeInPMMAStruct> RangeInPMMA = new List<RangeInPMMAStruct>();

        List<PlanPGMergedDataStruct> MergedData = new List<PlanPGMergedDataStruct>();

        List<double[]> PG_71 = new List<double[]>();
        List<double[]> PG_144 = new List<double[]>();

        private List<SpotMapStruct> spotMap;
        private List<BeamRangeMapStruct> beamRangeMap;


        #endregion

        public VM_LineScanning_PostProcessing()
        {
            Data_SelectType = Data_SelectTypes.Data_Patient;
            Trace.WriteLine("LineScanning_PostProcessing Initialized");
        }

        private void StaticParameterSetting()
        {
            RangeInPMMA = GetRangeInPMMA(DatasPath + "RangeInPMMA.txt");
        }

        #region Binding
        public ObservableCollection<SpotMapStruct> VM_SpotMap { get; set; }
        public ObservableCollection<BeamRangeMapStruct> VM_BeamRangeMap { get; set; }



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


        private string loadedFileName_Range;
        public string LoadedFileName_Range
        {
            get
            {
                return loadedFileName_Range;
            }
            set
            {
                loadedFileName_Range = value;
                OnPropertyChanged(nameof(LoadedFileName_Range));
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


        #endregion

        #region View Status(Converter)

        private bool is_DICOM_RP_FileLoaded_PostProcessing;
        public bool Is_DICOM_RP_FileLoaded_PostProcessing
        {
            get
            {
                return is_DICOM_RP_FileLoaded_PostProcessing;
            }
            set
            {
                is_DICOM_RP_FileLoaded_PostProcessing = value;
                OnPropertyChanged(nameof(Is_DICOM_RP_FileLoaded_PostProcessing));
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


        // isRangeFileLoaded_PostProcessing


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

        /// <summary>
        /// RadioButton Option (Retrieve data for patient)
        /// </summary>
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

        /// <summary>
        /// RadioButton Option (Select data individually)
        /// </summary>
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


        /// <summary>
        /// Load DICOM RP file and get information of segment
        /// </summary>
        private RelayCommand _LoadPatientDatasCommand;
        public ICommand LoadPatientDatasCommand
        {
            get
            {
                return _LoadPatientDatasCommand ?? (_LoadPatientDatasCommand = new RelayCommand(LoadPatientDatas));
            }
        }
        private async void LoadPatientDatas()
        {
            Plan_SMC = new List<PlanStruct_SMC>();
            PlanFileClass_SMC Class = new PlanFileClass_SMC();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = InitialPath;
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

        }


        private RelayCommand _PlanFileLoadCommand;
        public ICommand PlanFileLoadCommand
        {
            get
            {
                return _PlanFileLoadCommand ?? (_PlanFileLoadCommand = new RelayCommand(PlanFileLoad));
            }
        }
        private async void PlanFileLoad()
        {
            Plan_SMC = new List<PlanStruct_SMC>();
            PlanFileClass_SMC Class = new PlanFileClass_SMC();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = InitialPath;
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

            LoadedFileName_DICOM_RP = dialog.SafeFileName;
            Is_DICOM_RP_FileLoaded_PostProcessing = true;
        }


        private AsyncCommand loadPGDataCommand;
        public ICommand LoadPGDataCommand
        {
            get
            {
                return loadPGDataCommand ?? (loadPGDataCommand = new AsyncCommand(LoadPGData));
            }
        }
        private async Task LoadPGData()
        {
            PostProcessing_PG = new List<PGStruct>();
            PGDataClass Class = new PGDataClass();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = InitialPath;
            dialog.Filter = "Bin|*.bin|Text|*.txt|All|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                MessageBox.Show("PG Data Load Canceled", $"PG Data Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await Task.Run(() => PostProcessing_PG = Class.LoadPGData(dialog.FileName));

            if (PostProcessing_PG.Count != 0)
            {
                Debug_WriteLine_PGDataInformation(PostProcessing_PG, true);

                LoadedFileName_PG = dialog.SafeFileName;
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
            LoadedFileName_DICOM = "이 기능은 구현중입니다!";
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

            //if (IsReadyPostProcessingAnalysis == true)
            //{
                IsPostProcessingAnalysising = true;

                SMCAnalysisClass Class_SMC = new SMCAnalysisClass();
                MergedData = await Task.Run(() => Class_SMC.GenerateMergedData_PostProcessing(Plan_SMC_SelectedField, PostProcessing_PG));

                int StartLayer = 0;
                int LastLayer = MergedData.Last().a3_LayerNumber;

                Task<List<SpotMapStruct>> SpotMapTask = Class_SMC.GenerateSpotMap(MergedData, 1); // 초기에는 1번째 Layer가 보여지도록
                Task<List<BeamRangeMapStruct>> BeamRangeMapTask = Class_SMC.GenerateBeamRangeMap(MergedData, 7, StartLayer, LastLayer, 1);

                spotMap = await SpotMapTask;
                beamRangeMap = await BeamRangeMapTask;

                VM_SpotMap = new ObservableCollection<SpotMapStruct>();
                spotMap.ForEach(x => VM_SpotMap.Add(x));

                VM_BeamRangeMap = new ObservableCollection<BeamRangeMapStruct>();
                beamRangeMap.ForEach(x => VM_BeamRangeMap.Add(x));

                OnPropertyChanged(nameof(VM_SpotMap));
                OnPropertyChanged(nameof(VM_BeamRangeMap));

                //BeamRangeMap_Selected_StartLayer = StartLayer + 1;
                //BeamRangeMap_Selected_EndLayer = LastLayer + 1;
                IsPostProcessingAnalysising = false;

                Trace.WriteLine("Code Finish");

                sw.Stop();
                Trace.WriteLine($"{sw.ElapsedMilliseconds} msec");
            //}
            //else
            //{
            //    MessageBox.Show($"IsReadyPostProcessingAnalysis = false");
            //}




            #region OLD
            //bool flag = Class3.CheckNumberOfLayers_PlanChopperSignal(Plan_SMC_SelectedField.Last().a3_LayerNumber, Data_Trigger_Layer.Count);
            //if (flag == false)
            //{
            //    Trace.WriteLine($"Total number of layer does not match");
            //    return;
            //}
            //else
            //{
            //    for (int LayerIndex = 1; LayerIndex <= Plan_SMC_SelectedField.Last().a3_LayerNumber; LayerIndex++)
            //    {
            //        //MergedData = Class3.MergePlanPG(MergedData, PostProcessing_PG, Data_Trigger_Layer[LayerIndex - 1].Trigger, Plan_SMC_SelectedField, LayerIndex);
            //    }
            //}
            #endregion
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
            //PostProcessing_PlanFile_NCC = null;
            //PostProcessing_PG = null;
            //LogDatasPostProcessing_NCC = null;

            //LoadedFileName_Plan = "";
            //LoadedFileName_Log = "";
            //LoadedFileName_PG = "";
            //LoadedFileName_DICOM = "";

            //IsPlanFileLoaded_PostProcessing = false;
            //IsLogFileLoaded_PostProcessing = false;
            //IsPGDataLoaded_PostProcessing = false;
            //IsDICOMFileLoaded_PostProcessing = false;

            //IsPostProcessingAnalysising = false;
            //IsReadyPostProcessingAnalysis = false;

            //CheckForAnalysisReady();

            //VM_BeamRangeMap = new ObservableCollection<BeamRangeMapStruct>();
            //VM_BeamRangeMap.Add(new BeamRangeMapStruct(-10000, -10000, 10, new SolidColorBrush(Color.FromScRgb(1, 1, 1, 1))));
            //OnPropertyChanged(nameof(VM_BeamRangeMap));

            //DataClear();
        }


        private RelayCommand _TestCommand;
        public ICommand TestCommand
        {
            get
            {
                return _TestCommand ?? (_TestCommand = new RelayCommand(Test));
            }
        }
        private async void Test()
        {
            // Plan File(DICOM RP) Load
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
            Is_DICOM_RP_FileLoaded_PostProcessing = true;

            // PG data Load
            PostProcessing_PG = new List<PGStruct>();
            PGDataClass Class_PGLoad = new PGDataClass();
            var Data_PG_Path_First = "D:\\OneDrive - 한양대학교\\01. Research\\01. 통합 제어 프로그램\\99. FunctionTest\\05. SMC\\01. PG Data\\";
            var Data_PG_Name = "data_shallowCub2Gy_Shift5-10.bin";
            var FullPath_PG = Data_PG_Path_First + Data_PG_Name;
            await Task.Run(() =>
            {
                PostProcessing_PG = Class_PGLoad.LoadPGData(FullPath_PG);
            });
            LoadedFileName_PG = FullPath_PG;
            IsPGDataLoaded_PostProcessing = true;

            //// CheckForAnalysisReady();
            await PostProcessingAnalysisStart();

        }

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
            #region Test Code(2022-03-03), Un-Activated

            //for (int j = 0; j < 10; j++)
            //{
            //    Trace.WriteLine($"{j}: Segment Map Speed Evaluation Start");

            //    for (int i = 1; i <= 26; i++)
            //    {
            //        //Trace.WriteLine($"{i}: Start");

            //        Stopwatch sw_Local = new Stopwatch();
            //        sw_Local.Start();

            //        SMCAnalysisClass Class_Local = new SMCAnalysisClass();

            //        Task<List<SpotMapStruct>> SpotMapTask_Local = Class_Local.GenerateSpotMap(MergedData, i);
            //        spotMap = await SpotMapTask_Local;

            //        VM_SpotMap = new ObservableCollection<SpotMapStruct>();
            //        spotMap.ForEach(x => VM_SpotMap.Add(x));
            //        OnPropertyChanged(nameof(VM_SpotMap));

            //        sw_Local.Stop();
            //        Trace.WriteLine($"{sw_Local.ElapsedMilliseconds}");

            //    }
            //    Trace.WriteLine($"");
            //}

            #endregion

            Stopwatch sw = new Stopwatch();
            sw.Start();

            SMCAnalysisClass Class = new SMCAnalysisClass();

            Task<List<SpotMapStruct>> SpotMapTask = Class.GenerateSpotMap(MergedData, spotMap_SelectedLayer);
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
            #region Test Code(2022-03-03), Un-Activated

            for (int j = 0; j < 10; j++)
            {
                Trace.WriteLine($"{j}: Beam Range Map Speed Evaluation Start");

                for (int i = 1; i <= 26; i++)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    SMCAnalysisClass Class_Local = new SMCAnalysisClass();
                    Task<List<BeamRangeMapStruct>> BeamRangeMapTask_Local = Class_Local.GenerateBeamRangeMap(MergedData, 7, 1, i, 0.1);
                    beamRangeMap = await BeamRangeMapTask_Local;

                    VM_BeamRangeMap = new ObservableCollection<BeamRangeMapStruct>();
                    beamRangeMap.ForEach(x => VM_BeamRangeMap.Add(x));
                    OnPropertyChanged(nameof(VM_BeamRangeMap));

                    sw.Stop();
                    Trace.WriteLine($"{sw.ElapsedMilliseconds}");
                }
                Trace.WriteLine($"");
            }

            #endregion

            ////Stopwatch sw = new Stopwatch();
            ////sw.Start();

            //SMCAnalysisClass Class = new SMCAnalysisClass();

            //// Option 1: Possible
            //Task<List<BeamRangeMapStruct>> BeamRangeMapTask = Class.GenerateBeamRangeMap(MergedData, 7, beamRangeMap_Selected_StartLayer, beamRangeMap_Selected_EndLayer, 1);
            //beamRangeMap = await BeamRangeMapTask;
            //// Option 2: ImPossible ???
            ////beamRangeMap = await Task.Run(() => Class.GenerateBeamRangeMap(MergedData, 7, StartLayer, LastLayer, 1));

            //VM_BeamRangeMap = new ObservableCollection<BeamRangeMapStruct>();
            //beamRangeMap.ForEach(x => VM_BeamRangeMap.Add(x));
            //OnPropertyChanged(nameof(VM_BeamRangeMap));

            ////sw.Stop();
            ////Trace.WriteLine($"{sw.ElapsedMilliseconds} ms");
        }

        #endregion

        #region Sub-Functions
        private void Debug_WriteLine_PGDataInformation(List<PGStruct> PGData, bool flag)
        {
            if (flag == true)
            {
                Trace.WriteLine("");
                Trace.WriteLine("Loaded PG Data Information");
                Trace.WriteLine($" - Total Segment: {PGData.Count()}");
            }
        }

        private void CheckForAnalysisReady()
        {
            if (is_DICOM_RP_FileLoaded_PostProcessing == true && isPGDataLoaded_PostProcessing == true && isDICOMFileLoaded_PostProcessing == true)
            {
                IsReadyPostProcessingAnalysis = true;
            }
        }

        private List<RangeInPMMAStruct> GetRangeInPMMA(string path)
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

        #endregion
    }
}