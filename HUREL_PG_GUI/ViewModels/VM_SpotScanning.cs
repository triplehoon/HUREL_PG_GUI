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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using HUREL.PG.Ncc;
using System.Collections.Concurrent;
using System.Windows.Media;
using PG.Fpga;
using System.Net.NetworkInformation;

namespace HUREL_PG_GUI.ViewModels
{

    public class VM_SpotScanning : ViewModelBase
    {


        // Visualized Objects
        public ObservableCollection<BeamRangeMapStruct> VM_BeamRangeMap { get; set; }
        public ObservableCollection<SpotMapDrawing> VM_SpotMap { get; set; }
        public record SpotMapDrawing(double X, double Y, double MU, SolidColorBrush Color);


        //static public CRUXELLMSPGC FPGAControl;

        static public DateTime FirstTuningBeamTime = new DateTime(0); // 0528

        public VM_SpotScanning()
        {
            ConfigurationSetting_NCC();
        }
        private void ConfigurationSetting_NCC()
        {
            // Path
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


        private bool fpgaStatus = CruxellWrapper.GetFpgaStatus();
        public bool FpgaStatus
        {
            get
            {
                return fpgaStatus;
            }
            set
            {
                fpgaStatus = value;
                OnPropertyChanged(nameof(FpgaStatus));
            }
        }
        private string fpgaStatusStr = "";
        public string FpgaStatusStr
        {
            get
            {
                return fpgaStatusStr;
            }
            set
            {
                fpgaStatusStr = value; 
                OnPropertyChanged(nameof(FpgaStatusStr));
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
        // SH edit make Stop command

        private AsyncCommand _MonitoringStopCommand;
        public ICommand MonitoringStopCommand
        {
            get
            {
                return _MonitoringStopCommand ?? (_MonitoringStopCommand = new AsyncCommand(MonitoringStop));
            }
        }

        private async Task MonitoringStop()
        {
            if (IsMonitoring)
            {
                IsMonitoring = false;

                CruxellWrapper.StopFpgaDaq();
            }
            else
            {
                IsMonitoring = false;
            }
        }

        private async Task StartFpgaDaqAsync()
        {
            await Task.Run(() => CruxellWrapper.StartFpgaDaq());
        }

        public ICommand StartFpgaDaqCommand => new AsyncCommand(StartFpgaDaqAsync);


        // Sh edit end make Stop command
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

            // hold the thread by 20 seconds
            if (!IsMonitoring)
            {
                IsMonitoring = true;

                CruxellWrapper.StartFpgaDaq();

                Task drawing = DrawSpotMap();

                await drawing;
            }
            else
            {
                IsMonitoring = false;

            }
        }

        Mutex loopMutex = new Mutex(false, "loop mutex");
        private async Task DrawSpotMap()
        {
            while (isMonitoring)
            {
                await Task.Delay(100);
                string fpgaStatus = "";
                fpgaStatus += "Data count: " + CruxellWrapper.GetDataCount().ToString("N0") + "\n";
                fpgaStatus += "Sample data: " + "\n";
                if (CruxellWrapper.GetDataCount() > 0)
                {
                    DaqData daqData = CruxellWrapper.GetDaqData()[CruxellWrapper.GetDataCount() - 1];
                    fpgaStatus += "secTime: " + daqData.secTime.ToString("N0") + "\n";
                    //fpgaStatus +="chNumber: " + daqData.chNumber.ToString("N0")+"\n"
                    fpgaStatus += "chNumber: " + daqData.chNumber.ToString("D3") + "\n";

                    fpgaStatus += "preData: " + daqData.preData.ToString("N0") + "\n";
                    fpgaStatus += "vPulseData: " + daqData.vPulseData.ToString("D4") + "\n";
                    //fpgaStatus +="vPulseData: " + ((int)Math.Round(daqData.vPulseData)).ToString("D4")+"\n"

                    fpgaStatus += "tPulseTime: " + daqData.tPulseTime.ToString("N0") + "\n";
                    fpgaStatus += "-------Save values-------" + "\n";
                    fpgaStatus += "channel: " + daqData.channel.ToString("D3") + "\n";
                    fpgaStatus += "timestamp [ns]: " + daqData.timestamp.ToString("N0") + "\n";

                    fpgaStatus += "value [mV]: " + ((int)Math.Round(daqData.value)).ToString("D4") + "\n";
                }

                FpgaStatusStr = fpgaStatus + "\n";

                //List<NccLayer> layers = new List<NccLayer>();
                //List<SpotMap> spotMaps = new List<SpotMap>();
                //await Task.Run(() => { spotMaps = NccLayer.GetSpotMap(layers); });
                //for (int i = 0; i < spotMaps.Count; i++)
                //{
                //    VM_SpotMap.Add(new SpotMapDrawing(spotMaps[i].X, spotMaps[i].Y, spotMaps[i].MU, SetColor(spotMaps[i].RangeDifference, -10, 10)));
                //}
                //OnPropertyChanged(nameof(VM_SpotMap));

            }

            // when monitoring is stopped
            FpgaStatusStr += "FPGA Monitoring is stopped\n";

        }
        private SolidColorBrush SetColor(double Diff, float min, float max) // 파일로 받아서 작성하도록 수정 필요
        {
            float alpha = 1f;
            float R, G, B;

            float interval = max - min;

            if (Diff == -10000)
            {
                R = 1f;
                G = 1f;
                B = 1;
            }
            else
            {
                if (Diff <= min)
                {
                    R = 0f;
                    G = 0f;
                    B = 0.0514f;
                }
                else if (Diff < min + (1 * interval / 20))
                {
                    R = 0f;
                    G = 0.051f;
                    B = 0.667f;
                } //
                else if (Diff < min + (2 * interval / 20))
                {
                    R = 0.004f;
                    G = 0.098f;
                    B = 0.804f;
                } //
                else if (Diff < min + (3 * interval / 20))
                {
                    R = 0.004f;
                    G = 0.145f;
                    B = 0.941f;
                } //
                else if (Diff < min + (4 * interval / 20))
                {
                    R = 0.063f;
                    G = 0.259f;
                    B = 0.984f;
                } //
                else if (Diff < min + (5 * interval / 20))
                {
                    R = 0.141f;
                    G = 0.396f;
                    B = 0.965f;
                } //
                else if (Diff < min + (6 * interval / 20))
                {
                    R = 0.216f;
                    G = 0.522f;
                    B = 0.949f;
                } //
                else if (Diff < min + (7 * interval / 20))
                {
                    R = 0.400f;
                    G = 0.627f;
                    B = 0.945f;
                } //
                else if (Diff < min + (8 * interval / 20))
                {
                    R = 0.584f;
                    G = 0.733f;
                    B = 0.945f;
                } //
                else if (Diff < min + (9 * interval / 20))
                {
                    R = 0.757f;
                    G = 0.835f;
                    B = 0.941f;
                } //
                else if (Diff < min + (10 * interval / 20))
                {
                    R = 0.941f;
                    G = 0.941f;
                    B = 0.941f;
                }  //
                else if (Diff < min + (11 * interval / 20))
                {
                    R = 0.933f;
                    G = 0.855f;
                    B = 0.757f;
                }  //
                else if (Diff < min + (12 * interval / 20))
                {
                    R = 0.925f;
                    G = 0.765f;
                    B = 0.576f;
                }  //
                else if (Diff < min + (13 * interval / 20))
                {
                    R = 0.918f;
                    G = 0.682f;
                    B = 0.408f;
                }  //
                else if (Diff < min + (14 * interval / 20))
                {
                    R = 0.910f;
                    G = 0.596f;
                    B = 0.224f;
                }  //
                else if (Diff < min + (15 * interval / 20))
                {
                    R = 0.890f;
                    G = 0.482f;
                    B = 0.141f;
                }  //
                else if (Diff < min + (16 * interval / 20))
                {
                    R = 0.871f;
                    G = 0.369f;
                    B = 0.059f;
                }  //
                else if (Diff < min + (17 * interval / 20))
                {
                    R = 0.820f;
                    G = 0.267f;
                    B = 0.008f;
                }  //
                else if (Diff < min + (18 * interval / 20))
                {
                    R = 0.718f;
                    G = 0.180f;
                    B = 0.004f;
                }  //
                else if (Diff < min + (19 * interval / 20))
                {
                    R = 0.608f;
                    G = 0.090f;
                    B = 0.004f;
                }  //
                else
                {
                    R = 0.510f;
                    G = 0.008f;
                    B = 0f;
                }
            }

            var color = new SolidColorBrush(Color.FromScRgb(alpha, R, G, B));
            //var color = Color.FromScRgb(alpha, R, G, B);
            return color;
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
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Pld|*.pld|Text|*.txt|All|*.*";
            dialog.ShowDialog();
            VMStatus = "Loading Plan File";

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

        #endregion




    }
}
