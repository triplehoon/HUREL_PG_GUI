using FASTECH;
using GalaSoft.MvvmLight.CommandWpf;
using HUREL_PG_GUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace HUREL_PG_GUI.ViewModels
{
    public class VM_PositioningSystem : ViewModelBase
    {
        #region NEW
        //private byte m_nPortNo;
        //private byte NumberOfMotors = 0;
        //private uint dwBaud = 115200;

        //private string INI_PATH = Application.StartupPath + @"\\setting_positioning.ini";




        //#region Binding

        //private ObservableCollection<string> portNumberList = new ObservableCollection<string>();
        //public ObservableCollection<string> PortNumberList
        //{
        //    get
        //    {
        //        return portNumberList;
        //    }
        //    set
        //    {
        //        portNumberList = value;
        //        OnPropertyChanged(nameof(PortNumberList));
        //    }
        //}


        //private string portNumberSelected;
        //public string PortNumberSelected
        //{
        //    get
        //    {
        //        return portNumberSelected;
        //    }
        //    set
        //    {
        //        portNumberSelected = value;
        //        OnPropertyChanged(nameof(PortNumberSelected));
        //    }
        //}


        //private string tBox_speed;
        //public string TBox_speed
        //{
        //    get
        //    {
        //        return tBox_speed;
        //    }
        //    set
        //    {
        //        tBox_speed = value;
        //        OnPropertyChanged(nameof(TBox_speed));
        //    }
        //}


        //#region [Binding] PPS Coordinate Setting / Trolley Coordinate Setting

        //// X - Y - Z - Tilt(Roll) - Pivot(Pitch) - Swivel(Yaw)
        ////                                                                       < private >                        < public >        
        //// (   PPS   ) --- (  CurrentCoordinate   ) --- ( @ )    =====     pps_CurrentCoordinate_Swivel        --- PPS_CurrentCoordinate_Swivel
        //// (   PPS   ) --- (   InputCoordinate    ) --- ( @ )    =====     pps_InputCoordinate_Swivel          --- PPS_InputCoordinate_Swivel
        //// (   PPS   ) --- (  CalculatedVariance  ) --- ( @ )    =====     pps_CalculatedVariance_Swivel       --- PPS_CalculatedVariance_Swivel
        //// ( Trolley ) --- (  CurrentCoordinate   ) --- ( @ )    =====     trolley_CurrentCoordinate_Swivel    --- Trolley_CurrentCoordinate_Swivel
        //// ( Trolley ) --- (    InputVariance     ) --- ( @ )    =====     trolley_InputVariance_Swivel        --- Trolley_InputVariance_Swivel
        //// ( Trolley ) --- ( CalculatedCoordinate ) --- ( @ )    =====     trolley_CalculatedCoordinate_Swivel --- Trolley_CalculatedCoordinate_Swivel

        //#region X

        //private string pps_CurrentCoordinate_X;
        //public string PPS_CurrentCoordinate_X
        //{
        //    get
        //    {
        //        return pps_CurrentCoordinate_X;
        //    }
        //    set
        //    {
        //        pps_CurrentCoordinate_X = value;
        //        OnPropertyChanged(nameof(PPS_CurrentCoordinate_X));
        //    }
        //}


        //private string pps_InputCoordinate_X;
        //public string PPS_InputCoordinate_X
        //{
        //    get
        //    {
        //        return pps_InputCoordinate_X;
        //    }
        //    set
        //    {
        //        pps_InputCoordinate_X = value;
        //        OnPropertyChanged(nameof(PPS_InputCoordinate_X));
        //    }
        //}


        //private string pps_CalculatedVariance_X;
        //public string PPS_CalculatedVariance_X
        //{
        //    get
        //    {
        //        return pps_CalculatedVariance_X;
        //    }
        //    set
        //    {
        //        pps_CalculatedVariance_X = value;
        //        OnPropertyChanged(nameof(PPS_CalculatedVariance_X));
        //    }
        //}


        //private string trolley_CurrentCoordinate_X;
        //public string Trolley_CurrentCoordinate_X
        //{
        //    get
        //    {
        //        return trolley_CurrentCoordinate_X;
        //    }
        //    set
        //    {
        //        trolley_CurrentCoordinate_X = value;
        //        OnPropertyChanged(nameof(Trolley_CurrentCoordinate_X));
        //    }
        //}


        //private string trolley_InputVariance_X;
        //public string Trolley_InputVariance_X
        //{
        //    get
        //    {
        //        return trolley_InputVariance_X;
        //    }
        //    set
        //    {
        //        trolley_InputVariance_X = value;
        //        OnPropertyChanged(nameof(Trolley_InputVariance_X));
        //    }
        //}


        //private string trolley_CalculatedCoordinate_X;
        //public string Trolley_CalculatedCoordinate_X
        //{
        //    get
        //    {
        //        return trolley_CalculatedCoordinate_X;
        //    }
        //    set
        //    {
        //        trolley_CalculatedCoordinate_X = value;
        //        OnPropertyChanged(nameof(Trolley_CalculatedCoordinate_X));
        //    }
        //}

        //#endregion

        //#region Y

        //private string pps_CurrentCoordinate_Y;
        //public string PPS_CurrentCoordinate_Y
        //{
        //    get
        //    {
        //        return pps_CurrentCoordinate_Y;
        //    }
        //    set
        //    {
        //        pps_CurrentCoordinate_Y = value;
        //        OnPropertyChanged(nameof(PPS_CurrentCoordinate_Y));
        //    }
        //}


        //private string pps_InputCoordinate_Y;
        //public string PPS_InputCoordinate_Y
        //{
        //    get
        //    {
        //        return pps_InputCoordinate_Y;
        //    }
        //    set
        //    {
        //        pps_InputCoordinate_Y = value;
        //        OnPropertyChanged(nameof(PPS_InputCoordinate_Y));
        //    }
        //}


        //private string pps_CalculatedVariance_Y;
        //public string PPS_CalculatedVariance_Y
        //{
        //    get
        //    {
        //        return pps_CalculatedVariance_Y;
        //    }
        //    set
        //    {
        //        pps_CalculatedVariance_Y = value;
        //        OnPropertyChanged(nameof(PPS_CalculatedVariance_Y));
        //    }
        //}


        //private string trolley_CurrentCoordinate_Y;
        //public string Trolley_CurrentCoordinate_Y
        //{
        //    get
        //    {
        //        return trolley_CurrentCoordinate_Y;
        //    }
        //    set
        //    {
        //        trolley_CurrentCoordinate_Y = value;
        //        OnPropertyChanged(nameof(Trolley_CurrentCoordinate_Y));
        //    }
        //}


        //private string trolley_InputVariance_Y;
        //public string Trolley_InputVariance_Y
        //{
        //    get
        //    {
        //        return trolley_InputVariance_Y;
        //    }
        //    set
        //    {
        //        trolley_InputVariance_Y = value;
        //        OnPropertyChanged(nameof(Trolley_InputVariance_Y));
        //    }
        //}


        //private string trolley_CalculatedCoordinate_Y;
        //public string Trolley_CalculatedCoordinate_Y
        //{
        //    get
        //    {
        //        return trolley_CalculatedCoordinate_Y;
        //    }
        //    set
        //    {
        //        trolley_CalculatedCoordinate_Y = value;
        //        OnPropertyChanged(nameof(Trolley_CalculatedCoordinate_Y));
        //    }
        //}

        //#endregion

        //#region Z

        //private string pps_CurrentCoordinate_Z;
        //public string PPS_CurrentCoordinate_Z
        //{
        //    get
        //    {
        //        return pps_CurrentCoordinate_Z;
        //    }
        //    set
        //    {
        //        pps_CurrentCoordinate_Z = value;
        //        OnPropertyChanged(nameof(PPS_CurrentCoordinate_Z));
        //    }
        //}


        //private string pps_InputCoordinate_Z;
        //public string PPS_InputCoordinate_Z
        //{
        //    get
        //    {
        //        return pps_InputCoordinate_Z;
        //    }
        //    set
        //    {
        //        pps_InputCoordinate_Z = value;
        //        OnPropertyChanged(nameof(PPS_InputCoordinate_Z));
        //    }
        //}


        //private string pps_CalculatedVariance_Z;
        //public string PPS_CalculatedVariance_Z
        //{
        //    get
        //    {
        //        return pps_CalculatedVariance_Z;
        //    }
        //    set
        //    {
        //        pps_CalculatedVariance_Z = value;
        //        OnPropertyChanged(nameof(PPS_CalculatedVariance_Z));
        //    }
        //}


        //private string trolley_CurrentCoordinate_Z;
        //public string Trolley_CurrentCoordinate_Z
        //{
        //    get
        //    {
        //        return trolley_CurrentCoordinate_Z;
        //    }
        //    set
        //    {
        //        trolley_CurrentCoordinate_Z = value;
        //        OnPropertyChanged(nameof(Trolley_CurrentCoordinate_Z));
        //    }
        //}


        //private string trolley_InputVariance_Z;
        //public string Trolley_InputVariance_Z
        //{
        //    get
        //    {
        //        return trolley_InputVariance_Z;
        //    }
        //    set
        //    {
        //        trolley_InputVariance_Z = value;
        //        OnPropertyChanged(nameof(Trolley_InputVariance_Z));
        //    }
        //}


        //private string trolley_CalculatedCoordinate_Z;
        //public string Trolley_CalculatedCoordinate_Z
        //{
        //    get
        //    {
        //        return trolley_CalculatedCoordinate_Z;
        //    }
        //    set
        //    {
        //        trolley_CalculatedCoordinate_Z = value;
        //        OnPropertyChanged(nameof(Trolley_CalculatedCoordinate_Z));
        //    }
        //}

        //#endregion

        //#region Tilt(Roll)

        //private string pps_CurrentCoordinate_Tilt;
        //public string PPS_CurrentCoordinate_Tilt
        //{
        //    get
        //    {
        //        return pps_CurrentCoordinate_Tilt;
        //    }
        //    set
        //    {
        //        pps_CurrentCoordinate_Tilt = value;
        //        OnPropertyChanged(nameof(PPS_CurrentCoordinate_Tilt));
        //    }
        //}


        //private string pps_InputCoordinate_Tilt;
        //public string PPS_InputCoordinate_Tilt
        //{
        //    get
        //    {
        //        return pps_InputCoordinate_Tilt;
        //    }
        //    set
        //    {
        //        pps_InputCoordinate_Tilt = value;
        //        OnPropertyChanged(nameof(PPS_InputCoordinate_Tilt));
        //    }
        //}


        //private string pps_CalculatedVariance_Tilt;
        //public string PPS_CalculatedVariance_Tilt
        //{
        //    get
        //    {
        //        return pps_CalculatedVariance_Tilt;
        //    }
        //    set
        //    {
        //        pps_CalculatedVariance_Tilt = value;
        //        OnPropertyChanged(nameof(PPS_CalculatedVariance_Tilt));
        //    }
        //}


        //private string trolley_CurrentCoordinate_Tilt;
        //public string Trolley_CurrentCoordinate_Tilt
        //{
        //    get
        //    {
        //        return trolley_CurrentCoordinate_Tilt;
        //    }
        //    set
        //    {
        //        trolley_CurrentCoordinate_Tilt = value;
        //        OnPropertyChanged(nameof(Trolley_CurrentCoordinate_Tilt));
        //    }
        //}


        //private string trolley_InputVariance_Tilt;
        //public string Trolley_InputVariance_Tilt
        //{
        //    get
        //    {
        //        return trolley_InputVariance_Tilt;
        //    }
        //    set
        //    {
        //        trolley_InputVariance_Tilt = value;
        //        OnPropertyChanged(nameof(Trolley_InputVariance_Tilt));
        //    }
        //}


        //private string trolley_CalculatedCoordinate_Tilt;
        //public string Trolley_CalculatedCoordinate_Tilt
        //{
        //    get
        //    {
        //        return trolley_CalculatedCoordinate_Tilt;
        //    }
        //    set
        //    {
        //        trolley_CalculatedCoordinate_Tilt = value;
        //        OnPropertyChanged(nameof(Trolley_CalculatedCoordinate_Tilt));
        //    }
        //}

        //#endregion

        //#region Pivot(Pitch)

        //private string pps_CurrentCoordinate_Pivot;
        //public string PPS_CurrentCoordinate_Pivot
        //{
        //    get
        //    {
        //        return pps_CurrentCoordinate_Pivot;
        //    }
        //    set
        //    {
        //        pps_CurrentCoordinate_Pivot = value;
        //        OnPropertyChanged(nameof(PPS_CurrentCoordinate_Pivot));
        //    }
        //}


        //private string pps_InputCoordinate_Pivot;
        //public string PPS_InputCoordinate_Pivot
        //{
        //    get
        //    {
        //        return pps_InputCoordinate_Pivot;
        //    }
        //    set
        //    {
        //        pps_InputCoordinate_Pivot = value;
        //        OnPropertyChanged(nameof(PPS_InputCoordinate_Pivot));
        //    }
        //}


        //private string pps_CalculatedVariance_Pivot;
        //public string PPS_CalculatedVariance_Pivot
        //{
        //    get
        //    {
        //        return pps_CalculatedVariance_Pivot;
        //    }
        //    set
        //    {
        //        pps_CalculatedVariance_Pivot = value;
        //        OnPropertyChanged(nameof(PPS_CalculatedVariance_Pivot));
        //    }
        //}


        //private string trolley_CurrentCoordinate_Pivot;
        //public string Trolley_CurrentCoordinate_Pivot
        //{
        //    get
        //    {
        //        return trolley_CurrentCoordinate_Pivot;
        //    }
        //    set
        //    {
        //        trolley_CurrentCoordinate_Pivot = value;
        //        OnPropertyChanged(nameof(Trolley_CurrentCoordinate_Pivot));
        //    }
        //}


        //private string trolley_InputVariance_Pivot;
        //public string Trolley_InputVariance_Pivot
        //{
        //    get
        //    {
        //        return trolley_InputVariance_Pivot;
        //    }
        //    set
        //    {
        //        trolley_InputVariance_Pivot = value;
        //        OnPropertyChanged(nameof(Trolley_InputVariance_Pivot));
        //    }
        //}


        //private string trolley_CalculatedCoordinate_Pivot;
        //public string Trolley_CalculatedCoordinate_Pivot
        //{
        //    get
        //    {
        //        return trolley_CalculatedCoordinate_Pivot;
        //    }
        //    set
        //    {
        //        trolley_CalculatedCoordinate_Pivot = value;
        //        OnPropertyChanged(nameof(Trolley_CalculatedCoordinate_Pivot));
        //    }
        //}

        //#endregion

        //#region Swivel(Yaw)

        //private string pps_CurrentCoordinate_Swivel;
        //public string PPS_CurrentCoordinate_Swivel
        //{
        //    get
        //    {
        //        return pps_CurrentCoordinate_Swivel;
        //    }
        //    set
        //    {
        //        pps_CurrentCoordinate_Swivel = value;
        //        OnPropertyChanged(nameof(PPS_CurrentCoordinate_Swivel));
        //    }
        //}


        //private string pps_InputCoordinate_Swivel;
        //public string PPS_InputCoordinate_Swivel
        //{
        //    get
        //    {
        //        return pps_InputCoordinate_Swivel;
        //    }
        //    set
        //    {
        //        pps_InputCoordinate_Swivel = value;
        //        OnPropertyChanged(nameof(PPS_InputCoordinate_Swivel));
        //    }
        //}


        //private string pps_CalculatedVariance_Swivel;
        //public string PPS_CalculatedVariance_Swivel
        //{
        //    get
        //    {
        //        return pps_CalculatedVariance_Swivel;
        //    }
        //    set
        //    {
        //        pps_CalculatedVariance_Swivel = value;
        //        OnPropertyChanged(nameof(PPS_CalculatedVariance_Swivel));
        //    }
        //}


        //private string trolley_CurrentCoordinate_Swivel;
        //public string Trolley_CurrentCoordinate_Swivel
        //{
        //    get
        //    {
        //        return trolley_CurrentCoordinate_Swivel;
        //    }
        //    set
        //    {
        //        trolley_CurrentCoordinate_Swivel = value;
        //        OnPropertyChanged(nameof(Trolley_CurrentCoordinate_Swivel));
        //    }
        //}


        //private string trolley_InputVariance_Swivel;
        //public string Trolley_InputVariance_Swivel
        //{
        //    get
        //    {
        //        return trolley_InputVariance_Swivel;
        //    }
        //    set
        //    {
        //        trolley_InputVariance_Swivel = value;
        //        OnPropertyChanged(nameof(Trolley_InputVariance_Swivel));
        //    }
        //}


        //private string trolley_CalculatedCoordinate_Swivel;
        //public string Trolley_CalculatedCoordinate_Swivel
        //{
        //    get
        //    {
        //        return trolley_CalculatedCoordinate_Swivel;
        //    }
        //    set
        //    {
        //        trolley_CalculatedCoordinate_Swivel = value;
        //        OnPropertyChanged(nameof(Trolley_CalculatedCoordinate_Swivel));
        //    }
        //}

        //#endregion

        //#endregion










        //#endregion

        //#region Binding (status)

        //private bool isMotorConnected;
        //public bool IsMotorConnected
        //{
        //    get
        //    {
        //        return isMotorConnected;
        //    }
        //    set
        //    {
        //        isMotorConnected = value;
        //        OnPropertyChanged(nameof(IsMotorConnected));
        //    }
        //}


        //private bool isServoModeON;
        //public bool IsServoModeON
        //{
        //    get
        //    {
        //        return isServoModeON;
        //    }
        //    set
        //    {
        //        isServoModeON = value;
        //        OnPropertyChanged(nameof(IsServoModeON));
        //    }
        //}

        //#endregion

        //#region Command

        //private RelayCommand _ServoMotorConnectCommand;
        //public ICommand ServoMotorConnectCommand
        //{
        //    get
        //    {
        //        return _ServoMotorConnectCommand ?? (_ServoMotorConnectCommand = new RelayCommand(ServoMotorConnect));
        //    }
        //}
        //private void ServoMotorConnect()
        //{
        //    if (portNumberList.Count <= 0)
        //    {
        //        MessageBox.Show("No Proper Port");
        //        return;
        //    }

        //    if (isMotorConnected == false)
        //    {
        //        if (string.IsNullOrEmpty(portNumberSelected) == false)
        //        {
        //            m_nPortNo = byte.Parse(portNumberSelected.Substring(3));

        //            if (EziMOTIONPlusRLib.FAS_Connect(m_nPortNo, dwBaud) == 0)
        //            {
        //                // Failed to connect
        //                MessageBox.Show("Failed to connect");
        //            }
        //            else
        //            {
        //                // connected.
        //                NumberOfMotors = 0;

        //                for (byte i = 0; i < EziMOTIONPlusRLib.MAX_SLAVE_NUMS; i++)
        //                {
        //                    if (EziMOTIONPlusRLib.FAS_IsSlaveExist(m_nPortNo, i) != 0)
        //                    {
        //                        NumberOfMotors += 1;
        //                    }
        //                }

        //                if (NumberOfMotors == 0)
        //                {
        //                    Debug.WriteLine($"[Servo Motor] Connected, but motor is not found: Check the Power Supply");
        //                    return;
        //                }

        //                if (NumberOfMotors != 0)
        //                {
        //                    IsMotorConnected = true;
        //                    Debug.WriteLine($"[Servo Motor] {NumberOfMotors}EA Connected");
        //                }
        //            }

        //            if (ReadOrGenerateINI() == false)
        //            {
        //                Trace.WriteLine($"Error: INI file generation is failed");
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("Please Select COM port");
        //        }
        //    }
        //    else
        //    {
        //        EziMOTIONPlusRLib.FAS_Close(m_nPortNo);
        //        IsMotorConnected = false;
        //        WriteINI(); // window가 닫힐 때 추가되도록 구현하는 방법 찾아보기
        //        PanelDataClear();
        //        Debug.WriteLine("[Servo Motor] Disconnected");
        //    }
        //}


        //private RelayCommand _MotorDriveCommand;
        //public ICommand MotorDriveCommand
        //{
        //    get
        //    {
        //        return _MotorDriveCommand ?? (_MotorDriveCommand = new RelayCommand(MotorDrive));
        //    }
        //}
        //private void MotorDrive()
        //{
        //    byte[] SlavesNo = new byte[NumberOfMotors];
        //    for (byte MotorIndex = 0; MotorIndex < NumberOfMotors; MotorIndex++)
        //    {
        //        SlavesNo[MotorIndex] = MotorIndex;
        //    }

        //    //int[] IncPos = ConvertDistDegreeToPulse(tBox_RelPreset_Trolley_X, tBox_RelPreset_Trolley_Y, tBox_RelPreset_Trolley_Tilt, tBox_RelPreset_Trolley_Pivot, tBox_RelPreset_Trolley_Swivel);
        //    int[] IncPos = { Convert.ToInt32(trolley_InputVariance_X), Convert.ToInt32(trolley_InputVariance_Y), Convert.ToInt32(trolley_InputVariance_Tilt), Convert.ToInt32(trolley_InputVariance_Pivot), Convert.ToInt32(trolley_InputVariance_Swivel) };

        //    //uint DriveSpeed = SetMotorSpeed(IncPos);
        //    uint DriveSpeed = Convert.ToUInt32(tBox_speed);
        //    ushort wAccTime = 100; // 가감속 시간 100 ms                 

        //    int nRtn;
        //    nRtn = EziMOTIONPlusRLib.FAS_MoveLinearIncPos(m_nPortNo, NumberOfMotors, SlavesNo, IncPos, DriveSpeed, wAccTime);
        //    if (nRtn != EziMOTIONPlusRLib.FMM_OK)
        //    {
        //        Debug.WriteLine($"Error Code: {nRtn} during Multi-Axis Drive");
        //    }
        //    else
        //    {
        //        NewPositionUpdateToView();
        //    }
        //}


        //private RelayCommand _MotorStopCommand;
        //public ICommand MotorStopCommand
        //{
        //    get
        //    {
        //        return _MotorStopCommand ?? (_MotorStopCommand = new RelayCommand(MotorStop));
        //    }
        //}
        //private void MotorStop()
        //{
        //    int nRtn;

        //    if (isMotorConnected == false)
        //        return;

        //    if (NumberOfMotors <= 0)
        //    {
        //        MessageBox.Show("No Proper Motors");
        //        //textSlaveNo.Focus();
        //        return;
        //    }

        //    nRtn = EziMOTIONPlusRLib.FAS_AllMoveStop(m_nPortNo);
        //    if (nRtn != EziMOTIONPlusRLib.FMM_OK)
        //    {
        //        Debug.WriteLine($"Error Code: {nRtn} during All stop");
        //    }
        //}


        //private RelayCommand _AlarmResetCommand;
        //public ICommand AlarmResetCommand
        //{
        //    get
        //    {
        //        return _AlarmResetCommand ?? (_AlarmResetCommand = new RelayCommand(AlarmReset));
        //    }
        //}
        //private void AlarmReset()
        //{
        //    int nRtn;

        //    for (byte MotorIndex = 0; MotorIndex < NumberOfMotors; MotorIndex++)
        //    {
        //        nRtn = EziMOTIONPlusRLib.FAS_ServoAlarmReset(m_nPortNo, MotorIndex);
        //        if (nRtn != EziMOTIONPlusRLib.FMM_OK)
        //        {
        //            Debug.WriteLine($"Error Code: {nRtn} at {MotorIndex}'s Motor Alarm Reset");
        //        }
        //    }
        //}


        //#endregion

        //#region Sub-Functions

        //private void NewPositionUpdateToView()
        //{
        //    Trolley_CurrentCoordinate_X = Convert.ToString(Convert.ToDouble(trolley_CurrentCoordinate_X) + Convert.ToDouble(trolley_InputVariance_X));
        //    Trolley_CurrentCoordinate_Y = Convert.ToString(Convert.ToDouble(trolley_CurrentCoordinate_Y) + Convert.ToDouble(trolley_InputVariance_Y));
        //    Trolley_CurrentCoordinate_Z = Convert.ToString(Convert.ToDouble(trolley_CurrentCoordinate_Z) + Convert.ToDouble(trolley_InputVariance_Z));
        //    Trolley_CurrentCoordinate_Tilt = Convert.ToString(Convert.ToDouble(trolley_CurrentCoordinate_Tilt) + Convert.ToDouble(trolley_InputVariance_Tilt));
        //    Trolley_CurrentCoordinate_Pivot = Convert.ToString(Convert.ToDouble(trolley_CurrentCoordinate_Pivot) + Convert.ToDouble(trolley_InputVariance_Pivot));
        //    Trolley_CurrentCoordinate_Swivel = Convert.ToString(Convert.ToDouble(trolley_CurrentCoordinate_Swivel) + Convert.ToDouble(trolley_InputVariance_Swivel));
        //}

        //#endregion

        //#region Other(INI)

        //[DllImport("kernel32")]
        //private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        //[DllImport("kernel32")]
        //private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int sise, string filePath);

        //private void GetINI()
        //{
        //    StringBuilder X = new();
        //    StringBuilder Y = new();
        //    //StringBuilder Z = new();
        //    StringBuilder Tilt = new();
        //    StringBuilder Pivot = new();
        //    StringBuilder Swivel = new();

        //    GetPrivateProfileString("PositioningStatus", "X", Convert.ToString(0), X, 32, INI_PATH);
        //    GetPrivateProfileString("PositioningStatus", "Y", Convert.ToString(0), Y, 32, INI_PATH);
        //    //GetPrivateProfileString("PositioningStatus", "Z", Convert.ToString(0), Z, 32, INI_PATH);
        //    GetPrivateProfileString("PositioningStatus", "Tilt", Convert.ToString(0), Tilt, 32, INI_PATH);
        //    GetPrivateProfileString("PositioningStatus", "Pivot", Convert.ToString(0), Pivot, 32, INI_PATH);
        //    GetPrivateProfileString("PositioningStatus", "Swivel", Convert.ToString(0), Swivel, 32, INI_PATH);

        //    trolley_CurrentCoordinate_X = X.ToString();
        //    trolley_CurrentCoordinate_Y = Y.ToString();
        //    // trolley_CurrentCoordinate_Z = Z.ToString();
        //    trolley_CurrentCoordinate_Tilt = Tilt.ToString();
        //    trolley_CurrentCoordinate_Pivot = Pivot.ToString();
        //    trolley_CurrentCoordinate_Swivel = Swivel.ToString();
        //}

        //private void WriteINI()
        //{
        //    WritePrivateProfileString("PositioningStatus", "X", trolley_CurrentCoordinate_X, INI_PATH);
        //    WritePrivateProfileString("PositioningStatus", "Y", trolley_CurrentCoordinate_Y, INI_PATH);
        //    //WritePrivateProfileString("PositioningStatus", "Z", trolley_CurrentCoordinate_Z, INI_PATH);
        //    WritePrivateProfileString("PositioningStatus", "Tilt", trolley_CurrentCoordinate_Tilt, INI_PATH);
        //    WritePrivateProfileString("PositioningStatus", "Pivot", trolley_CurrentCoordinate_Pivot, INI_PATH);
        //    WritePrivateProfileString("PositioningStatus", "Swivel", trolley_CurrentCoordinate_Swivel, INI_PATH);
        //}

        //private bool ReadOrGenerateINI()
        //{
        //    bool chk = true;
        //    try
        //    {
        //        if (System.IO.File.Exists(INI_PATH))
        //        {
        //            GetINI();
        //        }
        //        else
        //        {
        //            WriteINI();
        //        }
        //    }
        //    catch
        //    {
        //        chk = false;
        //    }
        //    return chk;
        //}

        //private void PanelDataClear()
        //{
        //    Trolley_CurrentCoordinate_X = "";
        //    Trolley_CurrentCoordinate_Y = "";
        //    Trolley_CurrentCoordinate_Z = "";
        //    Trolley_CurrentCoordinate_Tilt = "";
        //    Trolley_CurrentCoordinate_Pivot = "";
        //    Trolley_CurrentCoordinate_Swivel = "";
        //}

        //#endregion
        #endregion

        byte m_nPortNo;
        byte NumberOfMotors = 0;
        uint dwBaud = 115200;


        //------------------------------+---------------------------------------------------------------
        // [함수] INI 관련
        //------------------------------+---------------------------------------------------------------

        string INI_PATH = Application.StartupPath + @"\\setting_positioning.ini";

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int sise, string filePath);

        private void GetINI()
        {
            StringBuilder X = new();
            StringBuilder Y = new();
            //StringBuilder Z = new();
            StringBuilder Tilt = new();
            StringBuilder Pivot = new();
            StringBuilder Swivel = new();

            GetPrivateProfileString("PositioningStatus", "X", Convert.ToString(0), X, 32, INI_PATH);
            GetPrivateProfileString("PositioningStatus", "Y", Convert.ToString(0), Y, 32, INI_PATH);
            //GetPrivateProfileString("PositioningStatus", "Z", Convert.ToString(0), Z, 32, INI_PATH);
            GetPrivateProfileString("PositioningStatus", "Tilt", Convert.ToString(0), Tilt, 32, INI_PATH);
            GetPrivateProfileString("PositioningStatus", "Pivot", Convert.ToString(0), Pivot, 32, INI_PATH);
            GetPrivateProfileString("PositioningStatus", "Swivel", Convert.ToString(0), Swivel, 32, INI_PATH);

            TBlock_Current_Trolley_X = X.ToString();
            TBlock_Current_Trolley_Y = Y.ToString();
            //TBlock_Current_Trolley_Z = Z.ToString();
            TBlock_Current_Trolley_Tilt = Tilt.ToString();
            TBlock_Current_Trolley_Pivot = Pivot.ToString();
            TBlock_Current_Trolley_Swivel = Swivel.ToString();
        }
        private void WriteINI()
        {
            WritePrivateProfileString("PositioningStatus", "X", tBlock_Current_Trolley_X, INI_PATH);
            WritePrivateProfileString("PositioningStatus", "Y", tBlock_Current_Trolley_Y, INI_PATH);
            //WritePrivateProfileString("PositioningStatus", "Z", tBlock_Current_Trolley_Z, INI_PATH);
            WritePrivateProfileString("PositioningStatus", "Tilt", tBlock_Current_Trolley_Tilt, INI_PATH);
            WritePrivateProfileString("PositioningStatus", "Pivot", tBlock_Current_Trolley_Pivot, INI_PATH);
            WritePrivateProfileString("PositioningStatus", "Swivel", tBlock_Current_Trolley_Swivel, INI_PATH);

            //WritePrivateProfileString("PositioningStatus", "X", 0.ToString(), INI_PATH);
            //WritePrivateProfileString("PositioningStatus", "Y", 0.ToString(), INI_PATH);
            //WritePrivateProfileString("PositioningStatus", "Z", 0.ToString(), INI_PATH);
            //WritePrivateProfileString("PositioningStatus", "Tilt", 0.ToString(), INI_PATH);
            //WritePrivateProfileString("PositioningStatus", "Pivot", 0.ToString(), INI_PATH);
            //WritePrivateProfileString("PositioningStatus", "Swivel", 0.ToString(), INI_PATH);
        }
        private bool ReadOrGenerateINI()
        {
            bool chk = true;
            try
            {
                if (System.IO.File.Exists(INI_PATH))
                {
                    GetINI();
                }
                else
                {
                    WriteINI();
                }
            }
            catch
            {
                chk = false;
            }
            return chk;
        }
        private void tBlockReset()
        {
            TBlock_Current_PPS_X = "";
            TBlock_Current_PPS_Y = "";
            TBlock_Current_PPS_Z = "";
            TBlock_Current_PPS_Tilt = "";
            TBlock_Current_PPS_Pivot = "";
            TBlock_Current_PPS_Swivel = "";

            TBlock_Current_Trolley_X = "";
            TBlock_Current_Trolley_Y = "";
            TBlock_Current_Trolley_Z = "";
            TBlock_Current_Trolley_Tilt = "";
            TBlock_Current_Trolley_Pivot = "";
            TBlock_Current_Trolley_Swivel = "";
        }


        // PC에 꽂힌 포트 확인하여 Combobox에 추가
        private void UpdateSerialPortList()
        {
            vmcomboBoxPortNo.Clear();

            string[] portlist = SerialPort.GetPortNames();
            foreach (string portString in portlist)
                vmcomboBoxPortNo.Add(portString);
        }


        // 드라이브 모듈과 통신 연결 시도
        private RelayCommand positioning_ConnectCommand;
        public ICommand Positioning_ConnectCommand
        {
            get
            {
                return positioning_ConnectCommand ?? (positioning_ConnectCommand = new RelayCommand(Positioning_Connect));
            }
        }
        private void Positioning_Connect()
        {
            if (vmcomboBoxPortNo.Count <= 0)
            {
                MessageBox.Show("No Proper Port");
                return;
            }

            if (isMotorConnected == false)
            {
                if (string.IsNullOrEmpty(selectedcomboBoxPortNo) == false)
                {
                    m_nPortNo = byte.Parse(selectedcomboBoxPortNo.Substring(3));

                    if (EziMOTIONPlusRLib.FAS_Connect(m_nPortNo, dwBaud) == 0)
                    {
                        // Failed to connect
                        MessageBox.Show("Failed to connect");
                    }
                    else
                    {
                        // connected.
                        NumberOfMotors = 0;

                        for (byte i = 0; i < EziMOTIONPlusRLib.MAX_SLAVE_NUMS; i++)
                        {
                            if (EziMOTIONPlusRLib.FAS_IsSlaveExist(m_nPortNo, i) != 0)
                            {
                                NumberOfMotors += 1;
                            }
                        }

                        if (NumberOfMotors == 0)
                        {
                            Debug.WriteLine($"[Servo Motor] Connected, but motor is not found: Check the Power Supply");
                            return;
                        }

                        if (NumberOfMotors != 0)
                        {
                            IsMotorConnected = true;
                            Debug.WriteLine($"[Servo Motor] {NumberOfMotors}EA Connected");
                        }
                    }

                    if (ReadOrGenerateINI() == false)
                    {
                        Trace.WriteLine($"Error: INI file generation is failed");
                    }
                }
                else
                {
                    MessageBox.Show("Please Select COM port");
                }
            }
            else
            {
                EziMOTIONPlusRLib.FAS_Close(m_nPortNo);
                IsMotorConnected = false;
                WriteINI(); // window가 닫힐 때 추가되도록 구현하는 방법 찾아보기
                tBlockReset();
                Debug.WriteLine("[Servo Motor] Disconnected");
            }
        }


        // 포트에 연결된 모든 드라이브 Servo 상태 ON/OFF
        private RelayCommand positioning_ServoONOFFCommand;
        public ICommand Positioning_ServoONOFFCommand
        {
            get
            {
                return positioning_ServoONOFFCommand ?? (positioning_ServoONOFFCommand = new RelayCommand(Positioning_ServoONOFF));
            }
        }
        private void Positioning_ServoONOFF()
        {
            int nRtn;

            if (isServoON == false)
            {
                if (isMotorConnected == false)
                    return;

                if (NumberOfMotors <= 0)
                {
                    MessageBox.Show("No Proper Motors");
                    return;
                }

                for (byte MotorIndex = 0; MotorIndex < NumberOfMotors; MotorIndex++)
                {
                    nRtn = EziMOTIONPlusRLib.FAS_ServoEnable(m_nPortNo, MotorIndex, 1);
                    if (nRtn != EziMOTIONPlusRLib.FMM_OK)
                    {
                        Debug.WriteLine($"Error Code: {nRtn} at {MotorIndex}'s Motor");
                    }
                }
                IsServoON = true;
            }
            else
            {
                if (isMotorConnected == false)
                    return;

                if (NumberOfMotors <= 0)
                {
                    MessageBox.Show("No Proper Motors");
                    return;
                }

                for (byte MotorIndex = 0; MotorIndex < NumberOfMotors; MotorIndex++)
                {
                    nRtn = EziMOTIONPlusRLib.FAS_ServoEnable(m_nPortNo, MotorIndex, 0);
                    if (nRtn != EziMOTIONPlusRLib.FMM_OK)
                    {
                        Debug.WriteLine($"Error Code: {nRtn} at {MotorIndex}'s Motor");
                    }
                }
                IsServoON = false;
            }
        }








        #region Based on Relative to Trolley, Sequential

        // 포트에 연결된 모든 드라이브 동시 운전
        private RelayCommand positioning_DriveCommand_SequentialTemp;
        public ICommand Positioning_DriveCommand_SequentialTemp
        {
            get
            {
                return positioning_DriveCommand_SequentialTemp ?? (positioning_DriveCommand_SequentialTemp = new RelayCommand(Positioning_Drive_SequentialTemp));
            }
        }
        private void Positioning_Drive_SequentialTemp()
        {
            // SERVO Motor Table
            // X-axis: 1 mm =  3,226 pulse ( -30 mm ~  30 mm )
            // Y-axis: 1 mm = 81,939 pulse (   0 mm ~ 200 mm )
            // Tilt:   1°  = 56,222 pulse (   -90°~ 90°   )
            // Pivot:  1°  =  6,795 pulse (   -90°~ 90°   )
            // Swivel: 1°  =  5,022 pulse (   -90°~ 90°   )

            // Proper Motor Speed [2021. 11. 18.]
            // X-axis: ~  30,000 ( use  10,000 )
            // Y-axis: ~ 450,000 ( use 200,000 )
            // Tilt:   ~ 300,000 ( use  50,000 )
            // Pivot:  ~  50,000 ( use  50,000 )
            // Swivel: ~  30,000 ( use  30,000 )            

            int Dist_Trolley_X = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(TBox_RelPreset_Trolley_X) * 3266));
            int Dist_Trolley_Y = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(TBox_RelPreset_Trolley_Y) * 81939));
            int Dist_Trolley_Tilt = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(TBox_RelPreset_Trolley_Tilt) * 56222));
            int Dist_Trolley_Pivot = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(TBox_RelPreset_Trolley_Pivot) * 6795));
            int Dist_Trolley_Swivel = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(TBox_RelPreset_Trolley_Swivel) * 5022));

            int[] IncPos = { Dist_Trolley_X, Dist_Trolley_Y, Dist_Trolley_Tilt, Dist_Trolley_Pivot, Dist_Trolley_Swivel };
            uint[] DriveSpeed = { 10000, 150000, 50000, 50000, 15000 };
            //ushort wAccTime = 100;

            if (NumberOfMotors == 5)
            {
                // -------------------------------------------
                // Roll(Tilt) -> Pitch(Pivot) -> Yaw(Swivel) 
                // -------------------------------------------

                for (byte MotorIndex = 2; MotorIndex < 5; MotorIndex++)
                {
                    int nRtn;
                    nRtn = EziMOTIONPlusRLib.FAS_MoveSingleAxisIncPos(m_nPortNo, MotorIndex, IncPos[MotorIndex], DriveSpeed[MotorIndex]);

                    if (nRtn != EziMOTIONPlusRLib.FMM_OK)
                    {
                        Debug.WriteLine($"Error Code: {nRtn} during {MotorIndex}'s axis Drive");
                    }
                }



                // -------------------------------------------
                // X -> Y
                // -------------------------------------------

                for (byte MotorIndex = 0; MotorIndex < 2; MotorIndex++)
                {

                    int nRtn;
                    nRtn = EziMOTIONPlusRLib.FAS_MoveSingleAxisIncPos(m_nPortNo, MotorIndex, IncPos[MotorIndex], DriveSpeed[MotorIndex]);

                    if (nRtn != EziMOTIONPlusRLib.FMM_OK)
                    {
                        Debug.WriteLine($"Error Code: {nRtn} during {MotorIndex}'s axis Drive");
                    }
                }
            }
        }



        //async Task<int> test(byte m_nPortNo, byte MotorIndex, int[] IncPos, uint[] DriveSpeed)
        //{
        //    Task<int> task = Task.Factory.StartNew<int>(() => EziMOTIONPlusRLib.FAS_MoveSingleAxisIncPos(m_nPortNo, MotorIndex, IncPos[MotorIndex], DriveSpeed[MotorIndex]));
        //    int nRtn = await task;
        //    return nRtn;
        //}

        #endregion

        #region Drive Based on PPS Coordinate

        // 포트에 연결된 모든 드라이브 동시 운전
        private RelayCommand positioning_DriveCommand_AbsPPS;
        public ICommand Positioning_DriveCommand_AbsPPS
        {
            get
            {
                return positioning_DriveCommand_AbsPPS ?? (positioning_DriveCommand_AbsPPS = new RelayCommand(Positioning_Drive_AbsPPS));
            }
        }
        private void Positioning_Drive_AbsPPS()
        {
            byte[] SlavesNo = new byte[NumberOfMotors];
            for (byte MotorIndex = 0; MotorIndex < NumberOfMotors; MotorIndex++)
            {
                SlavesNo[MotorIndex] = MotorIndex;
            }

            // ----------------------------------------------
            // Moving/Rotate Amount Calculation
            // ----------------------------------------------

            double Dist_Trolley_X = Convert.ToDouble(TBox_RelPreset_Trolley_X) - Convert.ToDouble(TBlock_Current_Trolley_X);
            double Dist_Trolley_Y = Convert.ToDouble(TBox_RelPreset_Trolley_Y) - Convert.ToDouble(TBlock_Current_Trolley_Y);
            //double Dist_Trolley_Z = Convert.ToDouble(TBox_RelPreset_Trolley_Z) - Convert.ToDouble(TBlock_Current_Trolley_Z);
            double Degree_Trolley_Tilt = Convert.ToDouble(TBox_RelPreset_Trolley_Tilt) - Convert.ToDouble(TBlock_Current_Trolley_Tilt);
            double Degree_Trolley_Pivot = Convert.ToDouble(TBox_RelPreset_Trolley_Pivot) - Convert.ToDouble(TBlock_Current_Trolley_Pivot);
            double Degree_Trolley_Swivel = Convert.ToDouble(TBox_RelPreset_Trolley_Swivel) - Convert.ToDouble(TBlock_Current_Trolley_Swivel);


            // ----------------------------------------------
            // Moving/Rotate Posibility Check
            // ----------------------------------------------

            // Code ~~~
            //
            //


            // ----------------------------------------------
            // Current Position Visualization Update
            // ----------------------------------------------

            TBlock_Current_Trolley_X = Convert.ToString(Convert.ToDouble(tBlock_Current_Trolley_X) + Dist_Trolley_X);
            TBlock_Current_Trolley_Y = Convert.ToString(Convert.ToDouble(tBlock_Current_Trolley_Y) + Dist_Trolley_Y);
            //TBlock_Current_Trolley_Z = Convert.ToString(Convert.ToDouble(tBlock_Current_Trolley_Z) + Dist_Trolley_Z);
            TBlock_Current_Trolley_Tilt = Convert.ToString(Convert.ToDouble(tBlock_Current_Trolley_Tilt) + Degree_Trolley_Tilt);
            TBlock_Current_Trolley_Pivot = Convert.ToString(Convert.ToDouble(tBlock_Current_Trolley_Pivot) + Degree_Trolley_Pivot);
            TBlock_Current_Trolley_Swivel = Convert.ToString(Convert.ToDouble(tBlock_Current_Trolley_Swivel) + Degree_Trolley_Swivel);

            int[] IncPos = ConvertDistDegreeToPulse(Dist_Trolley_X, Dist_Trolley_Y, Degree_Trolley_Tilt, Degree_Trolley_Pivot, Degree_Trolley_Swivel);
            uint DriveSpeed = SetMotorSpeed(IncPos);
            ushort wAccTime = 100; // 가감속 시간 100 ms

            int nRtn;
            nRtn = EziMOTIONPlusRLib.FAS_MoveLinearIncPos(m_nPortNo, NumberOfMotors, SlavesNo, IncPos, DriveSpeed, wAccTime);
            if (nRtn != EziMOTIONPlusRLib.FMM_OK)
            {
                Debug.WriteLine($"Error Code: {nRtn} during Multi-Axis Drive");
            }
        }

        #endregion

        #region Drive Based on Trolley Coordinate

        private RelayCommand positioning_DriveCommand;
        public ICommand Positioning_DriveCommand
        {
            get
            {
                return positioning_DriveCommand ?? (positioning_DriveCommand = new RelayCommand(Positioning_Drive));
            }
        }
        private void Positioning_Drive()
        {
            byte[] SlavesNo = new byte[NumberOfMotors];
            for (byte MotorIndex = 0; MotorIndex < NumberOfMotors; MotorIndex++)
            {
                SlavesNo[MotorIndex] = MotorIndex;
            }

            //int[] IncPos = ConvertDistDegreeToPulse(tBox_RelPreset_Trolley_X, tBox_RelPreset_Trolley_Y, tBox_RelPreset_Trolley_Tilt, tBox_RelPreset_Trolley_Pivot, tBox_RelPreset_Trolley_Swivel);
            int[] IncPos = { Convert.ToInt32(tBox_RelPreset_Trolley_X), Convert.ToInt32(tBox_RelPreset_Trolley_Y), Convert.ToInt32(tBox_RelPreset_Trolley_Tilt), Convert.ToInt32(tBox_RelPreset_Trolley_Pivot), Convert.ToInt32(tBox_RelPreset_Trolley_Swivel) };

            //uint DriveSpeed = SetMotorSpeed(IncPos);
            uint DriveSpeed = Convert.ToUInt32(tBox_speed);
            ushort wAccTime = 100; // 가감속 시간 100 ms                 

            int nRtn;
            nRtn = EziMOTIONPlusRLib.FAS_MoveLinearIncPos(m_nPortNo, NumberOfMotors, SlavesNo, IncPos, DriveSpeed, wAccTime);
            if (nRtn != EziMOTIONPlusRLib.FMM_OK)
            {
                Debug.WriteLine($"Error Code: {nRtn} during Multi-Axis Drive");
            }
            else
            {
                CurrentTrolleyPositionUpdate();
            }
        }

        #endregion



        private void CurrentTrolleyPositionUpdate()
        {
            TBlock_Current_Trolley_X = Convert.ToString(Convert.ToDouble(tBlock_Current_Trolley_X) + Convert.ToDouble(tBox_RelPreset_Trolley_X));
            TBlock_Current_Trolley_Y = Convert.ToString(Convert.ToDouble(tBlock_Current_Trolley_Y) + Convert.ToDouble(tBox_RelPreset_Trolley_Y));
            TBlock_Current_Trolley_Z = Convert.ToString(Convert.ToDouble(tBlock_Current_Trolley_Z) + Convert.ToDouble(tBox_RelPreset_Trolley_Z));
            TBlock_Current_Trolley_Tilt = Convert.ToString(Convert.ToDouble(tBlock_Current_Trolley_Tilt) + Convert.ToDouble(tBox_RelPreset_Trolley_Tilt));
            TBlock_Current_Trolley_Pivot = Convert.ToString(Convert.ToDouble(tBlock_Current_Trolley_Pivot) + Convert.ToDouble(tBox_RelPreset_Trolley_Pivot));
            TBlock_Current_Trolley_Swivel = Convert.ToString(Convert.ToDouble(tBlock_Current_Trolley_Swivel) + Convert.ToDouble(tBox_RelPreset_Trolley_Swivel));
        }



        // 포트에 연결된 모든 드라이브 동시 멈춤
        private RelayCommand positioning_StopCommand;
        public ICommand Positioning_StopCommand
        {
            get
            {
                return positioning_StopCommand ?? (positioning_StopCommand = new RelayCommand(Positioning_Stop));
            }
        }
        private void Positioning_Stop()
        {
            int nRtn;

            if (isMotorConnected == false)
                return;

            if (NumberOfMotors <= 0)
            {
                MessageBox.Show("No Proper Motors");
                //textSlaveNo.Focus();
                return;
            }

            nRtn = EziMOTIONPlusRLib.FAS_AllMoveStop(m_nPortNo);
            if (nRtn != EziMOTIONPlusRLib.FMM_OK)
            {
                Debug.WriteLine($"Error Code: {nRtn} during All stop");
            }
        }


        // 포트에 연결된 모든 드라이브 알람 리셋
        private RelayCommand positioning_AlarmResetCommand;
        public ICommand Positioning_AlarmResetCommand
        {
            get
            {
                return positioning_AlarmResetCommand ?? (positioning_AlarmResetCommand = new RelayCommand(Positioning_AlarmReset));
            }
        }
        private void Positioning_AlarmReset()
        {
            int nRtn;

            for (byte MotorIndex = 0; MotorIndex < NumberOfMotors; MotorIndex++)
            {
                nRtn = EziMOTIONPlusRLib.FAS_ServoAlarmReset(m_nPortNo, MotorIndex);
                if (nRtn != EziMOTIONPlusRLib.FMM_OK)
                {
                    Debug.WriteLine($"Error Code: {nRtn} at {MotorIndex}'s Motor Alarm Reset");
                }
            }
        }






        private uint SetMotorSpeed(int[] IncPos)
        {
            // Proper Motor Speed [2021. 11. 18.]
            // X-axis: ~  30,000 ( use  10,000 )
            // Y-axis: ~ 450,000 ( use 150,000 )
            // Tilt:   ~ 300,000 ( use  50,000 )
            // Pivot:  ~  50,000 ( use  50,000 )
            // Swivel: ~  30,000 ( use  15,000 )

            uint DriveSpeed;

            int Pulse_X = IncPos[0];
            int Pulse_Y = IncPos[1];
            int Pulse_Tilt = IncPos[2];
            int Pulse_Pivot = IncPos[3];
            int Pulse_Swivel = IncPos[4];

            if (Pulse_Y != 0) // Y-axis pulse exist
            {
                DriveSpeed = 150000;
            }
            else
            {
                if (Pulse_Tilt != 0) // Tilt pulse exist
                {
                    DriveSpeed = 50000;
                }
                else
                {
                    if (Pulse_Pivot != 0) // Pivot pulse exist
                    {
                        DriveSpeed = 50000;
                    }
                    else
                    {
                        if (Pulse_Swivel != 0) // Swivel pulse exist
                        {
                            DriveSpeed = 15000;
                        }
                        else // X-axis
                        {
                            DriveSpeed = 10000;
                        }
                    }
                }
            }

            //DriveSpeed = 30000;
            return DriveSpeed;
        }
        private int[] ConvertDistDegreeToPulse(string Distance_X, string Distance_Y, string Degree_Tilt, string Degree_Pivot, string Degree_Swivel)
        {
            // SERVO Motor Table
            // X-axis: 1 mm =  3,226 pulse ( -30 mm ~  30 mm )
            // Y-axis: 1 mm = 81,939 pulse (   0 mm ~ 200 mm )
            // Tilt:   1°  = 56,222 pulse (   -90°~ 90°   )
            // Pivot:  1°  =  6,795 pulse (   -90°~ 90°   )
            // Swivel: 1°  =  5,022 pulse (   -90°~ 90°   )

            int Pulse_X = -Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Distance_X) * 3226));
            int Pulse_Y = -Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Distance_Y) * 81939));
            int Pulse_Tilt = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Degree_Tilt) * 56222));
            int Pulse_Pivot = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Degree_Pivot) * 6795));
            int Pulse_Swivel = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Degree_Swivel) * 5022));

            int[] InputPulse = { Pulse_X, Pulse_Y, Pulse_Tilt, Pulse_Pivot, Pulse_Swivel };

            return InputPulse;
        }
        private int[] ConvertDistDegreeToPulse(double Distance_X, double Distance_Y, double Degree_Tilt, double Degree_Pivot, double Degree_Swivel)
        {
            // SERVO Motor Table
            // X-axis: 1 mm =  3,226 pulse ( -30 mm ~  30 mm )
            // Y-axis: 1 mm = 81,939 pulse (   0 mm ~ 200 mm )
            // Tilt:   1°  = 56,222 pulse (   -90°~ 90°   )
            // Pivot:  1°  =  6,795 pulse (   -90°~ 90°   )
            // Swivel: 1°  =  5,022 pulse (   -90°~ 90°   )

            int Pulse_X = Convert.ToInt32(Math.Ceiling(Distance_X * 3226));
            int Pulse_Y = Convert.ToInt32(Math.Ceiling(Distance_Y * 81939));
            int Pulse_Tilt = Convert.ToInt32(Math.Ceiling(Degree_Tilt * 56222));
            int Pulse_Pivot = Convert.ToInt32(Math.Ceiling(Degree_Pivot * 6795));
            int Pulse_Swivel = Convert.ToInt32(Math.Ceiling(Degree_Swivel * 5022));

            int[] InputPulse = { Pulse_X, Pulse_Y, Pulse_Tilt, Pulse_Pivot, Pulse_Swivel };

            return InputPulse;
        }



















        // --------------------------------------------------
        // Position in PPS Coordinate
        // --------------------------------------------------

        #region Position in PPS Coordinate, Current Position [ TBlock_Current_PPS_@@@ ]

        private string tBlock_Current_PPS_X;
        public string TBlock_Current_PPS_X
        {
            get
            {
                return tBlock_Current_PPS_X;
            }
            set
            {
                tBlock_Current_PPS_X = value;
                //Debug.WriteLine($"[Motor] X drive: {tBlock_Current_PPS_X}");
                OnPropertyChanged(nameof(TBlock_Current_PPS_X));
            }
        }

        private string tBlock_Current_PPS_Y;
        public string TBlock_Current_PPS_Y
        {
            get
            {
                return tBlock_Current_PPS_Y;
            }
            set
            {
                tBlock_Current_PPS_Y = value;
                //Debug.WriteLine($"[Motor] Y drive: {tBlock_Current_PPS_Y}");
                OnPropertyChanged(nameof(TBlock_Current_PPS_Y));
            }
        }

        private string tBlock_Current_PPS_Z;
        public string TBlock_Current_PPS_Z
        {
            get
            {
                return tBlock_Current_PPS_Z;
            }
            set
            {
                tBlock_Current_PPS_Z = value;
                //Debug.WriteLine($"[Motor] Zs drive: {tBlock_Current_PPS_Z}");
                OnPropertyChanged(nameof(TBlock_Current_PPS_Z));
            }
        }

        private string tBlock_Current_PPS_Tilt;
        public string TBlock_Current_PPS_Tilt
        {
            get
            {
                return tBlock_Current_PPS_Tilt;
            }
            set
            {
                tBlock_Current_PPS_Tilt = value;
                //Debug.WriteLine($"[Motor] Tilt drive: {tBlock_Current_PPS_Tilt}");
                OnPropertyChanged(nameof(TBlock_Current_PPS_Tilt));
            }
        }

        private string tBlock_Current_PPS_Pivot;
        public string TBlock_Current_PPS_Pivot
        {
            get
            {
                return tBlock_Current_PPS_Pivot;
            }
            set
            {
                tBlock_Current_PPS_Pivot = value;
                //Debug.WriteLine($"[Motor] Pivot drive: {tBlock_Current_PPS_Pivot}");
                OnPropertyChanged(nameof(TBlock_Current_PPS_Pivot));
            }
        }

        private string tBlock_Current_PPS_Swivel;
        public string TBlock_Current_PPS_Swivel
        {
            get
            {
                return tBlock_Current_PPS_Swivel;
            }
            set
            {
                tBlock_Current_PPS_Swivel = value;
                //Debug.WriteLine($"[Motor] Swivel drive: {tBlock_Current_PPS_Swivel}");
                OnPropertyChanged(nameof(TBlock_Current_PPS_Swivel));
            }
        }



        #endregion

        #region Position in PPS Coordinate, Relative Preset [ TBox_RelPreset_PPS_@@@ ]

        private string tBox_RelPreset_PPS_X;
        public string TBox_RelPreset_PPS_X
        {
            get
            {
                return tBox_RelPreset_PPS_X;
            }
            set
            {
                tBox_RelPreset_PPS_X = value;
                //Debug.WriteLine($"[Motor] X drive: {tBox_RelPreset_PPS_X}");
                OnPropertyChanged(nameof(TBox_RelPreset_PPS_X));
            }
        }

        private string tBox_RelPreset_PPS_Y;
        public string TBox_RelPreset_PPS_Y
        {
            get
            {
                return tBox_RelPreset_PPS_Y;
            }
            set
            {
                tBox_RelPreset_PPS_Y = value;
                //Debug.WriteLine($"[Motor] Y drive: {tBox_RelPreset_PPS_Y}");
                OnPropertyChanged(nameof(TBox_RelPreset_PPS_Y));
            }
        }

        private string tBox_RelPreset_PPS_Z;
        public string TBox_RelPreset_PPS_Z
        {
            get
            {
                return tBox_RelPreset_PPS_Z;
            }
            set
            {
                tBox_RelPreset_PPS_Z = value;
                //Debug.WriteLine($"[Motor] Z drive: {tBox_RelPreset_PPS_Z}");
                OnPropertyChanged(nameof(TBox_RelPreset_PPS_Z));
            }
        }

        private string tBox_RelPreset_PPS_Tilt;
        public string TBox_RelPreset_PPS_Tilt
        {
            get
            {
                return tBox_RelPreset_PPS_Tilt;
            }
            set
            {
                tBox_RelPreset_PPS_Tilt = value;
                //Debug.WriteLine($"[Motor] Tilt drive: {tBox_RelPreset_PPS_Tilt}");
                OnPropertyChanged(nameof(TBox_RelPreset_PPS_Tilt));
            }
        }

        private string tBox_RelPreset_PPS_Pivot;
        public string TBox_RelPreset_PPS_Pivot
        {
            get
            {
                return tBox_RelPreset_PPS_Pivot;
            }
            set
            {
                tBox_RelPreset_PPS_Pivot = value;
                //Debug.WriteLine($"[Motor] Pivot drive: {tBox_RelPreset_PPS_Pivot}");
                OnPropertyChanged(nameof(TBox_RelPreset_PPS_Pivot));
            }
        }

        private string tBox_RelPreset_PPS_Swivel;
        public string TBox_RelPreset_PPS_Swivel
        {
            get
            {
                return tBox_RelPreset_PPS_Swivel;
            }
            set
            {
                tBox_RelPreset_PPS_Swivel = value;
                //Debug.WriteLine($"[Motor] Swivel drive: {tBox_RelPreset_PPS_Swivel}");
                OnPropertyChanged(nameof(TBox_RelPreset_PPS_Swivel));
            }
        }

        #endregion

        #region Position in PPS Coordinate, Absolute Preset [ TBox_AbsPreset_PPS_@@@ ] 

        private string tBox_AbsPreset_PPS_X;
        public string TBox_AbsPreset_PPS_X
        {
            get
            {
                return tBox_AbsPreset_PPS_X;
            }
            set
            {
                tBox_AbsPreset_PPS_X = value;
                //Debug.WriteLine($"[Motor] X drive: {tBox_AbsPreset_PPS_X}");
                OnPropertyChanged(nameof(TBox_AbsPreset_PPS_X));
            }
        }

        private string tBox_AbsPreset_PPS_Y;
        public string TBox_AbsPreset_PPS_Y
        {
            get
            {
                return tBox_AbsPreset_PPS_Y;
            }
            set
            {
                tBox_AbsPreset_PPS_Y = value;
                //Debug.WriteLine($"[Motor] Y drive: {tBox_AbsPreset_PPS_Y}");
                OnPropertyChanged(nameof(TBox_AbsPreset_PPS_Y));
            }
        }

        private string tBox_AbsPreset_PPS_Z;
        public string TBox_AbsPreset_PPS_Z
        {
            get
            {
                return tBox_AbsPreset_PPS_Z;
            }
            set
            {
                tBox_AbsPreset_PPS_Z = value;
                //Debug.WriteLine($"[Motor] Z drive: {tBox_AbsPreset_PPS_Z}");
                OnPropertyChanged(nameof(TBox_AbsPreset_PPS_Z));
            }
        }

        private string tBox_AbsPreset_PPS_Tilt;
        public string TBox_AbsPreset_PPS_Tilt
        {
            get
            {
                return tBox_AbsPreset_PPS_Tilt;
            }
            set
            {
                tBox_AbsPreset_PPS_Tilt = value;
                //Debug.WriteLine($"[Motor] Tilt drive: {tBox_AbsPreset_PPS_Tilt}");
                OnPropertyChanged(nameof(TBox_AbsPreset_PPS_Tilt));
            }
        }

        private string tBox_AbsPreset_PPS_Pivot;
        public string TBox_AbsPreset_PPS_Pivot
        {
            get
            {
                return tBox_AbsPreset_PPS_Pivot;
            }
            set
            {
                tBox_AbsPreset_PPS_Pivot = value;
                //Debug.WriteLine($"[Motor] Pivot drive: {tBox_AbsPreset_PPS_Pivot}");
                OnPropertyChanged(nameof(TBox_AbsPreset_PPS_Pivot));
            }
        }

        private string tBox_AbsPreset_PPS_Swivel;
        public string TBox_AbsPreset_PPS_Swivel
        {
            get
            {
                return tBox_AbsPreset_PPS_Swivel;
            }
            set
            {
                tBox_AbsPreset_PPS_Swivel = value;
                //Debug.WriteLine($"[Motor] Swivel drive: {tBox_AbsPreset_PPS_Swivel}");
                OnPropertyChanged(nameof(TBox_AbsPreset_PPS_Swivel));
            }
        }

        #endregion


        // --------------------------------------------------
        // Position Relative to Trolley
        // --------------------------------------------------

        #region Position Relative to Trolley, Current Position [ TBlock_Current_Trolley_@@@ ]

        private string tBlock_Current_Trolley_X;
        public string TBlock_Current_Trolley_X
        {
            get
            {
                return tBlock_Current_Trolley_X;
            }
            set
            {
                tBlock_Current_Trolley_X = value;
                //Debug.WriteLine($"[Motor] X drive: {tBlock_Current_Trolley_X}");
                OnPropertyChanged(nameof(TBlock_Current_Trolley_X));
            }
        }

        private string tBlock_Current_Trolley_Y;
        public string TBlock_Current_Trolley_Y
        {
            get
            {
                return tBlock_Current_Trolley_Y;
            }
            set
            {
                tBlock_Current_Trolley_Y = value;
                //Debug.WriteLine($"[Motor] Y drive: {tBlock_Current_Trolley_Y}");
                OnPropertyChanged(nameof(TBlock_Current_Trolley_Y));
            }
        }

        private string tBlock_Current_Trolley_Z;
        public string TBlock_Current_Trolley_Z
        {
            get
            {
                return tBlock_Current_Trolley_Z;
            }
            set
            {
                tBlock_Current_Trolley_Z = value;
                //Debug.WriteLine($"[Motor] Zs drive: {tBlock_Current_Trolley_Z}");
                OnPropertyChanged(nameof(TBlock_Current_Trolley_Z));
            }
        }

        private string tBlock_Current_Trolley_Tilt;
        public string TBlock_Current_Trolley_Tilt
        {
            get
            {
                return tBlock_Current_Trolley_Tilt;
            }
            set
            {
                tBlock_Current_Trolley_Tilt = value;
                //Debug.WriteLine($"[Motor] Tilt drive: {tBlock_Current_Trolley_Tilt}");
                OnPropertyChanged(nameof(TBlock_Current_Trolley_Tilt));
            }
        }

        private string tBlock_Current_Trolley_Pivot;
        public string TBlock_Current_Trolley_Pivot
        {
            get
            {
                return tBlock_Current_Trolley_Pivot;
            }
            set
            {
                tBlock_Current_Trolley_Pivot = value;
                //Debug.WriteLine($"[Motor] Pivot drive: {tBlock_Current_Trolley_Pivot}");
                OnPropertyChanged(nameof(TBlock_Current_Trolley_Pivot));
            }
        }

        private string tBlock_Current_Trolley_Swivel;
        public string TBlock_Current_Trolley_Swivel
        {
            get
            {
                return tBlock_Current_Trolley_Swivel;
            }
            set
            {
                tBlock_Current_Trolley_Swivel = value;
                //Debug.WriteLine($"[Motor] Swivel drive: {tBlock_Current_Trolley_Swivel}");
                OnPropertyChanged(nameof(TBlock_Current_Trolley_Swivel));
            }
        }



        #endregion

        #region Position Relative to Trolley, Relative Preset [ TBox_RelPreset_Trolley_@@@ ]

        private string tBox_RelPreset_Trolley_X;
        public string TBox_RelPreset_Trolley_X
        {
            get
            {
                return tBox_RelPreset_Trolley_X;
            }
            set
            {
                tBox_RelPreset_Trolley_X = value;
                //Debug.WriteLine($"[Motor] X drive: {tBox_RelPreset_Trolley_X}");
                OnPropertyChanged(nameof(TBox_RelPreset_Trolley_X));
            }
        }

        private string tBox_RelPreset_Trolley_Y;
        public string TBox_RelPreset_Trolley_Y
        {
            get
            {
                return tBox_RelPreset_Trolley_Y;
            }
            set
            {
                tBox_RelPreset_Trolley_Y = value;
                //Debug.WriteLine($"[Motor] Y drive: {tBox_RelPreset_Trolley_Y}");
                OnPropertyChanged(nameof(TBox_RelPreset_Trolley_Y));
            }
        }

        private string tBox_RelPreset_Trolley_Z;
        public string TBox_RelPreset_Trolley_Z
        {
            get
            {
                return tBox_RelPreset_Trolley_Z;
            }
            set
            {
                tBox_RelPreset_Trolley_Z = value;
                //Debug.WriteLine($"[Motor] Z drive: {tBox_RelPreset_Trolley_Z}");
                OnPropertyChanged(nameof(TBox_RelPreset_Trolley_Z));
            }
        }

        private string tBox_RelPreset_Trolley_Tilt;
        public string TBox_RelPreset_Trolley_Tilt
        {
            get
            {
                return tBox_RelPreset_Trolley_Tilt;
            }
            set
            {
                tBox_RelPreset_Trolley_Tilt = value;
                //Debug.WriteLine($"[Motor] Tilt drive: {tBox_RelPreset_Trolley_Tilt}");
                OnPropertyChanged(nameof(TBox_RelPreset_Trolley_Tilt));
            }
        }

        private string tBox_RelPreset_Trolley_Pivot;
        public string TBox_RelPreset_Trolley_Pivot
        {
            get
            {
                return tBox_RelPreset_Trolley_Pivot;
            }
            set
            {
                tBox_RelPreset_Trolley_Pivot = value;
                //Debug.WriteLine($"[Motor] Pivot drive: {tBox_RelPreset_Trolley_Pivot}");
                OnPropertyChanged(nameof(TBox_RelPreset_Trolley_Pivot));
            }
        }

        private string tBox_RelPreset_Trolley_Swivel;
        public string TBox_RelPreset_Trolley_Swivel
        {
            get
            {
                return tBox_RelPreset_Trolley_Swivel;
            }
            set
            {
                tBox_RelPreset_Trolley_Swivel = value;
                //Debug.WriteLine($"[Motor] Swivel drive: {tBox_RelPreset_Trolley_Swivel}");
                OnPropertyChanged(nameof(TBox_RelPreset_Trolley_Swivel));
            }
        }

        #endregion

        #region Position Relative to Trolley, Absolute Preset [ TBox_AbsPreset_Trolley_@@@ ]

        private string tBox_AbsPreset_Trolley_X;
        public string TBox_AbsPreset_Trolley_X
        {
            get
            {
                return tBox_AbsPreset_Trolley_X;
            }
            set
            {
                tBox_AbsPreset_Trolley_X = value;
                //Debug.WriteLine($"[Motor] X drive: {tBox_AbsPreset_Trolley_X}");
                OnPropertyChanged(nameof(TBox_AbsPreset_Trolley_X));
            }
        }

        private string tBox_AbsPreset_Trolley_Y;
        public string TBox_AbsPreset_Trolley_Y
        {
            get
            {
                return tBox_AbsPreset_Trolley_Y;
            }
            set
            {
                tBox_AbsPreset_Trolley_Y = value;
                //Debug.WriteLine($"[Motor] Y drive: {tBox_AbsPreset_Trolley_Y}");
                OnPropertyChanged(nameof(TBox_AbsPreset_Trolley_Y));
            }
        }

        private string tBox_AbsPreset_Trolley_Z;
        public string TBox_AbsPreset_Trolley_Z
        {
            get
            {
                return tBox_AbsPreset_Trolley_Z;
            }
            set
            {
                tBox_AbsPreset_Trolley_Z = value;
                //Debug.WriteLine($"[Motor] Z drive: {tBox_AbsPreset_Trolley_Z}");
                OnPropertyChanged(nameof(TBox_AbsPreset_Trolley_Z));
            }
        }

        private string tBox_AbsPreset_Trolley_Tilt;
        public string TBox_AbsPreset_Trolley_Tilt
        {
            get
            {
                return tBox_AbsPreset_Trolley_Tilt;
            }
            set
            {
                tBox_AbsPreset_Trolley_Tilt = value;
                //Debug.WriteLine($"[Motor] Tilt drive: {tBox_AbsPreset_Trolley_Tilt}");
                OnPropertyChanged(nameof(TBox_AbsPreset_Trolley_Tilt));
            }
        }

        private string tBox_AbsPreset_Trolley_Pivot;
        public string TBox_AbsPreset_Trolley_Pivot
        {
            get
            {
                return tBox_AbsPreset_Trolley_Pivot;
            }
            set
            {
                tBox_AbsPreset_Trolley_Pivot = value;
                //Debug.WriteLine($"[Motor] Pivot drive: {tBox_AbsPreset_Trolley_Pivot}");
                OnPropertyChanged(nameof(TBox_AbsPreset_Trolley_Pivot));
            }
        }

        private string tBox_AbsPreset_Trolley_Swivel;
        public string TBox_AbsPreset_Trolley_Swivel
        {
            get
            {
                return tBox_AbsPreset_Trolley_Swivel;
            }
            set
            {
                tBox_AbsPreset_Trolley_Swivel = value;
                //Debug.WriteLine($"[Motor] Swivel drive: {tBox_AbsPreset_Trolley_Swivel}");
                OnPropertyChanged(nameof(TBox_AbsPreset_Trolley_Swivel));
            }
        }

        #endregion


        // --------------------------------------------------
        // Position Error
        // --------------------------------------------------

        #region Position Error -> Total Error(Linear & Angular)

        private string tBlock_Error_X;
        public string TBlock_Error_X
        {
            get
            {
                return tBlock_Error_X;
            }
            set
            {
                tBlock_Error_X = value;
                //Debug.WriteLine($"[Motor] X drive: {tBlock_Error_X}");
                OnPropertyChanged(nameof(TBlock_Error_X));
            }
        }

        private string tBlock_Error_Y;
        public string TBlock_Error_Y
        {
            get
            {
                return tBlock_Error_Y;
            }
            set
            {
                tBlock_Error_Y = value;
                //Debug.WriteLine($"[Motor] Y drive: {tBlock_Error_Y}");
                OnPropertyChanged(nameof(TBlock_Error_Y));
            }
        }

        private string tBlock_Error_Z;
        public string TBlock_Error_Z
        {
            get
            {
                return tBlock_Error_Z;
            }
            set
            {
                tBlock_Error_Z = value;
                //Debug.WriteLine($"[Motor] Z drive: {tBlock_Error_Z}");
                OnPropertyChanged(nameof(TBlock_Error_Z));
            }
        }

        private string tBlock_Error_Pivot;
        public string TBlock_Error_Pivot
        {
            get
            {
                return tBlock_Error_Pivot;
            }
            set
            {
                tBlock_Error_Pivot = value;
                //Debug.WriteLine($"[Motor] Pivot drive: {tBlock_Error_Pivot}");
                OnPropertyChanged(nameof(TBlock_Error_Pivot));
            }
        }

        private string tBlock_Error_Tilt;
        public string TBlock_Error_Tilt
        {
            get
            {
                return tBlock_Error_Tilt;
            }
            set
            {
                tBlock_Error_Tilt = value;
                //Debug.WriteLine($"[Motor] Tilt drive: {tBlock_Error_Tilt}");
                OnPropertyChanged(nameof(TBlock_Error_Tilt));
            }
        }

        private string tBlock_Error_Swivel;
        public string TBlock_Error_Swivel
        {
            get
            {
                return tBlock_Error_Swivel;
            }
            set
            {
                tBlock_Error_Swivel = value;
                //Debug.WriteLine($"[Motor] Swivel drive: {tBlock_Error_Swivel}");
                OnPropertyChanged(nameof(TBlock_Error_Swivel));
            }
        }

        #endregion



        private string tBox_speed;
        public string TBox_speed
        {
            get
            {
                return tBox_speed;
            }
            set
            {
                tBox_speed = value;
                Debug.WriteLine($"[Motor] Drive Speed: {tBox_speed}");
                OnPropertyChanged(nameof(TBox_speed));
            }
        }

        private ObservableCollection<string> vmcomboBoxPortNo = new ObservableCollection<string>();
        public ObservableCollection<string> VMcomboBoxPortNo
        {
            get
            {
                return vmcomboBoxPortNo;
            }
            set
            {
                vmcomboBoxPortNo = value;
                OnPropertyChanged(nameof(VMcomboBoxPortNo));
            }
        }

        private string selectedcomboBoxPortNo;
        public string SelectedcomboBoxPortNo
        {
            get
            {
                return selectedcomboBoxPortNo;
            }
            set
            {
                selectedcomboBoxPortNo = value;
                Debug.WriteLine($"{selectedcomboBoxPortNo} is Selected");
                OnPropertyChanged(nameof(SelectedcomboBoxPortNo));
            }
        }

        private bool isMotorConnected;
        public bool IsMotorConnected
        {
            get
            {
                return isMotorConnected;
            }
            set
            {
                isMotorConnected = value;
                OnPropertyChanged(nameof(IsMotorConnected));
            }
        }

        private bool isServoON;
        public bool IsServoON
        {
            get
            {
                return isServoON;
            }
            set
            {
                isServoON = value;
                OnPropertyChanged(nameof(IsServoON));
            }
        }
    }
}
