using CyUSB;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;


namespace HUREL.PG.MultiSlit
{
    public partial class CRUXELL_Original
    {
        // 1번째 Layer와 연결되어있는 2번째 Layer

        #region Initialize Function (Trust)

        private void init_data_and_buffer()
        {
            // 1. 버퍼 초기화
            DATA_BUFFER = new byte[334];
            SDATA_BUFFER = new ushort[167];

            // 2. 측정 인자 설정
            if (System.IO.File.Exists(INI_PATH))
            {
                Read_current_ini();
            }
            else
            {
                //Set_first_setting();
                Write_current_ini();
            }

            // 3. 차트 초기화
            for (int i = 0; i < 36; ++i)
            {
                PGdistribution.cnt_row1[i] = 0;
                PGdistribution.cnt_row2[i] = 0;
                PGdistribution.cnt_row3[i] = 0;
                PGdistribution.cnt_row4[i] = 0;

                PGdistribution.cnt_top[i] = 0;
                PGdistribution.cnt_bot[i] = 0;
            }
            for (int i = 0; i < 72; ++i)
            {
                PGdistribution.cnt_PGdist72[i] = 0;
            }
            for (int i = 0; i < 71; ++i)
            {
                PGdistribution.cnt_PGdist71[i] = 0;
            }
        }
        private void init_USB()
        {
            // Setup the callback routine for NullReference exception handling
            handleException = new ExceptionCallback(ThreadException);

            // Create the list of USB devices attached to the CyUSB3.sys driver.
            try
            {
                UsbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
            }
            catch
            {

            }
            SelectedDevice = new DeviceInfo();

            //Assign event handlers for device attachment and device removal.
            UsbDevices.DeviceAttached += new EventHandler(usbDevices_DeviceAttached);
            UsbDevices.DeviceRemoved += new EventHandler(usbDevices_DeviceRemoved);

            //Set and search the device with VID-PID 04b4-1003 and if found, selects the end point
            SetDevice(false);
        }

        #endregion

        string open_bin_path = "";
        bool is_convert = false;
        public static List<CounterData> FPGADatas = new List<CounterData>();
        public static List<CounterData> FPGADatasTemp = new List<CounterData>();

        public static List<FPGADataModel_71Ch> FPGADatas_71Ch = new List<FPGADataModel_71Ch>();

        public static DateTime MeasurementStartTime = new DateTime();
        public static int BeamOnIndex;
        public static int BeamOffIndex;
        public static bool isBeamOn;

        #region CRUXELL code Storage

        #region CyUSB parts

        //------------------------------+---------------------------------------------------------------
        // [변수] 재린 추가 변수
        //------------------------------+---------------------------------------------------------------

        public List<DeviceInfo> DeviceList = new List<DeviceInfo>();
        public DeviceInfo SelectedDevice;

        public class DeviceInfo
        {
            public USBDevice Device { get; set; }
            public CyUSBDevice CyDevice { get; set; }
            public string DeviceName { get; set; }
        }

        private List<string> EndPointList = new List<string>();

        private USBDeviceList UsbDevices;
        private CyUSBEndPoint EndPoint;
        private int endPointListSelectIdx;

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

        #endregion

        #region FM_main parts

        //------------------------------+---------------------------------------------------------------
        // [변수] Material Skin에 있던 변수, WPF에서 따로 생성(by 재린)
        //------------------------------+---------------------------------------------------------------
        string LB_current_tr, LB_current_tcr, LB_average_tr, LB_average_tcr;


        public string ver_string = "Ver 21.02.23";
        double test_data = 0;
        double test_data2 = 0;
        //int time_out = 0;
        BlockingCollection<byte[]> test_buffer = new BlockingCollection<byte[]>(10000);
        // byte[] test2_buffer = new byte[1024 * 1024 * 700];
        int p_head = 0; // test2_buffer의 50개중 몇번째인지
        int p_tail = 0;
        int i_head = 0; // 1메가 배열 내부의 인덱스
        int p_error = 0;
        // byte[] test_buffer = new byte[1024 * 1024 * 128];                
        public int is_init = 0;

        //------------------------------+---------------------------------------------------------------
        // [이벤트] 메인 폼 닫힐 때
        //------------------------------+---------------------------------------------------------------
        private void FM_main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (StartBtn.Text.Equals("Start") == false)
            //{
            //    Command_MonitoringStart();
            //}


            //// Executes on clicking close button
            //bRunning = false;
            //if (usbDevices != null)
            //    usbDevices.Dispose();

