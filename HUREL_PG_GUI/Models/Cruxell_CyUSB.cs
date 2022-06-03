using CyUSB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Concurrent;

namespace MSPGC_GUI.Model
{
    public partial class CRUXELLMSPGC
    {
        //public bool is_measuring;
        //------------------------------+---------------------------------------------------------------
        // [변수] 재린 추가 변수
        //------------------------------+---------------------------------------------------------------
        public bool Is_measuring;
        public List<DeviceInfo> DeviceList = new List<DeviceInfo>();
        public DeviceInfo SelectedDevice;
        public class DeviceInfo
        {
            public USBDevice Device { get; set; }
            public CyUSBDevice CyDevice { get; set; }
            public string DeviceName { get; set; }
        }
        //private string PpxInfo = "0";
        //private string QueueInfo = "0";

        //private string PpxInfo = "128";
        //private string QueueInfo = "256";

        private string PpxInfo = "16";
        private string QueueInfo = "256";

        private List<string> EndPointList = new List<string>();

        public double[] chart_Slit1 = new double[36];
        public double[] chart_Slit2 = new double[36];

        public double[] chart_Scintillator1 = new double[36];
        public double[] chart_Scintillator2 = new double[36];
        public double[] chart_Scintillator3 = new double[36];
        public double[] chart_Scintillator4 = new double[36];

        public double[] chart_Slit1_pre = new double[36];
        public double[] chart_Slit2_pre = new double[36];

        public double[] chart_Scintillator1_pre = new double[36];
        public double[] chart_Scintillator2_pre = new double[36];
        public double[] chart_Scintillator3_pre = new double[36];
        public double[] chart_Scintillator4_pre = new double[36];

        public double[] chart_PGdistribution = new double[72];

        public double[] chart_PGdistribution_avg = new double[72];

        //public double[] chart_PGdistribution_sum = new double[72];
        public double[] chart_PGdistribution_sum = new double[71];

        private CyUSB.USBDeviceList UsbDevices;
        private CyUSBEndPoint EndPoint;
        private int endPointListSelectIdx;

        // CountMode countMode;




        private int EndPointListSelectIdx
        {
            get
            {
                return endPointListSelectIdx;
            }
            set
            {
                endPointListSelectIdx = value;
                string sAlt = EndPointList[value].Substring(4, 1);
                byte a = Convert.ToByte(sAlt);
                SelectedDevice.CyDevice.AltIntfc = a;

                // Get the endpoint
                int aX = EndPointList[value].LastIndexOf("0x");
                string sAddr = EndPointList[value].Substring(aX, 4);
                byte addr = (byte)Util.HexToInt(sAddr);

                EndPoint = SelectedDevice.CyDevice.EndPointOf(addr);

                // Ensure valid PPX for this endpoint
                Ppx_SelectedIndexChanged();
            }
        }






        public const int VALID_DATA_BYTES = (336 + 0);//20201207 wj  
        public const int default_trash_count = 0;//20201207 wj
        public int trash_count = default_trash_count;// 최초 3라인 쓰레기가 섞여서 버려달라는 요청 20201207
        //------------------------------+---------------------------------------------------------------
        // [변수] USB 관련 변수
        //------------------------------+---------------------------------------------------------------
        bool bVista;

        //USBDeviceList usbDevices;
        //CyUSBDevice MyDevice;
        //CyUSBEndPoint EndPoint;



        DateTime t1, t2;
        TimeSpan elapsed;
        double XferBytes;
        long xferRate;
        static byte DefaultBufInitValue = 0xA5;

        int BufSz;
        int QueueSz;
        int PPX;
        int IsoPktBlockSize;
        int Successes;
        int Failures;

        Task tListen;
        Task tParsing;
        Task tConvert;
        static bool bRunning;
        static int bFinalCall;
        int b_first_recevied;
        // These are  needed for Thread to update the UI
        delegate void UpdateUICallback();
        UpdateUICallback updateUI;

        // These are needed to close the app from the Thread exception(exception handling)
        delegate void ExceptionCallback();
        ExceptionCallback handleException;

