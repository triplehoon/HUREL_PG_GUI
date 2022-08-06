using GalaSoft.MvvmLight.Command;
using HUREL_PG_GUI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace HUREL_PG_GUI.ViewModels
{
    public class VM_MainWindow : ViewModelBase
    {
        static public HUREL_PG_GUI.Views.Window_SpotScanningView_PostProcessing NCC_Post_View;
        static public HUREL_PG_GUI.Views.Window_LineScanningView_PostProcessing SMC_Post_View;

        static public HUREL_PG_GUI.Views.Window_PositioningSystem PositioningSystem_View;

        //static public CRUXELLMSPGC FPGAControl;

        public VM_MainWindow()
        {
        }

        #region Binding

        private bool isViewSelected = false;
        public bool IsViewSelected
        {
            get
            {
                return isViewSelected;
            }
            set
            {
                isViewSelected = value;
                OnPropertyChanged(nameof(IsViewSelected));
            }
        }


        public static string selectedView;
        //private string selectedView;
        public string SelectedView
        {
            get
            {
                return selectedView;
            }
            set
            {
                selectedView = value;
                OnPropertyChanged(nameof(SelectedView));
            }
        }

        #endregion

        #region View Change Command

        /// <summary>
        /// PBS select View (Home View)
        /// </summary>
        private RelayCommand _PBSselectViewCommand;
        public ICommand PBSselectViewCommand
        {
            get
            {
                return _PBSselectViewCommand ?? (_PBSselectViewCommand = new RelayCommand(PBSselectView));
            }
        }
        private void PBSselectView()
        {
            IsViewSelected = false;
            SelectedView = "PBSselectView";
        }


        /// <summary>
        /// Spot Scanning View (Real-time Monitoring)
        /// </summary>
        private RelayCommand _SpotScanningViewCommand;
        public ICommand SpotScanningViewCommand
        {
            get
            {
                return _SpotScanningViewCommand ?? (_SpotScanningViewCommand = new RelayCommand(SpotScanningView));
            }
        }
        private void SpotScanningView()
        {
            IsViewSelected = true;
            SelectedView = "SpotScanningView";
        }


        /// <summary>
        /// Line Scanning View (Real-time Monitoring)
        /// </summary>
        private RelayCommand _LineScanningViewCommand;
        public ICommand LineScanningViewCommand
        {
            get
            {
                return _LineScanningViewCommand ?? (_LineScanningViewCommand = new RelayCommand(LineScanningView));
            }
        }
        private void LineScanningView()
        {
            IsViewSelected = true;
            SelectedView = "LineScanningView";
        }


        /// <summary>
        /// Post-Processing View
        /// </summary>
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
            if (selectedView == "SpotScanningView")
            {
                if (NCC_Post_View == null)
                {
                    NCC_Post_View = new HUREL_PG_GUI.Views.Window_SpotScanningView_PostProcessing();
                    NCC_Post_View.Show();
                }
                else if (NCC_Post_View != null)
                {
                    NCC_Post_View.Visibility = Visibility.Visible;
                }
            }
            else if (selectedView == "LineScanningView")
            {

            }
            else
            {
                MessageBox.Show("Error Code 001");
            }

            #region Post-processing이 page 기반일 때(OLD)
            //if (selectedView == "SpotScanningView")
            //{
            //    SelectedView = "SpotScanningView_PostProcessing";
            //}
            //else if (selectedView == "LineScanningView")
            //{
            //    SelectedView = "LineScanningView_PostProcessing";
            //}
            //else
            //{
            //    MessageBox.Show("Check VM_MainWindow.cs -> private void PostProcessingView()");
            //}
            #endregion
        }


        /// <summary>
        /// Positioning System Control View
        /// </summary>
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

            #region Page 기반일때 (OLD)
            //IsViewSelected = true;
            //SelectedView = "PositioningSystemView";
            #endregion
        }


        /// <summary>
        /// Energy Calibration View
        /// </summary>
        private RelayCommand _EnergyCalibrationViewCommand;
        public ICommand EnergyCalibrationViewCommand
        {
            get
            {
                return _EnergyCalibrationViewCommand ?? (_EnergyCalibrationViewCommand = new RelayCommand(EnergyCalibrationView));
            }
        }
        private void EnergyCalibrationView()
        {
            IsViewSelected = true;
            SelectedView = "EnergyCalibrationView";
        }


        /// <summary>
        /// Laser Calibration View
        /// </summary>
        private RelayCommand _LaserCalibrationViewCommand;
        public ICommand LaserCalibrationViewCommand
        {
            get
            {
                return _LaserCalibrationViewCommand ?? (_LaserCalibrationViewCommand = new RelayCommand(LaserCalibrationView));
            }
        }
        private void LaserCalibrationView()
        {
            IsViewSelected = true;
            SelectedView = "LaserCalibrationView";
        }


        /// <summary>
        /// DAQ Setting View
        /// </summary>
        private RelayCommand _DAQsettingViewCommand;
        public ICommand DAQsettingViewCommand
        {
            get
            {
                return _DAQsettingViewCommand ?? (_DAQsettingViewCommand = new RelayCommand(DAQsettingView));
            }
        }
        private void DAQsettingView()
        {
            IsViewSelected = true;
            SelectedView = "DAQsettingView";
        }

        #endregion       
    }
}