            //Write_current_ini(); // 현재 설정 저장하기
            //terminate_file_save(); // 파일 저장 종료
            //terminate_thread(); // 스레드 종료
            //Trace.WriteLine("HY : EXIT");
        }
        #endregion

        #region HY_MULTI_SLIT_CAMERA_COUNTER parts

        public static bool CheckForIllegalCrossThreadCalls;
        //public static int DelayBeamIndexCount = 0;

        //------------------------------+---------------------------------------------------------------
        // [변수] parsing 관련
        //------------------------------+---------------------------------------------------------------
        bool thread_run = false;
        bool parsing_run = false;
        public enum PS // Parsing State
        {
            CUT_TRASH,                  // FE 들어오기 전 까지 다 버림
            FE_FIRST,                   // 첫 FE 확인
            FE_SECOND,                  // 두번째 FE 확인
            DATA_READ                   // 514 DATA_BUFFER 채우기
        }
        PS parsing_status = PS.CUT_TRASH;

        byte[] DATA_BUFFER; // FEFE제외하고 데이터만 담은 514 data buffer
        ushort[] SDATA_BUFFER; // short 버퍼
        int DATA_BUFFER_read_count; // 514 DATA_BUFFER 채울때 count

        //------------------------------+---------------------------------------------------------------
        // [변수] 파일저장
        //------------------------------+---------------------------------------------------------------
        public System.Windows.Forms.Timer save_timer { get; set; }
        StreamWriter file_main;
        StreamWriter file_adc;

        public string DIR_BinaryFile;
        public string DIR_MainFile;
        public string DIR_ADCFile;

        //------------------------------+---------------------------------------------------------------
        // [변수] parsing - File save
        //------------------------------+---------------------------------------------------------------
        StringBuilder sb = new StringBuilder(65536);

        public uint trig_on_off = 0;
        public uint pri_et = 0;
        public uint temp_trig_time = 0;
        public double trig_adc_a = 0;

        int[] temp_send_buffer = new int[1024];

        //------------------------------+---------------------------------------------------------------
        // [변수] parsing - 상태에 따라 파싱
        //------------------------------+---------------------------------------------------------------
        public int read_buffer_count = 0; // 514 데이터 버퍼 읽은 갯수 (종료시 0초기화 필요)
        public int read_fefe_count = 0; // FEFE단위 읽은 갯수        

        //------------------------------+---------------------------------------------------------------
        // [함수] save - 파일 저장 관련 초기화 작업
        //------------------------------+---------------------------------------------------------------
        private void close_file()
        {
            if (file_main != null)
                file_main.Close();

            if (file_adc != null)
                file_adc.Close();
        }

        private void init_file_save()
        {
            // 기존에 열려있었다면 stream 닫기
            close_file();

            string b_name = Path.GetFileNameWithoutExtension(open_bin_path);
            string b_path = Path.GetDirectoryName(open_bin_path);

            #region Save Directory 받는 것으로 구현하기
            //string p_main = TB_data_save_dir.Text;
            #endregion

            string p_main = "";

            if (p_main.Length == 0)
                p_main = Path.Combine(b_path, b_name + "_main.txt");

            if (!Directory.Exists(Path.GetDirectoryName(p_main)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(p_main));
            }
            p_main = GetUniqueFilePath(p_main);

            file_main = new StreamWriter(p_main, true);

            #region ADC 파일을 저장할지 묻는 것에 대해 구현하기
            //if (CB_save_adc.Checked) //init_file_save
            //{
            //    #region Save Directory 받는 것으로 구현하기
            //    //string p_adc = TB_trig_save_dir.Text;
            //    #endregion

            //    string p_adc = "";

            //    if (p_adc.Length == 0)
            //        p_adc = Path.Combine(b_path, b_name + "_adc.txt");

            //    if (!Directory.Exists(Path.GetDirectoryName(p_adc)))
            //    {
            //        Directory.CreateDirectory(Path.GetDirectoryName(p_adc));
            //    }
            //    p_adc = GetUniqueFilePath(p_adc);

            //    file_adc = new StreamWriter(p_adc, true);
            //}
            #endregion
        }

        #endregion

        #region 미분류 1