        public int tb_0x11, tb_0x12, tb_0x13, tb_0x14, tb_0x15, tb_0x16, tb_0x17, tb_0x18, tb_0x19, tb_0x1a;
        // ===================== 2022-05-27 Checked ===================== //
        // tb_0x11: Count Mode (0: Continuous mode & 1: TRIG1 & 2: TRIG2)
        // tb_0x12: Trig Vref (mV)
        // tb_0x13: Interval time (x10us)
        // tb_0x14: TRIG 1  Pre - margin
        // tb_0x15: TRIG 1 Post - margin
        // tb_0x16: TRIG 2  Pre - margin
        // tb_0x17: TRIG 2 Post - margin
        // tb_0x18: TRIG ADC rate


        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 변수
        //------------------------------+---------------------------------------------------------------
        private void init_USB()
        {
            #region 미사용: bVista, updateUI(F)
            // Form 로드시 컴포넌트 초기화
            //if (EndPointsComboBox.Items.Count > 0)
            //    EndPointsComboBox.SelectedIndex = 0;

            //bVista = (Environment.OSVersion.Version.Major < 6) ||
            //((Environment.OSVersion.Version.Major == 6) && Environment.OSVersion.Version.Minor == 0);
            //// Setup the callback routine for updating the UI
            //updateUI = new UpdateUICallback(StatusUpdate);
            #endregion

            // Setup the callback routine for NullReference exception handling
            handleException = new ExceptionCallback(ThreadException);

            // Create the list of USB devices attached to the CyUSB3.sys driver.
            try
            {
                UsbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
            }
            catch { }
            SelectedDevice = new DeviceInfo();

            //Assign event handlers for device attachment and device removal.
            UsbDevices.DeviceAttached += new EventHandler(usbDevices_DeviceAttached);
            UsbDevices.DeviceRemoved += new EventHandler(usbDevices_DeviceRemoved);

            //Set and search the device with VID-PID 04b4-1003 and if found, selects the end point
            SetDevice(false);
        }
        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | The callback routine delegated to updateUI.
        //------------------------------+---------------------------------------------------------------
        public void StatusUpdate()
        {
            #region 굳이 필요 없을 것 같음
            //////if (bRunning == false) return;
            //////if (xferRate > ProgressBar.Maximum)
            //////    ProgressBar.Maximum = (int)(xferRate * 1.25);

            //ProgressBar.Value = (int)xferRate;
            //ThroughputLabel.Text = ProgressBar.Value.ToString();

            //LB_Receive.Text = test_data.ToString();
            //LB_size2.Text = test_buffer.Count().ToString();
            // LB_timeout_error.Text = $"{bRunning} / {bFinalCall.ToString()}";

            //SuccessBox.Text = Successes.ToString();
            //FailuresBox.Text = Failures.ToString();
            //LB_head.Text = p_head.ToString();
            //LB_tail.Text = p_tail.ToString();
            //LB_error.Text = p_error.ToString();
            #endregion
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | The callback routine delegated to handleException.
        //------------------------------+---------------------------------------------------------------
        public void ThreadException()
        {
            bRunning = false;

            t2 = DateTime.Now;
            elapsed = t2 - t1;

            tListen = null;

            #region 미사용: StartBtn.Text, xferRate
            //StartBtn.Text = "Start";
            //xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
            //xferRate = xferRate / (int)100 * (int)100;
            #endregion
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | This is the event handler for device attachment. This method  searches for the device with 
        //                        VID-PID 04b4-00F1
        //------------------------------+---------------------------------------------------------------
        void usbDevices_DeviceAttached(object sender, EventArgs e)
        {
            SetDevice(false);
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | This is the event handler for device removal. This method resets the device count and searches for the device with 
        //                        VID-PID 04b4-1003
        //------------------------------+---------------------------------------------------------------
        void usbDevices_DeviceRemoved(object sender, EventArgs e)
        {
            bRunning = false;

            if (tListen != null /*&& tListen.IsAlive == true*/)
            {
                tListen.Wait();
            }

            //MyDevice = null;
            //EndPoint = null;
            //SetDevice(false);
            SelectedDevice = new DeviceInfo(); ;
            EndPoint = null;
            DeviceList.Clear();
            SetDevice(false);


            //if (StartBtn.Text.Equals("Start") == false)
            if (Is_measuring == false)
            {
                {
                    bRunning = false;

                    t2 = DateTime.Now;
                    elapsed = t2 - t1;

                    #region 미사용: DevicesComboBox, EndPointsComboBox, PpxBox, QueueBox, StartBtn, xferRate
                    //DevicesComboBox.Enabled = true;
                    //EndPointsComboBox.Enabled = true;
                    //PpxBox.Enabled = true;
                    //QueueBox.Enabled = true;
                    //StartBtn.Text = "Start";

                    //xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                    //xferRate = xferRate / (int)100 * (int)100;

                    //StartBtn.BackColor = Color.Aquamarine;
                    #endregion
                }
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | Search the device with VID-PID 04b4-00F1 and if found, select the end point
        //------------------------------+---------------------------------------------------------------   
        private void SetDevice(bool bPreserveSelectedDevice)
        {
            #region 미사용: DevicesComboBox에 여러개가 있을 경우 조정해주는 부분
            //int nCurSelection = 0;
            //if (DevicesComboBox.Items.Count > 0)
            //{
            //    nCurSelection = DevicesComboBox.SelectedIndex;
            //    DevicesComboBox.Items.Clear();
            //}
            #endregion

            int nDeviceList = UsbDevices.Count;
            for (int nCount = 0; nCount < nDeviceList; nCount++)
            {
                USBDevice fxDevice = UsbDevices[nCount];
                String strmsg;
                strmsg = "(0x" + fxDevice.VendorID.ToString("X4") + " - 0x" + fxDevice.ProductID.ToString("X4") + ") " + fxDevice.FriendlyName;
                // 추가 start
                DeviceInfo deviceInfo = new DeviceInfo { Device = fxDevice, DeviceName = strmsg };
                // 추가 end
                //DevicesComboBox.Items.Add(strmsg);
                DeviceList.Add(deviceInfo);
            }

            //if (DevicesComboBox.Items.Count > 0)
            //    DevicesComboBox.SelectedIndex = ((bPreserveSelectedDevice == true) ? nCurSelection : 0);
            if (DeviceList.Count > 0)
                SelectedDevice = ((bPreserveSelectedDevice == true) ? SelectedDevice : DeviceList[0]);

            //USBDevice dev = usbDevices[DevicesComboBox.SelectedIndex];
            USBDevice dev = SelectedDevice.Device;

            #region if (dev != null) 부분 예전 작성 코드
            //if (dev != null)
            //{
            //    MyDevice = (CyUSBDevice)dev;

            //    GetEndpointsOfNode(MyDevice.Tree);
            //    PpxBox.Text = "16"; //Set default value to 8 Packets
            //    QueueBox.Text = "128"; //128
            //    if (EndPointsComboBox.Items.Count > 0)
            //    {
            //        EndPointsComboBox.SelectedIndex = 0;
            //        StartBtn.Enabled = true;
            //    }
            //    else StartBtn.Enabled = false;

            //    // Text = MyDevice.FriendlyName;
            //}
            //else
            //{
            //    StartBtn.Enabled = false;
            //    EndPointsComboBox.Items.Clear();
            //    EndPointsComboBox.Text = "";
            //    // Text = "C# Streamer - no device";
            //}
            #endregion

            if (dev != null)
            {
                SelectedDevice.CyDevice = (CyUSBDevice)dev;

                GetEndpointsOfNode(SelectedDevice.CyDevice.Tree);
                //--- 국립암센터 실험 셋업(2021-03-27) ---//
                PpxInfo = "16"; //Set default value to 8 Packets
                QueueInfo = "128"; //128

                //PpxInfo = "128"; //Set default value to 8 Packets
                //QueueInfo = "256"; //128
                if (EndPointList.Count > 0)
                {
                    EndPointListSelectIdx = 0;
                }
            }
            else
            {
                EndPointList.Clear();
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | Recursive routine populates EndPointsComboBox with strings 
        //                        representing all the endpoints in the device.
        //------------------------------+---------------------------------------------------------------   
        private void GetEndpointsOfNode(TreeNode devTree)
        {
            //EndPointsComboBox.Items.Clear(); // 이거 주석되있어서 계속추가되는 버그있던데, 예제소스가 왜그런진 모르겠음
            EndPointList.Clear(); // 이거 주석되있어서 계속추가되는 버그있던데, 예제소스가 왜그런진 모르겠음
            foreach (TreeNode node in devTree.Nodes)
            {
                if (node.Nodes.Count > 0)
                    GetEndpointsOfNode(node);
                else
                {
                    CyUSBEndPoint ept = node.Tag as CyUSBEndPoint;
                    if (ept == null)
                    {
                        //return;
                    }
                    else if (!node.Text.Contains("Control"))
                    {
                        CyUSBInterface ifc = node.Parent.Tag as CyUSBInterface;
                        string s = string.Format("ALT-{0}, {1} Byte {2}", ifc.bAlternateSetting, ept.MaxPktSize, node.Text);
                        //EndPointsComboBox.Items.Add(s);
                        EndPointList.Add(s);
                    }
                }
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | This is the System event handler.
        //                        Enforces valid values for PPX(Packet per transfer)
        //------------------------------+---------------------------------------------------------------
        //private void PpxBox_SelectedIndexChanged(object sender, EventArgs e)
        private void Ppx_SelectedIndexChanged()
        {
            if (EndPoint == null) return;

            //int ppx = Convert.ToUInt16(PpxBox.Text);
            //int len = EndPoint.MaxPktSize * ppx;
            int ppx = PPX;
            int len = EndPoint.MaxPktSize * ppx;

            int maxLen = 0x400000; // 4MBytes
            if (len > maxLen)
            {
                //ppx = maxLen / (EndPoint.MaxPktSize) / 8 * 8;
                if (EndPoint.MaxPktSize == 0)
                {
                    MessageBox.Show("Please correct MaxPacketSize in Descriptor", "Invalid MaxPacketSize");
                    return;
                }
                ppx = maxLen / (EndPoint.MaxPktSize);
                ppx -= (ppx % 8);
                MessageBox.Show("Maximum of 4MB per transfer.  Packets reduced.", "Invalid Packets per Xfer.");

                //Update the DropDown list for the packets
                //int iIndex = PpxBox.SelectedIndex; // Get the packet index
                int iIndex = PPX; // Get the packet index

                #region 미사용: PpxBox의 값을 바꿔주는 함수, 고정된 값을 사용할 것
                //PpxBox.Items.Remove(PpxBox.Text); // Remove the Existing  Packet index
                //PpxBox.Items.Insert(iIndex, ppx.ToString()); // insert the ppx
                //PpxBox.SelectedIndex = iIndex; // update the selected item index
                #endregion

            }

            #region if ((MyDevice.bSuperSpeed || MyDevice.bHighSpeed) && (EndPoint.Attributes == 1) && (ppx < 8)) 예전 작성 코드
            //if ((MyDevice.bSuperSpeed || MyDevice.bHighSpeed) && (EndPoint.Attributes == 1) && (ppx < 8))
            //{
            //    PpxBox.Text = "8";
            //    MessageBox.Show("Minimum of 8 Packets per Xfer required for HS/SS Isoc.", "Invalid Packets per Xfer.");
            //}
            //if ((MyDevice.bHighSpeed) && (EndPoint.Attributes == 1))
            //{
            //    if (ppx > 128)
            //    {
            //        PpxBox.Text = "128";
            //        MessageBox.Show("Maximum 128 packets per transfer for High Speed Isoc", "Invalid Packets per Xfer.");
            //    }
            //}
            #endregion

            if ((SelectedDevice.CyDevice.bSuperSpeed || SelectedDevice.CyDevice.bHighSpeed) && (EndPoint.Attributes == 1) && (ppx < 8))
            {
                PPX = 8;
                MessageBox.Show("Minimum of 8 Packets per Xfer required for HS/SS Isoc.", "Invalid Packets per Xfer.");
            }
            if ((SelectedDevice.CyDevice.bHighSpeed) && (EndPoint.Attributes == 1))
            {
                if (ppx > 128)
                {
                    PPX = 128;
                    MessageBox.Show("Maximum 128 packets per transfer for High Speed Isoc", "Invalid Packets per Xfer.");
                }
            }
        }

        private void DeviceComboBox_SelectedIndexChanged(object sender, EventArgs e) // 콤보
        {
            #region WinForm 메인 윈도우에서 디바이스 콤보박스가 변경될떄마다 이벤트 발생하는 부분 -> MainViewModel에서 바뀌는 것 감지하는 것 // 또는 없애는 것으로 합의
            //MyDevice = null;
            //EndPoint = null;
            //SetDevice(true);
            #endregion
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | This is a system event handler, when the selected index changes(end point selection).
        //------------------------------+---------------------------------------------------------------        
        private void EndPointsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            #region 미사용: WinForm 메인 윈도우에서 콤보박스들 값 변경시 발생하는 이벤트와 관련된 부분 -> MainViewModel쪽에서 인식하는 방향으로 코드 수정
            //// Get the Alt setting
            //string sAlt = EndPointsComboBox.Text.Substring(4, 1);
            //byte a = Convert.ToByte(sAlt);
            //MyDevice.AltIntfc = a;

            //// Get the endpoint
            //int aX = EndPointsComboBox.Text.LastIndexOf("0x");
            //string sAddr = EndPointsComboBox.Text.Substring(aX, 4);
            //byte addr = (byte)Util.HexToInt(sAddr);

            //EndPoint = MyDevice.EndPointOf(addr);

            //// Ensure valid PPX for this endpoint
            //PpxBox_SelectedIndexChanged(sender, null);
            #endregion
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] START버튼 눌렀을 시 USB start or stop 관련 작업
        //------------------------------+---------------------------------------------------------------
        private void reset_chart()
        {
            #region reset_chart() 예전 작성 코드
            //// 차트 초기화
            //for (int i = 0; i < 120; ++i)
            //{
            //    chart2_data1[i] = 0;// rand.Next(500);
            //}
            //for (int i = 0; i < 72; ++i)
            //{
            //    chart3_data[i] = 0;//rand.Next(500);
            //    chart4_data[i] = 0;//rand.Next(500);
            //}
            //for (int i = 0; i < 144; ++i)
            //{
            //    chart1_data[i] = 0;//rand.Next(500);
            //}
            //chart1.Series[0].Points.DataBindY(chart1_data);
            //chart2.Series[0].Points.DataBindY(chart2_data1);
            //chart3.Series[0].Points.DataBindY(chart3_data);
            //chart4.Series[0].Points.DataBindY(chart4_data);
            #endregion

            for (int i = 0; i < 36; ++i)
            {
                chart_Slit1[i] = 0;          //rand.Next(500);
                chart_Slit1[i] = 0;          //rand.Next(500);
            }
            for (int i = 0; i < 36; ++i)
            {
                chart_Scintillator1[i] = 0;  //rand.Next(500);
                chart_Scintillator2[i] = 0;  //rand.Next(500);
                chart_Scintillator3[i] = 0;  //rand.Next(500);
                chart_Scintillator4[i] = 0;  //rand.Next(500);                
            }
            for (int i = 0; i < 72; ++i)
            {
                chart_PGdistribution[i] = 0;  //rand.Next(500); 
            }
        }



        private void get_data_from_box() // 이게 필요한가에 대한 의문점이 듦. 이미 MainViewModel에서 바로 Binding되어서 무슨 측정 모드(Continue, Trig1, Trig2)인지 알고 있을텐데
        {
            #region get_data_from_box() 예전 작성 코드
            //if (RB_continue.Checked == true)
            //    tb_0x11 = 1;
            //else if (RB_trig1.Checked == true)
            //    tb_0x11 = 2;
            //else
            //    tb_0x11 = 3;

            //int.TryParse(TB_0x12.Text, out tb_0x12);
            //int.TryParse(TB_0x13.Text, out tb_0x13);
            //int.TryParse(TB_0x14.Text, out tb_0x14);
            //int.TryParse(TB_0x15.Text, out tb_0x15);
            //int.TryParse(TB_0x16.Text, out tb_0x16);
            //int.TryParse(TB_0x17.Text, out tb_0x17);
            //int.TryParse(TB_0x18.Text, out tb_0x18);
            #endregion

            //if (countMode == CountMode.Continue)
            //    tb_0x11 = 1;
            //else if (countMode == CountMode.TRIG1)
            //    tb_0x11 = 2;
            //else
            //    tb_0x11 = 3;

            tb_0x19 = 1;
            tb_0x1a = 1;
        }

        private void usb_setting(int flag)
        {
            SetOutputData(flag);
        }

        public void start_stop_usb()
        {
            // start 눌렀을 때
            #region intput, output에 status를 사용할 경우            
            //if (!IsVariablesSet)
            //{
            //    status = "Variable is not set";
            //    return false;
            //}


            //// 사장님 - Start 눌렀을때
            //if (SelectedDevice.DeviceName == "")
            //{
            //    Trace.WriteLine("No selected Devices");
            //    status = "No selected Devices";
            //    return false;
            //}


            //if (QueueInfo == 0)
            //{
            //    Trace.WriteLine("Please Select Xfers to Queue Invalid Input");
            //    status = "Please Select Xfers to Queue Invalid Input";
            //    return false;
            //}

            //if (!IsVariablesSet)
            //{
            //    Trace.WriteLine("Please Set Variables");
            //    status = "Please Set Variables";
            //    return false;
            //}
            #endregion



            if (SelectedDevice.DeviceName == "")
            {
                Trace.WriteLine("No selected Devices");
                return;
            }

            if (QueueInfo == "")
            {
                Trace.WriteLine("Please Select Xfers to Queue Invalid Input");
                return;
            }


            if (Is_measuring == false)
            {
                // reset chart
                trash_count = default_trash_count;   //reset
                reset_chart();

                // Test GetData from box
                get_data_from_box(); // necessary?
                timer_start();

                Trace.WriteLine("HY : Start");

                // 1. 입력칸 비활성화 및 설정
                Trace.WriteLine("HY : [Try] input_disable ");
                input_disable(false);

                byte[] Item;
                while (test_buffer.TryTake(out Item, 1))
                {
                }

                Trace.WriteLine("HY : [Try] Send 0000... ");
                usb_setting(3); // send 00000

                // 2. 버퍼 비우기 
                Trace.WriteLine("HY : [Try] XferData reset loop");
                EndPointListSelectIdx = 1;
                EndPoint.TimeOut = 500;

                bool bResult = true;
                int xferLen = 4096;
                byte[] inData = new byte[xferLen];
                while (bResult)
                {
                    bResult = EndPoint.XferData(ref inData, ref xferLen);
                }
                EndPoint.TimeOut = 500;

                Trace.WriteLine("HY : [Try] send setting value");
                // 3. 셋팅값 전송
                usb_setting(1);

                // 4. 데이터 Read
                //EndPointsComboBox.SelectedIndex = 1;
                EndPointListSelectIdx = 1;

                BufSz = EndPoint.MaxPktSize * Convert.ToUInt16(PpxInfo);
                p_error = EndPoint.MaxPktSize; // 16384
                QueueSz = Convert.ToUInt16(QueueInfo); // 값 1로 고정
                PPX = Convert.ToUInt16(PpxInfo);

                EndPoint.XferSize = BufSz;
                Trace.WriteLine($"HY : [Try] BufSz[{BufSz}]QueueSz[{QueueSz}]MaxPktSize[{EndPoint.MaxPktSize}]");
                if (EndPoint is CyIsocEndPoint)
                    IsoPktBlockSize = (EndPoint as CyIsocEndPoint).GetPktBlockSize(BufSz);
                else
                    IsoPktBlockSize = 0;

                bRunning = true;
                bFinalCall = 0;
                thread_run = true;

                Trace.WriteLine("HY : [Try] Start XferThread");
                b_first_recevied = 0;
                tListen = new Task(new Action(XferThread));
                tListen.Start();
                Trace.WriteLine("HY : [Try] Start ParsingThread");

                Is_measuring = true;

                init_bin_save_dir();
                tParsing = new Task(new Action(ParsingThread));
                tParsing.Start();
            }
            else
            {
                Trace.WriteLine("HY : [Try] Stop Button Clicked / send Final Call");
                bFinalCall = 2;
                bRunning = false; // 20.04.22 test

                // usb단
                t2 = DateTime.Now;
                elapsed = t2 - t1;
                xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                xferRate = xferRate / (int)100 * (int)100;

                Trace.WriteLine("HY : [Try] tListen.Wait()");
                tListen.Wait();
                tListen = null;

                Trace.WriteLine("HY : [Try] tParsing.Wait()");
                thread_run = false;
                tParsing.Wait();
                tParsing = null;

                Trace.WriteLine("HY : [Try] send stop 0000...");
                usb_setting(3); // 모든처리 끝났을때 stop

                DATA_BUFFER_read_count = 0;
                read_buffer_count = 0;

                terminate_file_save();
                Trace.WriteLine("HY : [Try] input_disable enabled");
                input_disable(true);    // 입력칸 enable
                timer_end();
                b_first_recevied = 99;

                Is_measuring = false;

                //var kk = MainViewModel.SpotScanningDatas.NCCSpots[0].LogStartEpochTime - CRUXELLMSPGC.FPGADatas[0].StartEpochTime;
                //Debug.WriteLine($"[[[[[[[[[[[[[[JJR]]]]]]]]]]]]]]]]]]] Time Gap: {kk.TotalMilliseconds}, {kk}");
            }
        }


        //public bool Start_usb(out string status) - 세훈이가 짜는 방식
        //private void start_stop_usb()
        public bool Command_MonitoringStart(out string status)
        {
            #region PG distribution corrFactors           

            //using (FileStream fs = new FileStream(corrFactorsDirectory, FileMode.Open))
            //{
            //    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            //    {
            //        string lines = null;
            //        string[] tempString = null;

            //        while ((lines = sr.ReadLine()) != null)
            //        {
            //            tempString = lines.Split("\t");
            //            double[] tempcorrFactors = new double[36];

            //            for (int i = 0; i < 36; i++)
            //            {
            //                tempcorrFactors[i] = Convert.ToDouble(tempString[i]);
            //            }
            //            corrFactors.Add(tempcorrFactors);
            //        }
            //    }
            //}

            #endregion

            if (SelectedDevice.DeviceName == "")
            {
                Trace.WriteLine("No selected Devices");
                status = "No selected Devices";
                return false;
            }

            if (Is_measuring == false)
            {
                status = "DAQ Setting...";

                trash_count = default_trash_count;
                // Command_Reset_PGdist();
                //timer_start();

                Trace.WriteLine("HY : Start");

                // 1. 입력칸 비활성화 및 설정
                Trace.WriteLine("HY : [Try] input_disable ");
                input_disable(false);

                byte[] Item;
                while (test_buffer.TryTake(out Item, 1))
                {
                }

                Trace.WriteLine("HY : [Try] Send 0000... ");
                usb_setting(3); // send 00000

                // 2. 버퍼 비우기 
                Trace.WriteLine("HY : [Try] XferData reset loop");
                EndPointListSelectIdx = 1;
                EndPoint.TimeOut = 500;

                bool bResult = true;
                int xferLen = 4096;
                byte[] inData = new byte[xferLen];
                while (bResult)
                {
                    bResult = EndPoint.XferData(ref inData, ref xferLen);
                }
                EndPoint.TimeOut = 500;

                Trace.WriteLine("HY : [Try] send setting value");
                // 3. 셋팅값 전송
                usb_setting(1);

                // 4. 데이터 Read
                EndPointListSelectIdx = 1;

                BufSz = EndPoint.MaxPktSize * Convert.ToUInt16(PpxInfo);
                p_error = EndPoint.MaxPktSize; // 16384
                QueueSz = Convert.ToUInt16(QueueInfo); // 값 1로 고정
                PPX = Convert.ToUInt16(PpxInfo);

                EndPoint.XferSize = BufSz;
                Trace.WriteLine($"HY : [Try] BufSz[{BufSz}]QueueSz[{QueueSz}]MaxPktSize[{EndPoint.MaxPktSize}]");
                if (EndPoint is CyIsocEndPoint)
                    IsoPktBlockSize = (EndPoint as CyIsocEndPoint).GetPktBlockSize(BufSz);
                else
                    IsoPktBlockSize = 0;

                bRunning = true;
                bFinalCall = 0;
                thread_run = true;

                Trace.WriteLine("HY : [Try] Start XferThread");
                b_first_recevied = 0;
                tListen = new Task(new Action(XferThread));
                tListen.Start();
                Trace.WriteLine("HY : [Try] Start ParsingThread");

                Is_measuring = true;

                init_bin_save_dir();
                tParsing = new Task(new Action(ParsingThread));
                tParsing.Start();

                status = "Data Acquisition Start!";
                return true;
            }
            else
            {
                Trace.WriteLine("HY : [Try] Stop Button Clicked / send Final Call");
                bFinalCall = 2;
                bRunning = false; // 20.04.22 test

                // usb단
                t2 = DateTime.Now;
                elapsed = t2 - t1;
                xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                xferRate = xferRate / (int)100 * (int)100;

                Trace.WriteLine("HY : [Try] tListen.Wait()");
                tListen.Wait();
                tListen = null;

                Trace.WriteLine("HY : [Try] tParsing.Wait()");
                thread_run = false;
                tParsing.Wait();
                tParsing = null;

                Trace.WriteLine("HY : [Try] send stop 0000...");
                usb_setting(3); // 모든처리 끝났을때 stop

                DATA_BUFFER_read_count = 0;
                read_buffer_count = 0;

                terminate_file_save();
                Trace.WriteLine("HY : [Try] input_disable enabled");
                input_disable(true);    // 입력칸 enable
                //timer_end();
                b_first_recevied = 99;

                Is_measuring = false;

                status = "Idle";
                return false;
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | Data Xfer Thread entry point. Starts the thread on Start Button click 
        //------------------------------+---------------------------------------------------------------
        public unsafe void XferThread()
        {
            // Setup the queue buffers
            byte[][] cmdBufs = new byte[QueueSz][];
            byte[][] xferBufs = new byte[QueueSz][];
            byte[][] ovLaps = new byte[QueueSz][];
            ISO_PKT_INFO[][] pktsInfo = new ISO_PKT_INFO[QueueSz][];

            //int xStart = 0;

            //////////////////////////////////////////////////////////////////////////////
            ///////////////Pin the data buffer memory, so GC won't touch the memory///////
            //////////////////////////////////////////////////////////////////////////////

            GCHandle cmdBufferHandle = GCHandle.Alloc(cmdBufs[0], GCHandleType.Pinned);
            GCHandle xFerBufferHandle = GCHandle.Alloc(xferBufs[0], GCHandleType.Pinned);
            GCHandle overlapDataHandle = GCHandle.Alloc(ovLaps[0], GCHandleType.Pinned);
            GCHandle pktsInfoHandle = GCHandle.Alloc(pktsInfo[0], GCHandleType.Pinned);

            try
            {
                LockNLoad(cmdBufs, xferBufs, ovLaps, pktsInfo);
            }
            catch (NullReferenceException e)
            {
                // This exception gets thrown if the device is unplugged 
                // while we're streaming data
                e.GetBaseException();
                //this.Invoke(handleException);  /////////////////// invoke
            }

            //////////////////////////////////////////////////////////////////////////////
            ///////////////Release the pinned memory and make it available to GC./////////
            //////////////////////////////////////////////////////////////////////////////
            cmdBufferHandle.Free();
            xFerBufferHandle.Free();
            overlapDataHandle.Free();
            pktsInfoHandle.Free();
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 |
        // This is a recursive routine for pinning all the buffers used in the transfer in memory.
        // It will get recursively called QueueSz times.On the QueueSz_th call, it will call
        // XferData, which will loop, transferring data, until the stop button is clicked.
        // Then, the recursion will unwind.
        //------------------------------+---------------------------------------------------------------
        public unsafe void LockNLoad(byte[][] cBufs, byte[][] xBufs, byte[][] oLaps, ISO_PKT_INFO[][] pktsInfo)
        {
            int j = 0;
            int nLocalCount = j;

            GCHandle[] bufSingleTransfer = new GCHandle[QueueSz];
            GCHandle[] bufDataAllocation = new GCHandle[QueueSz];
            GCHandle[] bufPktsInfo = new GCHandle[QueueSz];
            GCHandle[] handleOverlap = new GCHandle[QueueSz];

            while (j < QueueSz)
            {
                // Allocate one set of buffers for the queue, Buffered IO method require user to allocate a buffer as a part of command buffer,
                // the BeginDataXfer does not allocated it. BeginDataXfer will copy the data from the main buffer to the allocated while initializing the commands.
                cBufs[j] = new byte[CyConst.SINGLE_XFER_LEN + IsoPktBlockSize + ((EndPoint.XferMode == XMODE.BUFFERED) ? BufSz : 0)];

                xBufs[j] = new byte[BufSz];

                //initialize the buffer with initial value 0xA5
                for (int iIndex = 0; iIndex < BufSz; iIndex++)
                    xBufs[j][iIndex] = DefaultBufInitValue;

                int sz = Math.Max(CyConst.OverlapSignalAllocSize, sizeof(OVERLAPPED));
                oLaps[j] = new byte[sz];
                pktsInfo[j] = new ISO_PKT_INFO[PPX];

                /*/////////////////////////////////////////////////////////////////////////////
                 * 
                 * fixed keyword is getting thrown own by the compiler because the temporary variables 
                 * tL0, tc0 and tb0 aren't used. And for jagged C# array there is no way, we can use this 
                 * temporary variable.
                 * 
                 * Solution  for Variable Pinning:
                 * Its expected that application pin memory before passing the variable address to the
                 * library and subsequently to the windows driver.
                 * 
                 * Cypress Windows Driver is using this very same memory location for data reception or
                 * data delivery to the device.
                 * And, hence .Net Garbage collector isn't expected to move the memory location. And,
                 * Pinning the memory location is essential. And, not through FIXED keyword, because of 
                 * non-usability of temporary variable.
                 * 
                /////////////////////////////////////////////////////////////////////////////*/
                //fixed (byte* tL0 = oLaps[j], tc0 = cBufs[j], tb0 = xBufs[j])  // Pin the buffers in memory
                //////////////////////////////////////////////////////////////////////////////////////////////
                bufSingleTransfer[j] = GCHandle.Alloc(cBufs[j], GCHandleType.Pinned);
                bufDataAllocation[j] = GCHandle.Alloc(xBufs[j], GCHandleType.Pinned);
                bufPktsInfo[j] = GCHandle.Alloc(pktsInfo[j], GCHandleType.Pinned);
                handleOverlap[j] = GCHandle.Alloc(oLaps[j], GCHandleType.Pinned);
                // oLaps "fixed" keyword variable is in use. So, we are good.
                /////////////////////////////////////////////////////////////////////////////////////////////            

                unsafe
                {
                    //fixed (byte* tL0 = oLaps[j])
                    {
                        CyUSB.OVERLAPPED ovLapStatus = new CyUSB.OVERLAPPED();
                        ovLapStatus = (CyUSB.OVERLAPPED)Marshal.PtrToStructure(handleOverlap[j].AddrOfPinnedObject(), typeof(CyUSB.OVERLAPPED));
                        ovLapStatus.hEvent = (IntPtr)PInvoke.CreateEvent(0, 0, 0, 0);
                        Marshal.StructureToPtr(ovLapStatus, handleOverlap[j].AddrOfPinnedObject(), true);

                        // Pre-load the queue with a request
                        int len = BufSz;
                        if (EndPoint.BeginDataXfer(ref cBufs[j], ref xBufs[j], ref len, ref oLaps[j]) == false)
                            Failures = 5;// Failures++;
                    }
                    j++;
                }
            }

            XferData(cBufs, xBufs, oLaps, pktsInfo, handleOverlap);          // All loaded. Let's go!

            unsafe
            {
                Trace.WriteLine("HY : [Try] TryTake loop(test_buffer reset)");
                while (true)
                {
                    if (test_buffer.Count() != 0)
                        Thread.Sleep(0);
                    else
                        break;
                }

                for (nLocalCount = 0; nLocalCount < QueueSz; nLocalCount++)
                {
                    CyUSB.OVERLAPPED ovLapStatus = new CyUSB.OVERLAPPED();
                    ovLapStatus = (CyUSB.OVERLAPPED)Marshal.PtrToStructure(handleOverlap[nLocalCount].AddrOfPinnedObject(), typeof(CyUSB.OVERLAPPED));
                    PInvoke.CloseHandle(ovLapStatus.hEvent);

                    /*////////////////////////////////////////////////////////////////////////////////////////////
                     * 
                     * Release the pinned allocation handles.
                     * 
                    ////////////////////////////////////////////////////////////////////////////////////////////*/
                    bufSingleTransfer[nLocalCount].Free();
                    bufDataAllocation[nLocalCount].Free();
                    bufPktsInfo[nLocalCount].Free();
                    handleOverlap[nLocalCount].Free();

                    cBufs[nLocalCount] = null;
                    xBufs[nLocalCount] = null;
                    oLaps[nLocalCount] = null;
                }
            }
            GC.Collect();
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | Called at the end of recursive method, LockNLoad().
        //                        XferData() implements the infinite transfer loop
        //------------------------------+---------------------------------------------------------------

        public static void Fill<T>(T[] array, T value)
        {

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }
        #region XferData
        //public unsafe void XferData(byte[][] cBufs, byte[][] xBufs, byte[][] oLaps, ISO_PKT_INFO[][] pktsInfo, GCHandle[] handleOverlap)
        //{
        //    Trace.WriteLine($"HY :start XferData!!!");
        //    int k = 0;
        //    int len = 0;
        //    int pre_successes = 0;

        //    Successes = 0;
        //    Failures = 0;

        //    XferBytes = 0;
        //    t1 = DateTime.Now;
        //    long nIteration = 0;
        //    //long lFailure = 0;
        //    CyUSB.OVERLAPPED ovData = new CyUSB.OVERLAPPED();
        //    // USB Receive Loop
        //    int valid_count = 0;
        //    bool[] valid_indexs = new bool[16];
        //    bool is_not_oxA5 = false;
        //    for (; bRunning;)
        //    {
        //        nIteration++;
        //        // WaitForXfer
        //        unsafe
        //        {
        //            //fixed (byte* tmpOvlap = oLaps[k])
        //            {
        //                //                        Trace.WriteLine("HY :  try to read....");
        //                ovData = (CyUSB.OVERLAPPED)Marshal.PtrToStructure(handleOverlap[k].AddrOfPinnedObject(), typeof(CyUSB.OVERLAPPED));
        //                if (!EndPoint.WaitForXfer(ovData.hEvent, 1000))
        //                {// 여기가 타임아웃발생한 경우 
        //                    EndPoint.Abort();
        //                    PInvoke.WaitForSingleObject(ovData.hEvent, 100);

        //                    EndPoint.FinishDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]);
        //                    //  Trace.WriteLine($"HY :  WaitForXfer time out [{nIteration}][{len}][{xBufs[k].Length}]!");
        //                    //찌꺼기때문에  8바이트 버리고 시작하자
        //                    //clear 

        //                    valid_count = 0;
        //                    for (int i = 0; i < 16; i++)
        //                        valid_indexs[i] = false;

        //                    for (int i = 0; i < 16; i++)
        //                    {
        //                        is_not_oxA5 = false;
        //                        for (int ii = 8; !is_not_oxA5 && (ii < (4 + 8)); ++ii)
        //                        {//eiohlei// read   사장님이 앞에 한 4바이트만  봐도 된다고 해서 4바이트
        //                            if (xBufs[k][i * 16384 + ii] != 0xa5)
        //                            {
        //                                valid_count++;
        //                                is_not_oxA5 = true;
        //                                valid_indexs[i] = true;
        //                            }
        //                        }
        //                        if (!is_not_oxA5)
        //                            break;//한줄이라도 나오면 더이상 판단할 필요없음
        //                    }
        //                    if (b_first_recevied < 1)
        //                    {
        //                        /*
        //                        Trace.WriteLine($"HY :  first receive t [{nIteration}][{len}][{valid_count}]");

        //                                                        using (BinaryWriter writer2 = new BinaryWriter(File.Open(Application.StartupPath + "\\dumpt.bin", FileMode.Append)))
        //                                                        {//eiohlei  filewrite
        //                                                            writer2.Write(xBufs[k], 0, 16384);
        //                                                            writer2.Write(xBufs[k + 1], 0, 16384);
        //                                                        }
        //                        */
        //                        b_first_recevied = 1;
        //                    }
        //                    if (valid_count < 1)
        //                    {
        //                        Trace.WriteLine($"HY : <<[{nIteration}] all 0xA5 valid_count is 0");
        //                        //                                break; //몽땅 초기값이면  아예 데이터 없는거니까 그냥 나가거라
        //                    }
        //                    else
        //                    {
        //                        if (valid_count > trash_count)
        //                        {
        //                            Trace.WriteLine($"HY : <<[{nIteration}][{valid_count}]  not 0xA5");

        //                            byte[] valid_datas = new byte[VALID_DATA_BYTES * (valid_count - trash_count)];
        //                            //for (int i = trash_count, j = 0; valid_indexs[i] && i < 16; i++, j++)
        //                            for (int i = trash_count, j = 0; valid_indexs[i] && i < 16; ++i, ++j)
        //                            {// i 는 0 or 3  j는 0
        //                                Buffer.BlockCopy(xBufs[k], 8 + (i * 16384), valid_datas, j * VALID_DATA_BYTES, VALID_DATA_BYTES);
        //                            }
        //                            trash_count = 0;
        //                            test_buffer.Add(valid_datas);
        //                            // 넣을때

        //                            //clear buffer;
        //                            /*
        //                                                        Fill(xBufs[k], DefaultBufInitValue);

        //                                                        for (int ii = 0; ii < 16384; ++ii)
        //                                                        {
        //                                                            //xBufs[k][myindex * 16384 + ii] = DefaultBufInitValue;
        //                                                            xBufs[k][ii] = DefaultBufInitValue;
        //                                                        }

        //                            */
        //                            //clear buffers


        //                            XferBytes += (16384 * valid_count);//just for debuging
        //                            test_data += (16384 * valid_count);//just for debuging 
        //                            test_data2 = test_buffer.Count();//just for debuging 
        //                            Successes++;
        //                            //                                    Trace.WriteLine($"HY : <<[{nIteration}][{valid_count}]  not 0xA5 done");
        //                        }
        //                        else
        //                        {
        //                            if (valid_count == trash_count)
        //                            {
        //                                Trace.WriteLine($"HY : <<[{nIteration}][{valid_count}]  not 0xA5 but trash_count == valid_count");
        //                                trash_count = 0;

        //                            }
        //                            else
        //                            {
        //                                Trace.WriteLine($"HY : <<[{nIteration}][{valid_count}]  not 0xA5 but trash_count[{trash_count}] > valid_count[{valid_count}]");
        //                                trash_count -= valid_count;
        //                            }

        //                        }
        //                        for (int ii = 0; ii < 16384; ++ii)
        //                            xBufs[k][ii] = DefaultBufInitValue;
        //                        for (int j = 1; j < 16; j++)
        //                            Buffer.BlockCopy(xBufs[k], 0, xBufs[k], j * 16384, 16384);
        //                    }
        //                    #region  타임아웃예전
        //                    /*
        //                                                //>> old
        //                                                for (int myindex = 0; myindex < 16; ++myindex) // 16384 * 16
        //                                                {
        //                                                    int check_write = 0;
        //                                                    //for (int ii = 0; ii < 16384; ++ii)
        //                                                    for (int ii = 8; ii < 16384; ++ii)
        //                                                    {//eiohlei// read
        //                                                        if (xBufs[k][myindex * 16384 + ii] != 0xa5)
        //                                                        {
        //                                                            check_write = 1;
        //                                                            break;
        //                                                        }
        //                                                    }
        //                                                    if (check_write == 0)
        //                                                    {
        //                    //                                    Trace.WriteLine($"HY : <<[{nIteration}][{myindex}]  all 0xA5");
        //                                                        break; //몽땅 초기값이면  아예 데이터 없는거니까 그냥 나가거라
        //                                                    }
        //                                                   // Trace.WriteLine($"HY : <<[{nIteration}][{myindex}]  not 0xA5");

        //                                                    if (b_first_recevied < 1 )
        //                                                    {
        //                                                        Trace.WriteLine($"HY :  first receive [{nIteration}][{len}]");
        //                                                        using (BinaryWriter writer2 = new BinaryWriter(File.Open(Application.StartupPath + "\\dumpt.bin", FileMode.Append)))
        //                                                        {//eiohlei  filewrite
        //                                                            writer2.Write(xBufs[k], 0, 16384);
        //                                                            writer2.Write(xBufs[k + 1], 0, 16384);
        //                                                        }
        //                                                        b_first_recevied = 1;
        //                                                    }
        //                                                    Trace.WriteLine($"HY :  not  all 0xA5 receive [{nIteration}][{len}]");
        //                                                    using (BinaryWriter writer2 = new BinaryWriter(File.Open(Application.StartupPath + "\\dump0x5A.bin", FileMode.Append)))
        //                                                    {//eiohlei  filewrite
        //                                                        writer2.Write(xBufs[k], 8, 336);
        //                                                        //writer2.Write(xBufs[k + 1], 0, 16384);
        //                                                    }


        //                                                    // 20.11.26
        //                                                    ////////byte[] temp_buffer = new byte[16384];
        //                                                    ////////// 336 byte만 유효
        //                                                    ////////for (int ii = 0; ii < 16384; ++ii)
        //                                                    ////////{
        //                                                    ////////    temp_buffer[ii] = xBufs[k][i * 16384 + ii];
        //                                                    ////////}
        //                                                    ///

        //                                                    byte[] temp_buffer = new byte[336];
        //                                                        // 336 byte만 유효
        //                                                        for (int ii = 0,j = 8; ii < 336; ++ii,++j)
        //                                                        {
        //                                                            //temp_buffer[ii] = xBufs[k][myindex * 336 + j];//bug
        //                                                            temp_buffer[ii] = xBufs[k][myindex * 16384 + j];
        //                                                        }
        //                                                        //Buffer.BlockCopy(xBufs[k], 0, temp_buffer, 0, 16384);
        //                                                        test_buffer.Add(temp_buffer);
        //                                                        // 넣을때

        //                                                        //clear buffer;
        //                                                        for (int ii = 0; ii < 16384; ++ii)
        //                                                        {
        //                                                            //xBufs[k][myindex * 16384 + ii] = DefaultBufInitValue;
        //                                                            xBufs[k][ii] = DefaultBufInitValue;
        //                                                        }
        //                                                        XferBytes += 16384;//just for debuging
        //                                                        test_data += 16384;//just for debuging 
        //                                                        test_data2 = test_buffer.Count();//just for debuging 
        //                                                        Successes++;
        //                                                }
        //                                                //>> old
        //                    */
        //                    #endregion
        //                }
        //                else
        //                {
        //                    // FinishDataXfer
        //                    //                            Trace.WriteLine("HY :  not timeout....");
        //                    if (EndPoint.FinishDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]))
        //                    {
        //                        //                                const int size = 16384;
        //                        //                                byte[] temp_buffer2 = new byte[size];
        //                        //Trace.WriteLine($"HY :  not WaitForXfer time out [{nIteration}][{len}][{lFailure}]!");
        //                        if (b_first_recevied < 2)
        //                        {
        //                            Trace.WriteLine($"HY :  first receive ok [{nIteration}][{len}][{xBufs[k].Length}]");
        //                            /*
        //                            using (BinaryWriter writer2 = new BinaryWriter(File.Open(Application.StartupPath + "\\dump.bin", FileMode.Append)))
        //                            {//eiohlei  filewrite
        //                                writer2.Write(xBufs[k], 0, len);
        //                            }
        //                            */
        //                            b_first_recevied = 2;
        //                        }
        //                        /*
        //                                                        if (nIteration % 100 == 0)
        //                                                            Trace.WriteLine($"HY :  not WaitForXfer time  and received data [{nIteration}][{len}]!");
        //                        */
        //                        //receive data 
        //                        //                                Trace.WriteLine("HY : not !EndPoint.WaitForXfer(ovData.hEvent, 1000)");

        //                        //eiohlei  데이터 받는 주체
        //                        // 20.11.26
        //                        //byte[] temp_buffer = new byte[len];
        //                        //Buffer.BlockCopy(xBufs[k], 0, temp_buffer, 0, len);
        //                        //test_buffer.Add(temp_buffer);
        //                        int chunk_count = len / 16384;
        //                        int remain_bytes = (len - (chunk_count * 16384));
        //                        int remain_count = chunk_count > 0 ? (remain_bytes > VALID_DATA_BYTES ? 1 : 0) : (remain_bytes > (VALID_DATA_BYTES + 8) ? 1 : 0);

        //                        byte[] temp_buffer2 = new byte[VALID_DATA_BYTES * (chunk_count + remain_count + trash_count)];

        //                        //Buffer.BlockCopy(xBufs[k], 0, temp_buffer2, 0, 336);

        //                        for (int i = trash_count, j = 0; i < chunk_count; i++, j++)
        //                        {
        //                            Buffer.BlockCopy(xBufs[k], 8 + (i * 16384), temp_buffer2, j * VALID_DATA_BYTES, VALID_DATA_BYTES);
        //                            XferBytes += 16384;
        //                            test_data += 16384;
        //                            Successes++;
        //                        }
        //                        if (remain_count > 0)
        //                        {
        //                            //  Trace.WriteLine($"HY :  ramain [{nIteration}][{len}][{remain_bytes}]"); // 문제 생기면 한번 봐야 할 곳
        //                            Buffer.BlockCopy(xBufs[k], 8 + (chunk_count * 16384), temp_buffer2, chunk_count * VALID_DATA_BYTES, VALID_DATA_BYTES);
        //                            XferBytes += remain_bytes;
        //                            test_data += remain_bytes;
        //                            Successes++;
        //                        }
        //                        trash_count = 0;//clear
        //                        test_buffer.Add(temp_buffer2);
        //                        test_data2 = test_buffer.Count();
        //                        for (int ii = 0; ii < xBufs[k].Length; ii++)
        //                            xBufs[k][ii] = DefaultBufInitValue;


        //                    }
        //                    else
        //                    {
        //                        /*
        //                                                        if(lFailure % 1000  == 0 )
        //                                                            Trace.WriteLine($"HY :  not WaitForXfer time out but failure [{nIteration}][{len}]!");
        //                                                        lFailure++;
        //                        */
        //                        Failures = 3;// Failures++;
        //                    }
        //                }
        //            }
        //        }

        //        if (bFinalCall == 2)
        //        {
        //            usb_setting(2); // stop 눌렸을때 Final call
        //            pre_successes = Successes;
        //            bFinalCall = 1;
        //        }
        //        else if (bFinalCall == 1)
        //        {
        //            if (pre_successes + 3 < Successes)
        //                bRunning = false;
        //        }

        //        // Re-submit this buffer into the queue
        //        len = BufSz;
        //        if (EndPoint.BeginDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]) == false)
        //            Failures = 4;// Failures++;

        //        k++;
        //        if (k == QueueSz)  // Only update displayed stats once each time through the queue
        //        {
        //            k = 0;

        //            t2 = DateTime.Now;
        //            elapsed = t2 - t1;

        //            xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
        //            xferRate = xferRate / (int)100 * (int)100;

        //            // Call StatusUpdate() in the main thread
        //            //                    if (bRunning == true) this.Invoke(updateUI);


        //            // For small QueueSz or PPX, the loop is too tight for UI thread to ever get service.   
        //            // Without this, app hangs in those scenarios.
        //            Thread.Sleep(0);
        //        }
        //        Thread.Sleep(0);

        //    } // End infinite loop
        //    // Let's recall all the queued buffer and abort the end point.
        //    EndPoint.Abort();
        //}
        #endregion
        public unsafe void XferData(byte[][] cBufs, byte[][] xBufs, byte[][] oLaps, ISO_PKT_INFO[][] pktsInfo, GCHandle[] handleOverlap)
        {
            Trace.WriteLine($"HY :start XferData!!!");
            int k = 0;
            int len = 0;
            int pre_successes = 0;

            Successes = 0;
            Failures = 0;

            XferBytes = 0;
            t1 = DateTime.Now;
            long nIteration = 0;
            // long lFailure = 0;
            CyUSB.OVERLAPPED ovData = new CyUSB.OVERLAPPED();
            // USB Receive Loop
            int valid_count = 0;
            bool[] valid_indexs = new bool[16];
            bool is_not_oxA5 = false;
            for (; bRunning;)
            {
                nIteration++;
                // WaitForXfer
                unsafe
                {
                    //fixed (byte* tmpOvlap = oLaps[k])
                    {
                        //                        Trace.WriteLine("HY :  try to read....");
                        ovData = (CyUSB.OVERLAPPED)Marshal.PtrToStructure(handleOverlap[k].AddrOfPinnedObject(), typeof(CyUSB.OVERLAPPED));
                        if (!EndPoint.WaitForXfer(ovData.hEvent, 1000))
                        {// 여기가 타임아웃발생한 경우 
                            EndPoint.Abort();
                            PInvoke.WaitForSingleObject(ovData.hEvent, 100);

                            EndPoint.FinishDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]);
                            //  Trace.WriteLine($"HY :  WaitForXfer time out [{nIteration}][{len}][{xBufs[k].Length}]!");
                            //찌꺼기때문에  8바이트 버리고 시작하자
                            //clear 

                            valid_count = 0;
                            for (int i = 0; i < 16; i++)
                                valid_indexs[i] = false;

                            for (int i = 0; i < 16; i++)
                            {
                                is_not_oxA5 = false;
                                for (int ii = 8; !is_not_oxA5 && (ii < (4 + 8)); ++ii)
                                {//eiohlei// read   사장님이 앞에 한 4바이트만  봐도 된다고 해서 4바이트
                                    if (xBufs[k][i * 16384 + ii] != 0xa5)
                                    {
                                        valid_count++;
                                        is_not_oxA5 = true;
                                        valid_indexs[i] = true;
                                    }
                                }
                                if (!is_not_oxA5)
                                    break;//한줄이라도 나오면 더이상 판단할 필요없음
                            }
                            if (b_first_recevied < 1)
                            {
                                /*
                                Trace.WriteLine($"HY :  first receive t [{nIteration}][{len}][{valid_count}]");
                               
                                                                using (BinaryWriter writer2 = new BinaryWriter(File.Open(Application.StartupPath + "\\dumpt.bin", FileMode.Append)))
                                                                {//eiohlei  filewrite
                                                                    writer2.Write(xBufs[k], 0, 16384);
                                                                    writer2.Write(xBufs[k + 1], 0, 16384);
                                                                }
                                */
                                b_first_recevied = 1;
                            }
                            if (valid_count < 1)
                            {
                                Trace.WriteLine($"HY : <<[{nIteration}] all 0xA5 valid_count is 0");
                                //                                break; //몽땅 초기값이면  아예 데이터 없는거니까 그냥 나가거라
                            }
                            else
                            {
                                if (valid_count > trash_count)
                                {
                                    Trace.WriteLine($"HY : <<[{nIteration}][{valid_count}]  not 0xA5");

                                    byte[] valid_datas = new byte[VALID_DATA_BYTES * (valid_count - trash_count)];
                                    for (int i = trash_count, j = 0; valid_indexs[i] && i < 16; i++, j++)
                                    {// i 는 0 or 3  j는 0
                                        Buffer.BlockCopy(xBufs[k], 8 + (i * 16384), valid_datas, j * VALID_DATA_BYTES, VALID_DATA_BYTES);
                                    }
                                    trash_count = 0;
                                    test_buffer.Add(valid_datas);
                                    // 넣을때

                                    //clear buffer;
                                    /*
                                                                Fill(xBufs[k], DefaultBufInitValue);

                                                                for (int ii = 0; ii < 16384; ++ii)
                                                                {
                                                                    //xBufs[k][myindex * 16384 + ii] = DefaultBufInitValue;
                                                                    xBufs[k][ii] = DefaultBufInitValue;
                                                                }

                                    */
                                    //clear buffers


                                    XferBytes += (16384 * valid_count);//just for debuging
                                    test_data += (16384 * valid_count);//just for debuging 
                                    test_data2 = test_buffer.Count();//just for debuging 
                                    Successes++;
                                    //                                    Trace.WriteLine($"HY : <<[{nIteration}][{valid_count}]  not 0xA5 done");
                                }
                                else
                                {
                                    if (valid_count == trash_count)
                                    {
                                        Trace.WriteLine($"HY : <<[{nIteration}][{valid_count}]  not 0xA5 but trash_count == valid_count");
                                        trash_count = 0;

                                    }
                                    else
                                    {
                                        Trace.WriteLine($"HY : <<[{nIteration}][{valid_count}]  not 0xA5 but trash_count[{trash_count}] > valid_count[{valid_count}]");
                                        trash_count -= valid_count;
                                    }

                                }
                                for (int ii = 0; ii < 16384; ++ii)
                                    xBufs[k][ii] = DefaultBufInitValue;
                                for (int j = 1; j < 16; j++)
                                    Buffer.BlockCopy(xBufs[k], 0, xBufs[k], j * 16384, 16384);
                            }
                        }
                        else
                        {
                            // FinishDataXfer
                            //                            Trace.WriteLine("HY :  not timeout....");
                            if (EndPoint.FinishDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]))
                            {
                                //                                const int size = 16384;
                                //                                byte[] temp_buffer2 = new byte[size];
                                //Trace.WriteLine($"HY :  not WaitForXfer time out [{nIteration}][{len}][{lFailure}]!");
                                if (b_first_recevied < 2)
                                {
                                    Trace.WriteLine($"HY :  first receive ok [{nIteration}][{len}][{xBufs[k].Length}]");
                                    /*
                                    using (BinaryWriter writer2 = new BinaryWriter(File.Open(Application.StartupPath + "\\dump.bin", FileMode.Append)))
                                    {//eiohlei  filewrite
                                        writer2.Write(xBufs[k], 0, len);
                                    }
                                    */
                                    b_first_recevied = 2;
                                }
                                /*
                                                                if (nIteration % 100 == 0)
                                                                    Trace.WriteLine($"HY :  not WaitForXfer time  and received data [{nIteration}][{len}]!");
                                */
                                //receive data 
                                //                                Trace.WriteLine("HY : not !EndPoint.WaitForXfer(ovData.hEvent, 1000)");

                                //eiohlei  데이터 받는 주체
                                // 20.11.26
                                //byte[] temp_buffer = new byte[len];
                                //Buffer.BlockCopy(xBufs[k], 0, temp_buffer, 0, len);
                                //test_buffer.Add(temp_buffer);
                                int chunk_count = len / 16384;
                                int remain_bytes = (len - (chunk_count * 16384));
                                int remain_count = chunk_count > 0 ? (remain_bytes > VALID_DATA_BYTES ? 1 : 0) : (remain_bytes > (VALID_DATA_BYTES + 8) ? 1 : 0);

                                byte[] temp_buffer2 = new byte[VALID_DATA_BYTES * (chunk_count + remain_count + trash_count)];

                                //Buffer.BlockCopy(xBufs[k], 0, temp_buffer2, 0, 336);

                                for (int i = trash_count, j = 0; i < chunk_count; i++, j++)
                                {
                                    Buffer.BlockCopy(xBufs[k], 8 + (i * 16384), temp_buffer2, j * VALID_DATA_BYTES, VALID_DATA_BYTES);
                                    XferBytes += 16384;
                                    test_data += 16384;
                                    Successes++;
                                }
                                if (remain_count > 0)
                                {
                                    //  Trace.WriteLine($"HY :  ramain [{nIteration}][{len}][{remain_bytes}]"); // 문제 생기면 한번 봐야 할 곳
                                    Buffer.BlockCopy(xBufs[k], 8 + (chunk_count * 16384), temp_buffer2, chunk_count * VALID_DATA_BYTES, VALID_DATA_BYTES);
                                    XferBytes += remain_bytes;
                                    test_data += remain_bytes;
                                    Successes++;
                                }
                                trash_count = 0;//clear
                                test_buffer.Add(temp_buffer2);
                                test_data2 = test_buffer.Count();
                                for (int ii = 0; ii < xBufs[k].Length; ii++)
                                    xBufs[k][ii] = DefaultBufInitValue;


                            }
                            else
                            {
                                /*
                                                                if(lFailure % 1000  == 0 )
                                                                    Trace.WriteLine($"HY :  not WaitForXfer time out but failure [{nIteration}][{len}]!");
                                                                lFailure++;
                                */
                                Failures = 3;// Failures++;
                            }
                        }
                    }
                }

                if (bFinalCall == 2)
                {
                    usb_setting(2); // stop 눌렸을때 Final call
                    pre_successes = Successes;
                    bFinalCall = 1;
                }
                else if (bFinalCall == 1)
                {
                    if (pre_successes + 3 < Successes)
                        bRunning = false;
                }

                // Re-submit this buffer into the queue
                len = BufSz;
                if (EndPoint.BeginDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]) == false)
                    Failures = 4;// Failures++;

                k++;
                if (k == QueueSz)  // Only update displayed stats once each time through the queue
                {
                    k = 0;

                    t2 = DateTime.Now;
                    elapsed = t2 - t1;

                    xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                    xferRate = xferRate / (int)100 * (int)100;

                    // Call StatusUpdate() in the main thread
                    //                    if (bRunning == true) this.Invoke(updateUI);


                    // For small QueueSz or PPX, the loop is too tight for UI thread to ever get service.   
                    // Without this, app hangs in those scenarios.
                    Thread.Sleep(0);
                }
                Thread.Sleep(0);

            } // End infinite loop
            // Let's recall all the queued buffer and abort the end point.
            EndPoint.Abort();
        }

        public unsafe void XferData2(byte[][] cBufs, byte[][] xBufs, byte[][] oLaps, ISO_PKT_INFO[][] pktsInfo, GCHandle[] handleOverlap)
        {
            Trace.WriteLine($"HY :start XferData!!!");
            int k = 0;
            int len = 0;
            int pre_successes = 0;

            Successes = 0;
            Failures = 0;

            XferBytes = 0;
            t1 = DateTime.Now;
            long nIteration = 0;
            // long lFailure = 0;
            CyUSB.OVERLAPPED ovData = new CyUSB.OVERLAPPED();
            // USB Receive Loop
            int valid_count = 0;
            bool[] valid_indexs = new bool[16];
            bool is_not_oxA5 = false;
            for (; bRunning;)
            {
                nIteration++;
                // WaitForXfer
                unsafe
                {
                    //fixed (byte* tmpOvlap = oLaps[k])
                    {
                        //                        Trace.WriteLine("HY :  try to read....");
                        ovData = (CyUSB.OVERLAPPED)Marshal.PtrToStructure(handleOverlap[k].AddrOfPinnedObject(), typeof(CyUSB.OVERLAPPED));
                        if (!EndPoint.WaitForXfer(ovData.hEvent, 1000))
                        {// 여기가 타임아웃발생한 경우 
                            EndPoint.Abort();
                            PInvoke.WaitForSingleObject(ovData.hEvent, 100);

                            EndPoint.FinishDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]);
                            //  Trace.WriteLine($"HY :  WaitForXfer time out [{nIteration}][{len}][{xBufs[k].Length}]!");
                            //찌꺼기때문에  8바이트 버리고 시작하자
                            //clear 

                            valid_count = 0;
                            for (int i = 0; i < 16; i++)
                                valid_indexs[i] = false;

                            for (int i = 0; i < 16; i++)
                            {
                                is_not_oxA5 = false;
                                for (int ii = 8; !is_not_oxA5 && (ii < (4 + 8)); ++ii)
                                {//eiohlei// read   사장님이 앞에 한 4바이트만  봐도 된다고 해서 4바이트
                                    if (xBufs[k][i * 16384 + ii] != 0xa5)
                                    {
                                        valid_count++;
                                        is_not_oxA5 = true;
                                        valid_indexs[i] = true;
                                    }
                                }
                                if (!is_not_oxA5)
                                    break;//한줄이라도 나오면 더이상 판단할 필요없음
                            }
                            if (b_first_recevied < 1)
                            {
                                /*
                                Trace.WriteLine($"HY :  first receive t [{nIteration}][{len}][{valid_count}]");
                               
                                                                using (BinaryWriter writer2 = new BinaryWriter(File.Open(Application.StartupPath + "\\dumpt.bin", FileMode.Append)))
                                                                {//eiohlei  filewrite
                                                                    writer2.Write(xBufs[k], 0, 16384);
                                                                    writer2.Write(xBufs[k + 1], 0, 16384);
                                                                }
                                */
                                b_first_recevied = 1;
                            }
                            if (valid_count < 1)
                            {
                                Trace.WriteLine($"HY : <<[{nIteration}] all 0xA5 valid_count is 0");
                                //                                break; //몽땅 초기값이면  아예 데이터 없는거니까 그냥 나가거라
                            }
                            else
                            {
                                if (valid_count > trash_count)
                                {
                                    Trace.WriteLine($"HY : <<[{nIteration}][{valid_count}]  not 0xA5");

                                    byte[] valid_datas = new byte[VALID_DATA_BYTES * (valid_count - trash_count)];
                                    for (int i = trash_count, j = 0; valid_indexs[i] && i < 16; i++, j++)
                                    {// i 는 0 or 3  j는 0
                                        Buffer.BlockCopy(xBufs[k], 8 + (i * 16384), valid_datas, j * VALID_DATA_BYTES, VALID_DATA_BYTES);
                                    }
                                    trash_count = 0;
                                    test_buffer.Add(valid_datas);
                                    // 넣을때

                                    //clear buffer;
                                    /*
                                                                Fill(xBufs[k], DefaultBufInitValue);

                                                                for (int ii = 0; ii < 16384; ++ii)
                                                                {
                                                                    //xBufs[k][myindex * 16384 + ii] = DefaultBufInitValue;
                                                                    xBufs[k][ii] = DefaultBufInitValue;
                                                                }

                                    */
                                    //clear buffers


                                    XferBytes += (16384 * valid_count);//just for debuging
                                    test_data += (16384 * valid_count);//just for debuging 
                                    test_data2 = test_buffer.Count();//just for debuging 
                                    Successes++;
                                    //                                    Trace.WriteLine($"HY : <<[{nIteration}][{valid_count}]  not 0xA5 done");
                                }
                                else
                                {
                                    if (valid_count == trash_count)
                                    {
                                        Trace.WriteLine($"HY : <<[{nIteration}][{valid_count}]  not 0xA5 but trash_count == valid_count");
                                        trash_count = 0;

                                    }
                                    else
                                    {
                                        Trace.WriteLine($"HY : <<[{nIteration}][{valid_count}]  not 0xA5 but trash_count[{trash_count}] > valid_count[{valid_count}]");
                                        trash_count -= valid_count;
                                    }

                                }
                                for (int ii = 0; ii < 16384; ++ii)
                                    xBufs[k][ii] = DefaultBufInitValue;
                                for (int j = 1; j < 16; j++)
                                    Buffer.BlockCopy(xBufs[k], 0, xBufs[k], j * 16384, 16384);
                            }
                        }
                        else
                        {
                            // FinishDataXfer
                            //                            Trace.WriteLine("HY :  not timeout....");
                            if (EndPoint.FinishDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]))
                            {
                                //                                const int size = 16384;
                                //                                byte[] temp_buffer2 = new byte[size];
                                //Trace.WriteLine($"HY :  not WaitForXfer time out [{nIteration}][{len}][{lFailure}]!");
                                if (b_first_recevied < 2)
                                {
                                    Trace.WriteLine($"HY :  first receive ok [{nIteration}][{len}][{xBufs[k].Length}]");
                                    /*
                                    using (BinaryWriter writer2 = new BinaryWriter(File.Open(Application.StartupPath + "\\dump.bin", FileMode.Append)))
                                    {//eiohlei  filewrite
                                        writer2.Write(xBufs[k], 0, len);
                                    }
                                    */
                                    b_first_recevied = 2;
                                }
                                /*
                                                                if (nIteration % 100 == 0)
                                                                    Trace.WriteLine($"HY :  not WaitForXfer time  and received data [{nIteration}][{len}]!");
                                */
                                //receive data 
                                //                                Trace.WriteLine("HY : not !EndPoint.WaitForXfer(ovData.hEvent, 1000)");

                                //eiohlei  데이터 받는 주체
                                // 20.11.26
                                //byte[] temp_buffer = new byte[len];
                                //Buffer.BlockCopy(xBufs[k], 0, temp_buffer, 0, len);
                                //test_buffer.Add(temp_buffer);
                                int chunk_count = len / 16384;
                                int remain_bytes = (len - (chunk_count * 16384));
                                int remain_count = chunk_count > 0 ? (remain_bytes > VALID_DATA_BYTES ? 1 : 0) : (remain_bytes > (VALID_DATA_BYTES + 8) ? 1 : 0);

                                byte[] temp_buffer2 = new byte[VALID_DATA_BYTES * (chunk_count + remain_count + trash_count)];

                                //Buffer.BlockCopy(xBufs[k], 0, temp_buffer2, 0, 336);

                                for (int i = trash_count, j = 0; i < chunk_count; i++, j++)
                                {
                                    Buffer.BlockCopy(xBufs[k], 8 + (i * 16384), temp_buffer2, j * VALID_DATA_BYTES, VALID_DATA_BYTES);
                                    XferBytes += 16384;
                                    test_data += 16384;
                                    Successes++;
                                }
                                if (remain_count > 0)
                                {
                                    //  Trace.WriteLine($"HY :  ramain [{nIteration}][{len}][{remain_bytes}]"); // 문제 생기면 한번 봐야 할 곳
                                    Buffer.BlockCopy(xBufs[k], 8 + (chunk_count * 16384), temp_buffer2, chunk_count * VALID_DATA_BYTES, VALID_DATA_BYTES);
                                    XferBytes += remain_bytes;
                                    test_data += remain_bytes;
                                    Successes++;
                                }
                                trash_count = 0;//clear
                                test_buffer.Add(temp_buffer2);
                                test_data2 = test_buffer.Count();
                                for (int ii = 0; ii < xBufs[k].Length; ii++)
                                    xBufs[k][ii] = DefaultBufInitValue;


                            }
                            else
                            {
                                /*
                                                                if(lFailure % 1000  == 0 )
                                                                    Trace.WriteLine($"HY :  not WaitForXfer time out but failure [{nIteration}][{len}]!");
                                                                lFailure++;
                                */
                                Failures = 3;// Failures++;
                            }
                        }
                    }
                }

                if (bFinalCall == 2)
                {
                    usb_setting(2); // stop 눌렸을때 Final call
                    pre_successes = Successes;
                    bFinalCall = 1;
                }
                else if (bFinalCall == 1)
                {
                    if (pre_successes + 3 < Successes)
                        bRunning = false;
                }

                // Re-submit this buffer into the queue
                len = BufSz;
                if (EndPoint.BeginDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]) == false)
                    Failures = 4;// Failures++;

                k++;
                if (k == QueueSz)  // Only update displayed stats once each time through the queue
                {
                    k = 0;

                    t2 = DateTime.Now;
                    elapsed = t2 - t1;

                    xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                    xferRate = xferRate / (int)100 * (int)100;

                    // Call StatusUpdate() in the main thread
                    //                    if (bRunning == true) this.Invoke(updateUI);


                    // For small QueueSz or PPX, the loop is too tight for UI thread to ever get service.   
                    // Without this, app hangs in those scenarios.
                    Thread.Sleep(0);
                }
                Thread.Sleep(0);

            } // End infinite loop
            // Let's recall all the queued buffer and abort the end point.
            EndPoint.Abort();
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] 파일경로열기
        //------------------------------+---------------------------------------------------------------
        public string ShowFileOpenDialog()
        {
            //파일오픈창 생성 및 설정
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open bin";
            ofd.FileName = "name";
            ofd.Filter = "데이터 (*.bin) | *.bin; | 모든 파일 (*.*) | *.*";

            //파일 오픈창 로드
            DialogResult dr = ofd.ShowDialog();

            //OK버튼 클릭시
            if (dr == DialogResult.OK)
            {
                //File명과 확장자를 가지고 온다.
                string fileName = ofd.SafeFileName;
                //File경로와 File명을 모두 가지고 온다.
                string fileFullName = ofd.FileName;
                //File경로만 가지고 온다.
                string filePath = fileFullName.Replace(fileName, "");

                //출력 예제용 로직
                //label1.Text = "File Name  : " + fileName;
                //label2.Text = "Full Name  : " + fileFullName;
                //label3.Text = "File Path  : " + filePath;
                //File경로 + 파일명 리턴
                return fileFullName;
            }
            //취소버튼 클릭시 또는 ESC키로 파일창을 종료 했을경우
            else if (dr == DialogResult.Cancel)
            {
                return "";
            }

            return "";
        }
    }
}
