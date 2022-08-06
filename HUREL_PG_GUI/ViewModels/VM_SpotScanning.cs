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
using HUREL.PG.MultiSlit;
using System.Windows.Media;

namespace HUREL_PG_GUI.ViewModels
{
    public class VM_SpotScanning : ViewModelBase
    {


        // Visualized Objects
        public ObservableCollection<BeamRangeMapStruct> VM_BeamRangeMap { get; set; }
        public ObservableCollection<SpotMapDraing> VM_SpotMap { get; set; }
        public record SpotMapDraing(double X, double Y, double MU, SolidColorBrush Color);


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
                isMonitoring = true;
                await MultislitControl.MonitoringRunFtpAndFpgaLoop();
            }
            else
            {
                string status = "";
                await MultislitControl.StopMonitoringRunFtpAndFpgaLoop();
                //bool isFPGAstart = await Task.Run(() => FPGAControl.Command_MonitoringStart(out status)).ConfigureAwait(false);
                //await Task.Run(() => FPGAControl.start_stop_usb());
                bool isFPGAstart = await Task.Run(() => VM_MainWindow.FPGAControl.Command_MonitoringStart(out status, "")).ConfigureAwait(false);
                //bool isPGdisUpdate = await Task.Run(() => PGdistUpdate());

                VMStatus = "Idle";
                IsMonitoring = false;
            }
        }

        Mutex loopMutex = new Mutex(false, "loop mutex");
        private async Task DrawSpotMap()
        {
            while(isMonitoring)
            {
                VM_SpotMap 
            }
            
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
            MultislitControl.CurrentSession.LoadPlanFile(dialog.FileName);
            if (MultislitControl.CurrentSession.IsPlanLoad)
            {
                PatientID = "00000";
                PatientName = "Test";
                VMStatus = "Plan File loaded";
                planFileName = Path.GetFileNameWithoutExtension(dialog.FileName);
                Plan_TotalLayer = MultislitControl.CurrentSession.Layers[MultislitControl.CurrentSession.Layers.Count].LayerNumber;
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
        
        #endregion




    }
}