        public bool SetVariables(Variables_FPGA variables)
        {
            //if (variables.CountMode_0x11 == -5000)
            //{
            //    // 뭔가 정상적인 if 조건문이 들어가면 좋을 것 같음. 그게 아니라면, 없애버리기
            //    // 예를들면, 0x11 ~ 0x19까지 int가 아니라 이상한 것이 들어가있는 경우에 false를 받고, VMStatus에 제대로 된 변수를 입력해달라고
            //    return false;
            //}
            //variables_FPGA = variables;
            return true;
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] START 버튼 클릭 시
        //------------------------------+---------------------------------------------------------------
        private void BT_start_Click(object sender, EventArgs e)
        {
            #region Command 함수로 대체
            //Command_MonitoringStart();
            #endregion
        }

        private void event_mode_change(object sender, EventArgs e)
        {
            // MainViewModel에서 모드가 바뀌는 경우 바로바로 박스 visibility 변경하도록 수정하기
        }

        //------------------------------+---------------------------------------------------------------
        // [이벤트] 인풋값 정수/실수만 받도록
        //------------------------------+---------------------------------------------------------------
        private void digit_keypress(object sender, KeyPressEventArgs e)
        {
            //숫자만 입력되도록 필터링
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
            {
                e.Handled = true;
            }
        }
        private void digit2_keypress(object sender, KeyPressEventArgs e)
        {
            //숫자만 입력되도록 필터링
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back) || e.KeyChar == '.' || e.KeyChar == '-'))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
            {
                e.Handled = true;
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [변수] 타이머 관련 변수
        //------------------------------+---------------------------------------------------------------
        public int timer_all_stop = 0; // 강제종료 확인 변수
        public int timer_mode = 0; // 0 : 종료 / 1 : main만 / 2 : main + sub
        System.Threading.Timer timer_main; // 누적 시간
        System.Threading.Timer timer_sub; // 개별 타이머
        public int timer_count = 0; // 누적 시간 데이터
        public int timer_count2 = 0; // 곱해서 계산해논 시간
        public int timer_sub_val = 0; // 개별 타이머 기록값
        delegate void TimerEventFiredDelegate();

        //------------------------------+---------------------------------------------------------------
        // [함수] timer_main 관련
        //------------------------------+---------------------------------------------------------------
        private void timer_main_callback(Object state)
        {
            #region 추후 구현
            //BeginInvoke(new TimerEventFiredDelegate(timer_main_callback_work));
            #endregion
        }

        double sum_tr, sum_tcr;
        double current_tr, current_tcr, average_tr, average_tcr = 0;
        int sec_count = 0;



        private void timer_main_on()
        {
            // main timer on
            current_tr = 0;
            current_tcr = 0;
            average_tr = 0;
            average_tcr = 0;

            sum_tcr = 0;
            sum_tr = 0;
            sec_count = 0;

            LB_current_tr = current_tr.ToString();
            LB_current_tcr = current_tcr.ToString();
            LB_average_tr = average_tr.ToString();
            LB_average_tcr = average_tcr.ToString();

            timer_main = new System.Threading.Timer(timer_main_callback);
            timer_main.Change(0, 1000); // 대기 : 0 / 반복주기 1초

            MeasurementStartTime = DateTime.Now;
        }

        private void timer_main_off()
        {
            // timer_main 종료
            if (timer_main != null)
            {
                //    1.대기,반복 무한대로 타이머 종료와같음
                timer_main.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                //////////// 2. 변수 초기화
                //////////timer_count = 0;
                //////////LB_r_time.Text = $"Record Time : 00:00:00";
                //////////LB_setting_time.Text = $"Setting Time : 00:00:00";
                //////////LB_read_count.Text = "Count rate : 0";
                //////////read_fefe_count = 0;
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] timer usb 관련
        //------------------------------+---------------------------------------------------------------
        private void timer_start()
        {
            #region Cruxell 주석
            ////////timer_count2 = tb_0x0a * tb_0x0b;
            ////////if (timer_count2 != 0)
            ////////    LB_setting_time.Text = $"Setting Time : {(timer_count2 / 3600):D2}:{(timer_count2 % 3600 / 60):D2}:{(timer_count2 % 3600 % 60):D2}";
            ////////else
            ////////    LB_setting_time.Text = $"Setting Time : 00:00:00";
            #endregion

            timer_main_on();
        }
        private void timer_end()
        {
            timer_main_off();
        }

        private void ConvertThread()
        {
            using (BinaryReader b = new BinaryReader(File.Open(open_bin_path, FileMode.Open)))
            {
                long pos = 0;
                long length = b.BaseStream.Length;
                byte[] buffer = new byte[1024 * 1024 * 10];
                while (pos < length)
                {
                    int read_length = 1024 * 1024 * 10;
                    if (read_length + pos >= length)
                        read_length = (int)(length - pos);

                    buffer = b.ReadBytes(read_length);
                    for (int i = 0; i < read_length; ++i)
                        check_parsing_state(buffer[i]);
                    pos += read_length;
                }
            }
            if (file_main != null)
                file_main.Close();
            if (file_adc != null)
                file_adc.Close();
        }

        private void print_init(int mode, uint pre, uint post, uint interval_time)
        {
            // 1. Interval time과 현재 시간 출력
            switch (mode)
            {
                case 1:
                    // Continue 모드에서의 출력
                    // file_main.WriteLine($"{interval_time * 10}us, Time: {System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}");
                    file_main.WriteLine($"Time: {System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}");
                    file_main.WriteLine($"Continue Mode, Recording interval: {interval_time * 10}us");
                    file_main.WriteLine();
                    file_main.WriteLine($"Type    Start time(us)      End time(us)        counts on the channels(#)");
                    file_main.WriteLine();
                    break;
                case 2:
                    file_main.WriteLine($"Time: {System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}");
                    file_main.WriteLine($"Trigger Mode 1, Recording interval: {interval_time * 10}us, Timing margin pre = {pre}, post = {post}");
                    file_main.WriteLine();
                    file_main.WriteLine($"Number  Start time(us)      End time(us)        counts on the channels(#)");
                    file_main.WriteLine();
                    break;
                case 3:
                    file_main.WriteLine($"Time: {System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}");
                    file_main.WriteLine($"Trigger Mode 2, Timing margin pre = {pre}, post = {post}");
                    file_main.WriteLine();
                    file_main.WriteLine($"Number  Start time(us)      End time(us)        counts odn the channels(#)");
                    file_main.WriteLine();
                    break;
                default:
                    break;
            }
        }


        //------------------------------+---------------------------------------------------------------
        // [함수] save - 파일 저장 관련 종료 작업
        //------------------------------+---------------------------------------------------------------
        private void terminate_file_save()
        {
            // StreamWriter 닫기

            if (file_main != null)
                file_main.Close();

            if (file_adc != null)
                file_adc.Close();
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] terminate thread
        //------------------------------+---------------------------------------------------------------
        private void terminate_thread()
        {
            if (tListen != null)
                tListen.Wait();
            if (tParsing != null)
                tParsing.Wait();
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] input disable
        //------------------------------+---------------------------------------------------------------
        private void input_disable(bool state)
        {
            // 1. DAQ Setting 창 open button disable
            // 2. DAQ Setting 창 open되어있을 경우, 입력 disable

            test_data = 0;
            parsing_status = PS.CUT_TRASH;
        }

        uint numberOfSetBits(uint i)
        {
            // C or C++: use uint32_t
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }


        //------------------------------+---------------------------------------------------------------
        // [함수] ParsingThread
        //------------------------------+---------------------------------------------------------------
        private void ParsingThread()
        {
            trig_on_off = 0;
            pri_et = 0;
            temp_trig_time = 0;

            trig_adc_a = 0;

            Trace.WriteLine("HY : ParsingThread start");
            while (thread_run)
            {
                CheckForIllegalCrossThreadCalls = false;
                using (BinaryWriter writer = new BinaryWriter(File.Open(DIR_BinaryFile, FileMode.Append)))
                {
                    byte[] Item;
                    while (test_buffer.TryTake(out Item, 1))
                    {
                        writer.Write(Item);
                        writer.Flush();

                        for (int i = 0; i < Item.Length; ++i)
                            check_parsing_state(Item[i]);
                        Thread.Sleep(0);
                    }
                }
            }
            close_file();
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] 최초 셋팅 값 한번만 보내기 용
        //------------------------------+---------------------------------------------------------------
        private void reset_send_buffer(ref byte[] outData, ref int outData_BufSz)
        {
            for (int i = 0; i < 1024; ++i)
                temp_send_buffer[i] = 0;
            for (int i = 0; i < outData_BufSz; ++i)
                outData[i] = 0;
        }
        private unsafe void tryparse_send(ref byte[] outData, ref int outData_BufSz, int data, int add)
        {
            reset_send_buffer(ref outData, ref outData_BufSz);

            int get_data = data;

            for (int i = 0; i < 700; ++i)
                temp_send_buffer[i] = (add << 24) | (get_data & 0xFFFFFF);

            Buffer.BlockCopy(temp_send_buffer, 0, outData, 0, temp_send_buffer.Length * 4);

            ///////
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //EndPoint.TimeOut = 10;
            for (int i = 0; i < 1000; ++i)
            {
                if (EndPoint.XferData(ref outData, ref outData_BufSz))
                    break;
                //Thread.Sleep(10);
            }
            sw.Stop();
            Trace.WriteLine($"[JJR] Ellapsed Time: {sw.ElapsedMilliseconds} ms");

        }
        private void SetOutputData(int flag)
        {
            // 1. out endpoint로 설정
            //EndPointsComboBox.SelectedIndex = 0;
            EndPointListSelectIdx = 0;

            // 2. USB endpoint 연결
            int outData_BufSz = 4096;

            EndPoint.XferSize = outData_BufSz;

            if (EndPoint is CyIsocEndPoint)
                IsoPktBlockSize = (EndPoint as CyIsocEndPoint).GetPktBlockSize(outData_BufSz);
            else
                IsoPktBlockSize = 0; // 이쪽으로

            byte[] outData = new byte[outData_BufSz];
            if (flag == 1)
            {
                //tryparse_send(ref outData, ref outData_BufSz, 0, 0x19);
                //// 최초 셋팅값 전송
                //tryparse_send(ref outData, ref outData_BufSz, variables_FPGA.CountMode_0x11, 0x11);                                 // Mode
                //tryparse_send(ref outData, ref outData_BufSz, variables_FPGA.IntervalTime_0x13, 0x12);                                 // Interval Time
                //tryparse_send(ref outData, ref outData_BufSz, (int)(variables_FPGA.TrigVref_0x12 / 5000.0 * 65535.0 + 0.5), 0x13); // Trig Vref
                //tryparse_send(ref outData, ref outData_BufSz, variables_FPGA.TRIG1_PreMargin_0x14, 0x14);                                 // TRIG 1 Pre-margin
                //tryparse_send(ref outData, ref outData_BufSz, variables_FPGA.TRIG1_PostMargin_tb_0x15, 0x15);                                 // TRIG 1 Post-margin
                //tryparse_send(ref outData, ref outData_BufSz, variables_FPGA.TRIG2_PreMargin_0x16, 0x16);                                 // TRIG 2 Pre-margin
                //tryparse_send(ref outData, ref outData_BufSz, variables_FPGA.TRIG2_PostMargin_0x17, 0x17);                                 // TRIG 2 Post-margin
                //tryparse_send(ref outData, ref outData_BufSz, (int)(1.0 / variables_FPGA.ADC_Rate_0x18 * 1000.0), 0x18);           // TRIG ADC Rate

                tryparse_send(ref outData, ref outData_BufSz, 0, 0x19);                                       // 0x19 ADC Rate 시작전에 0던지기
                // 최초 셋팅값 전송
                tryparse_send(ref outData, ref outData_BufSz, tb_0x11, 0x11);                                 // Mode
                tryparse_send(ref outData, ref outData_BufSz, tb_0x13, 0x12);                                 // Interval Time
                tryparse_send(ref outData, ref outData_BufSz, (int)(tb_0x12 / 5000.0 * 65535.0 + 0.5), 0x13); // Trig Vref
                tryparse_send(ref outData, ref outData_BufSz, tb_0x14, 0x14);                                 // TRIG 1 Pre-margin
                tryparse_send(ref outData, ref outData_BufSz, tb_0x15, 0x15);                                 // TRIG 1 Post-margin
                tryparse_send(ref outData, ref outData_BufSz, tb_0x16, 0x16);                                 // TRIG 2 Pre-margin
                tryparse_send(ref outData, ref outData_BufSz, tb_0x17, 0x17);                                 // TRIG 2 Post-margin
                tryparse_send(ref outData, ref outData_BufSz, (int)(1.0 / tb_0x18 * 1000.0), 0x18);           // TRIG ADC Rate

                tryparse_send(ref outData, ref outData_BufSz, 1, 0x19); // TRIG ADC Rate
                //tryparse_send(ref outData, ref outData_BufSz, 1, 0x1a); // TRIG ADC Rate
                // 런신호 (768 / 770)
                reset_send_buffer(ref outData, ref outData_BufSz);
                temp_send_buffer[768] = 1;
                temp_send_buffer[770] = 1;

                Buffer.BlockCopy(temp_send_buffer, 0, outData, 0, temp_send_buffer.Length * 4);

                EndPoint.TimeOut = 10;
                for (int i = 0; i < 100; ++i)
                {
                    if (EndPoint.XferData(ref outData, ref outData_BufSz))
                        break;
                    Thread.Sleep(10);
                }
            }
            else if (flag == 2)
            {
                // Final call (768 / 770)
                reset_send_buffer(ref outData, ref outData_BufSz);
                temp_send_buffer[768] = 1;
                temp_send_buffer[769] = 1;
                temp_send_buffer[770] = 1;

                Buffer.BlockCopy(temp_send_buffer, 0, outData, 0, temp_send_buffer.Length * 4);

                EndPoint.TimeOut = 10;
                int i = 0;
                for (i = 0; i < 100; ++i)
                {
                    if (EndPoint.XferData(ref outData, ref outData_BufSz))
                        break;
                    //EndPoint.XferData(ref outData, ref outData_BufSz);
                    Thread.Sleep(10);
                }
                Trace.WriteLine($"HY : flag2 {i}");
            }
            else if (flag == 3)
            {
                Trace.WriteLine("HY : [Try] Reset_send_buffer [3]");
                // Stop (전부 0)
                reset_send_buffer(ref outData, ref outData_BufSz);

                EndPoint.TimeOut = 10;
                int i = 0;
                for (i = 0; i < 100; ++i)
                {
                    if (EndPoint.XferData(ref outData, ref outData_BufSz))
                        break;
                    Thread.Sleep(10);
                }
                Trace.WriteLine($"HY : [Try] 0000 send done : {i}");
            }

            EndPointListSelectIdx = 1;
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] parsing - 상태에 따라 파싱
        //------------------------------+---------------------------------------------------------------
        private void check_parsing_state(byte result)
        {
            switch (parsing_status)
            {
                case PS.CUT_TRASH: // FE만날때 까지 버리기 -> FE면 다음FE확인(FE_SECOND)
                    if (result == 0xFE)
                    {
                        parsing_status = PS.FE_SECOND;
                        //file_main.WriteLine();
                        //file_main.Write($"{result:X2} ");
                    }
                    else
                    {
                        //file_main.Write($"{result:X2} ");
                    }
                    break;
                case PS.FE_FIRST: // 다시 첫 FE가 등장했는가?
                    if (result == 0xFE)
                    {
                        //file_main.Write($"\n{result:X2} ");
                        read_buffer_count++;
                        //Buffer.BlockCopy(DATA_BUFFER, 0, SDATA_BUFFER, 0, DATA_BUFFER.Length); // 514 byte버퍼 -> 257 short버퍼
#if false
                        //20210111 마지막줄 저장안되는 버그 수정을 위해 옮김
                        Buffer.BlockCopy(DATA_BUFFER, 0, SDATA_BUFFER, 0, DATA_BUFFER.Length); // 334개 쌓고 왜 ???
#endif

                        read_fefe_count++;
                        /// 실제 출력
                        //if (thread_run) // 1sec start mode
                        //	change_state();
                        //if (!thread_run) // convert modef
#if false
                        parsing_save();
#endif
                        parsing_status = PS.FE_SECOND;
                    }
                    else
                    {
                        parsing_status = PS.CUT_TRASH;
                        //file_main.WriteLine($"\n##CUT_TRASH##");
                        //file_main.Write($"{result:X2} ");
                    }
                    // 읽은 DATA초기화, count 초기화
                    break;
                case PS.FE_SECOND: // 두번째 FE 인가? -> FE면 DATA_READ / 아니면 CUT_TRASH
                    if (result == 0xFE)
                    {
                        parsing_status = PS.DATA_READ;
                        //file_main.Write($"{result:X2} ");
                    }
                    else
                    {
                        parsing_status = PS.CUT_TRASH;
                        //file_main.WriteLine($"\n##CUT_TRASH##");
                        //file_main.Write($"{result:X2} ");
                    }
                    break;
                case PS.DATA_READ: // 257개 다 읽었는가?
                                   //file_main.Write($"{result:X2} ");
                    DATA_BUFFER[DATA_BUFFER_read_count] = result;
                    DATA_BUFFER_read_count++;
                    if (DATA_BUFFER_read_count == 334)
                    {
                        DATA_BUFFER_read_count = 0;
                        parsing_status = PS.FE_FIRST;
                        Buffer.BlockCopy(DATA_BUFFER, 0, SDATA_BUFFER, 0, DATA_BUFFER.Length); // 334개 쌓고 왜 ???
                        parsing_save();
                    }
                    break;
                default:
                    break;
            }
        }

        private void usb_setting(int flag)
        {
            SetOutputData(flag);
        }


        private void init_bin_save_dir()
        {
            DIR_BinaryFile = "";
            //DIR_MainFile = "";
            //DIR_ADCFile = "";

            // 1. 비어있는경우 기본경로로 대체
            if (DIR_BinaryFile.Length == 0)
                DIR_BinaryFile = Application.StartupPath + "\\data\\data.bin";

            DIR_BinaryFile = GetUniqueFilePath(DIR_BinaryFile);

            if (Directory.Exists(Path.GetDirectoryName(DIR_BinaryFile)) == false)
                Directory.CreateDirectory(Path.GetDirectoryName(DIR_BinaryFile));
        }


        public static string GetUniqueFilePath(string filepath)
        {
            if (File.Exists(filepath))
            {
                string folder = Path.GetDirectoryName(filepath);
                string filename = Path.GetFileNameWithoutExtension(filepath);
                string extension = Path.GetExtension(filepath);
                int number = 1;

                Match regex = Regex.Match(filepath, @"(.+) \((\d+)\)\.\w+");

                if (regex.Success)
                {
                    filename = regex.Groups[1].Value;
                    number = int.Parse(regex.Groups[2].Value);
                }

                do
                {
                    number++;
                    filepath = Path.Combine(folder, string.Format("{0} ({1}){2}", filename, number, extension));
                }
                while (File.Exists(filepath));
            }

            return filepath;
        }


        //------------------------------+---------------------------------------------------------------
        // [함수] INI 관련
        //------------------------------+---------------------------------------------------------------
        string INI_PATH = Application.StartupPath + @"\\setting.ini";
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int sise, string filePath);

        private void WriteINI(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, INI_PATH);
        }
        private string ReadINI(string section, string key)
        {
            StringBuilder sb = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", sb, sb.Capacity, INI_PATH);
            return sb.ToString();
        }

        private void Read_current_ini()
        {
            string rb = ReadINI("setting", "Mode");
            if (string.Compare("Continue", rb, true) == 0)
            {
                countMode = CountMode.Continue;
                tb_0x11 = 1;
            }

            else if (string.Compare("TRIG1", rb, true) == 0)
            {
                countMode = CountMode.TRIG1;
                tb_0x11 = 2;
            }
            else
            {
                countMode = CountMode.TRIG2;
                tb_0x11 = 3;
            }

            tb_0x12 = Convert.ToUInt16(ReadINI("setting", "0x12"));
            tb_0x13 = Convert.ToUInt16(ReadINI("setting", "0x13"));
            tb_0x14 = Convert.ToUInt16(ReadINI("setting", "0x14"));
            tb_0x15 = Convert.ToUInt16(ReadINI("setting", "0x15"));
            tb_0x16 = Convert.ToUInt16(ReadINI("setting", "0x16"));
            tb_0x17 = Convert.ToUInt16(ReadINI("setting", "0x17"));
            tb_0x18 = Convert.ToUInt16(ReadINI("setting", "0x18"));

            //switch (countMode)
            //{
            //    case CountMode.Continue:
            //        variables_FPGA.CountMode_0x11 = 0;
            //        break;
            //    case CountMode.TRIG1:
            //        variables_FPGA.CountMode_0x11 = 1;
            //        break;
            //    case CountMode.TRIG2:
            //        variables_FPGA.CountMode_0x11 = 2;
            //        break;
            //}

            //variables_FPGA.TrigVref_0x12 = Convert.ToUInt16(ReadINI("setting", "0x12"));
            //variables_FPGA.IntervalTime_0x13 = Convert.ToUInt16(ReadINI("setting", "0x13"));
            //variables_FPGA.TRIG1_PreMargin_0x14 = Convert.ToUInt16(ReadINI("setting", "0x14"));
            //variables_FPGA.TRIG1_PostMargin_tb_0x15 = Convert.ToUInt16(ReadINI("setting", "0x15"));
            //variables_FPGA.TRIG2_PreMargin_0x16 = Convert.ToUInt16(ReadINI("setting", "0x16"));
            //variables_FPGA.TRIG2_PostMargin_0x17 = Convert.ToUInt16(ReadINI("setting", "0x17"));
            //variables_FPGA.ADC_Rate_0x18 = Convert.ToUInt16(ReadINI("setting", "0x18"));

            //Debug.WriteLine($"");
            //Debug.WriteLine($"Setting Variables Info");
            //Debug.WriteLine($"{variables_FPGA.CountMode_0x11}");
            //Debug.WriteLine($"{variables_FPGA.TrigVref_0x12}");
            //Debug.WriteLine($"{variables_FPGA.IntervalTime_0x13}");
            //Debug.WriteLine($"{variables_FPGA.TRIG1_PreMargin_0x14}");
            //Debug.WriteLine($"{variables_FPGA.TRIG1_PostMargin_tb_0x15}");
            //Debug.WriteLine($"{variables_FPGA.TRIG2_PreMargin_0x16}");
            //Debug.WriteLine($"{variables_FPGA.TRIG2_PostMargin_0x17}");
            //Debug.WriteLine($"{variables_FPGA.ADC_Rate_0x18}");
            //Debug.WriteLine($"");
            //Debug.WriteLine($"");


        }

        private void Write_current_ini()
        {
            //WriteINI("setting", "0x12", Convert.ToString(variables_FPGA.TrigVref_0x12));
            //WriteINI("setting", "0x13", Convert.ToString(variables_FPGA.IntervalTime_0x13));
            //WriteINI("setting", "0x14", Convert.ToString(variables_FPGA.TRIG1_PreMargin_0x14));
            //WriteINI("setting", "0x15", Convert.ToString(variables_FPGA.TRIG1_PostMargin_tb_0x15));
            //WriteINI("setting", "0x16", Convert.ToString(variables_FPGA.TRIG2_PreMargin_0x16));
            //WriteINI("setting", "0x17", Convert.ToString(variables_FPGA.TRIG2_PostMargin_0x17));
            //WriteINI("setting", "0x18", Convert.ToString(variables_FPGA.ADC_Rate_0x18));

            WriteINI("setting", "0x12", Convert.ToString(tb_0x12));
            WriteINI("setting", "0x13", Convert.ToString(tb_0x13));
            WriteINI("setting", "0x14", Convert.ToString(tb_0x14));
            WriteINI("setting", "0x15", Convert.ToString(tb_0x15));
            WriteINI("setting", "0x16", Convert.ToString(tb_0x16));
            WriteINI("setting", "0x17", Convert.ToString(tb_0x17));
            WriteINI("setting", "0x18", Convert.ToString(tb_0x18));

            if (countMode == CountMode.Continue)
                WriteINI("setting", "Mode", "Continue");
            else if (countMode == CountMode.TRIG1)
                WriteINI("setting", "Mode", "TRIG1");
            else
                WriteINI("setting", "Mode", "TRIG2");
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

                xBufs[j] = new byte[BufSz]; // 262144

                //initialize the buffer with initial value 0xA5
                for (int iIndex = 0; iIndex < BufSz; iIndex++)
                    xBufs[j][iIndex] = DefaultBufInitValue;

                int sz = Math.Max(CyConst.OverlapSignalAllocSize, sizeof(OVERLAPPED)); // 32
                oLaps[j] = new byte[sz];                                               // 32
                pktsInfo[j] = new ISO_PKT_INFO[PPX];                                   // 16

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

            if (is_measuring == false)
            {
                {
                    bRunning = false;

                    t2 = DateTime.Now;
                    elapsed = t2 - t1;
                }
            }
        }


        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | Search the device with VID-PID 04b4-00F1 and if found, select the end point
        //------------------------------+---------------------------------------------------------------   
        private void SetDevice(bool bPreserveSelectedDevice)
        {
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
            EndPointList.Clear();       // 이거 주석되있어서 계속추가되는 버그있던데, 예제소스가 왜그런진 모르겠음
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
            }


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




        #endregion

        #region 미분류 2

        public event EventHandler USBChangeHandler;
        private void USBChange()
        {
            if (USBChangeHandler != null)
            {
                USBChangeHandler(this, EventArgs.Empty);
            }
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


        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | The callback routine delegated to updateUI.
        //------------------------------+---------------------------------------------------------------
        private void StatusUpdate()
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
        // [이벤트] 콤보박스들 값 변경시 이벤트들
        //------------------------------+---------------------------------------------------------------
        private void event_EndPointsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            EndPointsComboBox_SelectedIndexChanged(sender, e);
        }

        private void event_DevicesComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            DeviceComboBox_SelectedIndexChanged(sender, e);
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

        private void DeviceComboBox_SelectedIndexChanged(object sender, EventArgs e) // 콤보
        {
            #region WinForm 메인 윈도우에서 디바이스 콤보박스가 변경될떄마다 이벤트 발생하는 부분 -> MainViewModel에서 바뀌는 것 감지하는 것 // 또는 없애는 것으로 합의
            //MyDevice = null;
            //EndPoint = null;
            //SetDevice(true);
            #endregion
        }

        #endregion

        #endregion
    }
}
