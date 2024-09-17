using CyUSB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#pragma warning disable CS0414, CS0169, CS0649, CS8600, CS8604, CS8625, CS8602, CS8618, CS0219, CS8605, CS0649, CS0168, CS0649
namespace PG.Fpga.Cruxell
{

    internal class CruxellBase : IDisposable
    {
        // additional variables
        const string T_Delay = "2300";
        const string F_Value = "0.5";
        const string SmoothingWindow = "32";
        //------------------------------+---------------------------------------------------------------
        //[StructLayout(LayoutKind.Sequential)]
        public int setting_sleep_time = 10;
        public struct STRUCT_INI
        {
            public int mode;
            public int smoothing;
            public int t_delay;
            public float f_value;   // kiwa72(2022.11.09 h15) - 변수명 변경 f -> f_value
            public int t_spectrum_period;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 144)]
            public int[] high_thres_pulse;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 144)]
            public int[] low_thres_pulse;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 144)]
            public int[] low_thres_spectrum;

            // kiwa72(2022.11.09 h15) - ini 추가
            public int h_thres;
            public int l_thres;

            public void PrintIniValues()
            {
                Trace.WriteLine($"mode : {mode}");
                Trace.WriteLine($"smoothing : {smoothing}");
                Trace.WriteLine($"t_delay : {t_delay}");
                Trace.WriteLine($"f_value : {f_value}");
                Trace.WriteLine($"t_spectrum_period : {t_spectrum_period}");
                Trace.WriteLine($"h_thres : {h_thres}");
                Trace.WriteLine($"l_thres : {l_thres}");
                Trace.WriteLine($"high_thres_pulse : {string.Join(",", high_thres_pulse)}");
                Trace.WriteLine($"low_thres_pulse : {string.Join(",", low_thres_pulse)}");
                Trace.WriteLine($"low_thres_spectrum : {string.Join(",", low_thres_spectrum)}");
            }
        };

        public STRUCT_INI struct_ini = new STRUCT_INI();

        // [변수] parsing 관련
        //public const int BYTES_PER_CH_OF_PULSE = 16;// pre 2 t puse2, v pulse2  t time 8 // ch 2 pre 2 pulse 2 time 8

        // kiwa72(2022.10.22) - 최소 코어 개수
        public const int CORE_MIN_CNT = 4;

        // kiwa72(2022.10.22) - 최대 코어 개수
        public const int CORE_MAX_CNT = 12;

        // Binary에서 데이터 시작을 0xFEFEFEFE(4Byte)로 파악함
        public const int BYTES_OF_SOF = 4;

        // kiwa72 추가(2022.10.21 h21) - 원본 ~.bin 파일 헤더 크기
        public const int ORG_BIN_HEAD_SIZE = 14;    // 14 Byte

        // kiwa72 추가(2022.10.21 h21) - Pulse Mode 에서 데이터 크기 및 구조
        public const int SEC_TIME = 4;      // 4 Byte
        public const int CH_NUMBER = 2;     // 2 Byte
        public const int PRE_DATA = 2;      // 2 Byte
        public const int T_PLUSE_DATA = 2;  // 2 Byte
        public const int V_PLUSE_DATA = 2;  // 2 Byte
        public const int T_PLUSE_TIME = 4;  // 4 byte

        // kiwa72(2022.10.21 h21) - Pulse Mode 에서 ch당, SOF 4바이트를 제외한 데이터 바이트 수
        public const int BYTES_PER_CH_OF_PULSE = (SEC_TIME + CH_NUMBER + PRE_DATA + T_PLUSE_DATA + V_PLUSE_DATA + T_PLUSE_TIME);
        // kiwa72(2022.10.21 h21) - 패킷 기본 크기
        public const int BYTES_SIZE_CH_PACKET = BYTES_OF_SOF + BYTES_PER_CH_OF_PULSE;

        // master보드에  연결되는 slave 보드의 총 수
        public const int COUNT_OF_BOARD = 8;
        // 보드당 채널 수  
        public const int CH_PER_BOARD = 18;
        //	slave 보드 수 * 채널 수 
        public const int COUNT_OF_TOTAL_CH = 144;
        // kiwa72(2022.11.09 h15) - 마스터 채널 1개 추가
        public const int ADD_MASTER_CH = 1;

        public const int BYTES_PER_CYCLE_OF_PULSE_MODE = BYTES_PER_CH_OF_PULSE * 1; //	현재는  BYTES_PER_CH_OF_PULSE 와 동일함
        public const int COUNT_OF_SPECTRUM_STEP = 16384;    //	spectrum 모드일 경우  스펙트럼의 총 단계  (0~16383)
        public const int BYTE_PER_CHUNK_OF_SPECTRUM = sizeof(ushort) * 2 * COUNT_OF_BOARD;  //	1채널당  스펙트럼 데이터 수
        public const int BYTE_PER_CH_OF_SPECTRUM = sizeof(ushort) * COUNT_OF_SPECTRUM_STEP; //	스텝당 데이터 수
        public const int BYTE_PER_CYCLE_OF_SPECTRUM = COUNT_OF_TOTAL_CH * BYTE_PER_CH_OF_SPECTRUM + sizeof(double); //	"스펙트럼모드 1cycle 총 데이터량  + 스펙트럼 데이터 + 더블형 자료 1
                                                                                                                    //2022.09.04 기준 정의된 형태이며 펌웨어단에서 실제 구현되지 않았음"
        public const int COMMAND_RUN_INDEX = 768;   //	보드에 보내는 명령 배열에서    런 신호를 주는 index
        public const int COMMAND_FINAL_CALL_INDEX = 769;    //	보드에 보내는 명령 배열에서    finall 콜을 요청하는 index
        public const int COMMAND_LED_INDEX = 770;   //	보드에 보내는 명령 배열에서    검증용 LED를 on/off하는 index
        public const int SMOOTHING_INDEX = 0x030;   //	보드에 보내는 명령 배열에서    smoothing 항목 세팅하는 index
        public const int T_DELAY_INDEX = 0x33;  // 보드에 보내는 명령 배열에서    t_delay 항목 세팅하는 index
        public const int F_VALUE_INDEX = 0x34;  // 보드에 보내는 명령 배열에서    f_value 항목 세팅하는 index
        public const int T_SPECTRUM_PERIOD_INDEX = 0x35;    //	보드에 보내는 명령 배열에서    spectrum_period 항목 세팅하는 index
        public const int INPUT_HIGH_THRESHOLD_INDEX_P = 0x40;   //	pulse모드  input high threshold 첫번째 값을 설정하는 index( 총 144개)
        public const int INPUT_LOW_THRESHOLD_INDEX_P = 0x41;    //	pulse모드  input low threshold 첫번째 값을 설정하는 index(총 144개)
        public const int INPUT_LOW_THRESHOLD_INDEX_S = 0x41;    //	spectrum 모드  input low threshold 첫번째 값을 설정하는 index(총 144개)
        public const int BOARD_MODE_INDEX = 0x11;   //	현재 동작모드를 설정하는 index( 현재 pulse모드만 구현됨)
        public const int SMOOTHING_DEFAULT = 16;    //	smoothing 기본값
        public const int INPUT_HIGH_DEFAULT = 16383;    //	input high threshold 기본값
        public const int INPUT_LOW_DEFAULT = 0; //	input low threashold 기본값
        public const int F_DEFAULT = 10;    //	f 기본값
        public const int T_DELAY_DEFAULT = 0;   //	t_delay 기본값

        // kiwa72(2022.11.09 h15) - 추가 H_THRES_INDEX, L_THRES_INDEX
        public const int H_THRES_INDEX = 0x0160;    // 보드에 보내는 명령 배열에서 [trig] h_thres 항목 세팅하는 index
        public const int L_THRES_INDEX = 0x0161;    // 보드에 보내는 명령 배열에서 [trig] l_thres 항목 세팅하는 index

        public readonly int[] Smoothing_step = { 0, 1, 2, 3, 4, 5, 6 }; //smoothing 함수  셀제 펌웨어로 보내는 스텝



        public enum enBOARD_MODE : int
        {
            bmPulse = 0,
            bmSpectrum = 1,
            bmCount = 2,
            bmADC = 3
        }

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
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct st_pulse_ch_data
        {
            public short pre_data;
            public short t_pulse_data;
            public short post_data;
            public short v_pulse_data;
            public double v_pulse_time;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct st_spectrum_cycle_data
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16384)] public short[] mvs;
            public double time;
        }
        byte[] DATA_BUFFER; // FEFE제외하고 데이터만 담은 294 data buffer
        ushort[] SDATA_BUFFER; // short 버퍼
        ushort[,] SORT_BUFFER = new ushort[144, 16384]; // 소팅용 버퍼
        int DATA_BUFFER_read_count; // 514 DATA_BUFFER 채울때 count
                                    //------------------------------+---------------------------------------------------------------
                                    // [변수] 파일저장
                                    //------------------------------+---------------------------------------------------------------
        string file_main_path;
        public System.Timers.Timer save_timer { get; set; }
        StreamWriter file_single0;// 최대 16개  cpu 코어 스레드 병렬작업용 streamwriter
        StreamWriter file_single1;
        StreamWriter file_single2;
        StreamWriter file_single3;
        StreamWriter file_single4;
        StreamWriter file_single5;
        StreamWriter file_single6;
        StreamWriter file_single7;
        StreamWriter file_single8;
        StreamWriter file_single9;
        StreamWriter file_singleA;
        StreamWriter file_singleB;
        StreamWriter file_singleC;
        StreamWriter file_singleD;
        StreamWriter file_singleE;
        StreamWriter file_singleF;

        StreamWriter file_coin;
        StreamWriter file_cs2_scatter; // 하위
        StreamWriter file_cs2_absorber; // 상위



        public TextBoxReplica TB_0x16 = new TextBoxReplica("TB_0x16"); // T_Delay
        public TextBoxReplica TB_0x17 = new TextBoxReplica("TB_0x17"); // F_Value
        public TextBoxReplica TB_0x18 = new TextBoxReplica("TB_0x18");

        public ComboBoxReplica cmb_smwindow_pulse = new ComboBoxReplica("cmb_smwindow_pulse");

        public RadioButtonReplica RB_coin = new RadioButtonReplica("RB_coin", "Setting"); // pulse mode
        public RadioButtonReplica RB_single = new RadioButtonReplica("RB_single", "Setting"); // pulse mode
        public RadioButtonReplica RB_single_coin_1 = new RadioButtonReplica("RB_single_coin_1", "Setting");
        public RadioButtonReplica RB_single_coin_2 = new RadioButtonReplica("RB_single_coin_2", "Setting");

        public ComboBoxReplica EndPointsComboBox = new ComboBoxReplica("EndpointComboBox");
        public ComboBoxReplica DevicesComboBox = new ComboBoxReplica("DevicesComboBox");
        public ComboBoxReplica PpxBox = new ComboBoxReplica("PPXBox");
        public ComboBoxReplica QueueBox = new ComboBoxReplica("QueueBox");

        public TextBoxReplica txt_setting_sleep_time = new TextBoxReplica("txt_setting_sleep_time");

        public TextBoxReplica TB_file_name = new TextBoxReplica("TB_file_name");

        public ButtonReplica StartBtn = new ButtonReplica("StartBtn");

        public TextBoxReplica materialLabel8 = new TextBoxReplica("materialLabel8");

        public CruxellBase()
        {
            // 1. InitializeComponent
            struct_ini.high_thres_pulse = new int[144];
            struct_ini.low_thres_pulse = new int[144];
            struct_ini.low_thres_spectrum = new int[144];
            materialLabel8.Text = version_string;
            this.EndPointsComboBox.SelectedIndexChanged += new System.EventHandler(this.EndPointsComboBox_SelectedIndexChanged);
            DevicesComboBox.SelectedIndexChanged += new System.EventHandler(this.DeviceComboBox_SelectedIndexChanged);
            PpxBox.SelectedIndexChanged += new System.EventHandler(this.PpxBox_SelectedIndexChanged);
            cmb_smwindow_pulse.SelectedIndexChanged += new System.EventHandler(this.cmb_smwindow_pulse_SelectedIndexChanged);
            RB_coin.CheckedChanged += new System.EventHandler(this.RB_coin_CheckedChanged);
            RB_coin.Checked = true;

            cmb_smwindow_pulse.Items.Add("0");
            cmb_smwindow_pulse.Items.Add("2");
            cmb_smwindow_pulse.Items.Add("4");
            cmb_smwindow_pulse.Items.Add("8");
            cmb_smwindow_pulse.Items.Add("16");
            cmb_smwindow_pulse.Items.Add("32");
            cmb_smwindow_pulse.Items.Add("64");

            PpxBox.Items.Add("1");
            PpxBox.Items.Add("2");
            PpxBox.Items.Add("4");
            PpxBox.Items.Add("8");
            PpxBox.Items.Add("16");
            PpxBox.Items.Add("32");
            PpxBox.Items.Add("64");
            PpxBox.Items.Add("128");
            PpxBox.Items.Add("256");
            PpxBox.Items.Add("512");

            QueueBox.Items.Add("1");
            QueueBox.Items.Add("2");
            QueueBox.Items.Add("4");
            QueueBox.Items.Add("8");
            QueueBox.Items.Add("16");
            QueueBox.Items.Add("32");
            QueueBox.Items.Add("64");
            QueueBox.Items.Add("128");

            cmb_smwindow_pulse.Text = SmoothingWindow;
            TB_0x16.Text = T_Delay;
            TB_0x17.Text = F_Value;

            init_data_and_buffer();//ini파일도 세팅함 
            init_USB();
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] DATA
        //------------------------------+---------------------------------------------------------------
        private void init_data_and_buffer()
        {
            // 1. 버퍼 초기화
            DATA_BUFFER = new byte[BYTE_PER_CYCLE_OF_SPECTRUM];// 리얼타임수집에서는 다 안쓰지만 파싱할땐 다 쓴다.
            SDATA_BUFFER = new ushort[BYTE_PER_CYCLE_OF_SPECTRUM / sizeof(ushort)];// 리얼타임수집에서는 다 안쓰지만 파싱할땐 다 쓴다.

            // 2. 초기값 설정 (ini 확인 후 있으면 적용, 없으면 작성)
            if (System.IO.File.Exists(INI_PATH))
            {
                Read_current_ini(false);//from start sw;
                                        //MessageBox.Show($"t delay{struct_ini.t_delay.ToString()} f {struct_ini.f_value.ToString()} period{struct_ini.t_spectrum_period.ToString()} smoothing{struct_ini.smoothing.ToString()}");
                TB_0x16.Text = struct_ini.t_delay.ToString();
                TB_0x17.Text = struct_ini.f_value.ToString();
                TB_0x18.Text = struct_ini.t_spectrum_period.ToString();
                cmb_smwindow_pulse.SelectedIndex = struct_ini.smoothing;
            }
            else
            {
                Set_first_setting();
                Write_current_ini(false);//don't update from gui
                TB_0x16.Text = struct_ini.t_delay.ToString();// "0";//t_delay
                TB_0x17.Text = struct_ini.f_value.ToString();// "0";//f
                TB_0x18.Text = struct_ini.t_spectrum_period.ToString();// "20";//t_spectrum_period
                cmb_smwindow_pulse.SelectedIndex = 0;//smoothing
                RB_coin.Checked = true;
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] ParsingThread
        //------------------------------+---------------------------------------------------------------
        private void ParsingThread()
        {
            Trace.WriteLine("HY : ParsingThread start");
            using (StreamWriter writer = new StreamWriter(File.Open(file_main_path, FileMode.CreateNew)))
            {
                switch (struct_ini.mode)
                {//write할때는 struct_ini.mode
                    case 0:// enBOARD_MODE.bmPulse:
                        writer.Write("PMODE\n");
                        break;
                    case 1:// enBOARD_MODE.bmSpectrum:
                        writer.Write("SMODE\n");
                        break;
                    case 2:// enBOARD_MODE.bmCount:
                        writer.Write("CMODE\n");
                        break;
                    case 3:// enBOARD_MODE.bmADC:
                        writer.Write("AMODE\n");
                        break;
                }
            }

            while (thread_run)
            {
                byte[] Item;

                while (test_buffer.TryTake(out Item, 1)) // write to dump file
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Open(file_main_path, FileMode.Append)))
                    {
                        writer.Write(Item);
                    }

                    Thread.Sleep(0);
                }
            }
        }


        private void RealtimeParsingThread()
        {
            Trace.WriteLine("CSH : ParsingThread start");

            bool flagChunk = false;
            byte[] chunk = new byte[20];
            int chunkIndex = 0;
            int feCount = 0;

            // print bounded buffer size
            Trace.WriteLine("CSH : RealtimeParsingBuffer bounded capacity : " + RealtimeParsingBuffer.BoundedCapacity);
              
            while (thread_run)
            {
                byte[] Item;

                while (RealtimeParsingBuffer.TryTake(out Item, 1)) // write to dump file
                {
                    int i = 0; 
                    while (!flagChunk && i < Item.Length)
                    {
                        if (Item[i] == 254)
                        {
                            feCount++;
                            if (feCount == 4)
                            {
                                if (i + 1 >= Item.Length)
                                {
                                    i++;
                                    break;
                                }
                                if (Item[i + 1] != 254)
                                {
                                    flagChunk = true;
                                    feCount = 0;
                                    Trace.WriteLine("CSH: Find FEFEFEFE");
                                    i++;
                                    break;
                                }
                                else
                                {
                                    feCount = 0;
                                }
                            }
                        }
                        else
                        {
                            feCount = 0;
                        }
                        i++;
                    }
                    if (flagChunk)
                    {
                        // seperate chunk by 20 bytes
                        while (i < Item.Length)
                        {
                            chunk[chunkIndex] = Item[i];
                            chunkIndex++;
                            i++;
                            if (chunkIndex == 20)
                            {   // write to Trace
                                chunkIndex = 0;
                                // check last 4 bytes FE, FE, FE, FE
                                if (!(chunk[chunk.Length - 4] == 254 && chunk[chunk.Length - 3] == 254 && chunk[chunk.Length - 2] == 254 && chunk[chunk.Length - 1] == 254))
                                {
                                    Trace.WriteLine("CSH: Chunk is not end with FEFEFEFE. Retry to serch FEFEFEFE");
                                    flagChunk = false;
                                    break;
                                }
                                ParsedBuffer.Add(chunk);
                                chunk = new byte[20];
                            }
                        }
                    }
                    Thread.Sleep(0);
                }
            }            
        }

        private void DaqDataGeneratorThread()
        {
            DaqDataList = new List<DaqData>();
            Trace.WriteLine("CSH : DaqDataGeneratorThread start");
            while (thread_run)
            {
                while (ParsedBuffer.TryTake(out byte[] Item, 1)) // write to dump file
                {
                    Debug.Assert(Item.Length == 20, "Item length is not 20");
                    DaqData daqData = new DaqData(Item);
                    DaqDataList.Add(daqData);
                }

            }

        }

        //------------------------------+---------------------------------------------------------------
        // [함수] 최초 셋팅 값 한번만 보내기 용
        //------------------------------+---------------------------------------------------------------
        int[] temp_send_buffer = new int[1024];
        private void reset_send_buffer(ref byte[] outData, ref int outData_BufSz)
        {
            for (int i = 0; i < 1024; ++i)
            {
                temp_send_buffer[i] = 0;
            }
            for (int i = 0; i < outData_BufSz; ++i)
            {
                outData[i] = 0;
            }
        }

        private unsafe void tryparse_send(ref byte[] outData, ref int outData_BufSz, int data, int add, int sub_add = -1)
        {
            reset_send_buffer(ref outData, ref outData_BufSz);

            int get_data = data;

            for (int i = 0; i < 700; ++i)
            {
                if (sub_add != -1)
                    temp_send_buffer[i] = (add << 24) | (sub_add << 16) | (get_data & 0xFFFF);
                else
                    temp_send_buffer[i] = (add << 16) | (get_data & 0xFFFFFF);
            }

            Buffer.BlockCopy(temp_send_buffer, 0, outData, 0, temp_send_buffer.Length * 4);

            EndPoint.TimeOut = 10;
            for (int i = 0; i < 1000; ++i)
            {
                if (EndPoint.XferData(ref outData, ref outData_BufSz))
                    break;
                Thread.Sleep(10);
            }
        }

        /**
		 * @brief	flag가 1 이면 시작 2면  finallcall 요청 3이면 중지
		 */
        private void SetOutputData(int flag)
        {
            // 1. out endpoint로 설정
            EndPointsComboBox.SelectedIndex = 0;

            // 2. USB endpoint 연결
            int outData_BufSz = 4096; // 한번에 USB로 수신/송신 하는  byte 수

            EndPoint.XferSize = outData_BufSz;

            if (EndPoint is CyIsocEndPoint)
            {
                IsoPktBlockSize = (EndPoint as CyIsocEndPoint).GetPktBlockSize(outData_BufSz);
            }
            else
            {
                IsoPktBlockSize = 0;
            }

            byte[] outData = new byte[outData_BufSz];

            // ** 시작 명령 **
            if (flag == 1)
            {
                Int32.TryParse(txt_setting_sleep_time.Text, out setting_sleep_time);


                // 장치 세팅 할 때는 struct_ini.mode
                switch (struct_ini.mode)
                {
                    // enBOARD_MODE.bmPulse
                    case 0:
                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.smoothing + 0, SMOOTHING_INDEX);
                        Thread.Sleep(setting_sleep_time);

                        for (int i = 0; i < COUNT_OF_TOTAL_CH; i++)
                        {
                            tryparse_send(ref outData, ref outData_BufSz, struct_ini.high_thres_pulse[i], INPUT_HIGH_THRESHOLD_INDEX_P + (i * 2));
                            Thread.Sleep(setting_sleep_time);
                            tryparse_send(ref outData, ref outData_BufSz, struct_ini.low_thres_pulse[i], INPUT_LOW_THRESHOLD_INDEX_P + (i * 2));
                            Thread.Sleep(setting_sleep_time);
                        }

                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.t_delay / 25, T_DELAY_INDEX);
                        Thread.Sleep(setting_sleep_time);
                        tryparse_send(ref outData, ref outData_BufSz, (int)(struct_ini.f_value * 100), F_VALUE_INDEX);
                        Thread.Sleep(setting_sleep_time);
                        tryparse_send(ref outData, ref outData_BufSz, 1, BOARD_MODE_INDEX);
                        Thread.Sleep(setting_sleep_time);

                        // kiwa72(2022.11.09 h15) - ????
                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.h_thres, H_THRES_INDEX);
                        Thread.Sleep(setting_sleep_time);
                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.l_thres, L_THRES_INDEX);
                        Thread.Sleep(setting_sleep_time);

                        Trace.WriteLine("HY : [Try] START PULSE MODE");
                        break;

                    case 1:// enBOARD_MODE.bmSpectrum:
                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.smoothing + 0, SMOOTHING_INDEX);
                        Thread.Sleep(10);
                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.t_spectrum_period * 125, T_SPECTRUM_PERIOD_INDEX);
                        Thread.Sleep(10);

                        for (int i = 0; i < COUNT_OF_TOTAL_CH; i++)
                        {
                            tryparse_send(ref outData, ref outData_BufSz, struct_ini.low_thres_spectrum[i], INPUT_LOW_THRESHOLD_INDEX_P + i);
                            Thread.Sleep(10);
                        }

                        tryparse_send(ref outData, ref outData_BufSz, 2, BOARD_MODE_INDEX);
                        Thread.Sleep(10);

                        // kiwa72(2022.11.09 h15) - ????
                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.h_thres, H_THRES_INDEX);
                        Thread.Sleep(10);
                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.l_thres, L_THRES_INDEX);
                        Thread.Sleep(10);

                        Trace.WriteLine("HY : [Try] START SPECTRUME MODE");
                        break;

                    case 2:// enBOARD_MODE.bmCount:
                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.smoothing + 0, SMOOTHING_INDEX);
                        Thread.Sleep(10);
                        tryparse_send(ref outData, ref outData_BufSz, 3, BOARD_MODE_INDEX);
                        Thread.Sleep(10);

                        // kiwa72(2022.11.09 h15) - ????
                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.h_thres, H_THRES_INDEX);
                        Thread.Sleep(10);
                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.l_thres, L_THRES_INDEX);
                        Thread.Sleep(10);

                        Trace.WriteLine("HY : [Try] START COUNT MODE");
                        break;

                    case 3://  enBOARD_MODE.bmADC:
                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.smoothing + 0, SMOOTHING_INDEX);
                        Thread.Sleep(10);
                        tryparse_send(ref outData, ref outData_BufSz, 4, BOARD_MODE_INDEX);
                        Thread.Sleep(10);

                        // kiwa72(2022.11.09 h15) - ????
                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.h_thres, H_THRES_INDEX);
                        Thread.Sleep(10);
                        tryparse_send(ref outData, ref outData_BufSz, struct_ini.l_thres, L_THRES_INDEX);
                        Thread.Sleep(10);

                        Trace.WriteLine("HY : [Try] START ADC MODE");
                        break;
                }

                // 런신호 (768 / 770)
                reset_send_buffer(ref outData, ref outData_BufSz);

                // Array.Clear(temp_send_buffer2, 0, temp_send_buffer2.Length);
                temp_send_buffer[768] = 1;  // 런 명령
                temp_send_buffer[769] = 0;  // 파이널 콜 명령
                temp_send_buffer[770] = 1;  // LED 활성화 명령

                Buffer.BlockCopy(temp_send_buffer, 0, outData, 0, temp_send_buffer.Length * 4);

                EndPoint.TimeOut = 10;
                for (int i = 0; i < 1000; ++i)
                {
                    if (EndPoint.XferData(ref outData, ref outData_BufSz))
                    {
                        break;
                    }

                    Thread.Sleep(10);
                }
                // 최초 셋팅값 전송
            }
            // ** 파이널 콜 요청 **
            else if (flag == 2)
            {
                // Final call (768 / 770)
                reset_send_buffer(ref outData, ref outData_BufSz);
                //Array.Clear(temp_send_buffer2, 0, temp_send_buffer2.Length);
                temp_send_buffer[768] = 1;//런 명령
                temp_send_buffer[769] = 1;// 파이널 콜 명령
                temp_send_buffer[770] = 1; // LED 활성화 명령

                Buffer.BlockCopy(temp_send_buffer, 0, outData, 0, temp_send_buffer.Length * 4);

                EndPoint.TimeOut = 10;
                int i = 0;
                for (i = 0; i < 100; ++i)
                {
                    if (EndPoint.XferData(ref outData, ref outData_BufSz))
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                Trace.WriteLine($"HY : flag2 {i}");
            }
            // ** 중지 명령 **
            else if (flag == 3)
            {
                Trace.WriteLine("HY : [Try] Reset_send_buffer [3]");
                // Stop (전부 0)
                /*
				temp_send_buffer[768] = 0;//런 명령
				temp_send_buffer[769] = 0;// 파이널 콜 명령
				temp_send_buffer[770] = 0; // LED 활성화 명령
				*/
                reset_send_buffer(ref outData, ref outData_BufSz);  // 내부적을로 768 769 770 을 0으로 만듬
                EndPoint.TimeOut = 10;
                int i = 0;
                for (i = 0; i < 1000; ++i)
                {
                    if (EndPoint.XferData(ref outData, ref outData_BufSz))
                    {
                        break;
                    }

                    Thread.Sleep(10);
                }

                Trace.WriteLine($"HY : [Try] 0000 send done : {i}");
            }

            EndPointsComboBox.SelectedIndex = 1;
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] parsing - 상태에 따라 파싱
        //------------------------------+---------------------------------------------------------------
        public int read_buffer_count = 0; // 514 데이터 버퍼 읽은 갯수 (종료시 0초기화 필요)
        public int read_fefe_count = 0; // FEFE단위 읽은 갯수
        private int read_fe_count = 0;
        private void check_parsing_state(byte result)
        {
            switch (parsing_status)
            {
                case PS.CUT_TRASH: // FE만날때 까지 버리기 -> FE면 다음FE확인(FE_SECOND)
                    if (result == 0xFE)
                    {
                        read_fe_count = 1;
                        parsing_status = PS.FE_SECOND;

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
                        read_fe_count = 1;
                        read_buffer_count++;
                        Buffer.BlockCopy(DATA_BUFFER, 0, SDATA_BUFFER, 0, DATA_BUFFER.Length); // 526 byte버퍼 -> 263 short버퍼
                        read_fefe_count++;
                        parsing_status = PS.FE_SECOND;
                    }
                    else
                    {
                        read_fe_count = 0;
                        parsing_status = PS.CUT_TRASH;

                    }
                    // 읽은 DATA초기화, count 초기화
                    break;
                case PS.FE_SECOND: // 두번째 FE 인가? -> FE면 DATA_READ / 아니면 CUT_TRASH
                    if (result == 0xFE)
                    {
                        if (++read_fe_count > 3)
                        {
                            parsing_status = PS.DATA_READ;
                            read_fe_count = 0;//clear
                        }
                        //file_main.Write($"{result:X2} ");
                    }
                    else
                    {
                        parsing_status = PS.CUT_TRASH;
                        read_fe_count = 0;

                    }
                    break;
                case PS.DATA_READ: // 294개 다 읽었는가?
                                   //file_main.Write($"{result:X2} ");
                    DATA_BUFFER[DATA_BUFFER_read_count] = result;
                    DATA_BUFFER_read_count++;
                    switch (convert_mode)
                    {//파일데이터 파싱할때는 convert_mode
                        case enBOARD_MODE.bmPulse:
                            if (DATA_BUFFER_read_count == BYTES_PER_CYCLE_OF_PULSE_MODE)
                            {
                                DATA_BUFFER_read_count = 0;
                                parsing_status = PS.FE_FIRST;
                                Buffer.BlockCopy(DATA_BUFFER, 0, SDATA_BUFFER, 0, BYTES_PER_CYCLE_OF_PULSE_MODE);
                                parsing_save(BYTES_PER_CYCLE_OF_PULSE_MODE);//in the check_parsing_state pulse
                            }
                            break;
                        case enBOARD_MODE.bmSpectrum:
                            if (DATA_BUFFER_read_count == BYTE_PER_CYCLE_OF_SPECTRUM)
                            {//144개채널 16384개 모두  받았을때
                                DATA_BUFFER_read_count = 0;
                                parsing_status = PS.FE_FIRST;
                                Buffer.BlockCopy(DATA_BUFFER, 0, SDATA_BUFFER, 0, BYTE_PER_CYCLE_OF_SPECTRUM);
                                parsing_save(BYTE_PER_CYCLE_OF_SPECTRUM);//in the check_parsing_state spectrum
                            }
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
        //------------------------------+---------------------------------------------------------------
        // [함수] parsing - File save
        //------------------------------+---------------------------------------------------------------
        StringBuilder sb = new StringBuilder(65536);
        void mode_single(short b)
        {
            ushort us_temp = 0;
            switch (b)
            {
                case 0x00: // save data on 0 file
                    for (int i = 0; i < 9; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_single0.Write($"{us_temp,5} ");
                    }
                    file_single0.Write("\n");
                    break;

                case 0x01: // save data on 1 file
                    for (int i = 9; i < 18; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_single1.Write($"{us_temp,5} ");
                    }
                    file_single1.Write("\n");
                    break;

                case 0x02: // save data on 2 file
                    for (int i = 18; i < 27; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_single2.Write($"{us_temp,5} ");
                    }
                    file_single2.Write("\n");
                    break;

                case 0x03: // save data on 3 file
                    for (int i = 27; i < 36; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_single3.Write($"{us_temp,5} ");
                    }
                    file_single3.Write("\n");
                    break;

                case 0x04: // save data on 4 file
                    for (int i = 36; i < 45; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_single4.Write($"{us_temp,5} ");
                    }
                    file_single4.Write("\n");
                    break;

                case 0x05: // save data on 5 file
                    for (int i = 45; i < 54; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_single5.Write($"{us_temp,5} ");
                    }
                    file_single5.Write("\n");
                    break;

                case 0x06: // save data on 6 file
                    for (int i = 54; i < 63; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_single6.Write($"{us_temp,5} ");
                    }
                    file_single6.Write("\n");
                    break;

                case 0x07: // save data on 7 file
                    for (int i = 63; i < 72; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_single7.Write($"{us_temp,5} ");
                    }
                    file_single7.Write("\n");
                    break;

                case 0x08: // save data on 8 file
                    for (int i = 72; i < 81; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_single8.Write($"{us_temp,5} ");
                    }
                    file_single8.Write("\n");
                    break;

                case 0x09: // save data on 9 file
                    for (int i = 81; i < 90; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_single9.Write($"{us_temp,5} ");
                    }
                    file_single9.Write("\n");
                    break;

                case 0x0a: // save data on a file
                    for (int i = 90; i < 99; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_singleA.Write($"{us_temp,5} ");
                    }
                    file_singleA.Write("\n");
                    break;

                case 0x0b: // save data on b file
                    for (int i = 99; i < 108; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_singleB.Write($"{us_temp,5} ");
                    }
                    file_singleB.Write("\n");
                    break;

                case 0x0c: // save data on c file
                    for (int i = 108; i < 117; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_singleC.Write($"{us_temp,5} ");
                    }
                    file_singleC.Write("\n");
                    break;

                case 0x0d: // save data on d file
                    for (int i = 117; i < 128; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_singleD.Write($"{us_temp,5} ");
                    }
                    file_singleD.Write("\n");
                    break;

                case 0x0e: // save data on e file
                    for (int i = 126; i < 135; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_singleE.Write($"{us_temp,5} ");
                    }
                    file_singleE.Write("\n");
                    break;

                case 0x0f: // save data on f file
                    for (int i = 135; i < 144; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_singleF.Write($"{us_temp,5} ");
                    }
                    file_singleF.Write("\n");
                    break;

                default:
                    break;
            }
        }

        void mode_coin()
        {
            ushort us_temp = 0;
            for (int i = 0; i < 144; ++i)
            {
                us_temp = (ushort)SDATA_BUFFER[i];
                file_coin.Write($"{us_temp,5} ");
            }
            file_coin.Write("\n");
        }

        void mode_cs1()
        {
            byte b_left = (byte)(SDATA_BUFFER[144] >> 8);
            byte b_right = (byte)(SDATA_BUFFER[144]);

            if (b_left != 0 && b_right != 0) // 둘다 값 있을시
            {
                mode_coin();
            }
            else
            {
                for (int i = 0; i < 16; ++i)
                {
                    if (((SDATA_BUFFER[144] >> i) & 0x0001) != 0)
                    {
                        mode_single((short)i);
                    }
                }
            }
        }

        int cc = 0;
        void mode_cs2()
        {
            ushort us_temp = 0;
            byte b_left = (byte)(SDATA_BUFFER[144] >> 8);
            byte b_right = (byte)(SDATA_BUFFER[144]);
            cc++;
            if (cc == 35065)
            {
                int a = 0;
            }
            if (b_left != 0 && b_right != 0) // 둘다 값 있을시
            {
                mode_coin();
            }
            else
            {
                if (b_left != 0)
                {
                    for (int i = 72; i < 144; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_cs2_absorber.Write($"{us_temp,5} ");
                    }
                    file_cs2_absorber.Write("\n");
                }
                else // b_right != 0
                {
                    for (int i = 0; i < 72; ++i)
                    {
                        us_temp = (ushort)SDATA_BUFFER[i];
                        file_cs2_scatter.Write($"{i} : {us_temp,5} ");
                    }
                    file_cs2_scatter.Write("\n");
                }
            }
        }

        readonly string[] modestrings = new string[] { "PULSE", "SPECTRUM", "COUNT", "ADC" };

        public void print_init_new(ref StreamWriter tgt, int mode)
        {
            tgt.WriteLine($"Time: {System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}");
            switch (mode)
            {//저장할때는 struct_ini.mode
                case 0:// enBOARD_MODE.bmPulse:
                    tgt.WriteLine($"Mode: {modestrings[0]}");
                    tgt.WriteLine($"CH\tT_pulse(nsec)\tV_pulse(mV)");
                    break;
                case 1:// enBOARD_MODE.bmSpectrum:
                    tgt.WriteLine($"Mode: {modestrings[1]}");
                    tgt.Write($"CH\t");
                    for (int i = 0; i < 16384; i++)
                        file_main.Write($"{i,16}mv\t");
                    tgt.Write("\n");
                    break;
                case 2:// enBOARD_MODE.bmCount:
                    tgt.WriteLine($"Mode: {modestrings[2]}");
                    break;
                case 3:// enBOARD_MODE.bmADC:
                    tgt.WriteLine($"Mode: {modestrings[3]}");
                    break;
            }
        }

        public int is_init = 0;
        int cur_index_of_ch = 0;
        int cur_index_of_sarray = 0;

        const int PULSE_TIMETAG_INDEX = 0;
        const int PULSE_CH_INDEX = 1;
        const int PULSE_PRE_INDEX = 2;
        const int PULSE_TPD_INDEX = 3;
        const int PULSE_VPD_INDEX = 4;
        const int PULSE_TPT_INDEX = 5;

        ulong[] ulong_buffer = new ulong[] { 0, 0, 0 }; // int total_bytes_of_ch = 0;	// 파라미터로 넘겨 받으니 패스
        bool is_first_calced = true;
        double regr_c = 0;

        // kiwa72(2022.10.23 h00) - 단순 회귀분석 ?? 뭔소리...
        private double simpleRegression(int pre, int pulse)
        {
            //pre값은 이미 -1 곱해져서 들어옴 
            double x0 = -25f;
            double x1 = 0f;

            double y0 = (double)pre;
            double y1 = (double)pulse;

            double aa = (y1 - y0) / (x1 - x0);  // 기울기 = a
#if (false)
			double bb = y0 - (aa * x0);			// bb 값: y = ax + b 

			// y가 0인 x 지점을 리턴 
			//0 = ax + b -> x = -b / a
			return (-1 * bb) / aa;
#else
            // kiwa72(2022.10.25 h00) - ( = 현재 Y좌표 / 기울기)
            return (-1 * y1) / aa;
#endif
        }

        long zero_count = 0;
        long calc_count = 0;

        private void parsing_save(int total_bytes_of_ch)
        {
            if (is_init == 0)
            {
                zero_count = 0;
                calc_count = 0;
                is_init = 1;
                //print_init_old(SDATA_BUFFER[162], SDATA_BUFFER[163], SDATA_BUFFER[164], ((uint)((uint)SDATA_BUFFER[166] << 16) | ((uint)SDATA_BUFFER[165])));
                init_file_save(0);
                print_init_new(ref file_main, struct_ini.mode);
                cur_index_of_ch = 0;
            }

            switch (convert_mode)
            {// 데이터 분석할때는 파일에서 읽은 모드
                case enBOARD_MODE.bmPulse:
                    if (SDATA_BUFFER[PULSE_TPD_INDEX] == 0)
                    {//계산할 필요 없음 
                        ulong_buffer[0] = 0;
                        ulong_buffer[1] = 0;
                        regr_c = 0;
                        zero_count++;
                    }
                    else
                    {
                        calc_count++;
                        regr_c = simpleRegression((int)SDATA_BUFFER[PULSE_PRE_INDEX] * -1, SDATA_BUFFER[PULSE_TPD_INDEX]);
                        Buffer.BlockCopy(SDATA_BUFFER, PULSE_TPT_INDEX * sizeof(ushort), ulong_buffer, 0, 8);//double_buffer[0];
                        ulong_buffer[1] = (ulong_buffer[0] * 8) - (ulong)(50 - regr_c);
                    }

                    file_main.Write($"{(SDATA_BUFFER[PULSE_CH_INDEX]):D4}\t");
                    file_main.Write($"{(ulong_buffer[1]),16}\t");
                    file_main.Write($"{(SDATA_BUFFER[cur_index_of_sarray + PULSE_VPD_INDEX]),10}\n");
                    file_main.Flush();
                    break;

                case enBOARD_MODE.bmSpectrum:
                    {
                        //cur_index_of_sarray = cur_index_of_ch * BYTE_PER_CYCLE_OF_SPECTRUM;
                        Buffer.BlockCopy(SDATA_BUFFER, CH_PER_BOARD * BYTE_PER_CH_OF_SPECTRUM, ulong_buffer, 0, 8);

                        int next_src = 0;
                        int next_dst = 0;
                        // for(int ch = 0;)
                        SORT_BUFFER[0, next_dst] = SDATA_BUFFER[next_src++];
                        SORT_BUFFER[0, next_dst + 1] = SDATA_BUFFER[next_src++];

                        //adsfasd

                        file_main.Write($"elapsed time : {(ulong_buffer[0]),16} ms\n");

                        for (int ch = 0; ch < COUNT_OF_TOTAL_CH; ch++)
                        {
                            file_main.Write($"{(ch + 1):D4}\t");
                            for (int j = 0; j < COUNT_OF_SPECTRUM_STEP; j++)
                            {
                                //file_main.Write($"{(SDATA_BUFFER[/*cur_index_of_sarray +*/ j]),16}\t");
                                file_main.Write($"{(SORT_BUFFER[ch, j]),16}\t");
                            }
                            file_main.Write("\n");
                        }
                    }
                    break;

                case enBOARD_MODE.bmCount:
                    break;

                case enBOARD_MODE.bmADC:
                    break;
            }
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

        private void Set_first_setting()
        {
            RB_coin.Checked = true;//pulse mode
            struct_ini.mode = 0;// enBOARD_MODE.bmPulse
            struct_ini.smoothing = 0;
            cmb_smwindow_pulse.SelectedIndex = 0;//처음 초기화

            for (int i = 0; i < COUNT_OF_TOTAL_CH; i++)
            {
                struct_ini.high_thres_pulse[i] = 2000;
                struct_ini.low_thres_pulse[i] = 1500;
                struct_ini.low_thres_spectrum[i] = 1500;
            }

            struct_ini.t_delay = 25;
            struct_ini.f_value = 1;
            struct_ini.t_spectrum_period = 20;

            // kiwa72(2022.11.09 h15) - 추가
            struct_ini.h_thres = 1000;
            struct_ini.l_thres = 500;
        }

        /**
		 * kiwa72(2022.11.09 h15)
		 * @brief	앱 시작하면서 setting.ini 읽기
		 */
        private void Read_current_ini(bool is_array_only)
        {
            // at first only
            if (!is_array_only)
            {
                int.TryParse(ReadINI("common", "mode"), out struct_ini.mode);
                switch (struct_ini.mode)
                {
                    case 0:
                        RB_coin.Checked = true;
                        break;

                    case 1:
                        throw new NotImplementedException("Not implemented for Spectrum Mode");

                    case 2:
                        throw new NotImplementedException("Not implemented for Count Mode");
                    case 3:
                        throw new NotImplementedException("Not implemented for ADC Mode");
                }

                int.TryParse(ReadINI("common", "smoothing"), out struct_ini.smoothing);
                int.TryParse(ReadINI("pulse", "t_delay"), out struct_ini.t_delay);
                float.TryParse(ReadINI("pulse", "f_value"), out struct_ini.f_value);
                int.TryParse(ReadINI("spectrum", "spectrum_period"), out struct_ini.t_spectrum_period);

                // kiwa72(2022.11.09 h15) - 추가
                int.TryParse(ReadINI("trig", "h_thres"), out struct_ini.h_thres);
                int.TryParse(ReadINI("trig", "l_thres"), out struct_ini.l_thres);
            }

            //TB_0x12.Text = ReadINI("setting", "0x12");
            for (int i = 0; i < COUNT_OF_TOTAL_CH; i++)
            {
                int.TryParse(ReadINI("pulse", $"h_thres{i:D3}"), out struct_ini.high_thres_pulse[i]);
            }

            for (int i = 0; i < COUNT_OF_TOTAL_CH; i++)
            {
                int.TryParse(ReadINI("pulse", $"l_thres{i:D3}"), out struct_ini.low_thres_pulse[i]);
                int.TryParse(ReadINI("spectrum", $"l_thres{i:D3}"), out struct_ini.low_thres_spectrum[i]);
            }

            // kiwa72(2022.11.09 h15) - 읽기 [trig] h_thres, l_thres
            int.TryParse(ReadINI("trig", "h_thres"), out struct_ini.h_thres);
            int.TryParse(ReadINI("trig", "l_thres"), out struct_ini.l_thres);
        }

        private void Write_current_ini(bool is_last)
        {
            if (is_last)
            {
                update_struct_ini_from_gui_n_ini(false);//Write_current_ini
            }

            WriteINI("common", "mode", struct_ini.mode.ToString());
            WriteINI("common", "smoothing", struct_ini.smoothing.ToString());
            WriteINI("pulse", "t_delay", struct_ini.t_delay.ToString());
            WriteINI("pulse", "f_value", struct_ini.f_value.ToString("F2"));    // 2023.02.14 [intellee] 소숫점 표현
            WriteINI("spectrum", "spectrum_period", struct_ini.t_spectrum_period.ToString());

            if (!is_last)
            {
                for (int i = 0; i < COUNT_OF_TOTAL_CH; i++)
                {//현재 gui로 하는게 없으므로 최초에만 기록함 
                    WriteINI("pulse", $"h_thres{i:D3}", struct_ini.high_thres_pulse[i].ToString());
                }

                for (int i = 0; i < COUNT_OF_TOTAL_CH; i++)
                {//현재 gui로 하는게 없으므로 최초에만 기록함 
                    WriteINI("pulse", $"l_thres{i:D3}", struct_ini.low_thres_pulse[i].ToString());
                }

                for (int i = 0; i < COUNT_OF_TOTAL_CH; i++)
                {//현재 gui로 하는게 없으므로 최초에만 기록함 
                    WriteINI("spectrum", $"l_thres{i:D3}", struct_ini.low_thres_spectrum[i].ToString());
                }
            }

            // kiwa72(2022.11.09 h15) - 추가
            WriteINI("trig", "h_thres", struct_ini.h_thres.ToString());
            WriteINI("trig", "l_thres", struct_ini.l_thres.ToString());
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] save - 파일 저장 관련 초기화 작업
        //------------------------------+---------------------------------------------------------------
        private void close_file_save_single()
        {
            if (file_single0 != null)
                file_single0.Close();

            if (file_single1 != null)
                file_single1.Close();

            if (file_single2 != null)
                file_single2.Close();

            if (file_single3 != null)
                file_single3.Close();

            if (file_single4 != null)
                file_single4.Close();

            if (file_single5 != null)
                file_single5.Close();

            if (file_single6 != null)
                file_single6.Close();

            if (file_single7 != null)
                file_single7.Close();

            if (file_single8 != null)
                file_single8.Close();

            if (file_single9 != null)
                file_single9.Close();

            if (file_singleA != null)
                file_singleA.Close();

            if (file_singleB != null)
                file_singleB.Close();

            if (file_singleC != null)
                file_singleC.Close();

            if (file_singleD != null)
                file_singleD.Close();

            if (file_singleE != null)
                file_singleE.Close();

            if (file_singleF != null)
                file_singleF.Close();
        }
        private void init_file_save_single(int mode_cs = 0)
        {
            // 기존에 열려있었다면 stream 닫기
            close_file_save_single();

            // prefix text
            string prefix;
            if (mode_cs == 0)
            {
                prefix = "_SINGLE";
            }
            else // cs1
            {
                prefix = "_SingCoM1_SINGLE";
            }

            // 파일경로 중복 방지 설정
            string p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + "_B0_0.txt");
            int file_name_counter = 0;
            while (System.IO.File.Exists(p_single))
            {
                file_name_counter++;
                p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + $"_B0_{file_name_counter}.txt");
            }
            file_single0 = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B1_{file_name_counter}.txt");
            file_single1 = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B2_{file_name_counter}.txt");
            file_single2 = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B3_{file_name_counter}.txt");
            file_single3 = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B4_{file_name_counter}.txt");
            file_single4 = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B5_{file_name_counter}.txt");
            file_single5 = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B6_{file_name_counter}.txt");
            file_single6 = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B7_{file_name_counter}.txt");
            file_single7 = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B8_{file_name_counter}.txt");
            file_single8 = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B9_{file_name_counter}.txt");
            file_single9 = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B10_{file_name_counter}.txt");
            file_singleA = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B11_{file_name_counter}.txt");
            file_singleB = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B12_{file_name_counter}.txt");
            file_singleC = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B13_{file_name_counter}.txt");
            file_singleD = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B14_{file_name_counter}.txt");
            file_singleE = new StreamWriter(p_single, true);

            p_single = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_B15_{file_name_counter}.txt");
            file_singleF = new StreamWriter(p_single, true);
        }
        private void close_file_save_coin()
        {
            if (file_coin != null)
                file_coin.Close();

            if (file_cs2_scatter != null)
                file_cs2_scatter.Close();

            if (file_cs2_absorber != null)
                file_cs2_absorber.Close();
        }
        private void init_file_save_coin(int mode_cs = 0)
        {
            // 기존에 열려있었다면 stream 닫기
            close_file_save_coin();

            // prefix text
            string prefix;
            if (mode_cs == 0)
            {
                prefix = "_COIN";
            }
            else if (mode_cs == 1)
            {
                prefix = "_SingCoM1_COIN";
            }
            else // 2
            {
                prefix = "_SingCoM2_COIN";
            }

            // 파일경로 중복 방지 설정
            string p_coin = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + ".txt");
            int file_name_counter = 0;
            while (System.IO.File.Exists(p_coin))
            {
                file_name_counter++;
                p_coin = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_{file_name_counter}.txt");
            }
            file_coin = new StreamWriter(p_coin, true);
        }
        private void close_file_save_cs2_sa()
        {
            if (file_coin != null)
                file_coin.Close();

            if (file_cs2_scatter != null)
                file_cs2_scatter.Close();
            if (file_cs2_absorber != null)
                file_cs2_absorber.Close();
        }
        private void init_file_save_cs2_sa()
        {
            // 기존에 열려있었다면 stream 닫기
            close_file_save_cs2_sa();

            // prefix text
            string prefix;
            int file_name_counter = 0;

            // cs2 scatter
            prefix = "_SingCoM2_scatter";
            string p_cs2_scatter = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + ".txt");
            file_name_counter = 0;
            while (System.IO.File.Exists(p_cs2_scatter))
            {
                file_name_counter++;
                p_cs2_scatter = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_{file_name_counter}.txt");
            }
            file_cs2_scatter = new StreamWriter(p_cs2_scatter, true);


            // cs2 absorber
            prefix = "_SingCoM2_absorber";
            string p_cs2_absorber = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + ".txt");
            file_name_counter = 0;
            while (System.IO.File.Exists(p_cs2_absorber))
            {
                file_name_counter++;
                p_cs2_absorber = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + prefix + $"_{file_name_counter}.txt");
            }
            file_cs2_absorber = new StreamWriter(p_cs2_absorber, true);
        }
        private void close_file()
        {
            if (file_main != null)
                file_main.Close();
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
        private void init_file_save(int mode)
        {
            close_file();

            string prefix = "";
            switch (struct_ini.mode)
            {// 저장할때는 struct_ini.mode
                case 0://case enBOARD_MODE.bmPulse:
                    prefix = "PMODE";
                    break;
                case 1://case enBOARD_MODE.bmSpectrum:
                    prefix = "SMODE";
                    break;
                case 2://case enBOARD_MODE.bmCount:
                    prefix = "CMODE";
                    break;
                case 3://case enBOARD_MODE.bmADC:
                    prefix = "AMODE";
                    break;
            }

            // 파일경로 중복 방지 설정
            string p_coin = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + "_" + prefix + ".txt");
            if (!Directory.Exists(Path.GetDirectoryName(p_coin)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(p_coin));
            }
            p_coin = GetUniqueFilePath(p_coin);

            file_main = new StreamWriter(p_coin, true);

        }

        private void init_file_save_bin()
        {
            // .bin 저장을 위한 경로
            string file_path = Application.StartupPath + "\\" + TB_file_name.Text;
            DirectoryInfo di = new DirectoryInfo(file_path);
            if (di.Exists == false) // if new folder not exits
            {
                di.Create(); // create Folder
                file_main_path = (file_path + "\\" + TB_file_name.Text + ".bin");
            }
            else // if exits
            {
                int file_name_counter = 0;

                while (true)
                {
                    string new_file_path = Application.StartupPath + "\\" + TB_file_name.Text + $"_{file_name_counter}";
                    di = new DirectoryInfo(new_file_path);
                    if (di.Exists == false)
                    {
                        di.Create(); // create Folder
                        file_main_path = (new_file_path + "\\" + TB_file_name.Text + $"_{file_name_counter}" + ".bin");
                        break;
                    }
                    else
                    {
                        file_name_counter++;
                    }
                }
            }
        }
        //------------------------------+---------------------------------------------------------------
        // [함수] save - 파일 저장 관련 종료 작업
        //------------------------------+---------------------------------------------------------------
        private void terminate_file_save()
        {
            // StreamWriter 닫기

            if (file_single0 != null)
                file_single0.Close();

            if (file_single1 != null)
                file_single1.Close();

            if (file_single2 != null)
                file_single2.Close();

            if (file_single3 != null)
                file_single3.Close();

            if (file_single4 != null)
                file_single4.Close();

            if (file_single5 != null)
                file_single5.Close();

            if (file_single6 != null)
                file_single6.Close();

            if (file_single7 != null)
                file_single7.Close();

            if (file_single8 != null)
                file_single8.Close();

            if (file_single9 != null)
                file_single9.Close();

            if (file_singleA != null)
                file_singleA.Close();

            if (file_singleB != null)
                file_singleB.Close();

            if (file_singleC != null)
                file_singleC.Close();

            if (file_singleD != null)
                file_singleD.Close();

            if (file_singleE != null)
                file_singleE.Close();

            if (file_singleF != null)
                file_singleF.Close();


            if (file_coin != null)
                file_coin.Close();

            if (file_cs2_scatter != null)
                file_coin.Close();
            if (file_cs2_absorber != null)
                file_coin.Close();
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
            if (tParsing2 != null)
                tParsing2.Wait();
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] input disable
        //------------------------------+---------------------------------------------------------------
        private void input_disable(bool state)
        {
            test_data = 0;
            parsing_status = PS.CUT_TRASH;
            if (!state) // usb start
            {
                StartBtn.Text = "Stop";
            }
            else // usb stop
            {
                StartBtn.Text = "Start";
            }
        }

        public const string version_string = "Ver 22.11.13";
        double test_data = 0;
        double test_data2 = 0;
        int time_out = 0;
        BlockingCollection<byte[]> test_buffer = new BlockingCollection<byte[]>();
        BlockingCollection<byte[]> RealtimeParsingBuffer = new BlockingCollection<byte[]>();
        BlockingCollection<byte[]> ParsedBuffer = new BlockingCollection<byte[]>();
        public List<DaqData> DaqDataList = new List<DaqData>();
        // byte[] test2_buffer = new byte[1024 * 1024 * 700];
        int p_head = 0; // test2_buffer의 50개중 몇번째인지
        int p_tail = 0;
        int i_head = 0; // 1메가 배열 내부의 인덱스
        int p_error = 0;
        // byte[] test_buffer = new byte[1024 * 1024 * 128];


        public void Dispose()
        {
            FM_main_FormClosing(null, null);
        }
        bool is_first_activated = true;
        //------------------------------+---------------------------------------------------------------
        // [이벤트] 하위컨트롤들까지 모두 완료된 후
        //------------------------------+---------------------------------------------------------------
        private void HY_ADC_FPGA_Activated(object sender, EventArgs e)
        {
            if (!is_first_activated)
                return;
            is_first_activated = false;
            init_data_and_buffer();//ini파일도 세팅함 
            init_USB();
        }
        //------------------------------+---------------------------------------------------------------
        // [이벤트] 메인 폼 닫힐 때
        //------------------------------+---------------------------------------------------------------
        private void FM_main_FormClosing(object? sender, FormClosingEventArgs? e)
        {
            // Executes on clicking close button
            bRunning = false;
            if (usbDevices != null)
                usbDevices.Dispose();

            Write_current_ini(true); // 현재 설정 저장하기
            terminate_file_save(); // 파일 저장 종료
            terminate_thread(); // 스레드 종료
            Trace.WriteLine("HY : EXIT");
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] START 버튼 클릭 시
        //------------------------------+---------------------------------------------------------------
        private void BT_start_Click(object sender, EventArgs e)
        {
            //    Read_current_ini(true); // 2023.02.14 [intellee] start 직전에 ini 파일 읽기
            // 아래 함수에서 update_struct_ini_from_gui_n_ini() 함수를 호출하여 gui 상의 변경 값을 반영하는 것으로 보임
            // 하지만 gui에서 변경된 설정값 중 ini 파일에 저장이 안되는 경우가 있는 것으로 보임
            // gui에서 focus를 잃거나 enter를 누르는 경우에 ini 파일에 쓰도록 하려 함
            // update_struct_ini_from_gui_n_ini 호출을 제거
            start_stop_usb();
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

        ////------------------------------+---------------------------------------------------------------
        //// [변수] 타이머 관련 변수
        ////------------------------------+---------------------------------------------------------------
        //public int timer_all_stop = 0; // 강제종료 확인 변수
        //public int timer_mode = 0; // 0 : 종료 / 1 : main만 / 2 : main + sub
        //System.Threading.Timer timer_main; // 누적 시간
        //System.Threading.Timer timer_sub; // 개별 타이머
        //public int timer_count = 0; // 누적 시간 데이터
        //public int timer_count2 = 0; // 곱해서 계산해논 시간
        //public int timer_sub_val = 0; // 개별 타이머 기록값
        //delegate void TimerEventFiredDelegate();

        ////------------------------------+---------------------------------------------------------------
        //// [함수] timer_main 관련
        ////------------------------------+---------------------------------------------------------------
        //private void timer_main_callback(Object state)
        //{
        //    BeginInvoke(new TimerEventFiredDelegate(timer_main_callback_work));
        //}

        //private void timer_main_callback_work()
        //{
        //    timer_count++;
        //    if (timer_count2 == 0 || timer_count2 > timer_count)
        //    {
        //        elapedTimeLabel.Text = $"Acquiring Data : {(timer_count / 3600):D2}:{(timer_count % 3600 / 60):D2}:{(timer_count % 3600 % 60):D2}";
        //        //                elapedTimeLabel.Text = $"Record Time : {(timer_count / 3600):D2}:{(timer_count % 3600 / 60):D2}:{(timer_count % 3600 % 60):D2}";
        //        //                LB_read_count.Text = $"Count rate : {read_fefe_count}";
        //    }
        //    else if (timer_count2 != 0 && timer_count2 == timer_count)
        //    {
        //        elapedTimeLabel.Text = "Saving.";
        //    }
        //    else if (timer_count2 != 0 && timer_count2 + 2 == timer_count)
        //    {
        //        elapedTimeLabel.Text = "Saving.";
        //        start_stop_usb();
        //    }
        //}

        //private void timer_main_on()
        //{
        //    // main timer on
        //    timer_main = new System.Threading.Timer(timer_main_callback);
        //    timer_main.Change(0, 1000); // 대기 : 0 / 반복주기 1초
        //}

        //private void timer_main_off()
        //{
        //    // timer_main 종료
        //    if (timer_main != null)
        //    {
        //        // 1. 대기,반복 무한대로 타이머 종료와같음
        //        timer_main.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        //        // 2. 변수 초기화
        //        elapedTimeLabel.Text = "Stop";
        //        //                LB_setting_time.Text = $"Setting Time : 00:00:00";
        //        //                LB_read_count.Text = "Count rate : 0";
        //        read_fefe_count = 0;
        //    }
        //}

        ////------------------------------+---------------------------------------------------------------
        //// [함수] timer_sub 관련
        ////------------------------------+---------------------------------------------------------------
        //private void timer_sub_callback(Object state)
        //{
        //    BeginInvoke(new TimerEventFiredDelegate(timer_sub_callback_work));
        //}
        //private void timer_sub_callback_work()
        //{
        //    // 1. 현재 sub time / count값 가져오기
        //    int tb_sub_time = int.Parse(TB_r_time.Text);
        //    int tb_sub_count = int.Parse(TB_r_count.Text);

        //    // 2. 시간감소시키기
        //    tb_sub_time--;
        //    if (tb_sub_time > 0)
        //    {
        //        TB_r_time.Text = tb_sub_time.ToString();
        //    }
        //    else if (tb_sub_time == 0)
        //    {

        //    }
        //    else // tb_sub_time == 0
        //    {
        //        if (tb_sub_count != 0)
        //        {
        //            tb_sub_time = timer_sub_val;
        //            tb_sub_count--;
        //            TB_r_count.Text = tb_sub_count.ToString();
        //            TB_r_time.Text = tb_sub_time.ToString();
        //        }
        //        else
        //        {
        //        }
        //    }
        //}
        //private void timer_sub_on()
        //{
        //    // sub timer on
        //    timer_sub = new System.Threading.Timer(timer_sub_callback);
        //    timer_sub.Change(0, 1000); // 대기 : 0 / 반복주기 1초
        //}
        //private void timer_sub_off()
        //{
        //    // timer_sub 종료
        //    if (timer_sub != null)
        //    {
        //        // 1. 대기,반복 무한대로 타이머 종료와같음
        //        timer_sub.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        //        // 2. 변수 초기화
        //        timer_sub_val = 0;
        //        TB_r_time.Text = "0";
        //    }
        //}
        ////------------------------------+---------------------------------------------------------------
        //// [함수] timer usb 관련
        ////------------------------------+---------------------------------------------------------------
        //private void timer_start()
        //{
        //    timer_count = 0;
        //    timer_count2 = tb_0x0a * tb_0x0b;

        //    elapedTimeLabel.Text = $"Acquiring Data : {(timer_count / 3600):D2}:{(timer_count % 3600 / 60):D2}:{(timer_count % 3600 % 60):D2}";

        //    //if (timer_count2 != 0)
        //    //    LB_setting_time.Text = $"Setting Time : {(timer_count2 / 3600):D2}:{(timer_count2 % 3600 / 60):D2}:{(timer_count2 % 3600 % 60):D2}";
        //    //else
        //    //    LB_setting_time.Text = $"Setting Time : 00:00:00";

        //    timer_main_on();

        //}

        //private void timer_end()
        //{
        //    timer_main_off();

        //}

        //------------------------------+---------------------------------------------------------------
        // [이벤트] 콤보박스들 값 변경시 이벤트들
        //------------------------------+---------------------------------------------------------------       
        /**
		 * kiwa72(2022.11.09 h15)
		 * @brief	종료하면서 호출됨
		 */
        // write 하기 전 불림, start 하기 전 불림
        private bool update_struct_ini_from_gui_n_ini(bool is_start)
        {
            bool result = true;
            if (bRunning && is_start)
            {
                return result;
            }
            else
            {
                int.TryParse(TB_0x16.Text, out struct_ini.t_delay);             // t_delay 
                float.TryParse(TB_0x17.Text, out struct_ini.f_value);           // f_value
                int.TryParse(TB_0x18.Text, out struct_ini.t_spectrum_period);   // t spectrum
                struct_ini.smoothing = cmb_smwindow_pulse.SelectedIndex;        // 공통 //update_struct_ini_from_gui_n_ini

                Read_current_ini(true);

                if (!is_start)
                {
                    // writing to ini before closing
                    return true;
                }

                //if (is_start)
                {
                    // update threshold array only
                    Read_current_ini(true);
                    //read struct_ini.t_delay struct_ini.f_value struct_ini.t_spectrum_period  struct_ini.smoothin from ini
                }

                switch (struct_ini.mode)
                {
                    case 0:// enBOARD_MODE.bmPulse:
                        #region pulse 파라미터
                        for (int i = 0; i < COUNT_OF_TOTAL_CH; i++)
                        {
                            if (struct_ini.high_thres_pulse[i] > 16383 || struct_ini.high_thres_pulse[i] < 0)
                            {
                                System.Windows.Forms.MessageBox.Show("허용값을 벗어났습니다.", $"high threshold_pulse{i:D3}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                result = false;
                                break;
                            }

                            if (struct_ini.low_thres_pulse[i] > 16383 || struct_ini.low_thres_pulse[i] < 0)
                            {
                                System.Windows.Forms.MessageBox.Show("허용값을 벗어났습니다.", $"low threshold_pulse{i:D3}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                result = false;
                                break;
                            }
                        }

                        if (struct_ini.t_delay > 10000 || struct_ini.t_delay < 0)
                        {
                            System.Windows.Forms.MessageBox.Show("허용값을 벗어났습니다.", "t delay", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            result = false;
                        }

                        if (((struct_ini.t_delay / 25) * 25) != struct_ini.t_delay)
                        {
                            System.Windows.Forms.MessageBox.Show("25의 배수만 허용합니다.", "t delay", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            result = false;
                        }

                        if (struct_ini.f_value > 1 || struct_ini.f_value < 0f || (struct_ini.f_value > 0 && struct_ini.f_value < 0.01))
                        {
                            System.Windows.Forms.MessageBox.Show("허용 값을 벗어났습니다.", " f ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            result = false;
                        }
                        else
                        {
                            //struct_ini.f_value = (int)(ftemp * 100);
                        }
                        #endregion
                        break;

                    case 1:// enBOARD_MODE.bmSpectrum:
#if true
                        #region spectrume 영역
                        if (struct_ini.t_spectrum_period > 10000 || struct_ini.t_spectrum_period < 20)
                        {
                            System.Windows.Forms.MessageBox.Show("허용값을 벗어났습니다.", "spectrum t period", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            result = false;
                        }

                        for (int i = 0; i < COUNT_OF_TOTAL_CH; i++)
                        {
                            if (struct_ini.low_thres_spectrum[i] > 16383 || struct_ini.low_thres_spectrum[i] < 0)
                            {
                                System.Windows.Forms.MessageBox.Show("허용값을 벗어났습니다.", $"low threshold_spectrum{i:D3}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                result = false;
                                break;
                            }
                        }
                        //
                        #endregion
#endif
                        break;

                    case 2:// enBOARD_MODE.bmCount:
                        break;

                    case 3:// enBOARD_MODE.bmADC:
                        break;

                }
            }

            return result;
        }

        private void RB_coin_CheckedChanged(object? sender, EventArgs e)
        {
            if (bRunning)
                return;
            struct_ini.mode = 0;// enBOARD_MODE.bmPulse;
        }
        private void ConvertData(string filename)
        {
            // data 폴더 생성
            var exePath = Path.GetDirectoryName(Application.ExecutablePath);
            string dataPath = exePath + "\\data\\";
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            // bin 생성
            // PMT_DAQ_100MSPS_3.bin 파일을 복사하여 이름 변경(test)
            string src = filename;
            string[] files = Directory.GetFiles(dataPath);

            string tag = "";
            switch (struct_ini.mode)
            {
                case 0: // pulse
                    tag = "lm";
                    break;
                case 1: // spectrum
                    tag = "sm";
                    break;
                case 2: // count
                    tag = "cm";
                    break;
                case 3: // ADC
                    tag = "am";
                    break;
            }

            List<string> binFileNames = new List<string>();

            foreach (var s in files)
            {
                if (s.Contains(tag) && Path.GetExtension(s) == ".bin")
                    binFileNames.Add(s);
            }

            int idx = 1;
            if (binFileNames.Count > 0)
            {
                binFileNames.Sort();
                string s = Path.GetFileNameWithoutExtension(binFileNames[binFileNames.Count - 1]);
                s = s.Substring(7, 4);
                idx = int.Parse(s) + 1;
            }
            string newBinFile = "daq_" + tag + "_" + idx.ToString("D4") + ".bin";
            File.Copy(src, dataPath + newBinFile);

            // bin 파일을 convert 프로그램에 전달
            string convertEXE = exePath + "\\Analyzer_HY.exe";
            var convertProcess = Process.Start(convertEXE, dataPath + newBinFile);
            //elapedTimeLabel.Text = "Converting";

            //convertProcess.Exited += (o, e) =>
            //{
            //    elapedTimeLabel.Text = "Converted";
            //};
        }

        private void cmb_smwindow_pulse_SelectedIndexChanged(object? sender, EventArgs e)
        {
            struct_ini.smoothing = cmb_smwindow_pulse.SelectedIndex;
            WriteINI("common", "smoothing", struct_ini.smoothing.ToString());
        }

        //------------------------------+---------------------------------------------------------------
        // [변수] USB 관련 변수
        //------------------------------+---------------------------------------------------------------
        public const string chunk_splitter = "====================\n";//chunk end

        bool bVista;

        USBDeviceList usbDevices;
        CyUSBDevice MyDevice;
        CyUSBEndPoint EndPoint;

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
        Task tParsing2;
        Task tRealtimeParsing;
        Task tDaqDataGenerator;
        static public bool bRunning;
        static int bFinalCall;

        // These are  needed for Thread to update the UI
        delegate void UpdateUICallback();
        UpdateUICallback updateUI;

        // These are needed to close the app from the Thread exception(exception handling)
        delegate void ExceptionCallback();
        ExceptionCallback handleException;
        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 변수
        //------------------------------+---------------------------------------------------------------
        private void init_USB()
        {
            // Form 로드시 컴포넌트 초기화
            if (EndPointsComboBox.Items.Count > 0)
                EndPointsComboBox.SelectedIndex = 0;

            bVista = (Environment.OSVersion.Version.Major < 6) ||
            ((Environment.OSVersion.Version.Major == 6) && Environment.OSVersion.Version.Minor == 0);
            // Setup the callback routine for updating the UI

            // Setup the callback routine for NullReference exception handling
            handleException = new ExceptionCallback(ThreadException);

            // Create the list of USB devices attached to the CyUSB3.sys driver.
            usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);

            //Assign event handlers for device attachment and device removal.
            usbDevices.DeviceAttached += new EventHandler(usbDevices_DeviceAttached);
            usbDevices.DeviceRemoved += new EventHandler(usbDevices_DeviceRemoved);

            //Set and search the device with VID-PID 04b4-1003 and if found, selects the end point
            SetDevice(false);
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | The callback routine delegated to handleException.
        //------------------------------+---------------------------------------------------------------
        public void ThreadException()
        {
            StartBtn.Text = "Start";
            bRunning = false;

            t2 = DateTime.Now;
            elapsed = t2 - t1;
            xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
            xferRate = xferRate / (int)100 * (int)100;

            tListen = null;
        }
        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | This is the event handler for device attachment. This method  searches for the device with 
        //                        VID-PID 04b4-00F1
        //------------------------------+---------------------------------------------------------------
        void usbDevices_DeviceAttached(object? sender, EventArgs e)
        {
            SetDevice(false);
        }
        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | This is the event handler for device removal. This method resets the device count and searches for the device with 
        //                        VID-PID 04b4-1003
        //------------------------------+---------------------------------------------------------------
        void usbDevices_DeviceRemoved(object? sender, EventArgs e)
        {
            bRunning = false;

            if (tListen != null /*&& tListen.IsAlive == true*/)
            {
                tListen.Wait();
                //tListen.Abort();
                //tListen.Join();
                //tListen = null;
            }

            MyDevice = null;
            EndPoint = null;
            SetDevice(false);

            if (StartBtn.Text.Equals("Start") == false)
            {
                {
                    StartBtn.Text = "Start";
                    bRunning = false;

                    t2 = DateTime.Now;
                    elapsed = t2 - t1;
                    xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                    xferRate = xferRate / (int)100 * (int)100;
                }
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | Search the device with VID-PID 04b4-00F1 and if found, select the end point
        //------------------------------+---------------------------------------------------------------   
        private void SetDevice(bool bPreserveSelectedDevice)
        {
            int nCurSelection = 0;

            if (DevicesComboBox.Items.Count > 0)
            {
                nCurSelection = DevicesComboBox.SelectedIndex;
                DevicesComboBox.Items.Clear();
            }

            int nDeviceList = usbDevices.Count;

            for (int nCount = 0; nCount < nDeviceList; nCount++)
            {
                USBDevice fxDevice = usbDevices[nCount];
                String strmsg;
                strmsg = "(0x" + fxDevice.VendorID.ToString("X4") + " - 0x" + fxDevice.ProductID.ToString("X4") + ") " + fxDevice.FriendlyName;
                DevicesComboBox.Items.Add(strmsg);
            }

            if (DevicesComboBox.Items.Count > 0)
            {
                DevicesComboBox.SelectedIndex = ((bPreserveSelectedDevice == true) ? nCurSelection : 0);
            }

            USBDevice dev = usbDevices[DevicesComboBox.SelectedIndex];

            if (dev != null)
            {
                MyDevice = (CyUSBDevice)dev;

                GetEndpointsOfNode(MyDevice.Tree);

                PpxBox.Text = "16";     // Set default value to 8 Packets
                QueueBox.Text = "128";  // 128

                if (EndPointsComboBox.Items.Count > 0)
                {
                    EndPointsComboBox.SelectedIndex = 0;
                }
                else
                {
                }

                // Text = MyDevice.FriendlyName;
            }
            else
            {
                EndPointsComboBox.Items.Clear();
                EndPointsComboBox.Text = "";
                // Text = "C# Streamer - no device";
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | Recursive routine populates EndPointsComboBox with strings 
        //                        representing all the endpoints in the device.
        //------------------------------+---------------------------------------------------------------   
        private void GetEndpointsOfNode(TreeNode devTree)
        {
            EndPointsComboBox.Items.Clear(); // 이거 주석되있어서 계속추가되는 버그있던데, 예제소스가 왜그런진 모르겠음
            foreach (TreeNode node in devTree.Nodes)
            {
                if (node.Nodes.Count > 0)
                {
                    GetEndpointsOfNode(node);
                }
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
                        EndPointsComboBox.Items.Add(s);
                    }
                }
            }
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | This is the System event handler.
        //                        Enforces valid values for PPX(Packet per transfer)
        //------------------------------+---------------------------------------------------------------
        private void PpxBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (EndPoint == null) return;

            int ppx = Convert.ToUInt16(PpxBox.Text);
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
                int iIndex = PpxBox.SelectedIndex; // Get the packet index
                PpxBox.Items.Remove(PpxBox.Text); // Remove the Existing  Packet index
                PpxBox.Items.Insert(iIndex, ppx.ToString()); // insert the ppx
                PpxBox.SelectedIndex = iIndex; // update the selected item index

            }


            if ((MyDevice.bSuperSpeed || MyDevice.bHighSpeed) && (EndPoint.Attributes == 1) && (ppx < 8))
            {
                PpxBox.Text = "8";
                MessageBox.Show("Minimum of 8 Packets per Xfer required for HS/SS Isoc.", "Invalid Packets per Xfer.");
            }
            if ((MyDevice.bHighSpeed) && (EndPoint.Attributes == 1))
            {
                if (ppx > 128)
                {
                    PpxBox.Text = "128";
                    MessageBox.Show("Maximum 128 packets per transfer for High Speed Isoc", "Invalid Packets per Xfer.");
                }
            }

        }

        private void DeviceComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            MyDevice = null;
            EndPoint = null;
            SetDevice(true);
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] USB 관련 함수 | This is a system event handler, when the selected index changes(end point selection).
        //------------------------------+---------------------------------------------------------------
        private void EndPointsComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Get the Alt setting
            string sAlt = EndPointsComboBox.Text.Substring(4, 1);
            byte a = Convert.ToByte(sAlt);
            MyDevice.AltIntfc = a;

            // Get the endpoint
            int aX = EndPointsComboBox.Text.LastIndexOf("0x");
            string sAddr = EndPointsComboBox.Text.Substring(aX, 4);
            byte addr = (byte)Util.HexToInt(sAddr);

            EndPoint = MyDevice.EndPointOf(addr);

            // Ensure valid PPX for this endpoint
            PpxBox_SelectedIndexChanged(sender, null);
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] START버튼 눌렀을 시 USB start or stop 관련 작업
        //------------------------------+---------------------------------------------------------------
        int tb_0x11, tb_0x12, tb_0x13, tb_0x14, tb_0x15, tb_0x16, tb_0x17, tb_0x18, tb_0x1a, tb_0x1b, tb_0x0a, tb_0x0b, tb_0x1c;
        int tb_0x19_0, tb_0x19_1, tb_0x19_2, tb_0x19_3, tb_0x19_4, tb_0x19_5, tb_0x19_6, tb_0x19_7, tb_0x19_8, tb_0x19_9, tb_0x19_10, tb_0x19_11, tb_0x19_12, tb_0x19_13, tb_0x19_14, tb_0x19_15;


        private void usb_setting(int flag)
        {
            SetOutputData(flag);
        }

        public void start_stop_usb()
        {
            // 사장님 - Start 눌렀을때
            if (MyDevice == null)
            {
                // kiwa72(2022.11.09 15h) - 장치 연결이 안된 경우 그냥 리턴
                return;
            }

            if (QueueBox.Text == "")
            {
                Trace.WriteLine("Please Select Xfers to Queue Invalid Input");
                return;
            }

            if (!bRunning)
            {
                // Test GetData from box
                //get_data_from_box();
                if (!update_struct_ini_from_gui_n_ini(true))//before start
                {
                    return;
                }
                Write_current_ini(false);   // 2023.02.14 [intellee] 위에서 gui로부터 update를 받았으나 ini 파일에 저장은 되지 않아 저장되도록 함

                Trace.WriteLine("HY : Start");
                // File save 작업
                Trace.WriteLine("HY : [Try] init_file_save ");
                init_file_save_bin();

                // 1. 입력칸 비활성화 및 설정
                Trace.WriteLine("HY : [Try] input_disable ");

                input_disable(false);


                // 2. 버퍼 비우기 
                byte[] Item;
                while (test_buffer.TryTake(out Item, 1))
                {
                }
                while (RealtimeParsingBuffer.TryTake(out Item, 1))
                {
                }
                while (ParsedBuffer.TryTake(out Item, 1))
                {
                }

                Trace.WriteLine("HY : [Try] Send 0000... ");

                usb_setting(3); // send 00000


                Trace.WriteLine("HY : [Try] XferData reset loop");
                EndPointsComboBox.SelectedIndex = 1;
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
                EndPointsComboBox.SelectedIndex = 1;

                BufSz = EndPoint.MaxPktSize * Convert.ToUInt16(PpxBox.Text);
                p_error = EndPoint.MaxPktSize; // 16384
                QueueSz = Convert.ToUInt16(QueueBox.Text);
                PPX = Convert.ToUInt16(PpxBox.Text);

                EndPoint.XferSize = BufSz;

                if (EndPoint is CyIsocEndPoint)
                {
                    IsoPktBlockSize = (EndPoint as CyIsocEndPoint).GetPktBlockSize(BufSz);
                }
                else
                {
                    IsoPktBlockSize = 0;
                }

                bRunning = true;
                bFinalCall = 0;
                thread_run = true;

                Trace.WriteLine("HY : [Try] Start XferThread");
                tListen = new Task(new Action(XferThread));
                tListen.Start();
                Trace.WriteLine("HY : [Try] Start ParsingThread");
                tParsing = new Task(new Action(ParsingThread));
                tParsing.Start();
                tRealtimeParsing = new Task(new Action(RealtimeParsingThread));
                tRealtimeParsing.Start();
                tDaqDataGenerator = new Task(new Action(DaqDataGeneratorThread));
                tDaqDataGenerator.Start();

            }
            else
            {
                Trace.WriteLine("HY : [Try] Stop Button Clicked / send Final Call");
                bFinalCall = 2;

                // usb단
                t2 = DateTime.Now;
                elapsed = t2 - t1;
                xferRate = (long)(XferBytes / elapsed.TotalMilliseconds);
                xferRate = xferRate / (int)100 * (int)100;
                if (tListen != null)
                {
                    tListen.Wait();
                    tListen = null;
                    bRunning = false;
                }

                if (tParsing != null)
                {
                    thread_run = false;
                    tParsing.Wait();
                    tParsing = null;

                    tRealtimeParsing.Wait();
                    tRealtimeParsing = null;

                    tDaqDataGenerator.Wait();
                    tDaqDataGenerator = null;
                }



                // 2023.02.15 [intellee] 최대 5초 기다리기
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (bFinalCall == 2)
                {
                    Thread.Sleep(10);
                    if (sw.ElapsedMilliseconds > 5000) // timeout 5초 걸기
                        break;
                }

                usb_setting(3); // 모든 처리 끝났을때 stop

                DATA_BUFFER_read_count = 0;
                read_buffer_count = 0;

                terminate_file_save();
                Trace.WriteLine("HY : [Try] input_disable enabled");
                input_disable(true);    // 입력칸 enable
                bRunning = false;
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
        public unsafe void XferData(byte[][] cBufs, byte[][] xBufs, byte[][] oLaps, ISO_PKT_INFO[][] pktsInfo, GCHandle[] handleOverlap)
        {
            int k = 0;
            int len = 0;
            int pre_successes = 0;

            Successes = 0;
            Failures = 0;

            XferBytes = 0;
            t1 = DateTime.Now;
            long nIteration = 0;
            CyUSB.OVERLAPPED ovData = new CyUSB.OVERLAPPED();
            // 사장님 - USB Receive Loop
            for (; bRunning;)
            {
                nIteration++;
                // WaitForXfer
                unsafe
                {
                    //fixed (byte* tmpOvlap = oLaps[k])
                    {
                        ovData = (CyUSB.OVERLAPPED)Marshal.PtrToStructure(handleOverlap[k].AddrOfPinnedObject(), typeof(CyUSB.OVERLAPPED));
                        if (!EndPoint.WaitForXfer(ovData.hEvent, 1000))
                        {
                            //
                            // 타임 아웃
                            //

                            EndPoint.Abort();
                            PInvoke.WaitForSingleObject(ovData.hEvent, 100);

                            EndPoint.FinishDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]);

                            for (int i = 0; i < 16; ++i) // 16384 * 16
                            {
                                int check_write = 0;
                                for (int ii = 0; ii < 16384; ++ii)
                                {
                                    if (xBufs[k][i * 16384 + ii] != 0xa5)
                                    {
                                        check_write = 1;
                                        break;
                                    }
                                }
                                if (check_write == 0)
                                    break;

                                byte[] temp_buffer = new byte[16384];
                                byte[] temp_buffer1 = new byte[16384];
                                for (int ii = 0; ii < 16384; ++ii)
                                {
                                    temp_buffer[ii] = xBufs[k][i * 16384 + ii];
                                    temp_buffer1[ii] = xBufs[k][i * 16384 + ii];
                                }
                                //Buffer.BlockCopy(xBufs[k], 0, temp_buffer, 0, 16384);
                                test_buffer.Add(temp_buffer);//timeout
                                                             // 넣을때
                                
                                RealtimeParsingBuffer.Add(temp_buffer1);

                                for (int ii = 0; ii < 16384; ++ii)
                                {
                                    xBufs[k][i * 16384 + ii] = DefaultBufInitValue;
                                }

                                XferBytes += 16384;
                                test_data += 16384;
                                test_data2 = test_buffer.Count();
                                Successes++;
                            }
                        }
                        else
                        {
                            //
                            // 드라이버 단에서 데이터 준비됨
                            //

                            // FinishDataXfer
                            if (EndPoint.FinishDataXfer(ref cBufs[k], ref xBufs[k], ref len, ref oLaps[k]))
                            {
                                byte[] temp_buffer = new byte[len];
                                byte[] temp_buffer1 = new byte[len];
                                Buffer.BlockCopy(xBufs[k], 0, temp_buffer, 0, len);
                                Buffer.BlockCopy(xBufs[k], 0, temp_buffer1, 0, len);

                                test_buffer.Add(temp_buffer);//normal
                                                             // 넣을때
                                RealtimeParsingBuffer.Add(temp_buffer1);


                                XferBytes += len;
                                test_data += len;
                                test_data2 = test_buffer.Count();
                                Successes++;

                                for (int i = 0; i < xBufs[k].Length; i++)
                                    xBufs[k][i] = DefaultBufInitValue;
                            }
                            else
                            {
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
                // File명과 확장자를 가지고 온다.
                string fileName = ofd.SafeFileName;
                // File경로와 File명을 모두 가지고 온다.
                string fileFullName = ofd.FileName;
                // File경로만 가지고 온다.
                string filePath = fileFullName.Replace(fileName, "");

                return fileFullName;
            }
            else if (dr == DialogResult.Cancel)
            {
                // 취소버튼 클릭시 또는 ESC키로 파일창을 종료 했을경우

                return "";
            }

            return "";
        }

        //------------------------------+---------------------------------------------------------------
        // [함수] bin -> txt
        //------------------------------+---------------------------------------------------------------
        const int reg_step = 50;
        StreamWriter file_main;
        string open_path;
        int pre_cut_file = 0;
        enBOARD_MODE convert_mode = enBOARD_MODE.bmPulse;
        public long bin_file_length = 0;
        const long size_margin = BYTES_OF_SOF + BYTES_PER_CH_OF_PULSE;
        const long SOF_SIZE = 4;

        void parse_data_parallel(long index_cnt, long index_last, ref byte[] dBuf, string open_path, DialogResult bDataByChenel)
        {
            using (StreamWriter writer = new StreamWriter($"tmp{index_cnt:D4}.bin", false))
            {
                double regr_c = 0;
                int ok_count = 0;

                // kiwa72(2022.10.22 21h) - 전체 크기 저장
                long nBufBlockSize = dBuf.Length;

                // kiwa72(2022.10.23 21h) - 결과 카운트
                long nRet_OK = 0;
                long nRet_FF = 0;

                // kiwa72(2022.11.09 21h) - 결과물 라인 개수
                long nLineCnt = 0;

                // kiwa72(2022.11.13 21h) - 첫 번째 블록일 경우 헤더 데이터 제외하고 분석 시작
                uint szHead_Jump = 0;
                if (index_cnt == 0)
                {
                    szHead_Jump = ORG_BIN_HEAD_SIZE;
                }

                // kiwa72(2022.11.13 21h) - 마지막 패킷 데이터 길이가 20byte 이하이면 버림 추가
                for (long i = szHead_Jump; ((i + size_margin) <= nBufBlockSize);)
                {
                    // kiwa72(2022.10.22 00h) - 헤더 확인
                    if (dBuf[i + 0] == 0xFE && dBuf[i + 1] == 0xFE && dBuf[i + 2] == 0xFE && dBuf[i + 3] == 0xFE)
                    {
                        // kiwa72(2022.10.22 00h) - PRE_DATA, T_PLUSE_DATA, V_PLUSE_DATA, T_PLUSE_TIME 데이터 모두 & 연산. 0xFF 인지 확인.
                        bool bChk0xFF = true;

                        for (long n = (BYTES_OF_SOF + SEC_TIME + CH_NUMBER); n < size_margin; n++)
                        {
                            if (dBuf[i + n] != 0xFF)
                            {
                                bChk0xFF = false;
                                break;
                            }
                        }

                        // 모두 0xFF 인 경우 For 문으로 Continue
                        if (bChk0xFF == true)
                        {
                            // kiwa72(2022.10.22 21h) - 읽기 다음 패킷 위치 이동
                            i += size_margin;

                            // kiwa72(2022.10.23 21h) - 불량 패킷의 개수
                            nRet_FF++;

                            continue;
                        }

                        /*!------------------------------------------------------------
						 * 최종 결과 계산
						 * ----------------------------------------------------------*/

                        //const int KPULSE_SEC_TIME_INDEX = 0;
                        const int KPULSE_CH_INDEX = 4;
                        const int KPULSE_PRE_INDEX = 6;
                        const int KPULSE_TPD_INDEX = 8;
                        const int KPULSE_VPD_INDEX = 10;
                        const int KPULSE_TPT_INDEX = 12;

                        uint nCh144 = 144;

                        uint Sec_Time_Tag = 10  /* sec */;
                        uint T_Pulse_Tag_8 = 8      /* ns */;    // 채널 0 ~ 143
                        uint T_Pulse_Tag_10 = 10    /* ns */;   // 채널 144


                        // kiwa72(2022.10.22 00h) - 패킷 데이터 저장
                        byte[] sbuf = new byte[BYTES_PER_CH_OF_PULSE];

                        // kiwa72(2022.10.22 00h) - 패킷 헤더(0xFEFEFEFE) 제외한 나머지 데이터를 복사. dBuf -> sbuf
                        Buffer.BlockCopy(dBuf, Convert.ToInt32(i + BYTES_OF_SOF), sbuf, 0, BYTES_PER_CH_OF_PULSE);

                        // kiwa72(2022.10.22 00h) - 계산 결과 저장 변수
                        ulong[] Sec_Time = new ulong[1];
                        ulong[] T_Pulse_Time = new ulong[1];
                        ulong[] T_Pulse_NanoSecond = new ulong[1];

                        //Time_Tag[0] = sbuf[0];  // time tag 1 //20220905
                        Buffer.BlockCopy(sbuf, 0, Sec_Time, 0, SEC_TIME);

                        // kiwa72(2022.11.13 h14) - 이전 상태로 복귀 위해 미적용
#if (false)
						// kiwa72(2022.11.09 15h) - 4, 5 바이트 정보 버림
						Sec_Time[0] = Sec_Time[0] >> 16;
#endif

                        // kiwa72(2022.11.09 15h) - 144번 채널 추가로 소스 위치 이동 저 아래서
                        uint nCH = 0;
                        nCH |= (((uint)sbuf[KPULSE_CH_INDEX + 1]) << 8) & 0xFF00;
                        nCH |= (((uint)sbuf[KPULSE_CH_INDEX + 0]) << 0) & 0x00FF;

                        // kiwa72(2022.11.09 15h) - 채널이 144가 아닌 경우 T_Pulse_Time 기본은 8ns
                        uint T_Pulse_Tag = T_Pulse_Tag_8;

                        if (sbuf[KPULSE_TPD_INDEX] == 0x00)
                        {
                            // 1차 함수 피팅 안함
                            regr_c = 0; // clear
                            Buffer.BlockCopy(sbuf, KPULSE_TPT_INDEX, T_Pulse_Time, 0, 4);

                            // kiwa72(2022.11.09 15h) - 채널이 144인 경우
                            if (nCH == nCh144)
                            {
                                T_Pulse_Tag = T_Pulse_Tag_10;
                            }

                            T_Pulse_NanoSecond[0] = T_Pulse_Time[0] * T_Pulse_Tag;
                        }
                        else
                        {
                            // 1차 함수 피팅 함
                            uint nPre = 0;
                            nPre |= (((uint)sbuf[KPULSE_PRE_INDEX + 1]) << 8) & 0xFF00;
                            nPre |= (((uint)sbuf[KPULSE_PRE_INDEX + 0]) << 0) & 0x00FF;

                            uint nTPD = 0;
                            nTPD |= (((uint)sbuf[KPULSE_TPD_INDEX + 1]) << 8) & 0xFF00;
                            nTPD |= (((uint)sbuf[KPULSE_TPD_INDEX + 0]) << 0) & 0x00FF;

                            // kiwa72(2022.10.23 00h) - 단순 회귀분석 ?? 뭔소리...
                            regr_c = simpleRegression((int)nPre * -1, (int)nTPD);
                            //...

                            Buffer.BlockCopy(sbuf, KPULSE_TPT_INDEX, T_Pulse_Time, 0, 4);

                            // kiwa72(2022.11.09 15h) - 채널이 144인 경우
                            if (nCH == nCh144)
                            {
                                T_Pulse_Tag = T_Pulse_Tag_10;
                            }

                            T_Pulse_NanoSecond[0] = (ulong)(T_Pulse_Time[0] * T_Pulse_Tag) + (ulong)regr_c;
                        }

                        // kiwa72(2022.10.25 00h) - T Pulse Time == 0 인 경우 최종 시간을 0(Zero)로 한다.
                        if (T_Pulse_Time[0] == 0)
                        {
                            T_Pulse_NanoSecond[0] = 0;
                            Sec_Time[0] = 0;
                        }

                        // kiwa72(2022.11.09 15h) - 계산식 변경. 기존 추석 처리
                        double total_time = (ulong)T_Pulse_NanoSecond[0] + ((ulong)Sec_Time[0] * 10000000000);

                        // kiwa72(2022.11.09 15h) - 144번 채널 추가로 소스 위치 이동 저 위로
                        uint nVPD = 0;
                        nVPD |= (((uint)sbuf[KPULSE_VPD_INDEX + 1]) << 8) & 0xFF00;
                        nVPD |= (((uint)sbuf[KPULSE_VPD_INDEX + 0]) << 0) & 0x00FF;

                        // kiwa72(2022.10.23 21h) - 파일 데이터 쓰기 규격
                        writer.Write($"{nCH:D3}\t{total_time:000000000000000}\t{nVPD,10}\n");

                        nLineCnt++;

#if (false)
						if (index_cnt == 7 && nLineCnt == 2365605)
						{
							nLineCnt = nLineCnt + 1 - 1;
						}
#endif

                        // kiwa72(2022.10.25 00h) - 채널별 저장 유/무
                        if (bDataByChenel == DialogResult.Yes)
                        {
                            StreamWriter writer_ch;
                            string p_coin_ch = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + $"_ch_{nCH:D3}.txt");
                            if (Directory.Exists(Path.GetDirectoryName(p_coin_ch)) == false)
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(p_coin_ch));
                                p_coin_ch = GetUniqueFilePath(p_coin_ch);
                            }

                            //writer_ch = new StreamWriter(p_coin_ch, false);
                            writer_ch = File.AppendText(p_coin_ch);

                            // kiwa72(2022.11.13 00h) - 패킷 정보 표시
                            writer_ch.Write($"{nCH:D3}\t{total_time:000000000000000}\t{nVPD,10}\n");

                            writer_ch.Close();
                        }

                        // kiwa72(2022.10.22 21h) - 다음 패킷으로 위치 이동
                        i += size_margin;

                        // kiwa72(2022.10.23 21h) - 정상 패킷의 개수
                        nRet_OK++;

                        // UI 동작을 위한 Sleep
                        if ((++ok_count % 1000) == 1)
                        {
                            Thread.Sleep(0);
                        }

                    }
                    else
                    {
                        // kiwa72(2022.10.22 21h) - 다음 패킷 확인을 위해 1 Byte 다음으로 이동
                        i++;
                    }

                }// end of for (long i = 0, remained = d.Lengthl ...

                // kiwa72(2022.10.23 21h) - 결과 메세지 출력
                //System.Windows.Forms.MessageBox.Show($"정상 개수: {nRet_OK:D}\r\n없는 개수: {nRet_FF:D}", "Convert 결과 확인", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }// end of using (StreamWriter writer = new StreamWriter($"tmp{index:D4}.bin", false))
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct _parsing_struct_
        {
            //[FieldOffset(0)] public fixed byte barray[16];//need unsafe
            //[FieldOffset(0)] public byte[] headerBytes;
            [FieldOffset(0)] public byte byte0;
            [FieldOffset(1)] public byte byte1;
            [FieldOffset(2)] public byte byte2;
            [FieldOffset(3)] public byte byte3;
            [FieldOffset(4)] public byte byte4;
            [FieldOffset(5)] public byte byte5;
            [FieldOffset(6)] public byte byte6;
            [FieldOffset(7)] public byte byte7;
            [FieldOffset(8)] public byte byte8;
            [FieldOffset(9)] public byte byte9;
            [FieldOffset(10)] public byte byteA;
            [FieldOffset(11)] public byte byteB;
            [FieldOffset(12)] public byte byteC;
            [FieldOffset(13)] public byte byteD;
            //[FieldOffset(14)] public byte byteE;
            //[FieldOffset(15)] public byte byteF;
            //[FieldOffset(0)] public uint time1;
            [FieldOffset(0)] public ushort time1;// uint -> ushort
            [FieldOffset(2)] public ushort ch;
            [FieldOffset(4)] public ushort pre;
            [FieldOffset(6)] public ushort tpulse;
            [FieldOffset(8)] public ushort vpulse;
            [FieldOffset(10)] public uint time2;
        }

        void verify_data(string p_coin)
        {
            try
            {
                int old_ch = 17;
                int new_ch = 0;
                ulong pulse_value_0 = 0;
                ulong pulse_value_other = 0;
                string ext_string = Path.GetExtension(p_coin).ToLower();
                StreamWriter writer3 = new StreamWriter(p_coin + "_ch_error.log", false);
                StreamWriter writer4 = new StreamWriter(p_coin + "_time_error.log", false);

                if (ext_string == ".txt")
                {
                    StreamReader reader2 = new StreamReader(p_coin);
                    string next = reader2.ReadLine();//skip ctime caption
                    next = reader2.ReadLine();//skip mode caption
                    next = reader2.ReadLine();// skip item caption
                    int line_count = 3;//skip capations
                    while (true)
                    {
                        next = reader2.ReadLine();
                        if (next == null)
                            break;
                        line_count++;
                        int.TryParse(next.Substring(0, 3), out new_ch);
                        if (new_ch == 0)
                        {
                            ulong.TryParse(next.Substring(4, 15), out pulse_value_0);
                            pulse_value_other = pulse_value_0;
                            if (old_ch != 17)
                                writer3.WriteLine($"ch error line {line_count:D8} line_old[{old_ch}]new[{new_ch}]");
                        }
                        else
                        {
                            ulong.TryParse(next.Substring(4, 15), out pulse_value_other);
                            if (new_ch - old_ch != 1)
                            {
                                writer3.WriteLine($"ch error line {line_count:D8} line_old[{old_ch}]new[{new_ch}]");
                            }
                        }
                        if (pulse_value_other != pulse_value_0)
                        {
                            writer4.WriteLine($"pulse error line {line_count:D8} current[{pulse_value_other}]ch_o[{pulse_value_0}]");
                        }
                        old_ch = new_ch;
                    }
                    reader2.Dispose();

                }
                else
                {
                    BinaryReader b = new BinaryReader(File.Open(p_coin, FileMode.Open));
                    long bin_file_length = b.BaseStream.Length;
                    int fefe_count = 0;
                    byte read_byte = 0;
                    _parsing_struct_ new_st = new _parsing_struct_();
                    _parsing_struct_ old_st = new _parsing_struct_();

                    //_parsing_struct_[] parsing_array = new _parsing_struct_[18];
                    old_st.ch = 17;
                    old_st.time2 = 0;
                    int data_count = 0;
                    long used_bytes = 0;
                    int array_index = 0;
                    ushort base_ch = 0;
                    while (bin_file_length > 0)
                    {
                        read_byte = b.ReadByte();
                        bin_file_length--;
                        used_bytes++;
                        if (read_byte == 0xFE)
                        {
                            fefe_count++;
                        }
                        else
                            fefe_count = 0;
                        if (fefe_count == 4)
                        {
                            if (bin_file_length > 13)
                            {
                                data_count++;
                                //new_st.headerBytes = b.ReadBytes(16);
                                {
                                    new_st.byte0 = b.ReadByte();
                                    new_st.byte1 = b.ReadByte();
                                    new_st.byte2 = b.ReadByte();
                                    new_st.byte3 = b.ReadByte();
                                    new_st.byte4 = b.ReadByte();
                                    new_st.byte5 = b.ReadByte();
                                    new_st.byte6 = b.ReadByte();
                                    new_st.byte7 = b.ReadByte();
                                    new_st.byte8 = b.ReadByte();
                                    new_st.byte9 = b.ReadByte();
                                    new_st.byteA = b.ReadByte();
                                    new_st.byteB = b.ReadByte();
                                    new_st.byteC = b.ReadByte();
                                    new_st.byteD = b.ReadByte();
                                    //new_st.byteE = b.ReadByte(); timetag가 2바이트 줄어듬
                                    //new_st.byteF = b.ReadByte();
                                }
                                bin_file_length -= 14;//16 -> 14
                                                      //if (new_st.ch == 0)
                                if (new_st.ch % 18 == 0)
                                {
                                    base_ch = new_st.ch;
                                    old_st.time1 = new_st.time1;//set base value
                                    old_st.pre = new_st.pre;//set base value
                                    old_st.time2 = new_st.time2;//set base value
                                    if ((old_st.ch % 18) != 17)
                                        writer3.WriteLine($"ch error bytes {used_bytes - 4:D10} ch_old[{old_st.ch}]ch_new[{new_st.ch}]");
                                }
                                else
                                {
                                    if (new_st.ch - old_st.ch != 1)
                                    {
                                        writer3.WriteLine($"ch error bytes {used_bytes - 4:D10} ch_old[{old_st.ch}]ch_new[{new_st.ch}]");
                                    }

                                    if (new_st.pre != old_st.pre)
                                    {
                                        writer4.WriteLine($"pre error bytes {used_bytes - 4:D10} ch{base_ch:D2}[{old_st.pre}]ch{new_st.ch:D2}[{new_st.pre}]");
                                    }
                                    if (new_st.time2 != old_st.time2)
                                    {
                                        writer4.WriteLine($"pulse time error bytes {used_bytes - 4:D10} ch{base_ch:D2}[{old_st.time2}]ch{new_st.ch:D2}[{new_st.time2}]");
                                    }

                                    if (new_st.time1 != old_st.time1)
                                    {
                                        writer4.WriteLine($"timetag error bytes {used_bytes - 4:D10}   ch{base_ch:D2}[{old_st.time1}]ch{new_st.ch:D2}[{new_st.time1}]");
                                    }

                                }
                                old_st.ch = new_st.ch;
                                used_bytes += 16;
                            }
                            fefe_count = 0;//clear
                        }
                    }
                    b.Dispose();
                }
                writer3.Dispose();
                writer4.Dispose();
            }
            catch (System.IO.IOException t)
            {
                MessageBox.Show("대상 파일을 다른 프로그램이  사용중이거나  정상적으로 접근 할 수 없습니다.");
                return;
            }
        }

        void merge_data(long count, string open_path)
        {
            //
            //using (StreamWriter writer = new StreamWriter(open_path + ".result", false))
            string p_coin = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + ".txt");
            if (!Directory.Exists(Path.GetDirectoryName(p_coin)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(p_coin));
            }
            p_coin = GetUniqueFilePath(p_coin);
            StreamWriter writer2 = new StreamWriter(p_coin, false);
            //{
            print_init_new(ref writer2, Convert.ToInt32(enBOARD_MODE.bmPulse));//ref writer 때문에 using 못씀
            string next = null;
            for (int index = 0; index < count; index++)
            {
                using (StreamReader reader = new StreamReader($"tmp{index:D4}.bin"))
                {

                    while (true)
                    {
                        next = reader.ReadLine();
                        if (next == null)
                            break;
                        else
                            writer2.WriteLine(next);
                    }
                }
            }
            //}
            writer2.Dispose();
            //verify_data(p_coin);
        }

        // 시스템 프로세스 개수
        long real_core_count = Environment.ProcessorCount;

        // 채널 개수
        const int Max_Ch_Count = 145;

        private void ConvertThread2()
        {
            try
            {
                using (BinaryReader binaryOrg = new BinaryReader(File.Open(open_path, FileMode.Open)))  // kiwa72(2022.10.21) - b ->> binaryOrg 변수명 변경
                {
                    long bin_file_length = binaryOrg.BaseStream.Length;
                    //long pos = 0;
                    string modestring = new string(binaryOrg.ReadChars(5)); // \n 버리자

                    if (modestring == "PMODE")
                    {
                        convert_mode = enBOARD_MODE.bmPulse;
                    }
                    else if (modestring == "SMODE")
                    {
                        convert_mode = enBOARD_MODE.bmSpectrum;
                    }
                    else if (modestring == "CMODE")
                    {
                        convert_mode = enBOARD_MODE.bmCount;
                    }
                    else if (modestring == "AMODE")
                    {
                        convert_mode = enBOARD_MODE.bmADC;
                    }
                    else
                    {
                        convert_mode = enBOARD_MODE.bmPulse;
                    }

                    // kiwa72(2022.10.21 21h) - 적용되는 코어 개수
                    long core_loop_count = 0;

                    // kiwa72(2022.11.13 21h) - 나머지 블록 유/무
                    long bBlock_Remainder = 0;

                    // kiwa72(2022.11.13 21h) - 기준 블록 크기
                    long Base_Block_Size = (BYTES_SIZE_CH_PACKET * 9000000);

                    // kiwa72(2022.10.21 21h) - 패킷 개수 9,000,000개 이상인 경우 멀티코어 적용
                    if (bin_file_length > Base_Block_Size)
                    {
                        // kiwa72(2022.11.13 21h) - 사용할 코어 개수 = core_loop_count
                        bBlock_Remainder = ((bin_file_length % Base_Block_Size) > 0 ? 1 : 0);
                        core_loop_count = (bin_file_length / Base_Block_Size) + bBlock_Remainder;
                    }
                    else
                    {
                        // kiwa72(2022.10.21 21h) - 패킷 개수 3,000,000개 미만인 경우 싱글 코어 적용
                        core_loop_count = 1;
                    }

                    //Trace.WriteLine($"ConvertThread2 core_block_size:{core_block_size}, base_chunk_size{core_block_size}, loop_count {core_loop_count}");

                    // kiwa72(2022.10.21 21h) - 코어 블록 별 저장할 데이터 크기
                    long[] Core_Chunk_Size = new long[core_loop_count];

                    // kiwa72(2022.10.21 21h) - 원본에서 데이터를 읽을 offset 위치
                    long[] BinOrg_Read_Offset = new long[core_loop_count];

                    // kiwa72(2022.10.21 21h) - 기존 임시 ~.bin 데이터 파일 삭제
                    //for (int i = 0; i < core_loop_count; i++)
                    for (int i = 0; i < 100; i++)
                    {
                        System.IO.File.Delete($"tmp{i:D4}.bin");
                    }

                    // kiwa72(2022.11.18 h19) - 채널별 파일 삭제
                    for (int ch = 0; ch <= Max_Ch_Count; ch++)
                    {
                        string pathCh = Path.Combine(Path.GetDirectoryName(open_path), Path.GetFileNameWithoutExtension(open_path) + $"_ch_{ch:D3}.txt");
                        System.IO.File.Delete(pathCh);
                    }

                    // kiwa72(2022.11.13 21h) - 코어 당 블록 크기 저장
                    if (core_loop_count > 1)
                    {
                        for (int i = 0; i < (core_loop_count - bBlock_Remainder); i++)
                        {
                            if (i == 0)
                            {
                                Core_Chunk_Size[i] = (Base_Block_Size + ORG_BIN_HEAD_SIZE);
                                BinOrg_Read_Offset[i] = 0;
                            }
                            else
                            {
                                Core_Chunk_Size[i] = Base_Block_Size;
                                BinOrg_Read_Offset[i] += Core_Chunk_Size[i - 1];
                            }

                            Trace.WriteLine($"ConvertThread2 Base_Block_Size:{Base_Block_Size}, Core_Chunk_Size:{Core_Chunk_Size[i]}, loop_count {i}");
                        }

                        if (bBlock_Remainder == 1)
                        {
                            long i = core_loop_count - bBlock_Remainder;
                            Core_Chunk_Size[i] = (bin_file_length % Base_Block_Size);// - ORG_BIN_HEAD_SIZE;
                            BinOrg_Read_Offset[i] += (i * Base_Block_Size);

                            Trace.WriteLine($"ConvertThread2 Base_Block_Size:{Base_Block_Size}, Core_Chunk_Size:{Core_Chunk_Size[i]}, loop_count {i}");
                        }
                    }
                    else
                    {
                        Core_Chunk_Size[0] = bin_file_length;
                        BinOrg_Read_Offset[0] = 0;
                    }

                    // kiwa72(2022.10.22 21h) - 스탑워치 시작
                    var s1 = Stopwatch.StartNew();

                    // kiwa72(2022.10.23 21h) - 똥글뱅이 돌아가는 동그라미 커서 (안됨)
                    Cursor.Current = Cursors.WaitCursor;

                    // kiwa72(2022.10.25 21h) - 채널별로 결과 만들지 선택 메세지
                    //DialogResult result = System.Windows.Forms.MessageBox.Show("채널별로 결과 파일을 생성할 수 있습니다.\r\n시간이 많이 걸릴 수 있습니다.\r\n하시겠습니까?", "채널별 파일 생성 유/무", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    DialogResult result = DialogResult.No;


#if (false)
					// kiwa72(2022.10.21) - 각 블록 데이터 저장 버퍼 생성
					byte[][] buffers = new byte[core_loop_count][];

					for (int i = 0; i < core_loop_count; i++)
					{
						//buffers[i] = new byte[Core_Chunk_Size[i]];
						buffers[0] = new byte[Core_Chunk_Size[i]];
						Trace.WriteLine($"i = {i, 4}\tbuffers[i].LongLength:{buffers[0].LongLength}");
					}

					for (long i = 0; i < core_loop_count; i++)
					{
						Trace.WriteLine($"current loop_count {i:D4} / {core_loop_count:D4}.bin ...");

						// kiwa72(2022.10.21 21h) - 버퍼 초기화
						Array.Clear(buffers[i], 0x00, buffers[i].Length);

						// kiwa72(2022.10.21 21h) - 파일 읽기 위치 이동
						binaryOrg.BaseStream.Seek(BinOrg_Read_Offset[i], SeekOrigin.Begin);

						// kiwa72(2022.10.21 21h) - 파일 읽기 : 1GB 이상이면 읽기 안됨 - 안전하게 500MB로 진행
						buffers[i] = binaryOrg.ReadBytes(Convert.ToInt32(Core_Chunk_Size[i]));

#if (false)
						// kiwa72(2022.10.21 21h) - 원본 파일을 (20 * 9000000) 크기로 파일을 나누어 병렬 쓰레드 처리
						// kiwa72(2022.11.13 21h) - 대용량에서 안됨 : 멀티코어 동작에 문제가 있는데 분석할 시간 없음
						Parallel.For(0, core_loop_count, k =>
						{
							// kiwa72(2022.10.21 21h) - 패킷을 파싱 한다
							parse_data_parallel(k, core_loop_count, ref buffers[k], open_path, result);
						});
#else
						// kiwa72(2022.11.13 21h) - 블록 단위로 싱글 코어로 적용
						parse_data_parallel(i, core_loop_count, ref buffers[i], open_path, result);
#endif

						update_converted_state((i * 100) / core_loop_count);
					}

#else

                    // kiwa72(2022.11.18) - 하나의 블록 버퍼에 데이터를 저장하고 데이터를 로딩 파싱
                    byte[] buffers;

                    for (int i = 0; i < core_loop_count; i++)
                    {
                        //buffers[i] = new byte[Core_Chunk_Size[i]];
                        buffers = new byte[Core_Chunk_Size[i]];
                        Trace.WriteLine($"i = {i,4}\tbuffers[i].LongLength:{buffers.Length}");

                        //.....................................................

                        // kiwa72(2022.10.21 21h) - 버퍼 초기화
                        Array.Clear(buffers, 0x00, buffers.Length);

                        // kiwa72(2022.10.21 21h) - 파일 읽기 위치 이동
                        binaryOrg.BaseStream.Seek(BinOrg_Read_Offset[i], SeekOrigin.Begin);

                        // kiwa72(2022.10.21 21h) - 파일 읽기 : 1GB 이상이면 읽기 안됨 - 안전하게 500MB로 진행
                        buffers = binaryOrg.ReadBytes(Convert.ToInt32(Core_Chunk_Size[i]));

                        // kiwa72(2022.11.13 21h) - 블록 단위로 싱글 코어로 적용
                        parse_data_parallel(i, core_loop_count, ref buffers, open_path, result);

                        //.....................................................

                    }
#endif

                    // kiwa72(2022.10.22 21h) - 스탑워치 종료
                    s1.Stop();

                    // kiwa72(2022.10.22 21h) - 스탑워치 결과 출력
                    Trace.WriteLine($"parsing time : {s1.Elapsed.TotalMilliseconds}msec");

                    s1.Start();

                    // kiwa72(2022.10.21 21h) - 병렬로 나누어 저장된 결과 파일을 파일 순서대로 합친다.
                    merge_data(core_loop_count, open_path);

                    s1.Stop();
                    Trace.WriteLine($"mersing time: {s1.Elapsed.TotalMilliseconds}msec");

#if DEBUG
                    Trace.WriteLine("do not delete temporary file");
#else
					for (int i = 0; i < core_loop_count; i++)
					{
						System.IO.File.Delete($"tmp{i:D4}.bin");
					}
#endif
                    //close_file_save_single();
                    //close_file_save_coin();
                    //close_file();//file_main
                }
            }
            catch (System.IO.IOException t)
            {
                MessageBox.Show("대상 파일을 다른 프로그램이  사용중이거나  정상적으로 접근 할 수 없습니다.");
                return;
            }
        }

        public bool is_converting = false;

    }
}
#pragma warning restore CS0414 // Naming Styles